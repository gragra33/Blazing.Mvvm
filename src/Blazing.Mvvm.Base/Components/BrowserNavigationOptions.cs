namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Represents options for browser navigation in Blazor applications, including force load, history entry replacement, and custom history state.
/// </summary>
public readonly struct BrowserNavigationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to bypass client-side routing and force the browser to load the new page from the server, even if the URI would normally be handled by the client-side router.
    /// </summary>
    public bool ForceLoad { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to replace the current entry in the browser history stack. If <c>true</c>, replaces the current entry; if <c>false</c>, appends a new entry.
    /// </summary>
    public bool ReplaceHistoryEntry { get; init; }

    /// <summary>
    /// Gets or sets the custom state to append to the browser history entry.
    /// </summary>
    public string? HistoryEntryState { get; init; }
}
