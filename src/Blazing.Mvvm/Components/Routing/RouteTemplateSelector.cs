using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Selects the most appropriate route template from a collection based on provided navigation parameters.
/// </summary>
/// <remarks>
/// This class implements the template selection algorithm that determines which route pattern best matches
/// the navigation intent when multiple <c>@page</c> directives are defined on a component.
/// <para>
/// Selection priority:
/// <list type="number">
/// <item><description>No parameters provided ? primary route (simplest)</description></item>
/// <item><description>Query string only ? primary route</description></item>
/// <item><description>Path segments ? match by parameter count (exact match > optional parameters > catch-all)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed partial class RouteTemplateSelector
{
    private readonly ILogger<RouteTemplateSelector> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteTemplateSelector"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    public RouteTemplateSelector(ILogger<RouteTemplateSelector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Selects the best-matching route template from a collection based on the provided parameters.
    /// </summary>
    /// <param name="templates">The collection of available route templates for the target ViewModel.</param>
    /// <param name="parameters">
    /// Optional navigation parameters that may include:
    /// <list type="bullet">
    /// <item><description>Query string: <c>?key=value</c></description></item>
    /// <item><description>Path segments: <c>value1/value2</c></description></item>
    /// <item><description>Combined: <c>value1/value2?key=value</c></description></item>
    /// </list>
    /// </param>
    /// <returns>The selected <see cref="RouteTemplate"/> that best matches the navigation intent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="templates"/> is null.</exception>
    /// <remarks>
    /// <para><b>Selection Algorithm:</b></para>
    /// <para>
    /// 1. <b>No parameters or query string only:</b> Returns the primary route.
    /// </para>
    /// <para>
    /// 2. <b>Path segments provided:</b> Analyzes the number of path segments and selects a template where:
    ///    <list type="bullet">
    ///    <item><description>The template can accommodate the provided segment count</description></item>
    ///    <item><description>Prefers exact match over optional parameters</description></item>
    ///    <item><description>Prefers required parameters over optional</description></item>
    ///    <item><description>Falls back to catch-all if available</description></item>
    ///    </list>
    /// </para>
    /// <para>
    /// 3. <b>No suitable match:</b> Returns the primary route as a fallback.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var selector = new RouteTemplateSelector(logger);
    /// 
    /// var collection = new RouteTemplateCollection
    /// {
    ///     PrimaryRoute = "/test",
    ///     AllRoutes = new List&lt;RouteTemplate&gt;
    ///     {
    ///         new() { Pattern = "/test", Parameters = new() },
    ///         new() { Pattern = "/test/{echo}", Parameters = new() { new() { Name = "echo" } } }
    ///     }
    /// };
    /// 
    /// // No parameters ? selects "/test"
    /// var template1 = selector.SelectBestTemplate(collection, null);
    /// 
    /// // Query string only ? selects "/test"
    /// var template2 = selector.SelectBestTemplate(collection, "?key=value");
    /// 
    /// // Path segment ? selects "/test/{echo}"
    /// var template3 = selector.SelectBestTemplate(collection, "value123");
    /// </code>
    /// </example>
    public RouteTemplate SelectBestTemplate(RouteTemplateCollection templates, string? parameters)
    {
        ArgumentNullException.ThrowIfNull(templates);

        // Case 1: No parameters provided ? use primary route
        if (string.IsNullOrWhiteSpace(parameters))
        {
            LogUsingPrimaryRoute(templates.PrimaryRoute, "no parameters provided");
            return templates.Primary;
        }

        // Case 2: Query string only (starts with ?) ? use primary route
        if (parameters.StartsWith('?'))
        {
            LogUsingPrimaryRoute(templates.PrimaryRoute, "query string only");
            return templates.Primary;
        }

        // Case 3: Path segments provided (with or without query string)
        // Split on ? to separate path from query string
        string pathPart = parameters.Contains('?') 
            ? parameters.Split('?')[0] 
            : parameters;

        // Count path segments
        string[] pathSegments = pathPart.Split('/', StringSplitOptions.RemoveEmptyEntries);
        int providedSegmentCount = pathSegments.Length;

        LogAnalyzingPathSegments(providedSegmentCount, pathPart);

        // Find templates that can accommodate the provided segment count
        var candidates = templates.AllRoutes
            .Where(t => CanAccommodateSegments(t, providedSegmentCount))
            .OrderByDescending(t => t.ParameterCount)     // Prefer more specific (more parameters)
            .ThenBy(t => t.Parameters.Count(p => p.IsOptional))  // Prefer required over optional
            .ThenByDescending(t => t.HasCatchAll)          // Catch-all as last resort
            .ToList();

        if (candidates.Count > 0)
        {
            var selected = candidates.First();
            LogTemplateSelected(selected.Pattern, $"{selected.ParameterCount} parameters, {selected.RequiredParameterCount} required");
            return selected;
        }

        // Fallback: No suitable match ? use primary route
        LogUsingPrimaryRoute(templates.PrimaryRoute, "no suitable template found for path segments");
        return templates.Primary;
    }

    /// <summary>
    /// Determines whether a route template can accommodate the specified number of path segments.
    /// </summary>
    /// <param name="template">The route template to evaluate.</param>
    /// <param name="segmentCount">The number of path segments in the navigation parameter.</param>
    /// <returns>
    /// <c>true</c> if the template can handle the segment count; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// A template can accommodate segments if:
    /// <list type="bullet">
    /// <item><description>It has a catch-all parameter (can accept any number of segments)</description></item>
    /// <item><description>The segment count is between the required parameter count and total parameter count</description></item>
    /// </list>
    /// </remarks>
    private static bool CanAccommodateSegments(RouteTemplate template, int segmentCount)
    {
        // Catch-all templates can accept any number of segments
        if (template.HasCatchAll)
            return true;

        // Template must have enough total parameters to accommodate all segments
        if (segmentCount > template.ParameterCount)
            return false;

        // Template must have few enough required parameters
        if (segmentCount < template.RequiredParameterCount)
            return false;

        return true;
    }

    #region Logging

    [LoggerMessage(LogLevel.Debug, "Using primary route '{PrimaryRoute}' because {Reason}")]
    private partial void LogUsingPrimaryRoute(string primaryRoute, string reason);

    [LoggerMessage(LogLevel.Debug, "Analyzing path segments: {SegmentCount} segments in '{PathPart}'")]
    private partial void LogAnalyzingPathSegments(int segmentCount, string pathPart);

    [LoggerMessage(LogLevel.Debug, "Selected template '{Pattern}' ({Details})")]
    private partial void LogTemplateSelected(string pattern, string details);

    #endregion
}
