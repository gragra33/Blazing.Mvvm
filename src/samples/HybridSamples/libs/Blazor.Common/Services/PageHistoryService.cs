using Microsoft.AspNetCore.Components;

namespace Blazing.Common.Services;

// Tracks the navigation page history of a specific capacity
public class PageHistoryService : IPageHistoryService
{
    public PageHistoryService(NavigationManager navigationManager, int bufferCapacity = 20)
    {
        _navigationManager = navigationManager;
        _buffer = new(bufferCapacity);
    }

    private readonly NavigationManager _navigationManager;

    private readonly CircularBuffer<string> _buffer;

    public bool CanGoBack
        => _buffer.Count() > 1;

    public void AddPageToHistory()
        => _buffer.PushFront(GetRelativePath());

    private string GetRelativePath()
        => "/" +
           (_navigationManager.Uri == _navigationManager.BaseUri
               ? string.Empty
               : _navigationManager.ToBaseRelativePath(_navigationManager.Uri));

    public void AddPageToHistory(string pageName)
        => _buffer.PushFront(pageName);


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
