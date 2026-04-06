using Blazing.Common.Services;
using Microsoft.AspNetCore.Components;

namespace Blazing.Common;

/// <summary>
/// Provides a base class for Blazor components that support page navigation and history management.
/// </summary>
public abstract class PageNavigationBase : ComponentControlBase
{
    #region injects
    
    /// <summary>
    /// Gets or sets the navigation manager used for page navigation.
    /// </summary>
    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    /// <summary>
    /// Gets or sets the page history service used for managing navigation history.
    /// </summary>
    [Inject]
    public IPageHistoryService? PageHistoryService { get; set; }

    #endregion

    #region Overrides

    /// <summary>
    /// Called when the component is initialized. Adds the current page to the navigation history.
    /// </summary>
    protected override void OnInitialized()
    {
        AddPageToHistory();
        base.OnInitialized();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the specified page to the navigation history, or the current page if no page is specified.
    /// </summary>
    /// <param name="page">The name of the page to add to history, or <c>null</c> to add the current page.</param>
    public virtual void AddPageToHistory(string? page = null)
    {
        if (PageHistoryService is null)
            return;

        if (page is null)
            PageHistoryService.AddPageToHistory();
        else
            PageHistoryService.AddPageToHistory(page);
    }

    /// <summary>
    /// Navigates back to the previous page in the navigation history.
    /// </summary>
    public void GoBack()
        => NavigationManager!.NavigateTo(PageHistoryService!.TryGetBackPage() ?? "");

    /// <summary>
    /// Disposes resources used by the component and clears navigation services.
    /// </summary>
    public override void Dispose()
    {
        if (NavigationManager is not null)
            NavigationManager = null;

        if (PageHistoryService is not null)
            PageHistoryService = null;

        base.Dispose();
    }

    #endregion
}
