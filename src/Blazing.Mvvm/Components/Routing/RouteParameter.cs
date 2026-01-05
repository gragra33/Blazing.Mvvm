namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Represents metadata for a route parameter extracted from a route template pattern.
/// </summary>
/// <remarks>
/// Route parameters are defined within curly braces in route templates and can have various modifiers:
/// <list type="bullet">
/// <item><description><c>{name}</c> - Required parameter</description></item>
/// <item><description><c>{name?}</c> - Optional parameter</description></item>
/// <item><description><c>{*name}</c> - Catch-all parameter</description></item>
/// <item><description><c>{name:constraint}</c> - Constrained parameter (e.g., int, guid, regex)</description></item>
/// </list>
/// </remarks>
public sealed class RouteParameter
{
    /// <summary>
    /// Gets the name of the route parameter.
    /// </summary>
    /// <example>
    /// For the route template <c>/users/{userId}/posts/{postId}</c>, the names would be "userId" and "postId".
    /// </example>
    public required string Name { get; init; }

    /// <summary>
    /// Gets a value indicating whether this parameter is optional.
    /// </summary>
    /// <remarks>
    /// Optional parameters are denoted by a question mark: <c>{name?}</c>
    /// </remarks>
    public bool IsOptional { get; init; }

    /// <summary>
    /// Gets a value indicating whether this parameter is a catch-all parameter.
    /// </summary>
    /// <remarks>
    /// Catch-all parameters are denoted by an asterisk: <c>{*name}</c>
    /// They capture the remainder of the URL path.
    /// </remarks>
    public bool IsCatchAll { get; init; }

    /// <summary>
    /// Gets the constraint applied to this parameter, if any.
    /// </summary>
    /// <remarks>
    /// Constraints specify the data type or format expected for the parameter.
    /// Examples: "int", "guid", "regex(^[A-Z]{3}-\\d{4}$)"
    /// </remarks>
    /// <example>
    /// For <c>{id:int}</c>, the Constraint value would be "int".
    /// For <c>{sku:regex(^[A-Z]{3}-\\d{4}$)}</c>, the Constraint value would be "regex(^[A-Z]{3}-\\d{4}$)".
    /// </example>
    public string? Constraint { get; init; }
}
