using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures properties marked with [ViewParameter] in ViewModels have corresponding [Parameter] properties in Views.
/// UPDATED: Using RegisterSymbolAction pattern like other working analyzers.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewParameterAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewParameterMismatch);

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
        var componentType = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes that inherit from MvvmComponentBase<TViewModel>
        if (!InheritsFromMvvmComponentBase(componentType))
        {
            return;
        }

        // Get the ViewModel type from the component's base class
        var viewModelType = GetViewModelTypeParameter(componentType);
        if (viewModelType == null)
        {
            return;
        }

        // Get properties with [ViewParameter] from ViewModel
        var viewParameterProperties = GetViewParameterProperties(viewModelType);
        if (viewParameterProperties.Count == 0)
        {
            return;
        }

        // Get properties with [Parameter] from the View (generated component)
        var viewParameterNames = GetParameterPropertyNames(componentType);

        // Check each [ViewParameter] property in ViewModel
        foreach (var viewParamProp in viewParameterProperties)
        {
            // If View doesn't have matching [Parameter] property, report diagnostic
            if (!viewParameterNames.Contains(viewParamProp.Name, StringComparer.OrdinalIgnoreCase))
            {
                var location = viewParamProp.Locations.FirstOrDefault() ?? componentType.Locations.FirstOrDefault() ?? Location.None;

                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.ViewParameterMismatch,
                    location,
                    viewParamProp.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool InheritsFromMvvmComponentBase(INamedTypeSymbol componentType)
    {
        // Walk up the inheritance chain to find MvvmComponentBase<TViewModel> or MvvmOwningComponentBase<TViewModel>
        var current = componentType.BaseType;
        while (current != null)
        {
            var originalDefinition = current.OriginalDefinition;
            var displayString = originalDefinition.ToDisplayString();

            if (displayString.StartsWith("Blazing.Mvvm.Components.MvvmComponentBase<") ||
                displayString.StartsWith("Blazing.Mvvm.Components.MvvmOwningComponentBase<"))
            {
                return true;
            }

            current = current.BaseType;
        }
        return false;
    }

    private static INamedTypeSymbol? GetViewModelTypeParameter(INamedTypeSymbol componentType)
    {
        // Walk up the inheritance chain to find MvvmComponentBase<TViewModel>
        var current = componentType.BaseType;
        while (current != null)
        {
            if ((current.Name == "MvvmComponentBase" || current.Name == "MvvmOwningComponentBase") &&
                current.ContainingNamespace.ToString().StartsWith("Blazing.Mvvm") &&
                current.TypeArguments.Length == 1)
            {
                return current.TypeArguments[0] as INamedTypeSymbol;
            }

            current = current.BaseType;
        }
        return null;
    }

    private static List<IPropertySymbol> GetViewParameterProperties(INamedTypeSymbol viewModelType)
    {
        var properties = new List<IPropertySymbol>();

        foreach (var member in viewModelType.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Check for [ViewParameter] attribute
            var hasViewParameter = property.GetAttributes().Any(attr =>
            {
                var attrName = attr.AttributeClass?.Name;
                return attrName == AnalyzerConstants.AttributeNames.ViewParameter ||
                       attrName == "ViewParameterAttribute";
            });

            if (hasViewParameter)
            {
                properties.Add(property);
            }
        }

        return properties;
    }

    private static HashSet<string> GetParameterPropertyNames(INamedTypeSymbol componentType)
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in componentType.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Check for [Parameter] attribute from Microsoft.AspNetCore.Components
            var hasParameter = property.GetAttributes().Any(attr =>
            {
                var attrName = attr.AttributeClass?.Name;
                var attrNamespace = attr.AttributeClass?.ContainingNamespace?.ToDisplayString();

                return (attrName == "Parameter" || attrName == "ParameterAttribute") &&
                       (attrNamespace == "Microsoft.AspNetCore.Components" || attrNamespace == null);
            });

            if (hasParameter)
            {
                properties.Add(property.Name);
            }
        }

        return properties;
    }
}
