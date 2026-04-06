namespace Blazing.Common.Models;

/// <summary>
/// Represents a CSS property with optional reverse and forward values.
/// </summary>
public class CssProperty
{
    /// <summary>
    /// Gets or sets the name of the CSS property.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the reverse value of the CSS property.
    /// </summary>
    public string? Reverse { get; set; }
    
    /// <summary>
    /// Gets or sets the forward value of the CSS property.
    /// </summary>
    public string? Forward { get; set; }

}
