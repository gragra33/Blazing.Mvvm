using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures @page route parameters have corresponding [Parameter] or [ViewParameter] properties.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RouteParameterBindingAnalyzer : DiagnosticAnalyzer
{
    // Regex pattern to match route parameters like {id}, {id:int}, {name:string}
    private static readonly Regex RouteParameterPattern = new(@"\{(\w+)(?::(\w+))?\}", RegexOptions.Compiled);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RouteParameterBindingMissing);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for additional file analysis to process .razor files
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        // Get all additional files (includes .razor files)
        foreach (var additionalFile in context.Options.AdditionalFiles)
        {
            if (!additionalFile.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                continue;

            var text = additionalFile.GetText(context.CancellationToken);
            if (text == null)
                continue;

            var content = text.ToString();

            // Find @page directive and extract route parameters
            var routeParameters = ExtractRouteParameters(content);
            if (routeParameters.Count == 0)
                continue;

            // Get the component type from the compilation
            var componentType = GetComponentTypeFromRazorFile(additionalFile.Path, context.Compilation);
            if (componentType == null)
                continue;

            // Check if the component has corresponding Parameter properties
            var parameterProperties = GetParameterProperties(componentType);

            // Also check ViewModel for ViewParameter properties
            var viewModelType = GetViewModelTypeParameter(componentType);
            var viewParameterProperties = viewModelType != null
                ? GetViewParameterProperties(viewModelType)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Check each route parameter
            foreach (var routeParam in routeParameters)
            {
                // Check if parameter exists in View or ViewModel
                if (!parameterProperties.Contains(routeParam, StringComparer.OrdinalIgnoreCase) &&
                    !viewParameterProperties.Contains(routeParam, StringComparer.OrdinalIgnoreCase))
                {
                    // Report diagnostic at the component location
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.RouteParameterBindingMissing,
                        componentType.Locations.FirstOrDefault() ?? Location.None,
                        routeParam);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static List<string> ExtractRouteParameters(string razorContent)
    {
        var parameters = new List<string>();
        var lines = razorContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (!trimmedLine.StartsWith("@page", StringComparison.OrdinalIgnoreCase))
                continue;

            // Extract route parameters from the @page directive
            var matches = RouteParameterPattern.Matches(trimmedLine);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    parameters.Add(match.Groups[1].Value);
                }
            }
        }

        return parameters;
    }

    private static INamedTypeSymbol? GetComponentTypeFromRazorFile(string razorFilePath, Compilation compilation)
    {
        // Extract component name from file path
        var fileName = System.IO.Path.GetFileNameWithoutExtension(razorFilePath);

        // Search for a type with this name in the compilation
        var types = compilation.GetSymbolsWithName(fileName, SymbolFilter.Type);
        
        return types.OfType<INamedTypeSymbol>().FirstOrDefault();
    }

    private static HashSet<string> GetParameterProperties(INamedTypeSymbol componentType)
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in componentType.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Check for [Parameter] attribute
            foreach (var attribute in property.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == "Parameter" ||
                    attribute.AttributeClass?.Name == "ParameterAttribute")
                {
                    properties.Add(property.Name);
                    break;
                }
            }
        }

        return properties;
    }

    private static HashSet<string> GetViewParameterProperties(INamedTypeSymbol viewModelType)
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in viewModelType.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Check for [ViewParameter] attribute
            foreach (var attribute in property.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == "ViewParameter" ||
                    attribute.AttributeClass?.Name == "ViewParameterAttribute")
                {
                    properties.Add(property.Name);
                    break;
                }
            }
        }

        return properties;
    }

    private static INamedTypeSymbol? GetViewModelTypeParameter(INamedTypeSymbol type)
    {
        var current = type.BaseType;
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
}
