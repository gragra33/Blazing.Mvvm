using Blazing.Common;
using Blazing.Lists;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Security.Claims;
using CSS = Blazing.Lists.Css.CssClass;

namespace Blazing.Lists;

/// <summary>
/// Represents a list box component for displaying and selecting items, supporting templates and keyboard navigation.
/// </summary>
/// <typeparam name="TItem">The type of items in the list box.</typeparam>
public partial class ListBox<TItem> : ComponentControlBase, IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// The currently selected item.
    /// </summary>
    private TItem? _selectedItem;
    /// <summary>
    /// The index of the currently selected item.
    /// </summary>
    private int _selectedIndex = -1;
    /// <summary>
    /// Indicates whether scrolling into view is required.
    /// </summary>
    private bool _scrollIntoViewRequired;
    /// <summary>
    /// Indicates whether the selection has changed.
    /// </summary>
    private bool _isSelectionChanged;
    /// <summary>
    /// Cached reference to the items list.
    /// </summary>
    private IList<TItem>? _items;
    /// <summary>
    /// Gets the count of items in the list.
    /// </summary>
    private int _itemsCount => _items?.Count ?? 0;
    /// <summary>
    /// Reference to the component element.
    /// </summary>
    private ElementReference? componentElement { get; set; }
    /// <summary>
    /// Reference to the selected item element.
    /// </summary>
    private ElementReference? SelectedItemElement { get; set; }
    /// <summary>
    /// Lazy loader for the list JS module.
    /// </summary>
    private Lazy<Task<IJSObjectReference>>? _listModuleTask;
    /// <summary>
    /// Lazy loader for the common JS module.
    /// </summary>
    private Lazy<Task<IJSObjectReference>>? _commonModuleTask;
    /// <summary>
    /// Reference to this instance for JS interop.
    /// </summary>
    private DotNetObjectReference<ListBox<TItem>>? DotNetInstance;
    /// <summary>
    /// The path to the list script file.
    /// </summary>
    private const string  ListScriptFile = "./_content/Blazing.Lists/scripts/lists.js";

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the JS runtime for interop.
    /// </summary>
    [Inject]
    public IJSRuntime? jsRuntime { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the list box.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "List";

    /// <summary>
    /// Gets or sets the template for rendering each item.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the header.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the footer.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the source collection of items.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? ItemSource { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<ListBoxEventArgs<TItem>> SelectionChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the list box is read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    [Parameter]
#pragma warning disable BL0007
    public TItem SelectedItem
#pragma warning restore BL0007
    {
        get => _selectedItem!;
        set => _selectedItem = value;
    }

    /// <summary>
    /// Gets or sets the selected index.
    /// </summary>
    [Parameter]
#pragma warning disable BL0007
    public int SelectedIndex
#pragma warning restore BL0007
    {
        get => _selectedIndex;
        set => _selectedIndex = value;
    }

    /// <summary>
    /// Gets or sets the callback invoked when the selected index changes.
    /// </summary>
    public EventCallback<int> SelectedIndexChanged { get; set; }
    
    #endregion

    #region Events

    /// <summary>
    /// Handles the item click event asynchronously.
    /// </summary>
    /// <param name="item">The item that was clicked.</param>
    private async Task ItemClickEventAsync(TItem item)
    {
        if (_selectedItem is not null && _selectedItem.Equals(item)) return;
        _selectedItem = item;
        _selectedIndex = _items!.IndexOf(item);
        await SelectionChanged.InvokeAsync(new ListBoxEventArgs<TItem>(this, _selectedItem)).ConfigureAwait(false);
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Called when the component is initialized. Sets up JS interop and module loading.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        DotNetInstance = DotNetObjectReference.Create(this);
        _listModuleTask = new(() => jsRuntime!.ModuleFactory(ListScriptFile));
        _commonModuleTask = new(() => jsRuntime!.ModuleFactory(jsRuntime!.GetCommonScriptPath()));
    }

    /// <summary>
    /// Called when component parameters are set. Updates the items and selection state.
    /// </summary>
    protected override void OnParametersSet()
    {
        // make a reference to avoid excessive casting
        if (_items is null || _items.Count != ItemSource?.Count())
            _items = ItemSource?.ToList();
        if (_items is null)
            return;
        if (ReadOnly)
        {
            _selectedIndex = -1;
            _selectedItem = default;
            return;
        }
        if (_selectedIndex >= _itemsCount)
            _selectedIndex = _itemsCount - 1;
        if (_selectedIndex > -1 && _selectedItem is null)
            _selectedItem = _items[_selectedIndex];
        if (_selectedIndex == -1 && _selectedItem is not null)
            _selectedIndex = _items.IndexOf(_selectedItem);
    }

    /// <summary>
    /// Called after the component has rendered. Handles scrolling and selection change events.
    /// </summary>
    /// <param name="firstRender">Indicates if this is the first render.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_scrollIntoViewRequired)
        {
            _scrollIntoViewRequired = false;
            if (SelectedItemElement is not null)
                await ScrollIntoView(SelectedItemElement!.Value).ConfigureAwait(false);
        }
        if (_isSelectionChanged && _selectedItem is not null && !ReadOnly)
        {
            _isSelectionChanged = false;
            await InvokeAsync(async () => await SelectionChanged
                .InvokeAsync(new ListBoxEventArgs<TItem>(this, _selectedItem!))
                .ConfigureAwait(false));
        }
    }

    #endregion

    #region Methods
   
    /// <summary>
    /// Gets the CSS class for the list box component.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    private string GetClass()
        => CssBuilder
            .Default(CSS.ListBox.Root)
            .AddClass(Class!, !string.IsNullOrWhiteSpace(Class))
            .Build();
 
    /// <summary>
    /// Gets the CSS class for an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The CSS class string.</returns>
    private string GetItemClass(TItem item)
        => CssBuilder
            .Default(CSS.ListBox.Item)
            .AddClass(CSS.ListBox.ItemModifier.Selected, IsSelectedState(item))
            .Build();

    /// <summary>
    /// Determines whether the specified item is selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if selected; otherwise, false.</returns>
    private bool IsSelectedState(TItem item)
    {
        bool isSelected = _selectedItem is not null && _selectedItem.Equals(item);
        return isSelected;
    }

    /// <summary>
    /// Sets the selected index and updates the selected item.
    /// </summary>
    /// <param name="index">The index to select.</param>
    private void SetSelectedIndex(int index)
    {
        if (_items is null || index >= _items.Count || ReadOnly)
            return;
        _selectedIndex = index;
        _selectedItem = index < 0 ? default : _items[index];
        _isSelectionChanged = true;
        ScrollIntoView();
    }

    /// <summary>
    /// Sets the selected item and updates the selected index.
    /// </summary>
    /// <param name="item">The item to select.</param>
    private void SetSelectedItem(TItem item)
    {
        if (_items is null || !_items.Contains(item) || ReadOnly)
            return;
        _selectedItem = item;
        _selectedIndex = _items.IndexOf(item);
       _isSelectionChanged = true;
        ScrollIntoView();
    }

    /// <summary>
    /// Focuses the next element in the list box.
    /// </summary>
    /// <param name="isReverse">True to focus in reverse order.</param>
    private async Task FocusNextElement(bool isReverse = false)
    {
        await (await GetModuleInstance(true))
            .FocusNextElement(componentElement!.Value, isReverse)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Requests scrolling the selected item into view.
    /// </summary>
    private void ScrollIntoView()
    {
        _scrollIntoViewRequired = true;
        StateHasChanged();
    }

    /// <summary>
    /// Scrolls the specified item element into view asynchronously.
    /// </summary>
    /// <param name="itemElement">The element to scroll into view.</param>
    public async Task ScrollIntoView(ElementReference? itemElement)
    {
        // now scroll the element into view
        await (await GetModuleInstance())
            .ScrollIntoViewAsync(itemElement!.Value)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the JS module instance.
    /// </summary>
    /// <param name="isCommonModule">True to get the common module; otherwise, the list module.</param>
    /// <returns>The JS object reference.</returns>
    private async Task<IJSObjectReference> GetModuleInstance(bool isCommonModule = false)
        => await (isCommonModule ? _commonModuleTask : _listModuleTask)!.Value;

    /// <summary>
    /// Handles key down events for list navigation.
    /// </summary>
    /// <param name="arg">The keyboard event arguments.</param>
    private async Task OnKeyDown(KeyboardEventArgs arg)
    {
        //Console.WriteLine($"** KEY: {arg.Code} | {arg.Key}");
        if (_itemsCount == 0)
            return;
        switch (arg.Key)
        {
            case "ArrowUp":
                MovePrevious();
                break;
            case "ArrowDown":
                MoveNext();
                break;
            case "Home":
                MoveFirst();
                break;
            case "End":
                MoveLast();
                break;
            case "Tab":
                await FocusNextElement(arg.ShiftKey);
                break;
        }
    }

    /// <summary>
    /// Moves to the previous item in the list.
    /// </summary>
    public void MovePrevious()
    {
        if (ReadOnly)
            return;
        if (_selectedIndex < 0)
        {
            MoveFirst();
            return;
        }
        if (_selectedIndex == 0)
            return;
        SetSelectedIndex(SelectedIndex - 1);
    }

    /// <summary>
    /// Moves to the next item in the list.
    /// </summary>
    public void MoveNext()
    {
        if (ReadOnly)
            return;
        if (_selectedIndex < 0)
        {
            MoveFirst();
            return;
        }
        if (_selectedIndex >= _itemsCount)
            return;
        SetSelectedIndex(SelectedIndex + 1);
    }

    /// <summary>
    /// Moves to the first item in the list.
    /// </summary>
    public void MoveFirst()
    {
        if (ReadOnly)
            return;
        if (_itemsCount == 0)
            return;
        SetSelectedIndex(0);
    }

    /// <summary>
    /// Moves to the last item in the list.
    /// </summary>
    public void MoveLast()
    {
        if (ReadOnly)
            return;
        int count = _itemsCount;
        if (count == 0)
            return;
        SetSelectedIndex(count - 1);
    }

    /// <summary>
    /// Disposes resources used by the component asynchronously.
    /// </summary>
    /// <returns>A value task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_items is not null)
            _items = null;
        if (_listModuleTask!.IsValueCreated)
        {
            IJSObjectReference module = await _listModuleTask.Value;
            await module.DisposeAsync();
        }
    }

    #endregion
}