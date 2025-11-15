using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// A Blazor component that renders an anchor tag and automatically toggles its 'active' CSS class
/// based on whether its 'href' matches the current URI. Navigation is performed using the associated ViewModel type.
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel used for navigation. Must implement <see cref="IViewModelBase"/>.</typeparam>
public sealed class MvvmNavLink<TViewModel> : MvvmNavLinkBase
    where TViewModel : IViewModelBase
{
    /// <summary>
    /// Resolves the navigation href for the anchor tag using the ViewModel type.
    /// </summary>
    /// <returns>The absolute URI string for navigation.</returns>
    protected override string ResolveHref()
        => NavigationManager.ToAbsoluteUri(MvvmNavigationManager.GetUri<TViewModel>()).AbsoluteUri;
}
