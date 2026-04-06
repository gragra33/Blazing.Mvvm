using Microsoft.AspNetCore.Components;

namespace Blazing.Common.Services;

/// <summary>
/// Tracks the navigation page history of a specific capacity and provides methods for managing page navigation history.
/// </summary>
public class PageHistoryService : IPageHistoryService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageHistoryService"/> class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager used to obtain page URIs.</param>
    /// <param name="bufferCapacity">The maximum number of pages to keep in history.</param>
    public PageHistoryService(NavigationManager navigationManager, int bufferCapacity = 20)
    {
        _navigationManager = navigationManager;
        _buffer = new(bufferCapacity);
    }

    /// <summary>
    /// The navigation manager used to obtain page URIs.
    /// </summary>
    private readonly NavigationManager _navigationManager;

    /// <summary>
    /// The circular buffer storing the page history.
    /// </summary>
    private readonly CircularBuffer<string> _buffer;

    /// <inheritdoc/>
    public bool CanGoBack
        => _buffer.Count() > 1;

    /// <inheritdoc/>
    public void AddPageToHistory()
        => _buffer.PushFront(GetRelativePath());

    /// <summary>
    /// Gets the relative path of the current page.
    /// </summary>
    /// <returns>The relative path of the current page.</returns>
    private string GetRelativePath()
        => "/" +
           (_navigationManager.Uri == _navigationManager.BaseUri
               ? string.Empty
               : _navigationManager.ToBaseRelativePath(_navigationManager.Uri));

    /// <inheritdoc/>
    public void AddPageToHistory(string pageName)
        => _buffer.PushFront(pageName);

    /// <inheritdoc/>
    public string? TryGetBackPage()
    {
        if (CanGoBack)
        {
            // You add a page on initialization, so you need to return the 2nd from the last
            _buffer.PopFront();
            return _buffer.PopValueFromFront()!;
        }

        // Can't go back because you didn't navigate enough
        return null;
    }
}
