using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that suggests using [Inject] for services instead of [CascadingParameter] in MVVM components.
/// Analyzes components that inherit from MvvmComponentBase/MvvmOwningComponentBase/MvvmLayoutComponentBase.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CascadingParameterVsInjectAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.InjectPreferredOverCascading);

    public override void Initialize(AnalysisContext context)
    {
        // Enable analysis of generated code (Razor components compile to generated C# code)
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        
        // Analyze generated component types directly - runs AFTER Razor source generation
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze Blazor components
        if (!InheritsFromComponentBase(typeSymbol, context.Compilation))
        {
            return;
        }

        // Check all properties in the MVVM component
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            // Check if property has CascadingParameter attribute
            var hasCascadingParameter = propertySymbol.GetAttributes().Any(attr =>
            {
                var attrName = attr.AttributeClass?.Name;
                var attrNamespace = attr.AttributeClass?.ContainingNamespace?.ToDisplayString();

                return (attrName == "CascadingParameter" || attrName == "CascadingParameterAttribute") &&
                       (attrNamespace == "Microsoft.AspNetCore.Components" || attrNamespace == null);
            });

            if (!hasCascadingParameter)
            {
                continue;
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
                                  (propertyType.Name.StartsWith("I", StringComparison.Ordinal) && 
                                   propertyType.Name.Length > 1 && 
                                   char.IsUpper(propertyType.Name[1]));

            if (isLikelyService)
            {
                var diagnosticLocation = propertySymbol.DeclaringSyntaxReferences
                    .Select(reference => reference.GetSyntax(context.CancellationToken))
                    .OfType<PropertyDeclarationSyntax>()
                    .Select(property => property.Type.GetLocation())
                    .FirstOrDefault() ?? propertySymbol.Locations.FirstOrDefault() ?? Location.None;

                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.InjectPreferredOverCascading,
                    diagnosticLocation,
                    propertySymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool InheritsFromComponentBase(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var componentBase = compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ComponentBase);
        if (componentBase == null)
        {
            return false;
        }

        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, componentBase))
            {
                return true;
            }

            current = current.BaseType;
        }
        return false;
    }
}
