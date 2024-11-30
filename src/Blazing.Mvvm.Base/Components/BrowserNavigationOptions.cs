namespace Blazing.Mvvm.ComponentModel;

public readonly struct BrowserNavigationOptions
{
    //
    // Summary:
    //     If true, bypasses client-side routing and forces the browser to load the new
    //     page from the server, whether or not the URI would normally be handled by the
    //     client-side router.
    public bool ForceLoad { get; init; }

    //
    // Summary:
    //     If true, replaces the currently entry in the history stack. If false, appends
    //     the new entry to the history stack.
    public bool ReplaceHistoryEntry { get; init; }

    //
    // Summary:
    //     Gets or sets the state to append to the history entry.
    public string? HistoryEntryState { get; init; }
}