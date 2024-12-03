namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Represents options for browser navigation.
/// </summary>
public readonly struct BrowserNavigationOptions
{
    /// <summary>
    /// If true, bypasses client-side routing and forces the browser to load the new
    /// page from the server, whether the URI would normally be handled by the
    /// client-side router.
    /// </summary>
    public bool ForceLoad { get; init; }

    /// <summary>
    /// If true, replaces the current entry in the history stack. If false, appends
    /// the new entry to the history stack.
    /// </summary>
    public bool ReplaceHistoryEntry { get; init; }

    /// <summary>
    /// Gets or sets the state to append to the history entry.
    /// </summary>
    public string? HistoryEntryState { get; init; }
}
