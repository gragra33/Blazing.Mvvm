using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures NavigateTo&lt;TViewModel&gt;() calls reference ViewModels with valid route mappings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NavigationTypeSafetyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.InvalidNavigationTarget);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var navigateToMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var viewModelRoutes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                CollectViewModelRoutes(symbolContext, viewModelRoutes);
            }, SymbolKind.NamedType);

            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                AnalyzeNavigateToInvocation(nodeContext, viewModelRoutes);
            }, SyntaxKind.InvocationExpression);
        });
    }

    private static void CollectViewModelRoutes(SymbolAnalysisContext context, HashSet<INamedTypeSymbol> viewModelRoutes)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Check if the class name ends with "ViewModel"
        if (!namedTypeSymbol.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return;
        }

        // Skip interfaces, abstract classes
        if (namedTypeSymbol.TypeKind != TypeKind.Class || namedTypeSymbol.IsAbstract)
        {
            return;
        }

        // Check if it has ViewModelDefinition attribute or inherits from ViewModelBase
        if (HasViewModelDefinitionAttribute(namedTypeSymbol) || InheritsFromViewModelBase(namedTypeSymbol, context.Compilation))
        {
            viewModelRoutes.Add(namedTypeSymbol);
        }
    }

    private static void AnalyzeNavigateToInvocation(
        SyntaxNodeAnalysisContext context,
        HashSet<INamedTypeSymbol> viewModelRoutes)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        // Check if this is a NavigateTo call
        var methodName = invocationExpression.Expression.ToString();
        if (!methodName.Contains("NavigateTo"))
        {
            return;
        }

        // Get semantic model to resolve generic type
        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Check if it's a generic method with type arguments
        if (!methodSymbol.IsGenericMethod || methodSymbol.TypeArguments.Length == 0)
        {
            return;
        }

        // Get the TViewModel type argument
        var viewModelType = methodSymbol.TypeArguments[0];
        if (viewModelType is not INamedTypeSymbol namedViewModelType)
        {
            return;
        }

        // Check if the ViewModel has a valid route mapping
        if (!viewModelRoutes.Contains(namedViewModelType, SymbolEqualityComparer.Default))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.InvalidNavigationTarget,
                invocationExpression.GetLocation(),
                namedViewModelType.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasViewModelDefinitionAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.Name == AnalyzerConstants.AttributeNames.ViewModelDefinition);
    }

    private static bool InheritsFromViewModelBase(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var viewModelBaseTypes = new[]
        {
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ViewModelBase),
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.RecipientViewModelBase),
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ValidatorViewModelBase)
        };

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (viewModelBaseTypes.Any(vb => vb != null && SymbolEqualityComparer.Default.Equals(baseType, vb)))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }
}
