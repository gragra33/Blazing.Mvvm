using Blazing.Common.Services;
using Microsoft.AspNetCore.Components;

namespace Blazing.Common;

public abstract class PageNavigationBase : ComponentControlBase
{
    #region injects
    
    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    [Inject]
    public IPageHistoryService? PageHistoryService { get; set; }

    #endregion

    #region Overrides

    protected override void OnInitialized()
    {
        AddPageToHistory();
        base.OnInitialized();
    }

    #endregion

    #region Methods

    public virtual void AddPageToHistory(string? page = null)
    {
        if (PageHistoryService is null)
            return;

        if (page is null)
            PageHistoryService.AddPageToHistory();
        else
            PageHistoryService.AddPageToHistory(page);
    }

    public void GoBack()
        => NavigationManager!.NavigateTo(PageHistoryService!.TryGetBackPage() ?? "");

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
