using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures Blazor components using ViewModels inherit from MvvmComponentBase&lt;TViewModel&gt;.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MvvmComponentBaseUsageAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MvvmComponentBaseMissing);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes that inherit from ComponentBase
        if (!InheritsFromComponentBase(namedTypeSymbol, context.Compilation))
        {
            return;
        }

        // Skip if already inherits from MvvmComponentBase or related classes
        if (InheritsFromMvvmComponentBase(namedTypeSymbol, context.Compilation))
        {
            return;
        }

        // Check if the component has a ViewModel property
        var hasViewModelProperty = namedTypeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Any(p => p.Name == AnalyzerConstants.PropertyNames.ViewModel &&
                     InheritsFromViewModelBase(p.Type as INamedTypeSymbol, context.Compilation));

        if (!hasViewModelProperty)
        {
            return;
        }

        // Report diagnostic
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.MvvmComponentBaseMissing,
            namedTypeSymbol.Locations[0],
            namedTypeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool InheritsFromComponentBase(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var componentBase = compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ComponentBase);
        if (componentBase == null)
        {
            return false;
        }

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, componentBase))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool InheritsFromMvvmComponentBase(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var mvvmComponentBaseTypes = new[]
        {
            AnalyzerConstants.TypeNames.MvvmComponentBase,
            AnalyzerConstants.TypeNames.MvvmOwningComponentBase,
            AnalyzerConstants.TypeNames.MvvmLayoutComponentBase
        };

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            var originalDefinition = baseType.OriginalDefinition;
            if (mvvmComponentBaseTypes.Contains(originalDefinition.ToDisplayString()))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool InheritsFromViewModelBase(INamedTypeSymbol? typeSymbol, Compilation compilation)
    {
        if (typeSymbol == null)
        {
            return false;
        }

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
