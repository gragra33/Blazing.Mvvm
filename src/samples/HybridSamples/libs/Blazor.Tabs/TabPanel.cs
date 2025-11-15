using Microsoft.AspNetCore.Components;
using Blazing.Common;

namespace Blazing.Tabs;

/// <summary>
/// Represents a panel within a tab control.
/// </summary>
public class TabPanel : ComponentControlBase
{
    #region Fields

    /// <summary>
    /// Indicates whether the tab panel is enabled.
    /// </summary>
    private bool _enabled = true;
    /// <summary>
    /// The parent tab control.
    /// </summary>
    private TabControl? _parent;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the parent tab control via cascading parameter.
    /// </summary>
    [CascadingParameter]
    private TabControl Parent
    {
        get => _parent!;
        set
        {
            if (_parent == value) return;
                
            _parent = value;
            _parent.AddPanel(this);
        }
    }

    /// <summary>
    /// Gets or sets the unique ID for the tab header.
    /// </summary>
    internal string? HeaderUniqueId { get; set; }

    /// <summary>
    /// Gets or sets the template for the tab header.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the tab header.
    /// </summary>
    [Parameter]
    public virtual string? HeaderClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the tab header.
    /// </summary>
    [Parameter]
    public virtual string? HeaderStyle { get; set; }

    /// <summary>
    /// Gets or sets the title of the tab panel.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the tab panel is selected.
    /// </summary>
    [Parameter]
    public bool Selected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tab panel is enabled.
    /// </summary>
    [Parameter]
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            InvokeAsync(async () => await NotifyEnableChangeAsync());
        }
    }

    /// <summary>
    /// Event callback triggered when the enabled state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> EnabledChanged { get; set; }

    #endregion

    #region Overrides

    /// <summary>
    /// Called when the component is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        if (Parent == null) throw new ArgumentNullException(nameof(Parent), "TabPage must exist within a Tab Control");
        HeaderUniqueId = GetUniqueId();

        base.OnInitialized();
    }

    /// <summary>
    /// Sets parameters supplied to the component.
    /// </summary>
    /// <param name="parameters">The parameters to set.</param>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.DidParameterChange(nameof(Selected), Selected))
            await Parent.SelectItemAsync(this);

        await base.SetParametersAsync(parameters);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Notifies that the enabled state has changed.
    /// </summary>
    private async Task NotifyEnableChangeAsync()
        => await EnabledChanged.InvokeAsync(_enabled);

    #endregion
}
