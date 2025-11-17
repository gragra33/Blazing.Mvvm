using System.Text;
using Blazing.Common.Models;

namespace Blazing.Common;

/// <summary>
/// Provides a builder for creating CSS keyframe animations programmatically.
/// </summary>
public class KeyframeBuilder
{
    #region Fields
    
    /// <summary>
    /// Stores the frame buffer for keyframe definitions.
    /// </summary>
    private string? _frameBuffer;
    /// <summary>
    /// The animation ID for the keyframes.
    /// </summary>
    private string _animationId;
    /// <summary>
    /// The direction of the keyframe animation.
    /// </summary>
    private KeyframeBuilderDirection _direction;                            // defaults to forward
    /// <summary>
    /// The template for the entire keyframes block.
    /// </summary>
    private static readonly string template = "@keyframes {0} {{ {1}}}";    // 0 = animationId, 1 = keyframes
    /// <summary>
    /// The template for an individual frame.
    /// </summary>
    private static readonly string frameTemplate = "{0} {{ {1}}} ";         // 0 = frame name, property
    /// <summary>
    /// The template for a CSS property value.
    /// </summary>
    private static readonly string valueTemplate = "{0}: {1}; ";            // 0 = property name, value

    #endregion

    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyframeBuilder"/> class with the specified animation ID and direction.
    /// </summary>
    /// <param name="animationId">The animation ID.</param>
    /// <param name="direction">The direction of the keyframe animation.</param>
    private KeyframeBuilder(string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        _direction = direction;
    }

    /// <summary>
    /// Creates a new <see cref="KeyframeBuilder"/> instance with the specified direction.
    /// </summary>
    /// <param name="direction">The direction of the keyframe animation.</param>
    /// <returns>A new <see cref="KeyframeBuilder"/> instance.</returns>
    public static KeyframeBuilder Factory(KeyframeBuilderDirection direction)
        => new(string.Empty, direction);

    /// <summary>
    /// Creates a new <see cref="KeyframeBuilder"/> instance with the specified animation ID and direction.
    /// </summary>
    /// <param name="animationId">The animation ID.</param>
    /// <param name="direction">The direction of the keyframe animation.</param>
    /// <returns>A new <see cref="KeyframeBuilder"/> instance.</returns>
    public static KeyframeBuilder Factory(string animationId, KeyframeBuilderDirection direction)
        => new(animationId, direction);

    #endregion

    #region Methods
    
    /// <summary>
    /// Sets the animation ID for the keyframes.
    /// </summary>
    /// <param name="value">The animation ID value.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder AnimationId(string value)
    {
        _animationId = value;
        return this;
    }

    #region CssKeyFrame
    
    /// <summary>
    /// Adds a collection of <see cref="CssKeyFrame"/> objects to the keyframe builder.
    /// </summary>
    /// <param name="frames">The collection of keyframes to add.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssKeyFrame> frames)
    {
        bool isReverse = _direction == KeyframeBuilderDirection.Reverse;

        foreach (IGrouping<string, CssKeyFrame> kvp in frames.GroupBy(keyframe => keyframe.Selector)!)
        {
            IReadOnlyList<string> parts = kvp
                .Where(keyframe => !string.IsNullOrEmpty(isReverse ? keyframe.Reverse : keyframe.Forward))
                .Select(keyframe => $"{keyframe.Name}: {(isReverse ? keyframe.Reverse : keyframe.Forward)};")
                .ToList();

            if (parts.Any())
            {
                Add(kvp.Key, string.Join(" ", parts));
            }
        }

        return this;
    }

    /// <summary>
    /// Adds a collection of <see cref="CssKeyFrame"/> objects to the keyframe builder and sets the animation ID.
    /// </summary>
    /// <param name="frames">The collection of keyframes to add.</param>
    /// <param name="animationId">The animation ID to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssKeyFrame> frames, string animationId)
    {
        _animationId = animationId;
        return Add(frames);
    }

    /// <summary>
    /// Adds a collection of <see cref="CssKeyFrame"/> objects to the keyframe builder and sets the direction.
    /// </summary>
    /// <param name="frames">The collection of keyframes to add.</param>
    /// <param name="direction">The direction to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssKeyFrame> frames, KeyframeBuilderDirection direction)
    {
        _direction = direction;
        return Add(frames);
    }

    /// <summary>
    /// Adds a collection of <see cref="CssKeyFrame"/> objects to the keyframe builder, sets the animation ID and direction.
    /// </summary>
    /// <param name="frames">The collection of keyframes to add.</param>
    /// <param name="animationId">The animation ID to set.</param>
    /// <param name="direction">The direction to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssKeyFrame> frames, string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        return Add(frames, direction);
    }

    #endregion

    #region CssProperty
    
    /// <summary>
    /// Adds a <see cref="CssProperty"/> to the keyframe builder.
    /// </summary>
    /// <param name="property">The CSS property to add.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(CssProperty property)
    {
        Add("from", string.Format(valueTemplate, property.Name, _direction == KeyframeBuilderDirection.Reverse ? property.Forward : property.Reverse));
        Add("to", string.Format(valueTemplate, property.Name, _direction == KeyframeBuilderDirection.Reverse ? property.Reverse : property.Forward));
        return this;
    }

    /// <summary>
    /// Adds a <see cref="CssProperty"/> to the keyframe builder and sets the animation ID.
    /// </summary>
    /// <param name="property">The CSS property to add.</param>
    /// <param name="animationId">The animation ID to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(CssProperty property, string animationId)
    {
        _animationId = animationId;
        return Add(property);
    }

    /// <summary>
    /// Adds a <see cref="CssProperty"/> to the keyframe builder and sets the direction.
    /// </summary>
    /// <param name="property">The CSS property to add.</param>
    /// <param name="direction">The direction to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(CssProperty property, KeyframeBuilderDirection direction)
    {
        _direction = direction;
        return Add(property);
    }

    /// <summary>
    /// Adds a <see cref="CssProperty"/> to the keyframe builder, sets the animation ID and direction.
    /// </summary>
    /// <param name="property">The CSS property to add.</param>
    /// <param name="animationId">The animation ID to set.</param>
    /// <param name="direction">The direction to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(CssProperty property, string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        _direction = direction;
        return Add(property);
    }

    /// <summary>
    /// Adds a collection of <see cref="CssProperty"/> objects to the keyframe builder and sets the animation ID.
    /// </summary>
    /// <param name="properties">The collection of CSS properties to add.</param>
    /// <param name="animationId">The animation ID to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssProperty> properties, string animationId)
    {
        _animationId = animationId;
        Add(properties);
        return this;
    }

    /// <summary>
    /// Adds a collection of <see cref="CssProperty"/> objects to the keyframe builder and sets the direction.
    /// </summary>
    /// <param name="properties">The collection of CSS properties to add.</param>
    /// <param name="direction">The direction to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssProperty> properties, KeyframeBuilderDirection direction)
    {
        _direction = direction;
        Add(properties);
        return this;
    }

    /// <summary>
    /// Adds a collection of <see cref="CssProperty"/> objects to the keyframe builder, sets the animation ID and direction.
    /// </summary>
    /// <param name="properties">The collection of CSS properties to add.</param>
    /// <param name="animationId">The animation ID to set.</param>
    /// <param name="direction">The direction to set.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssProperty> properties, string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        _direction = direction;
        Add(properties);
        return this;
    }

    /// <summary>
    /// Adds a collection of <see cref="CssProperty"/> objects to the keyframe builder.
    /// </summary>
    /// <param name="properties">The collection of CSS properties to add.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(IList<CssProperty> properties)
    {
        bool isReverse = _direction == KeyframeBuilderDirection.Reverse;

        StringBuilder fromSb = new();
        StringBuilder toSb = new();

        foreach (CssProperty property in properties)
        {
            fromSb.AppendFormat(valueTemplate, property.Name, isReverse ? property.Forward : property.Reverse);
            toSb.AppendFormat(valueTemplate, property.Name, isReverse ? property.Reverse : property.Forward);
        }

        Add("from", fromSb.ToString());
        Add("to", toSb.ToString());

        return this;
    } 

    #endregion

    /// <summary>
    /// Adds a frame with the specified name and value to the keyframe builder.
    /// </summary>
    /// <param name="frameName">The name of the frame.</param>
    /// <param name="frameValue">The value of the frame.</param>
    /// <returns>The current <see cref="KeyframeBuilder"/> instance.</returns>
    public KeyframeBuilder Add(string frameName, string frameValue)
    {
        _frameBuffer += string.Format(frameTemplate, frameName, frameValue);
        return this;
    }

    /// <summary>
    /// Validates whether the keyframe builder has a valid animation ID and frame buffer.
    /// </summary>
    /// <returns>True if valid; otherwise, false.</returns>
    private bool Validate()
        => !string.IsNullOrEmpty(_animationId) && !string.IsNullOrEmpty(_frameBuffer);

    /// <summary>
    /// Builds the CSS keyframes string from the current buffer and animation ID.
    /// </summary>
    /// <returns>The CSS keyframes string if valid; otherwise, an empty string.</returns>
    public string Build()
        => Validate()
            ? string.Format(template, _animationId, _frameBuffer)
            : string.Empty;

    /// <summary>
    /// Returns the built CSS keyframes string.
    /// </summary>
    /// <returns>The CSS keyframes string.</returns>
    public override string ToString() => Build(); 

    #endregion
}
