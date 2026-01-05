namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Represents a parsed route template with its pattern and extracted parameter metadata.
/// </summary>
/// <remarks>
/// This class encapsulates a route pattern string along with all route parameters found within it.
/// It provides convenience properties for common queries about the route's structure.
/// </remarks>
public sealed class RouteTemplate
{
    /// <summary>
    /// Gets the original route pattern string.
    /// </summary>
    /// <example>
    /// Examples of route patterns:
    /// <list type="bullet">
    /// <item><description><c>/users</c> - Simple route with no parameters</description></item>
    /// <item><description><c>/users/{userId}</c> - Route with one required parameter</description></item>
    /// <item><description><c>/users/{userId?}</c> - Route with one optional parameter</description></item>
    /// <item><description><c>/users/{userId}/posts/{postId}</c> - Route with multiple parameters</description></item>
    /// <item><description><c>/docs/{*path}</c> - Route with catch-all parameter</description></item>
    /// </list>
    /// </example>
    public required string Pattern { get; init; }

    /// <summary>
    /// Gets the list of parameters extracted from the route pattern.
    /// </summary>
    /// <remarks>
    /// Parameters are listed in the order they appear in the route pattern.
    /// </remarks>
    public required List<RouteParameter> Parameters { get; init; }

    /// <summary>
    /// Gets the total number of parameters in this route template.
    /// </summary>
    public int ParameterCount => Parameters.Count;

    /// <summary>
    /// Gets the number of required (non-optional) parameters in this route template.
    /// </summary>
    /// <remarks>
    /// This count excludes parameters marked as optional with the <c>?</c> modifier.
    /// Used for template selection to prefer routes with matching required parameter counts.
    /// </remarks>
    public int RequiredParameterCount => Parameters.Count(p => !p.IsOptional);

    /// <summary>
    /// Gets a value indicating whether this route template contains a catch-all parameter.
    /// </summary>
    /// <remarks>
    /// Catch-all parameters (denoted with <c>*</c>) capture the remainder of the URL path.
    /// A route template can have at most one catch-all parameter, and it must be the last parameter.
    /// </remarks>
    public bool HasCatchAll => Parameters.Any(p => p.IsCatchAll);
}
