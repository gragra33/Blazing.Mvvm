using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// A Blazor component that renders an anchor tag and automatically toggles its 'active' CSS class
/// based on whether its 'href' matches the current URI. Navigation is performed using a navigation key.
/// </summary>
public sealed class MvvmKeyNavLink : MvvmNavLinkBase
{
    /// <summary>
    /// Gets or sets the key to use for keyed navigation. This parameter is required.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required object NavigationKey { get; set; }

    /// <summary>
    /// Resolves the navigation href for the anchor tag using the specified navigation key.
    /// </summary>
    /// <returns>The absolute URI string for navigation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="NavigationKey"/> is <c>null</c>.</exception>
    protected override string ResolveHref()
    {
        if (NavigationKey is null)
        {
            throw new InvalidOperationException($"The {nameof(NavigationKey)} parameter must be specified.");
        }

        return NavigationManager.ToAbsoluteUri(MvvmNavigationManager.GetUri(NavigationKey)).AbsoluteUri;
    }
}
