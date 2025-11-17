namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Defines the contract for a cache that stores mappings between ViewModel types or keys and their associated route URIs.
/// </summary>
public interface IViewModelRouteCache
{
    /// <summary>
    /// Gets the cached routes for ViewModels, mapping ViewModel <see cref="Type"/> to route URI <see cref="string"/>.
    /// </summary>
    IReadOnlyDictionary<Type, string> ViewModelRoutes { get; }

    /// <summary>
    /// Gets the cached routes for keyed ViewModels, mapping navigation keys to route URI <see cref="string"/>.
    /// </summary>
    IReadOnlyDictionary<object, string> KeyedViewModelRoutes { get; }
}
