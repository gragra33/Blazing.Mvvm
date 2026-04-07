using System.Collections.Immutable;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures NavigateTo&lt;TViewModel&gt;() calls reference ViewModels with valid route mappings.
/// Validates that the target ViewModel inherits from ViewModelBase AND has [ViewModelDefinition] attribute.
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
            var viewModelRoutes = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

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

    private static void CollectViewModelRoutes(SymbolAnalysisContext context, ConcurrentDictionary<string, byte> viewModelRoutes)
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

        // Navigation requires DI registration, which requires BOTH:
        // 1. Inheritance from ViewModelBase (architectural requirement)
        // 2. [ViewModelDefinition] attribute (DI registration requirement)
        var inheritsFromBase = InheritsFromViewModelBase(namedTypeSymbol, context.Compilation);
        var hasDefinition = HasViewModelDefinitionAttribute(namedTypeSymbol);
        var hasAssociatedRoute = HasAssociatedRoute(namedTypeSymbol, context.Compilation);

        // Only ViewModels with BOTH are valid navigation targets
        if (inheritsFromBase && hasDefinition && hasAssociatedRoute)
        {
            viewModelRoutes.TryAdd(namedTypeSymbol.ToDisplayString(), 0);
        }
    }

    private static void AnalyzeNavigateToInvocation(
        SyntaxNodeAnalysisContext context,
        ConcurrentDictionary<string, byte> viewModelRoutes)
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
        if (!viewModelRoutes.ContainsKey(namedViewModelType.ToDisplayString()))
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
            attr.AttributeClass?.Name is AnalyzerConstants.AttributeNames.ViewModelDefinition or "ViewModelDefinitionAttribute");
    }

    private static bool HasAssociatedRoute(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not (AnalyzerConstants.AttributeNames.ViewModelDefinition or "ViewModelDefinitionAttribute"))
            {
                continue;
            }

            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key == "ViewType" && namedArgument.Value.Value is INamedTypeSymbol viewType)
                {
                    return HasRouteAttribute(viewType);
                }
            }
        }

        foreach (var candidate in EnumerateNamedTypes(compilation.Assembly.GlobalNamespace))
        {
            if (!HasRouteAttribute(candidate))
            {
                continue;
            }

            var baseType = candidate.BaseType;
            while (baseType != null)
            {
                var originalDefinition = baseType.OriginalDefinition.ToDisplayString();
                if ((originalDefinition.StartsWith("Blazing.Mvvm.Components.MvvmComponentBase<", StringComparison.Ordinal) ||
                     originalDefinition.StartsWith("Blazing.Mvvm.Components.MvvmOwningComponentBase<", StringComparison.Ordinal) ||
                     originalDefinition.StartsWith("Blazing.Mvvm.Components.MvvmLayoutComponentBase<", StringComparison.Ordinal)) &&
                    baseType.TypeArguments.Length > 0 &&
                    SymbolEqualityComparer.Default.Equals(baseType.TypeArguments[0], typeSymbol))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }
        }

        return false;
    }

    private static bool HasRouteAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.Name is "RouteAttribute" or "Route");
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetTypeMembers())
        {
            yield return member;

            foreach (var nested in EnumerateNestedTypes(member))
            {
                yield return nested;
            }
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var member in EnumerateNamedTypes(childNamespace))
            {
                yield return member;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol typeSymbol)
    {
        foreach (var nested in typeSymbol.GetTypeMembers())
        {
            yield return nested;

            foreach (var child in EnumerateNestedTypes(nested))
            {
                yield return child;
            }
        }
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
