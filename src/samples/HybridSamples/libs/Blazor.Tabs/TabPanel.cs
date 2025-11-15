using Blazing.Common;
using Blazing.Tabs;
using Blazing.Common;
using Microsoft.AspNetCore.Components;

namespace Blazing.Tabs;

public class TabPanel : ComponentControlBase
{
    #region Fields

    private bool _enabled = true;
    private TabControl? _parent;

    #endregion

    #region Properties

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

    internal string? HeaderUniqueId { get; set; }

    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    [Parameter]
    public virtual string? HeaderClass { get; set; }

    [Parameter]
    public virtual string? HeaderStyle { get; set; }

    [Parameter]
    public string? Title { get; set; }
    
    [Parameter]
    public bool Selected { get; set; }

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

    [Parameter]
    public EventCallback<bool> EnabledChanged { get; set; }

    #endregion

    #region Overrides

    protected override void OnInitialized()
    {
        if (Parent == null) throw new ArgumentNullException(nameof(Parent), "TabPage must exist within a Tab Control");
        HeaderUniqueId = GetUniqueId();

        base.OnInitialized();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.DidParameterChange(nameof(Selected), Selected))
            await Parent.SelectItemAsync(this);

        await base.SetParametersAsync(parameters);
    }

    #endregion

    #region Methods

    private async Task NotifyEnableChangeAsync()
        => await EnabledChanged.InvokeAsync(_enabled);

    #endregion
}