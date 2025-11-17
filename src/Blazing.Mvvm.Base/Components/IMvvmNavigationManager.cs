using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Provides an abstraction for querying and managing navigation via ViewModel (class/interface) in Blazor MVVM applications.
/// Supports navigation by ViewModel type or key, with options for force load, history replacement, and custom browser navigation options.
/// </summary>
public interface IMvvmNavigationManager
{
    /// <summary>
    /// Navigates to the URI associated with the specified <typeparamref name="TViewModel"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The type implementing <see cref="IViewModelBase"/> to determine the navigation URI.</typeparam>
    /// <param name="forceLoad">If <c>true</c>, bypasses client-side routing and forces the browser to load the new page from the server.</param>
    /// <param name="replace">If <c>true</c>, replaces the current entry in the history stack; otherwise, appends a new entry.</param>
    void NavigateTo<TViewModel>(bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the URI associated with the specified <typeparamref name="TViewModel"/> using browser navigation options.
    /// </summary>
    /// <typeparam name="TViewModel">The type implementing <see cref="IViewModelBase"/> to determine the navigation URI.</typeparam>
    /// <param name="options">Additional <see cref="BrowserNavigationOptions"/> for navigation.</param>
    void NavigateTo<TViewModel>(BrowserNavigationOptions options)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the URI associated with the specified <typeparamref name="TViewModel"/>, appending a relative URI or query string.
    /// </summary>
    /// <typeparam name="TViewModel">The type implementing <see cref="IViewModelBase"/> to determine the navigation URI.</typeparam>
    /// <param name="relativeUri">The relative URI or query string to append.</param>
    /// <param name="forceLoad">If <c>true</c>, bypasses client-side routing and forces the browser to load the new page from the server.</param>
    /// <param name="replace">If <c>true</c>, replaces the current entry in the history stack.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
    void NavigateTo<TViewModel>(string relativeUri, bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the URI associated with the specified <typeparamref name="TViewModel"/>, appending a relative URI or query string and using browser navigation options.
    /// </summary>
    /// <typeparam name="TViewModel">The type implementing <see cref="IViewModelBase"/> to determine the navigation URI.</typeparam>
    /// <param name="relativeUri">The relative URI or query string to append.</param>
    /// <param name="options">Additional <see cref="BrowserNavigationOptions"/> for navigation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
    void NavigateTo<TViewModel>(string relativeUri, BrowserNavigationOptions options)
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Navigates to the URI associated with the specified key.
    /// </summary>
    /// <param name="key">The key used to determine the navigation URI.</param>
    /// <param name="forceLoad">If <c>true</c>, bypasses client-side routing and forces the browser to load the new page from the server.</param>
    /// <param name="replace">If <c>true</c>, replaces the current entry in the history stack.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    void NavigateTo(object key, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Navigates to the URI associated with the specified key using browser navigation options.
    /// </summary>
    /// <param name="key">The key used to determine the navigation URI.</param>
    /// <param name="options">Additional <see cref="BrowserNavigationOptions"/> for navigation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    void NavigateTo(object key, BrowserNavigationOptions options);

    /// <summary>
    /// Navigates to the URI associated with the specified key, appending a relative URI or query string.
    /// </summary>
    /// <param name="key">The key used to determine the navigation URI.</param>
    /// <param name="relativeUri">The relative URI or query string to append.</param>
    /// <param name="forceLoad">If <c>true</c>, bypasses client-side routing and forces the browser to load the new page from the server.</param>
    /// <param name="replace">If <c>true</c>, replaces the current entry in the history stack.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
    void NavigateTo(object key, string relativeUri, bool forceLoad = false, bool replace = false);

    /// <summary>
    /// Navigates to the URI associated with the specified key, appending a relative URI or query string and using browser navigation options.
    /// </summary>
    /// <param name="key">The key used to determine the navigation URI.</param>
    /// <param name="relativeUri">The relative URI or query string to append.</param>
    /// <param name="options">Additional <see cref="BrowserNavigationOptions"/> for navigation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
    void NavigateTo(object key, string relativeUri, BrowserNavigationOptions options);

    /// <summary>
    /// Gets the URI associated with the specified <typeparamref name="TViewModel"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The type implementing <see cref="IViewModelBase"/> to determine the navigation URI.</typeparam>
    /// <returns>A relative URI path for navigation.</returns>
    /// <exception cref="ArgumentException">Thrown when the ViewModel type has no associated route.</exception>
    string GetUri<TViewModel>()
        where TViewModel : IViewModelBase;

    /// <summary>
    /// Gets the URI associated with the specified key.
    /// </summary>
    /// <param name="key">The key used to determine the navigation URI.</param>
    /// <returns>A relative URI path for navigation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    string GetUri(object key);
}
