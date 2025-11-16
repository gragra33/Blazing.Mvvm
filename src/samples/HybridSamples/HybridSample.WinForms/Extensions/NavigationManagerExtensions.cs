using Microsoft.AspNetCore.Components;

namespace HybridSample.WinForms.Extensions;

/// <summary>
/// Extension methods for NavigationManager.
/// </summary>
public static class NavigationManagerExtensions
{
    /// <summary>
    /// Navigates to the specified URI with force load option.
    /// </summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="uri">The URI to navigate to.</param>
    /// <param name="forceLoad">Whether to force load the page.</param>
    public static void NavigateTo(this NavigationManager navigationManager, string uri, bool forceLoad)
    {
        navigationManager.NavigateTo(uri, forceLoad);
    }
}