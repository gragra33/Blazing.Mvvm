using System.Collections.Immutable;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Blazing.Mvvm.Analyzers.Helpers;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures NavigateTo&lt;TViewModel&gt;() calls reference ViewModels with valid route mappings.
/// Now supports multi-project architectures where ViewModels may be in referenced assemblies.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NavigationTypeSafetyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.InvalidNavigationTarget];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationContext =>
        {
            // Use thread-safe collection for concurrent execution
            var viewModelRoutes = new ConcurrentBag<INamedTypeSymbol>();
            var invocationsToCheck = new ConcurrentBag<(InvocationExpressionSyntax Invocation, SemanticModel Model)>();

            // First pass: collect all ViewModels from current compilation
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                CollectViewModelRoutes(symbolContext, viewModelRoutes);
            }, SymbolKind.NamedType);

            // Second pass: collect all NavigateTo invocations
            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                var invocationExpression = (InvocationExpressionSyntax)nodeContext.Node;
                var methodName = invocationExpression.Expression.ToString();
                
                if (methodName.Contains("NavigateTo"))
                {
                    invocationsToCheck.Add((invocationExpression, nodeContext.SemanticModel));
                }
            }, SyntaxKind.InvocationExpression);

            // Third pass: analyze all collected invocations after all symbols are collected
            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                // Also collect ViewModels from referenced assemblies for cross-project support
                foreach (var reference in endContext.Compilation.References)
                {
                    if (endContext.Compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                    {
                        var referencedViewModels = CrossProjectAnalyzerHelper.GetViewModelsFromAssembly(assemblySymbol);
                        foreach (var vm in referencedViewModels)
                        {
                            viewModelRoutes.Add(vm);
                        }
                    }
                }

                // Now analyze all invocations with complete ViewModel list (current + referenced)
                foreach (var (invocation, semanticModel) in invocationsToCheck)
                {
                    AnalyzeNavigateToInvocation(endContext, invocation, semanticModel, viewModelRoutes);
                }
            });
        });
    }

    private static void CollectViewModelRoutes(SymbolAnalysisContext context, ConcurrentBag<INamedTypeSymbol> viewModelRoutes)
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
        CompilationAnalysisContext context,
        InvocationExpressionSyntax invocationExpression,
        SemanticModel semanticModel,
        ConcurrentBag<INamedTypeSymbol> viewModelRoutes)
    {
        // Get semantic model to resolve generic type
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

        // Skip if it doesn't end with "ViewModel" suffix
        if (!namedViewModelType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return;
        }

        // Check if the ViewModel is valid (thread-safe check)
        var isValidViewModel = viewModelRoutes.Any(vm => SymbolEqualityComparer.Default.Equals(vm, namedViewModelType)) ||
                               HasViewModelDefinitionAttribute(namedViewModelType) ||
                               InheritsFromViewModelBase(namedViewModelType, context.Compilation);

        if (!isValidViewModel)
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
            attr.AttributeClass?.Name == AnalyzerConstants.AttributeNames.ViewModelDefinition ||
            attr.AttributeClass?.Name == $"{AnalyzerConstants.AttributeNames.ViewModelDefinition}Attribute");
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
