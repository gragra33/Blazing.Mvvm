// Copyright (c) 2011 - 2022 Ed Charbeneau
// License: MIT
// See https://github.com/EdCharbeneau/BlazorComponentUtilities

namespace Blazing.Common;

/// <summary>
/// Provides a builder for constructing conditional in-line style strings for Blazor components.
/// </summary>
public struct StyleBuilder
{
    #region Constructor

    /// <summary>
    /// Creates a <see cref="StyleBuilder"/> used to define conditional in-line style used in a component. Call <see cref="Build"/> to return the completed style as a string.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    public StyleBuilder(string prop, string value)
        => stringBuffer = $"{prop}:{value};";

    /// <summary>
    /// Creates a <see cref="StyleBuilder"/> used to define conditional in-line style used in a component. Call <see cref="Build"/> to return the completed style as a string.
    /// </summary>
    /// <param name="style">The initial style string.</param>
    public StyleBuilder(string style)
        => stringBuffer = style;

    #endregion
    
    #region Fields

    /// <summary>
    /// The internal string buffer used to build the style value.
    /// </summary>
    private string? stringBuffer;

    #endregion

    #region Methods

    #region Factory

    /// <summary>
    /// Creates a <see cref="StyleBuilder"/> used to define conditional in-line style used in a component. Call <see cref="Build"/> to return the completed style as a string.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">The CSS property value.</param>
    /// <returns>A new <see cref="StyleBuilder"/> instance.</returns>
    public static StyleBuilder Default(string prop, string value)
        => new(prop, value);

    /// <summary>
    /// Creates a <see cref="StyleBuilder"/> used to define conditional in-line style used in a component. Call <see cref="Build"/> to return the completed style as a string.
    /// </summary>
    /// <param name="style">The initial style string.</param>
    /// <returns>A new <see cref="StyleBuilder"/> instance.</returns>
    public static StyleBuilder Default(string style)
        => new(style);

    /// <summary>
    /// Creates an empty <see cref="StyleBuilder"/>.
    /// </summary>
    /// <returns>An empty <see cref="StyleBuilder"/> instance.</returns>
    public static StyleBuilder Empty()
        => new();

    #endregion

    /// <summary>
    /// Adds a style string to the builder.
    /// </summary>
    /// <param name="style">The style string to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string style)
        => !string.IsNullOrWhiteSpace(style) ? AddRaw($"{style};") : this;

    /// <summary>
    /// Adds a raw string to the builder that will be concatenated with the next style or value added to the builder.
    /// </summary>
    /// <param name="style">The raw string to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    private StyleBuilder AddRaw(string? style)
    {
        stringBuffer += style;
        return this;
    }

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">Style to add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, string value)
        => AddRaw($"{prop}:{value};");

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="style">The style string to add.</param>
    /// <param name="when">Condition in which the style is added.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string style, bool when = true)
        => when ? AddRaw(style) : this;

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">Style to conditionally add.</param>
    /// <param name="when">Condition in which the style is added.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, string value, bool when = true)
        => when ? AddStyle(prop, value) : this;

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">Style to conditionally add.</param>
    /// <param name="when">Condition in which the style is added.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, Func<string> value, bool when = true)
        => when ? AddStyle(prop, value()) : this;

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">Style to conditionally add.</param>
    /// <param name="when">Condition in which the style is added.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, string value, Func<bool>? when = null)
        => AddStyle(prop, value, when?.Invoke() ?? true);

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="value">Style to conditionally add.</param>
    /// <param name="when">Condition in which the style is added.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(string prop, Func<string> value, Func<bool>? when = null)
        => AddStyle(prop, value(), when?.Invoke() ?? true);

    /// <summary>
    /// Adds a conditional nested <see cref="StyleBuilder"/> to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="builder">Style Builder to conditionally add.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(StyleBuilder builder) => AddRaw(builder.Build());

    /// <summary>
    /// Adds a conditional nested <see cref="StyleBuilder"/> to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="builder">Style Builder to conditionally add.</param>
    /// <param name="when">Condition in which the style is added.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(StyleBuilder builder, bool when = true)
        => when ? AddRaw(builder.Build()) : this;

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// </summary>
    /// <param name="builder">Style Builder to conditionally add.</param>
    /// <param name="when">Condition in which the styles are added.</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyle(StyleBuilder builder, Func<bool>? when = null)
        => AddStyle(builder, when?.Invoke() ?? true);

    /// <summary>
    /// Adds a conditional in-line style to the builder with space separator and closing semicolon.
    /// A <see cref="ValueBuilder"/> action defines a complex set of values for the property.
    /// </summary>
    /// <param name="prop">The CSS property name.</param>
    /// <param name="builder">The <see cref="ValueBuilder"/> action to define values.</param>
    /// <param name="when">Condition in which the style is added.</param>
    public StyleBuilder AddStyle(string prop, Action<ValueBuilder> builder, bool when = true)
    {
        ValueBuilder? values = new ValueBuilder();
        builder(values);
        return AddStyle(prop, values.ToString(), when && values.HasValue);
    }

    /// <summary>
    /// Adds a conditional in-line style when it exists in a dictionary to the builder with separator.
    /// Null safe operation.
    /// </summary>
    /// <param name="additionalAttributes">Additional Attribute splat parameters</param>
    /// <returns>The current <see cref="StyleBuilder"/> instance.</returns>
    public StyleBuilder AddStyleFromAttributes(IReadOnlyDictionary<string, object>? additionalAttributes) =>
        additionalAttributes == null
            ? this
            : additionalAttributes.TryGetValue("style", out object? c)
                ? AddRaw(c.ToString())
                : this;

    /// <summary>
    /// Finalizes the completed style as a string.
    /// </summary>
    /// <returns>The completed style string.</returns>
    public string? Build()
    {
        // String buffer finalization code
        return stringBuffer != null ? stringBuffer.Trim() : string.Empty;
    }

    #endregion

    /// <summary>
    /// Returns the built style string.
    /// </summary>
    /// <returns>The completed style string.</returns>
    public override string? ToString()
        => Build();
}
