using Blazing.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CSS = Blazing.Tabs.Css.CssClass;

namespace Blazing.Tabs;

/// <summary>
/// Represents a tab control component that manages a collection of <see cref="TabPanel"/>s, selection, and navigation.
/// </summary>
public partial class TabControl : ComponentControlBase
{
    #region Fields

    /// <summary>
    /// The currently focused tab panel.
    /// </summary>
    TabPanel? _focusPanel;

    /// <summary>
    /// The list of tab panels managed by this control.
    /// </summary>
    List<TabPanel> _panels = new();

    /// <summary>
    /// The index of the currently selected tab panel.
    /// </summary>
    private int _selectedIndex;

    /// <summary>
    /// Lazy loader for the common JS module.
    /// </summary>
    private Lazy<Task<IJSObjectReference>>? _commonModuleTask;
    
    /// <summary>
    /// Reference to this instance for JS interop.
    /// </summary>
    private DotNetObjectReference<TabControl>? DotNetInstance;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the JS runtime for interop.
    /// </summary>
    [Inject]
    public IJSRuntime? jsRuntime { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the tab header.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Gets or sets the render fragment containing the tab panels.
    /// </summary>
    [Parameter]
    public RenderFragment? Panels { get; set; }

    /// <summary>
    /// Gets the currently active tab panel.
    /// </summary>
    public TabPanel? ActivePanel { get; internal set; }

    /// <summary>
    /// Gets or sets the index of the selected tab panel.
    /// </summary>
#pragma warning disable CS0105, BL0007
    [Parameter]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value || value < 0) return;
            _selectedIndex = value;
            InvokeAsync(() => SetSelectedIndexAsync(value));
        }
    }
#pragma warning restore CS0105, BL0007

    /// <summary>
    /// Gets or sets the callback invoked when the selected index changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> SelectedIndexChanged { get; set; }

    #endregion

    #region Overrides

    /// <summary>
    /// Called when the component is initialized. Sets up JS interop and module loading.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        DotNetInstance = DotNetObjectReference.Create(this);
        _commonModuleTask = new(() => jsRuntime!.ModuleFactory(jsRuntime!.GetCommonScriptPath()));
    }

    /// <summary>
    /// Called after the component has rendered. Handles initial selection of tab panels.
    /// </summary>
    /// <param name="firstRender">Indicates if this is the first render.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            IReadOnlyCollection<TabPanel> panels = _panels.Where(panel => panel.Selected).ToList();
            if (panels.Any())
            {
                foreach (TabPanel panel in panels)
                    await SelectItemAsync(panel);
                StateHasChanged();
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether the tab at the specified index is enabled.
    /// </summary>
    /// <param name="index">The index of the tab.</param>
    /// <returns>True if enabled; otherwise, false.</returns>
    public bool IsTabEnabled(int index)
        => _panels.Count != 0 && index < _panels.Count && index >= 0 && _panels[index].Enabled;

    /// <summary>
    /// Adds a tab panel to the control.
    /// </summary>
    /// <param name="tabPanel">The tab panel to add.</param>
#pragma warning disable BL0005
    internal void AddPanel(TabPanel tabPanel)
    {
        if (!_panels.Any(panel => panel.Selected))
            tabPanel.Selected = true;
        _panels.Add(tabPanel);
        Refresh();
    }
#pragma warning restore BL0005

    /// <summary>
    /// Sets the selected index asynchronously.
    /// </summary>
    /// <param name="value">The index to select.</param>
    private async Task SetSelectedIndexAsync(int value)
    {
        if (_panels.Count == 0) return;
        int index = Math.Clamp(value, 0, _panels.Count - 1);
        await SelectItemAsync(_panels[index]);
    }

    /// <summary>
    /// Selects the specified tab panel asynchronously.
    /// </summary>
    /// <param name="tabPanel">The tab panel to select.</param>
    internal async Task SelectItemAsync(TabPanel tabPanel)
    {
        if (_panels.Count == 0) return;
        if (!tabPanel.Enabled) return;
        await ActivatePage(tabPanel);
    }

    /// <summary>
    /// Activates the specified tab panel.
    /// </summary>
    /// <param name="panel">The tab panel to activate.</param>
#pragma warning disable BL0005
    private async Task ActivatePage(TabPanel panel)
    {
        if (!panel.Enabled) return;
        _selectedIndex = _panels.IndexOf(panel);
        ActivePanel = panel;
        _focusPanel = panel;
        foreach (TabPanel tabPanel in _panels)
            tabPanel.Selected = tabPanel.UniqueId == ActivePanel.UniqueId;
        await SelectedIndexChanged.InvokeAsync(_selectedIndex);
    }
#pragma warning restore BL0005

    /// <summary>
    /// Triggers a UI refresh for the component.
    /// </summary>
    internal void Refresh() => StateHasChanged();

    /// <summary>
    /// Gets the CSS class for the tabs container.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    private string GetTabsClass()
        => CssBuilder
            .Default(CSS.Root)
            .AddClass(Class!, !string.IsNullOrWhiteSpace(Class))
            .Build();

    /// <summary>
    /// Gets the CSS class for the tab header.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    private string GetHeaderClass()
        => CssBuilder
            .Default(CSS.Header)
            .AddClass(HeaderClass!, !string.IsNullOrWhiteSpace(HeaderClass))
            .Build();

    /// <summary>
    /// Gets the CSS class for the tab button.
    /// </summary>
    /// <param name="panel">The tab panel.</param>
    /// <returns>The CSS class string.</returns>
    string GetButtonClass(TabPanel panel)
        => CssBuilder
            .Default(CSS.Button)
            .AddClass(CSS.ButtonModifier.Active, _panels.IndexOf(panel) == _selectedIndex)
            .AddClass(CSS.ButtonModifier.Disabled, !panel.Enabled)
            .AddClass(panel.HeaderClass!, !string.IsNullOrWhiteSpace(panel.HeaderClass))
            .Build();

    /// <summary>
    /// Gets the CSS class for the tab button title.
    /// </summary>
    /// <param name="panel">The tab panel.</param>
    /// <returns>The CSS class string.</returns>
    private string GetButtonTitleClass(TabPanel panel)
        => CssBuilder
            .Default(CSS.Title)
            .Build();

    /// <summary>
    /// Gets the CSS class for the tab panel.
    /// </summary>
    /// <param name="tabPanel">The tab panel.</param>
    /// <returns>The CSS class string.</returns>
    private string GetTabPanelClass(TabPanel tabPanel)
        => CssBuilder
            .Default(CSS.Panel)
            .AddClass(tabPanel.Class!, !string.IsNullOrWhiteSpace(tabPanel.Class))
            .Build();

    /// <summary>
    /// Gets the style for the tabs container.
    /// </summary>
    /// <returns>The style string.</returns>
    private string GetTabsStyle()
        => Style ?? "";

    /// <summary>
    /// Gets the style for the tab header.
    /// </summary>
    /// <returns>The style string.</returns>
    private string GetHeaderStyle()
        => "";

    /// <summary>
    /// Gets the style for the tab button.
    /// </summary>
    /// <param name="tabPanel">The tab panel.</param>
    /// <returns>The style string.</returns>
    private string GetButtonStyle(TabPanel tabPanel)
        => tabPanel.HeaderStyle ?? "";

    /// <summary>
    /// Gets the style for the tab button title.
    /// </summary>
    /// <param name="panel">The tab panel.</param>
    /// <returns>The style string.</returns>
    private string GetButtonTitleStyle(TabPanel panel)
        => "";

    /// <summary>
    /// Gets the style for the tab panel.
    /// </summary>
    /// <param name="tabPanel">The tab panel.</param>
    /// <returns>The style string.</returns>
    private string GetTabPanelStyle(TabPanel tabPanel)
        => tabPanel.Style ?? "";

    /// <summary>
    /// Focuses the next element in the tab order.
    /// </summary>
    /// <param name="isReverse">True to focus in reverse order.</param>
    private async Task FocusNextElement(bool isReverse = false)
    {
        await (await GetModuleInstance())
            .FocusNextElement(isReverse: isReverse)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the JS module instance.
    /// </summary>
    /// <returns>The JS object reference.</returns>
    private async Task<IJSObjectReference> GetModuleInstance()
        => await _commonModuleTask!.Value;

    /// <summary>
    /// Handles key down events for tab navigation.
    /// </summary>
    /// <param name="arg">The keyboard event arguments.</param>
    private async Task OnKeyDown(KeyboardEventArgs arg)
    {
        //Console.WriteLine($"** KEY: {arg.Code} | {arg.Key}");
        switch (arg.Key)
        {
            case "ArrowLeft":
                await MovePreviousAsync();
                break;
            case "ArrowRight":
                await MoveNextAsync();
                break;
            case "Home":
                await MoveFirstAsync();
                break;
            case "End":
                await MoveLastAsync();
                break;
            case "Tab":
                await FocusNextElement(arg.ShiftKey);
                break;
        }
    }

    /// <summary>
    /// Moves to the previous enabled tab panel.
    /// </summary>
    public async Task MovePreviousAsync()
    {
        if (_focusPanel is null || _panels.Count < 2) return;
        int start = _panels.IndexOf(_focusPanel);
        int index = start;
        do
        {
            if (--index < 0) index = _panels.Count - 1;
            TabPanel tabPanel = _panels[index];
            if (tabPanel.Enabled)
            {
                _focusPanel = tabPanel;
                await SelectItemAsync(tabPanel);
                break;
            }
        } while (index != start);
    }

    /// <summary>
    /// Moves to the next enabled tab panel.
    /// </summary>
    public async Task MoveNextAsync()
    {
        if (_focusPanel is null || _panels.Count < 2) return;
        int start = _panels.IndexOf(_focusPanel);
        int index = start;
        do
        {
            if (++index == _panels.Count) index = 0;
            TabPanel tabPanel = _panels[index];
            if (tabPanel.Enabled)
            {
                _focusPanel = tabPanel;
                await SelectItemAsync(tabPanel);
                break;
            }
        } while (index != start);
    }

    /// <summary>
    /// Moves to the first enabled tab panel.
    /// </summary>
    public async Task MoveFirstAsync()
    {
        if (_panels.Count == 0) return;
        TabPanel? tabPanel = _panels.FirstOrDefault(panel => panel.Enabled);
        if (tabPanel is null) return;
        _focusPanel = tabPanel;
        await SelectItemAsync(tabPanel);
    }

    /// <summary>
    /// Moves to the last enabled tab panel.
    /// </summary>
    public async Task MoveLastAsync()
    {
        if (_panels.Count == 0) return;
        TabPanel? tabPanel = _panels.LastOrDefault(panel => panel.Enabled);
        if (tabPanel is null) return;
        _focusPanel = tabPanel;
        await SelectItemAsync(tabPanel);
    }

    #endregion
}