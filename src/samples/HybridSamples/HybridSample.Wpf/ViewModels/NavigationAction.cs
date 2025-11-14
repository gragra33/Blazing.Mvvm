namespace HybridSample.Wpf.ViewModels;

/// <summary>
/// Represents a navigation action with a title and an associated action to execute.
/// </summary>
/// <param name="Title">The title of the navigation action.</param>
/// <param name="Action">The action to execute.</param>
public record NavigationAction(string Title, Action Action);
