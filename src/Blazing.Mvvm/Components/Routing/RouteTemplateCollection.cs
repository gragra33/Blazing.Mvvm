namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Represents a collection of route templates associated with a single ViewModel or keyed ViewModel.
/// </summary>
/// <remarks>
/// When a Blazor component has multiple <c>@page</c> directives, this collection stores all route templates
/// and designates one as the primary route for simple navigation scenarios.
/// </remarks>
public sealed class RouteTemplateCollection
{
    /// <summary>
    /// Gets the primary route pattern used for simple navigation without parameters.
    /// </summary>
    /// <remarks>
    /// The primary route is typically the simplest route (one without parameters, or with the fewest parameters).
    /// It is used as the default when navigating to a ViewModel without specifying route parameters.
    /// </remarks>
    /// <example>
    /// For a component with routes <c>/test</c> and <c>/test/{echo}</c>, 
    /// the primary route would be <c>/test</c>.
    /// </example>
    public required string PrimaryRoute { get; init; }

    /// <summary>
    /// Gets all route templates associated with this ViewModel.
    /// </summary>
    /// <remarks>
    /// This list contains parsed representations of all <c>@page</c> directives defined on the component.
    /// The templates are used during navigation to select the best-matching route based on provided parameters.
    /// </remarks>
    public required List<RouteTemplate> AllRoutes { get; init; }

    /// <summary>
    /// Gets the <see cref="RouteTemplate"/> object corresponding to the <see cref="PrimaryRoute"/> pattern.
    /// </summary>
    /// <remarks>
    /// This is a convenience property that retrieves the primary route's template from <see cref="AllRoutes"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the primary route pattern is not found in <see cref="AllRoutes"/>.
    /// This should never occur if the collection is properly initialized.
    /// </exception>
    public RouteTemplate Primary => AllRoutes.First(r => r.Pattern == PrimaryRoute);
}
