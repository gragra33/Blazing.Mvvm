using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Common;

/// <summary>
/// Provides a base class for Blazor input controls with support for unique IDs, references, styles, and child content.
/// </summary>
/// <typeparam name="TValue">The type of the input value.</typeparam>
public abstract class ComponentInputControlBase<TValue> : InputBase<TValue>, IDisposable
{
    #region Properties

    /// <summary>
    /// Gets the unique ID for the component instance.
    /// </summary>
    public string? UniqueId { get; private set; }

    /// <summary>
    /// Gets or sets the element reference for the component.
    /// </summary>
    [Parameter]
    public ElementReference Reference { get; set; }

    /// <summary>
    /// Gets or sets the child content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the component.
    /// </summary>
    [Parameter]
    public virtual string? Class { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the component.
    /// </summary>
    [Parameter]
    public virtual string? Style { get; set; }

    /// <summary>
    /// Gets the current style as a dictionary of property-value pairs.
    /// </summary>
    public IDictionary<string, string> CurrentStyle
    {
        get
        {
            Dictionary<string, string> currentStyle = new();

            if (string.IsNullOrEmpty(Style)) return currentStyle;
            
            foreach (string pair in Style.Split(';'))
            {
                string[] keyAndValue = pair.Split(':');
                    
                if (keyAndValue.Length != 2) continue;
                    
                string key = keyAndValue[0].Trim();
                string value = keyAndValue[1].Trim();

                currentStyle[key] = value;
            }

            return currentStyle;
        }
    }
    
    #endregion

    #region Overrides
    
    /// <summary>
    /// Called when the component is initialized. Sets the unique ID for the component.
    /// </summary>
    protected override void OnInitialized()
    {
        UniqueId = GetUniqueId();
        base.OnInitialized();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the ID for the component, using the value from additional attributes if available.
    /// </summary>
    /// <returns>The ID string.</returns>
    protected string? GetId()
        => AdditionalAttributes is not null
           && AdditionalAttributes.TryGetValue("id", out object? id)
           && !string.IsNullOrEmpty(Convert.ToString(id))
            ? $"{id}"
            : UniqueId;

    /// <summary>
    /// Generates a unique ID string for the component instance.
    /// </summary>
    /// <returns>The unique ID string.</returns>
    public string GetUniqueId()
        => "id_" + Guid.NewGuid().ToShortString();

    /// <summary>
    /// Gets the CSS class for the component.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    public virtual string GetComponentCssClass() => Class ?? "";

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    public virtual void Dispose()
    {
        //
    }

    #endregion
}
