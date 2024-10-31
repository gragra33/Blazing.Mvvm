using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Provides an abstraction for querying and managing navigation via ViewModel (class/interface).
/// </summary>
public interface IMvvmNavigationManager
{
    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
    void NavigateTo<TViewModel>(bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
    void NavigateTo<TViewModel>(NavigationOptions options)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="relativeUri">relative URI or QueryString appended to the navigation Uri.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
    void NavigateTo<TViewModel>(string relativeUri, bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="relativeUri">relative URI or QueryString appended to the navigation Uri.</param>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
    void NavigateTo<TViewModel>(string relativeUri, NavigationOptions options)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the specified associated URI using a key.
    /// </summary>
    /// <param name="key">The key to use to determine the URI to navigate to.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack.</param>
    void NavigateTo(string key, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Navigates to the specified associated URI using a key.
    /// </summary>
    /// <param name="key">The key to use to determine the URI to navigate to.</param>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
    public void NavigateTo(string key, NavigationOptions options);

    /// <summary>
    /// Navigates to the specified associated URI using a key.
    /// </summary>
    /// <param name="key">The key to use to determine the URI to navigate to.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server.</param>
    /// <param name="relativeUri">relative URI or QueryString appended to the navigation Uri.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack.</param>
    void NavigateTo(string key, string relativeUri, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Navigates to the specified associated URI using a key.
    /// </summary>
    /// <param name="key">The key to use to determine the URI to navigate to.</param>
    /// <param name="relativeUri">relative URI or QueryString appended to the navigation Uri.</param>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
    void NavigateTo(string key, string relativeUri, NavigationOptions options);

    /// <summary>
    /// Get the <see cref="IViewModelBase"/> associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <returns>A relative URI path.</returns>
    /// <exception cref="ArgumentException"></exception>
    string GetUri<TViewModel>() where TViewModel : IViewModelBase;

    /// <summary>
    /// Get the URI associated with the specified key.
    /// </summary>
    /// <param name="key">The key to use to determine the URI to navigate to.</param>
    /// <returns>A relative URI path.</returns>
    /// <exception cref="ArgumentException"></exception>
    string GetUri(string key);
}
