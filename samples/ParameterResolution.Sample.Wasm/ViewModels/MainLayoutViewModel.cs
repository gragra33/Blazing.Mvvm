using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace ParameterResolution.Sample.Wasm.ViewModels;

/// <summary>
/// ViewModel for the main layout that tracks navigation events.
/// </summary>
/// <remarks>
/// This ViewModel demonstrates how to integrate with Blazor's <see cref="NavigationManager"/>
/// to track and display the number of navigation events that occur during the application's lifecycle.
/// It is registered as a Singleton to maintain state across all pages.
/// </remarks>
[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
public sealed partial class MainLayoutViewModel : ViewModelBase, IDisposable
{
    private readonly NavigationManager _navigationManager;

    /// <summary>
    /// Gets the count of navigation events that have occurred since the application started.
    /// </summary>
    /// <value>
    /// An integer representing the total number of times the user has navigated to a different page.
    /// </value>
    [ObservableProperty]
    private int _counter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainLayoutViewModel"/> class.
    /// </summary>
    /// <param name="navigationManager">The Blazor navigation manager to monitor navigation events.</param>
    public MainLayoutViewModel(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// Unsubscribes from the <see cref="NavigationManager.LocationChanged"/> event to prevent memory leaks.
    /// </remarks>
    public void Dispose()
        => _navigationManager.LocationChanged -= OnLocationChanged;

    /// <summary>
    /// Handles the location changed event from the navigation manager.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data containing information about the navigation.</param>
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => Counter++;
}
