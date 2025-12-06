using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures ViewModels have the [ViewModelDefinition] attribute for proper DI registration.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewModelDefinitionAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewModelDefinitionMissing);

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

        // Skip abstract classes
        if (namedTypeSymbol.IsAbstract)
        {
            return;
        }

        // Check if the ViewModelDefinition attribute is present
        var viewModelDefinitionAttribute = context.Compilation.GetTypeByMetadataName(
            AnalyzerConstants.TypeNames.ViewModelDefinitionAttribute);

        if (viewModelDefinitionAttribute == null)
        {
            return;
        }

        var hasAttribute = namedTypeSymbol.GetAttributes()
            .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, viewModelDefinitionAttribute));

        if (hasAttribute)
        {
            return;
        }

        // Report diagnostic
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ViewModelDefinitionMissing,
            namedTypeSymbol.Locations[0],
            namedTypeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
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
