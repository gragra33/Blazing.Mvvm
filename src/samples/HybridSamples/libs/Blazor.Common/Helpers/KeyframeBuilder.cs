using System.Text;
using Blazing.Common.Models;

namespace Blazing.Common;

public class KeyframeBuilder
{
    #region Fields
    
    private string? _frameBuffer;
    private string _animationId;

    private KeyframeBuilderDirection _direction;                            // defaults to forward

    private static readonly string template = "@keyframes {0} {{ {1}}}";    // 0 = animationId, 1 = keyframes
    private static readonly string frameTemplate = "{0} {{ {1}}} ";         // 0 = frame name, property
    private static readonly string valueTemplate = "{0}: {1}; ";            // 0 = property name, value

    #endregion

    #region Constructors
    
    private KeyframeBuilder(string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        _direction = direction;
    }

    public static KeyframeBuilder Factory(KeyframeBuilderDirection direction)
        => new(string.Empty, direction);

    public static KeyframeBuilder Factory(string animationId, KeyframeBuilderDirection direction)
        => new(animationId, direction);

    #endregion

    #region Methods
    
    public KeyframeBuilder AnimationId(string value)
    {
        _animationId = value;
        return this;
    }

    #region CssKeyFrame
    
    public KeyframeBuilder Add(IList<CssKeyFrame> frames)
    {
        bool isReverse = _direction == KeyframeBuilderDirection.Reverse;

        foreach (IGrouping<string, CssKeyFrame> kvp in frames.GroupBy(keyframe => keyframe.Selector))
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

    public KeyframeBuilder Add(IList<CssKeyFrame> frames, string animationId)
    {
        _animationId = animationId;
        return Add(frames);
    }

    public KeyframeBuilder Add(IList<CssKeyFrame> frames, KeyframeBuilderDirection direction)
    {
        _direction = direction;
        return Add(frames);
    }

    public KeyframeBuilder Add(IList<CssKeyFrame> frames, string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        return Add(frames, direction);
    }

    #endregion

    #region CssProperty
    
    public KeyframeBuilder Add(CssProperty property)
    {
        Add("from", string.Format(valueTemplate, property.Name, _direction == KeyframeBuilderDirection.Reverse ? property.Forward : property.Reverse));
        Add("to", string.Format(valueTemplate, property.Name, _direction == KeyframeBuilderDirection.Reverse ? property.Reverse : property.Forward));
        return this;
    }

    public KeyframeBuilder Add(CssProperty property, string animationId)
    {
        _animationId = animationId;
        return Add(property);
    }

    public KeyframeBuilder Add(CssProperty property, KeyframeBuilderDirection direction)
    {
        _direction = direction;
        return Add(property);
    }

    public KeyframeBuilder Add(CssProperty property, string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        _direction = direction;
        return Add(property);
    }

    public KeyframeBuilder Add(IList<CssProperty> properties, string animationId)
    {
        _animationId = animationId;
        Add(properties);
        return this;
    }

    public KeyframeBuilder Add(IList<CssProperty> properties, KeyframeBuilderDirection direction)
    {
        _direction = direction;
        Add(properties);
        return this;
    }

    public KeyframeBuilder Add(IList<CssProperty> properties, string animationId, KeyframeBuilderDirection direction)
    {
        _animationId = animationId;
        _direction = direction;
        Add(properties);
        return this;
    }

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

    public KeyframeBuilder Add(string frameName, string frameValue)
    {
        _frameBuffer += string.Format(frameTemplate, frameName, frameValue);
        return this;
    }

    private bool Validate()
        => !string.IsNullOrEmpty(_animationId) && !string.IsNullOrEmpty(_frameBuffer);

    // String buffer finalization code
    public string Build()
        => Validate()
            ? string.Format(template, _animationId, _frameBuffer)
            : string.Empty;

    // ToString should only and always call Build to finalize the rendered string.
    public override string ToString() => Build(); 

    #endregion
}
