namespace Blazing.Common.Services;

public interface IPageHistoryService
{
    bool CanGoBack { get; }

    void AddPageToHistory();

    void AddPageToHistory(string pageName);
    
    string? TryGetBackPage();
}
