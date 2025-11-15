namespace Blazing.Mvvm.Components;

/// <summary>
/// Exception thrown when a ViewModel or key has no associated page route in the navigation cache.
/// </summary>
public class ViewModelRouteNotFoundException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelRouteNotFoundException"/> class for a ViewModel type.
    /// </summary>
    /// <param name="viewModelType">The ViewModel type that has no associated page.</param>
    public ViewModelRouteNotFoundException(Type viewModelType) 
        : base($"{viewModelType} has no associated page")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelRouteNotFoundException"/> class for a keyed ViewModel.
    /// </summary>
    /// <param name="key">The key that has no associated page.</param>
    public ViewModelRouteNotFoundException(object key) 
        : base($"No associated page for key '{key}'")
    {
    }
}