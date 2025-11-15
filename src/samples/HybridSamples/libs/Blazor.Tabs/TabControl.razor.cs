using Blazing.Common;
using Blazing.Tabs;
using Blazing.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Security.Claims;
using CSS = Blazing.Tabs.Css.CssClass;

namespace Blazing.Tabs;

public partial class TabControl : ComponentControlBase
{
    #region Fields

    TabPanel? _focusPanel;

    List<TabPanel> _panels = new();

    private int _selectedIndex;

    private Lazy<Task<IJSObjectReference>>? _commonModuleTask;
    
    private DotNetObjectReference<TabControl>? DotNetInstance;

    #endregion

    #region Properties

    [Inject]
    public IJSRuntime? jsRuntime { get; set; }

    [Parameter]
    public string? HeaderClass { get; set; }

    [Parameter]
    public RenderFragment? Panels { get; set; }

    public TabPanel? ActivePanel { get; internal set; }

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

    [Parameter]
    public EventCallback<int> SelectedIndexChanged { get; set; }

    #endregion

    #region Overrides

    protected override void OnInitialized()
    {
        base.OnInitialized();

        DotNetInstance = DotNetObjectReference.Create(this);
        _commonModuleTask = new(() => jsRuntime!.ModuleFactory(jsRuntime!.GetCommonScriptPath()));
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
          
                StateHasChanged();
            }
        }
    }

    #endregion

    #region Methods

    public bool IsTabEnabled(int index)
        => _panels.Count != 0 && index < _panels.Count && index >= 0 && _panels[index].Enabled;

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
        ActivePanel = panel;
        _focusPanel = panel;

        foreach (TabPanel tabPanel in _panels)
            tabPanel.Selected = tabPanel.UniqueId == ActivePanel.UniqueId;

        await SelectedIndexChanged.InvokeAsync(_selectedIndex);
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
            .AddClass(HeaderClass!, !string.IsNullOrWhiteSpace(HeaderClass))
            .Build();

    string GetButtonClass(TabPanel panel)
        => CssBuilder
            .Default(CSS.Button)
            .AddClass(CSS.ButtonModifier.Active, _panels.IndexOf(panel) == _selectedIndex)
            .AddClass(CSS.ButtonModifier.Disabled, !panel.Enabled)
            .AddClass(panel.HeaderClass!, !string.IsNullOrWhiteSpace(panel.HeaderClass))
            .Build();

    private string GetButtonTitleClass(TabPanel panel)
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

    private string GetButtonTitleStyle(TabPanel panel)
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

    public async Task MoveFirstAsync()
    {
        if (_panels.Count == 0) return;

        TabPanel? tabPanel = _panels.FirstOrDefault(panel => panel.Enabled);
     
        if (tabPanel is null) return;
        
        _focusPanel = tabPanel;

        await SelectItemAsync(tabPanel);
    }

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