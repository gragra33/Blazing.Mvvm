namespace HybridSample.MAUI.States;

/// <summary>
/// Provides application-wide state for the MAUI application.
/// </summary>
public static class AppState
{
    /// <summary>
    /// Gets or sets the navigation service for the application.
    /// </summary>
    public static INavigation Navigation { get; set; } = null!;
}
