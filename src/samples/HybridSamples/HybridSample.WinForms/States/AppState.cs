namespace HybridSample.WinForms.States;

/// <summary>
/// Provides application-wide state for the WinForms application.
/// </summary>
public static class AppState
{
    /// <summary>
    /// Gets or sets the navigation service for the application.
    /// </summary>
    public static INavigation Navigation { get; set; } = null!;
}