using Blazing.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CSS = Blazing.Tabs.Css.CssClass;

/// <summary>
/// Represents a tab control component for managing and displaying tab panels in Blazor.
/// </summary>
namespace Blazing.Tabs;

public partial class TabControl : ComponentControlBase
{
    #region Fields

    /// <summary>
    /// The currently focused tab panel.
    /// </summary>
    TabPanel? _focusPanel;

    /// <summary>
    /// The collection of tab panels managed by this control.
    /// </summary>
    private readonly List<TabPanel> _panels = new();

    /// <summary>
    /// The index of the currently selected tab panel.
    /// </summary>
    private int _selectedIndex;

    /// <summary>
    /// Lazy-loaded JavaScript module for common tab control operations.
    /// </summary>
    private Lazy<Task<IJSObjectReference>>? _commonModuleTask;

    #endregion

    #region Properties

    /// <summary>
    /// The injected JavaScript runtime for interop operations.
    /// </summary>
    [Inject]
    public IJSRuntime? _jsRuntime { get; set; }

    /// <summary>
    /// The CSS class applied to the tab header.
    /// </summary>
    [Parameter]
    public string? _headerClass { get; set; }

    /// <summary>
    /// The render fragment containing tab panels.
    /// </summary>
    [Parameter]
    public RenderFragment? _panelsFragment { get; set; }

    /// <summary>
    /// The currently active tab panel.
    /// </summary>
    public TabPanel? _activePanel { get; internal set; }

    /// <summary>
    /// The selected tab index parameter.
    /// </summary>
    [Parameter]
    public int _selectedIndexParameter
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value || value < 0) return;

            _selectedIndex = value;
            InvokeAsync(() => SetSelectedIndexAsync(value));
        }
    }

    /// <summary>
    /// Event callback triggered when the selected tab index changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> _selectedIndexChanged { get; set; }

    #endregion

    #region Overrides

    protected override void OnInitialized()
    {
        base.OnInitialized();

        DotNetObjectReference.Create(this);
        _commonModuleTask = new(() => _jsRuntime!.ModuleFactory(_jsRuntime!.GetCommonScriptPath()));
    }

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
          
                await StateHasChangedAsync();
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines if the tab at the specified index is enabled.
    /// </summary>
    /// <param name="index">The index of the tab.</param>
    /// <returns><c>true</c> if the tab is enabled; otherwise, <c>false</c>.</returns>
    public bool IsTabEnabled(int index)
        => _panels.Count != 0 && index < _panels.Count && index >= 0 && _panels[index].Enabled;

    /// <summary>
    /// Adds a panel to the tab control.
    /// </summary>
    /// <param name="tabPanel">The tab panel to add.</param>
    internal void AddPanel(TabPanel tabPanel)
    {
        if (!_panels.Any(panel => panel.Selected))
            tabPanel.Selected = true;

        _panels.Add(tabPanel);
        Refresh();
    }

    private async Task SetSelectedIndexAsync(int value)
    {
        if (_panels.Count == 0) return;

        int index = Math.Clamp(value, 0, _panels.Count - 1);

        await SelectItemAsync(_panels[index]);
    }

    internal async Task SelectItemAsync(TabPanel tabPanel)
    {
        if (_panels.Count == 0) return;

        if (!tabPanel.Enabled) return;

        await ActivatePage(tabPanel);
    }

    private async Task ActivatePage(TabPanel panel)
    {
        if (!panel.Enabled) return;

        _selectedIndex = _panels.IndexOf(panel);
        _activePanel = panel;
        _focusPanel = panel;

        foreach (TabPanel tabPanel in _panels)
            tabPanel.Selected = tabPanel.UniqueId == _activePanel.UniqueId;

        await _selectedIndexChanged.InvokeAsync(_selectedIndex);
    }

    internal void Refresh() => StateHasChanged();

    private string GetTabsClass()
        => CssBuilder
            .Default(CSS.Root)
            .AddClass(Class!, !string.IsNullOrWhiteSpace(Class))
            .Build();

    private string GetHeaderClass()
        => CssBuilder
            .Default(CSS.Header)
            .AddClass(_headerClass!, !string.IsNullOrWhiteSpace(_headerClass))
            .Build();

    /// <summary>
    /// Gets the CSS class for the tab button.
    /// </summary>
    /// <param name="panel">The tab panel associated with the button.</param>
    /// <returns>The CSS class for the tab button.</returns>
    string GetButtonClass(TabPanel panel)
        => CssBuilder
            .Default(CSS.Button)
            .AddClass(CSS.ButtonModifier.Active, _panels.IndexOf(panel) == _selectedIndex)
            .AddClass(CSS.ButtonModifier.Disabled, !panel.Enabled)
            .AddClass(panel.HeaderClass!, !string.IsNullOrWhiteSpace(panel.HeaderClass))
            .Build();

    private string GetButtonTitleClass()
        => CssBuilder
            .Default(CSS.Title)
            .Build();

    private string GetTabPanelClass(TabPanel tabPanel)
        => CssBuilder
            .Default(CSS.Panel)
            .AddClass(tabPanel.Class!, !string.IsNullOrWhiteSpace(tabPanel.Class))
            .Build();

    private string GetTabsStyle()
        => Style ?? "";

    private string GetHeaderStyle()
        => "";

    private string GetButtonStyle(TabPanel tabPanel)
        => tabPanel.HeaderStyle ?? "";

    private string GetButtonTitleStyle()
        => "";

    private string GetTabPanelStyle(TabPanel tabPanel)
        => tabPanel.Style ?? "";

    private async Task FocusNextElement(bool isReverse = false)
    {
        await (await GetModuleInstance())
            .FocusNextElement(isReverse: isReverse)
            .ConfigureAwait(false);
    }

    private async Task<IJSObjectReference> GetModuleInstance()
        => await _commonModuleTask!.Value;

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
    /// Moves the focus to the previous tab panel.
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
    /// Moves the focus to the next tab panel.
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
    /// Moves the focus to the first enabled tab panel.
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
    /// Moves the focus to the last enabled tab panel.
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
