// Copyright (c) 2011 - 2022 Ed Charbeneau
// License: MIT
// See https://github.com/EdCharbeneau/BlazorComponentUtilities

namespace Blazing.Common;

/// <summary>
/// Provides a builder for constructing space-separated string values, commonly used for CSS class or style attributes.
/// </summary>
public class ValueBuilder
{
    #region Fields
    
    /// <summary>
    /// The internal string buffer used to build the value.
    /// </summary>
    private string? stringBuffer; 

    #endregion

    #region Methods

    /// <summary>
    /// Gets a value indicating whether the builder contains a non-empty value.
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(stringBuffer);

    /// <summary>
    /// Adds a space separated conditional value to a property.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <param name="when">Determines whether the value should be added.</param>
    /// <returns>The current <see cref="ValueBuilder"/> instance.</returns>
    public ValueBuilder AddValue(string value, bool when = true)
        => when ? AddRaw($"{value} ") : this;

    /// <summary>
    /// Adds a space separated conditional value to a property using a value factory.
    /// </summary>
    /// <param name="value">A function that returns the value to add.</param>
    /// <param name="when">Determines whether the value should be added.</param>
    /// <returns>The current <see cref="ValueBuilder"/> instance.</returns>
    public ValueBuilder AddValue(Func<string> value, bool when = true)
        => when ? AddRaw($"{value()} ") : this;

    /// <summary>
    /// Adds a raw string to the builder that will be concatenated with the next style or value added to the builder.
    /// </summary>
    /// <param name="style">The raw string to add.</param>
    /// <returns>The current <see cref="ValueBuilder"/> instance.</returns>
    private ValueBuilder AddRaw(string? style)
    {
        stringBuffer += style;
        return this;
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Returns the built string value, trimmed of any leading or trailing whitespace.
    /// </summary>
    /// <returns>The built string value.</returns>
    public override string ToString()
        => stringBuffer != null
            ? stringBuffer.Trim()
            : string.Empty;

    #endregion
}
