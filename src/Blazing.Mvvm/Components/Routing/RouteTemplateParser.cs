using System.Text.RegularExpressions;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Parses route template patterns into structured <see cref="RouteTemplate"/> objects.
/// </summary>
/// <remarks>
/// This parser extracts route parameters from template strings and identifies their modifiers (optional, catch-all, constraints).
/// Supports Blazor/ASP.NET Core route template syntax including:
/// <list type="bullet">
/// <item><description><c>{name}</c> - Required parameter</description></item>
/// <item><description><c>{name?}</c> - Optional parameter</description></item>
/// <item><description><c>{*name}</c> - Catch-all parameter</description></item>
/// <item><description><c>{name:constraint}</c> - Constrained parameter</description></item>
/// </list>
/// </remarks>
public sealed class RouteTemplateParser
{
    // Regex pattern explanation:
    // \{           - Opening brace (literal)
    // (\*)?        - Optional catch-all marker (group 1)
    // (\w+)        - Parameter name (group 2) - one or more word characters
    // (\?)?        - Optional marker (group 3)
    // (?::([^}]+))?  - Optional constraint (group 4) - colon followed by anything except closing brace
    // \}           - Closing brace (literal)
    private static readonly Regex ParameterPatternRegexInstance = 
        new(@"\{(\*)?(\w+)(\?)?(?::([^}]+))?\}", RegexOptions.Compiled);
    
    private static Regex ParameterPatternRegex() => ParameterPatternRegexInstance;

    /// <summary>
    /// Parses a route template pattern string into a <see cref="RouteTemplate"/> object.
    /// </summary>
    /// <param name="pattern">The route template pattern to parse (e.g., "/users/{userId}/posts/{postId}").</param>
    /// <returns>A <see cref="RouteTemplate"/> object containing the pattern and extracted parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is null.</exception>
    /// <remarks>
    /// This method extracts all route parameters from the pattern and returns a structured representation.
    /// If no parameters are found, the returned <see cref="RouteTemplate"/> will have an empty <see cref="RouteTemplate.Parameters"/> list.
    /// </remarks>
    /// <example>
    /// <code>
    /// var parser = new RouteTemplateParser();
    /// 
    /// // Parse a simple route
    /// var template1 = parser.Parse("/users");
    /// // Result: Pattern="/users", Parameters=[]
    /// 
    /// // Parse a route with required parameter
    /// var template2 = parser.Parse("/users/{userId}");
    /// // Result: Pattern="/users/{userId}", Parameters=[{Name="userId", IsOptional=false, IsCatchAll=false}]
    /// 
    /// // Parse a route with optional parameter
    /// var template3 = parser.Parse("/users/{userId?}");
    /// // Result: Pattern="/users/{userId?}", Parameters=[{Name="userId", IsOptional=true, IsCatchAll=false}]
    /// 
    /// // Parse a route with catch-all parameter
    /// var template4 = parser.Parse("/docs/{*path}");
    /// // Result: Pattern="/docs/{*path}", Parameters=[{Name="path", IsOptional=false, IsCatchAll=true}]
    /// 
    /// // Parse a route with constraint
    /// var template5 = parser.Parse("/orders/{orderId:int}");
    /// // Result: Pattern="/orders/{orderId:int}", Parameters=[{Name="orderId", Constraint="int"}]
    /// </code>
    /// </example>
    public RouteTemplate Parse(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        var regex = ParameterPatternRegex();
        var matches = regex.Matches(pattern);

        var parameters = new List<RouteParameter>(matches.Count);

        foreach (Match match in matches)
        {
            var parameter = new RouteParameter
            {
                Name = match.Groups[2].Value,
                IsCatchAll = match.Groups[1].Success,
                IsOptional = match.Groups[3].Success,
                Constraint = match.Groups[4].Success ? match.Groups[4].Value : null
            };

            parameters.Add(parameter);
        }

        return new RouteTemplate
        {
            Pattern = pattern,
            Parameters = parameters
        };
    }

    /// <summary>
    /// Parses multiple route template patterns into a collection of <see cref="RouteTemplate"/> objects.
    /// </summary>
    /// <param name="patterns">The route template patterns to parse.</param>
    /// <returns>A list of <see cref="RouteTemplate"/> objects, one for each input pattern.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="patterns"/> is null.</exception>
    /// <remarks>
    /// This is a convenience method for parsing multiple route patterns in a single call.
    /// Each pattern is parsed independently using the <see cref="Parse(string)"/> method.
    /// </remarks>
    /// <example>
    /// <code>
    /// var parser = new RouteTemplateParser();
    /// var patterns = new[] { "/test", "/test/{echo}", "/test/{echo?}" };
    /// var templates = parser.ParseMany(patterns);
    /// // Result: List of 3 RouteTemplate objects
    /// </code>
    /// </example>
    public List<RouteTemplate> ParseMany(IEnumerable<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        return patterns.Select(Parse).ToList();
    }
}
