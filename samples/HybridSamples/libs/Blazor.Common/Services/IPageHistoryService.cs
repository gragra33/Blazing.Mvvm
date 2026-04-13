namespace Blazing.Common.Services;

/// <summary>
/// Provides methods for managing page navigation history in an application.
/// </summary>
public interface IPageHistoryService
{
    /// <summary>
    /// Gets a value indicating whether navigation to a previous page is possible.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Adds the current page to the navigation history.
    /// </summary>
    void AddPageToHistory();

    /// <summary>
    /// Adds the specified page to the navigation history.
    /// </summary>
    /// <param name="pageName">The name of the page to add to history.</param>
    void AddPageToHistory(string pageName);
    
    /// <summary>
    /// Attempts to retrieve the name of the previous page from the navigation history.
    /// </summary>
    /// <returns>The name of the previous page, or <c>null</c> if not available.</returns>
    string? TryGetBackPage();
}
