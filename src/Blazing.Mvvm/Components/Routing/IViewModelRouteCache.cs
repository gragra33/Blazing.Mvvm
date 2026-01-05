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

    /// <summary>
    /// Gets the cached route template collections for ViewModels, mapping ViewModel <see cref="Type"/> to <see cref="RouteTemplateCollection"/>.
    /// </summary>
    /// <remarks>
    /// This property supports multi-route template navigation, allowing ViewModels with multiple <c>@page</c> directives
    /// to have all their route templates cached and available for smart template selection during navigation.
    /// <para>
    /// When a component has multiple routes (e.g., <c>@page "/test"</c> and <c>@page "/test/{echo}"</c>),
    /// the collection contains all parsed templates along with metadata to select the most appropriate route
    /// based on the navigation parameters provided.
    /// </para>
    /// </remarks>
    IReadOnlyDictionary<Type, RouteTemplateCollection> ViewModelRouteTemplates { get; }

    /// <summary>
    /// Gets the cached route template collections for keyed ViewModels, mapping navigation keys to <see cref="RouteTemplateCollection"/>.
    /// </summary>
    /// <remarks>
    /// This property supports multi-route template navigation for keyed ViewModels, enabling components registered
    /// with a navigation key to define multiple route patterns.
    /// <para>
    /// Similar to <see cref="ViewModelRouteTemplates"/>, this allows smart route selection based on the
    /// navigation parameters provided when navigating by key.
    /// </para>
    /// </remarks>
    IReadOnlyDictionary<object, RouteTemplateCollection> KeyedViewModelRouteTemplates { get; }
}
