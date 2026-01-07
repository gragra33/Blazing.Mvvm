using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Blazing.Mvvm.Analyzers.Helpers;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures @page route parameters have corresponding [Parameter] or [ViewParameter] properties.
/// Supports both simple route parameters and multi-parameter routes with slash-separated values.
/// Now supports multi-project architectures where Views and ViewModels are in separate assemblies.
/// </summary>
/// <remarks>
/// This analyzer validates two routing patterns:
/// <list type="bullet">
/// <item><description>Simple routes: /users/{userId} ? requires [ViewParameter] public string? UserId</description></item>
/// <item><description>Multi-parameter routes: /users/{userId}/posts/{postId} ? requires both UserId and PostId properties</description></item>
/// </list>
/// The navigation parameter substitution (e.g., NavigateTo("1/101")) is handled by MvvmNavigationManager,
/// but this analyzer ensures the ViewModel has the required [ViewParameter] properties.
/// 
/// <para>
/// <strong>Cross-Project Support:</strong> This analyzer can discover ViewModels from referenced assemblies,
/// enabling it to work correctly when Views (in Blazor project) reference ViewModels (in Core/Shared project).
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RouteParameterBindingAnalyzer : DiagnosticAnalyzer
{
    // Regex pattern to match route parameters like {id}, {id:int}, {id?}, {*path}
    // Supports: required, optional (?), catch-all (*), and constrained (:type) parameters
    private static readonly Regex RouteParameterPattern = new(@"\{(\*)?(\w+)(\?)?(?::(\w+))?\}", RegexOptions.Compiled);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RouteParameterBindingMissing);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register compilation-end analysis to access ViewModels from referenced assemblies
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        // Analyze .razor files for route parameter bindings
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

            // Get ViewModel type - try from component base type first
            var viewModelType = CrossProjectAnalyzerHelper.GetViewModelFromComponent(componentType);
            
            // If not found, try extracting from @inherits directive and search across projects
            if (viewModelType == null)
            {
                var viewModelTypeName = ExtractViewModelTypeName(content);
                if (!string.IsNullOrWhiteSpace(viewModelTypeName))
                {
                    viewModelType = CrossProjectAnalyzerHelper.FindViewModel(context.Compilation, viewModelTypeName!);
                }
            }

            // Get [ViewParameter] properties from ViewModel (works across projects)
            var viewParameterProperties = viewModelType != null
                ? new HashSet<string>(CrossProjectAnalyzerHelper.GetViewParameters(viewModelType).Keys, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Check each route parameter
            foreach (var routeParam in routeParameters)
            {
                // Check if parameter exists in View or ViewModel
                if (!parameterProperties.Contains(routeParam, StringComparer.OrdinalIgnoreCase) &&
                    !viewParameterProperties.Contains(routeParam, StringComparer.OrdinalIgnoreCase))
                {
                    // Determine the best type name to report in the diagnostic
                    var typeNameForDiagnostic = viewModelType?.Name ?? componentType.Name;
                    
                    // Report diagnostic at the component location
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.RouteParameterBindingMissing,
                        componentType.Locations.FirstOrDefault() ?? Location.None,
                        routeParam,
                        typeNameForDiagnostic);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    /// <summary>
    /// Extracts route parameters from @page directives in Razor content.
    /// Supports multiple @page directives and various parameter formats.
    /// Converts route parameter names to PascalCase to match C# property naming conventions.
    /// </summary>
    /// <param name="razorContent">The content of the Razor file.</param>
    /// <returns>A list of parameter names found in route templates, converted to PascalCase.</returns>
    /// <remarks>
    /// Examples of supported route patterns:
    /// <list type="bullet">
    /// <item><description>@page "/users/{userId}" ? ["UserId"] (converted to PascalCase)</description></item>
    /// <item><description>@page "/users/{userId}/posts/{postId}" ? ["UserId", "PostId"] (converted to PascalCase)</description></item>
    /// <item><description>@page "/items/{id:int}" ? ["Id"]</description></item>
    /// <item><description>@page "/docs/{*path}" ? ["Path"]</description></item>
    /// </list>
    /// Route parameters are converted to PascalCase because:
    /// - Blazor routes use camelCase: /users/{userId}
    /// - C# properties use PascalCase: public string? UserId { get; set; }
    /// - Blazor's routing engine matches them case-insensitively
    /// </remarks>
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
                if (match.Groups.Count > 2 && match.Groups[2].Success)
                {
                    var paramName = match.Groups[2].Value;
                    if (!string.IsNullOrWhiteSpace(paramName))
                    {
                        // Convert to PascalCase to match C# property naming conventions
                        // Example: "userId" -> "UserId", "postId" -> "PostId"
                        var pascalCaseName = ToPascalCase(paramName);
                        parameters.Add(pascalCaseName);
                    }
                }
            }
        }

        return parameters;
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    /// <param name="input">The input string (e.g., "userId", "user_id", "UserId").</param>
    /// <returns>The PascalCase string (e.g., "UserId").</returns>
    /// <remarks>
    /// Handles various input formats:
    /// - camelCase: "userId" -> "UserId"
    /// - snake_case: "user_id" -> "UserId"
    /// - PascalCase: "UserId" -> "UserId" (no change)
    /// - kebab-case: "user-id" -> "UserId"
    /// </remarks>
    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // If already starts with uppercase, likely already PascalCase
        if (char.IsUpper(input[0]))
            return input;

        // Handle camelCase (most common): "userId" -> "UserId"
        if (!input.Contains('_') && !input.Contains('-'))
        {
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }

        // Handle snake_case or kebab-case: "user_id" or "user-id" -> "UserId"
        var parts = input.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        var result = string.Concat(parts.Select(part =>
            char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));

        return result;
    }

    /// <summary>
    /// Extracts the ViewModel type name from @inherits directive in Razor content.
    /// </summary>
    /// <param name="razorContent">The content of the Razor file.</param>
    /// <returns>The ViewModel type name if found, otherwise null.</returns>
    private static string? ExtractViewModelTypeName(string razorContent)
    {
        var lines = razorContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (!trimmedLine.StartsWith("@inherits", StringComparison.OrdinalIgnoreCase))
                continue;

            // Look for MvvmComponentBase<TViewModel> or similar patterns
            var inheritsPattern = new Regex(
                @"@inherits\s+Mvvm(?:Component|Owning|Layout)ComponentBase\s*<\s*(\w+)\s*>",
                RegexOptions.IgnoreCase);
            
            var match = inheritsPattern.Match(trimmedLine);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the component type symbol from a Razor file path.
    /// Only returns types that are Blazor components (inherit from ComponentBase or MvvmComponentBase).
    /// </summary>
    private static INamedTypeSymbol? GetComponentTypeFromRazorFile(string razorFilePath, Compilation compilation)
    {
        var fileName = System.IO.Path.GetFileNameWithoutExtension(razorFilePath);
        
        // Get all types with this name
        var candidateTypes = compilation.GetSymbolsWithName(fileName, SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .ToList();

        // Filter to only Blazor components (must inherit from ComponentBase)
        foreach (var type in candidateTypes)
        {
            if (IsBlazorComponent(type))
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines if a type is a Blazor component (inherits from ComponentBase).
    /// </summary>
    private static bool IsBlazorComponent(INamedTypeSymbol type)
    {
        // Must be a class
        if (type.TypeKind != TypeKind.Class)
            return false;

        // Walk up the inheritance chain
        var current = type.BaseType;
        while (current != null)
        {
            var fullName = current.ToDisplayString();
            
            // Check for ComponentBase or any Mvvm*ComponentBase
            if (fullName == "Microsoft.AspNetCore.Components.ComponentBase" ||
                fullName.StartsWith("Blazing.Mvvm.Components.Mvvm") ||
                current.Name == "ComponentBase" ||
                current.Name == "MvvmComponentBase" ||
                current.Name == "MvvmOwningComponentBase" ||
                current.Name == "MvvmLayoutComponentBase")
            {
                return true;
            }
            
            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Gets all [Parameter] properties from a component type.
    /// </summary>
    private static HashSet<string> GetParameterProperties(INamedTypeSymbol componentType)
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in componentType.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            foreach (var attribute in property.GetAttributes())
            {
                var attrName = attribute.AttributeClass?.Name;
                if (attrName == "Parameter" || attrName == "ParameterAttribute")
                {
                    properties.Add(property.Name);
                    break;
                }
            }
        }

        return properties;
    }
}
