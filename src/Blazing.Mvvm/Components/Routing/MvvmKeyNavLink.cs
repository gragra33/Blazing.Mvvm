using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// A component that renders an anchor tag, automatically toggling its 'active'
/// class based on whether its 'href' matches the current URI. Navigation is based on ViewModel (class/interface).
/// </summary>
public sealed class MvvmKeyNavLink : MvvmNavLinkBase
{
    /// <summary>
    /// The key to use for keyed navigation.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required object NavigationKey { get; set; }

    /// <inheritdoc />
    protected override string ResolveHref()
    {
        if (NavigationKey is null)
        {
            throw new InvalidOperationException($"The {nameof(NavigationKey)} parameter must be specified.");
        }

        return NavigationManager.ToAbsoluteUri(MvvmNavigationManager.GetUri(NavigationKey)).AbsoluteUri;
    }
}
