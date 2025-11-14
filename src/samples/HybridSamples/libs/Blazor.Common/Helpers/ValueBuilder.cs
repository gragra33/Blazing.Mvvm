// Copyright (c) 2011 - 2022 Ed Charbeneau
// License: MIT
// See https://github.com/EdCharbeneau/BlazorComponentUtilities

namespace Blazing.Common;

public class ValueBuilder
{
    #region Fields
    
    private string? stringBuffer; 

    #endregion

    #region Methods

    public bool HasValue => !string.IsNullOrWhiteSpace(stringBuffer);

    /// <summary>
    /// Adds a space separated conditional value to a property.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="when"></param>
    /// <returns></returns>
    public ValueBuilder AddValue(string value, bool when = true)
        => when ? AddRaw($"{value} ") : this;

    /// <summary>
    /// Adds a space separated conditional value to a property.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="when"></param>
    /// <returns></returns>
    public ValueBuilder AddValue(Func<string> value, bool when = true)
        => when ? AddRaw($"{value()} ") : this;

    /// <summary>
    /// Adds a raw string to the builder that will be concatenated with the next style or value added to the builder.
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="value"></param>
    /// <returns>StyleBuilder</returns>
    private ValueBuilder AddRaw(string? style)
    {
        stringBuffer += style;
        return this;
    }

    #endregion

    #region Overrides

    public override string ToString()
        => stringBuffer != null
            ? stringBuffer.Trim()
            : string.Empty;

    #endregion
}
