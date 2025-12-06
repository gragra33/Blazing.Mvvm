using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that suggests overriding lifecycle methods when constructors contain initialization logic.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LifecycleMethodOverrideAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.LifecycleMethodSuggestion);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes that inherit from ViewModelBase
        if (!InheritsFromViewModelBase(namedTypeSymbol, context.Compilation))
        {
            return;
        }

        // Check if the class has a constructor with non-trivial logic
        var constructors = namedTypeSymbol.Constructors
            .Where(c => !c.IsImplicitlyDeclared && c.MethodKind == MethodKind.Constructor)
            .ToList();

        if (!constructors.Any())
        {
            return;
        }

        // Check if any lifecycle methods are already overridden
        var hasLifecycleOverride = HasLifecycleMethodOverride(namedTypeSymbol);

        if (hasLifecycleOverride)
        {
            return;
        }

        // Check if constructor has meaningful code (more than just assignments)
        foreach (var constructor in constructors)
        {
            if (constructor.DeclaringSyntaxReferences.Length == 0)
            {
                continue;
            }

            var syntaxReference = constructor.DeclaringSyntaxReferences[0];
            var syntax = syntaxReference.GetSyntax(context.CancellationToken);

            // Simple heuristic: if constructor has body with statements, suggest lifecycle methods
            var hasBody = syntax.DescendantNodes().Any(n => 
                n.IsKind(SyntaxKind.Block));

            if (hasBody)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.LifecycleMethodSuggestion,
                    constructor.Locations[0],
                    AnalyzerConstants.MethodNames.OnInitializedAsync);

                context.ReportDiagnostic(diagnostic);
                break;
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

    private static bool HasLifecycleMethodOverride(INamedTypeSymbol typeSymbol)
    {
        var lifecycleMethodNames = new[]
        {
            AnalyzerConstants.MethodNames.OnInitialized,
            AnalyzerConstants.MethodNames.OnInitializedAsync,
            AnalyzerConstants.MethodNames.OnParametersSet,
            AnalyzerConstants.MethodNames.OnParametersSetAsync,
            AnalyzerConstants.MethodNames.OnAfterRender,
            AnalyzerConstants.MethodNames.OnAfterRenderAsync
        };

        return typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => lifecycleMethodNames.Contains(m.Name) && m.IsOverride);
    }
}
