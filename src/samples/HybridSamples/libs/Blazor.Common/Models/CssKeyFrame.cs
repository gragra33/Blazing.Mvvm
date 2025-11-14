namespace Blazing.Common.Models;

/// <summary>
/// Represents a CSS keyframe, including selector and inherited CSS property values.
/// </summary>
public class CssKeyFrame : CssProperty
{
    /// <summary>
    /// Gets or sets the selector for the CSS keyframe.
    /// </summary>
    public string? Selector { get; set; }
}
