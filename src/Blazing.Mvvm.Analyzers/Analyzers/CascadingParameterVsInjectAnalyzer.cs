using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that suggests using [Inject] for services instead of [CascadingParameter].
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CascadingParameterVsInjectAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.InjectPreferredOverCascading);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;

        // Check if property has CascadingParameter attribute
        var cascadingAttribute = propertySymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.Name == "CascadingParameterAttribute" ||
            attr.AttributeClass?.Name == "CascadingParameter");

        if (cascadingAttribute == null)
        {
            return;
        }

        // Check if the property type is an interface or service-like type
        var propertyType = propertySymbol.Type;
        
        // Common service patterns
        var isLikelyService = propertyType.TypeKind == TypeKind.Interface ||
                              propertyType.Name.EndsWith("Service", StringComparison.Ordinal) ||
                              propertyType.Name.EndsWith("Manager", StringComparison.Ordinal) ||
                              propertyType.Name.EndsWith("Repository", StringComparison.Ordinal) ||
                              propertyType.Name.EndsWith("Provider", StringComparison.Ordinal) ||
                              propertyType.Name.EndsWith("Factory", StringComparison.Ordinal) ||
                              propertyType.Name.EndsWith("Client", StringComparison.Ordinal) ||
                              propertyType.Name.StartsWith("I", StringComparison.Ordinal) && 
                              propertyType.Name.Length > 1 && 
                              char.IsUpper(propertyType.Name[1]);

        if (isLikelyService)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.InjectPreferredOverCascading,
                propertySymbol.Locations[0],
                propertySymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
