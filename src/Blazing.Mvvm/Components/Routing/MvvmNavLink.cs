using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// A component that renders an anchor tag, automatically toggling its 'active'
/// class based on whether its 'href' matches the current URI. Navigation is based on ViewModel (class/interface).
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel.</typeparam>
public sealed class MvvmNavLink<TViewModel> : MvvmNavLinkBase
    where TViewModel : IViewModelBase
{
    /// <inheritdoc />
    protected override string ResolveHref()
        => NavigationManager.ToAbsoluteUri(MvvmNavigationManager.GetUri<TViewModel>()).AbsoluteUri;
}
