using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Manages navigation via ViewModel.
/// </summary>
public partial class MvvmNavigationManager : IMvvmNavigationManager
{
    private readonly NavigationManager _navigationManager;
    private readonly IViewModelRouteCache _routeCache;
    private readonly ILogger<MvvmNavigationManager> _logger;
    private readonly IOptions<LibraryConfiguration> _libraryConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="MvvmNavigationManager"/> class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="routeCache">The ViewModel route cache.</param>
    /// <param name="libraryConfiguration">The library configuration options.</param>
    public MvvmNavigationManager(NavigationManager navigationManager, ILogger<MvvmNavigationManager> logger, IViewModelRouteCache routeCache, IOptions<LibraryConfiguration> libraryConfiguration)
    {
        _navigationManager = navigationManager;
        _logger = logger;
        _routeCache = routeCache;
        _libraryConfiguration = libraryConfiguration;
    }

    private string AdjustUriFromCache(string cachedUri)
    {
        string? configuredBasePath = _libraryConfiguration.Value.BasePath;

        if (!string.IsNullOrEmpty(configuredBasePath))
        {
            return cachedUri;
        }

        configuredBasePath = "/";

        if (cachedUri.StartsWith(configuredBasePath, StringComparison.OrdinalIgnoreCase))
        {
            string relativePath = cachedUri.Substring(configuredBasePath.Length);
            if (!relativePath.StartsWith("/"))
            {
                relativePath = "/" + relativePath;
            }
            return relativePath;
        }

        //_logger.LogWarning("Cached URI '{CachedUri}' did not start with configured BasePath '{ConfiguredBasePath}'. Returning URI as is. This might lead to incorrect navigation.", cachedUri, configuredBasePath);
        return cachedUri;
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase
    {
        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        string effectiveUri = AdjustUriFromCache(uriFromCache);
        LogNavigationEvent(typeof(TViewModel).FullName, effectiveUri);
        _navigationManager.NavigateTo(effectiveUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(BrowserNavigationOptions options)
        where TViewModel : IViewModelBase
    {
        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }
        
        string effectiveUri = AdjustUriFromCache(uriFromCache);
        LogNavigationEvent(typeof(TViewModel).FullName, effectiveUri);
        _navigationManager.NavigateTo(effectiveUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(string relativeUri, bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        string baseNavigationUri = AdjustUriFromCache(uriFromCache);
        string absoluteBase = _navigationManager.ToAbsoluteUri(baseNavigationUri).AbsoluteUri;
        string finalUri = BuildUri(absoluteBase, relativeUri);

        LogNavigationEvent(typeof(TViewModel).FullName, finalUri);
        _navigationManager.NavigateTo(finalUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(string relativeUri, BrowserNavigationOptions options)
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        string baseNavigationUri = AdjustUriFromCache(uriFromCache);
        string absoluteBase = _navigationManager.ToAbsoluteUri(baseNavigationUri).AbsoluteUri;
        string finalUri = BuildUri(absoluteBase, relativeUri);

        LogNavigationEvent(typeof(TViewModel).FullName, finalUri);
        _navigationManager.NavigateTo(finalUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        string effectiveUri = AdjustUriFromCache(uriFromCache);
        LogKeyedNavigationEvent(key, effectiveUri);
        _navigationManager.NavigateTo(effectiveUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, BrowserNavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        string effectiveUri = AdjustUriFromCache(uriFromCache);
        LogKeyedNavigationEvent(key, effectiveUri);
        _navigationManager.NavigateTo(effectiveUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, string relativeUri, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }
        
        string baseNavigationUri = AdjustUriFromCache(uriFromCache);
        string absoluteBase = _navigationManager.ToAbsoluteUri(baseNavigationUri).AbsoluteUri;
        string finalUri = BuildUri(absoluteBase, relativeUri);

        LogKeyedNavigationEvent(key, finalUri);
        _navigationManager.NavigateTo(finalUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, string relativeUri, BrowserNavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        string baseNavigationUri = AdjustUriFromCache(uriFromCache);
        string absoluteBase = _navigationManager.ToAbsoluteUri(baseNavigationUri).AbsoluteUri;
        string finalUri = BuildUri(absoluteBase, relativeUri);
        
        LogKeyedNavigationEvent(key, finalUri);
        _navigationManager.NavigateTo(finalUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public string GetUri<TViewModel>()
        where TViewModel : IViewModelBase
    {
        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        return AdjustUriFromCache(uriFromCache);
    }

    /// <inheritdoc/>
    public string GetUri(object key)
    {
        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        return AdjustUriFromCache(uriFromCache);
    }

    #region Internals

    /// <summary>
    /// Builds a complete URI from a base URI and a relative URI.
    /// </summary>
    /// <param name="uri">The base URI.</param>
    /// <param name="relativeUri">The relative URI or query string.</param>
    /// <returns>The complete URI.</returns>
    private static string BuildUri(string uri, string relativeUri)
    {
        if (string.IsNullOrWhiteSpace(relativeUri))
        {
            return uri;
        }

        UriBuilder builder = new(uri);

        if (relativeUri.StartsWith('?'))
        {
            builder.Query = relativeUri.TrimStart('?');
        }
        else if (relativeUri.Contains('?'))
        {
            string[] parts = relativeUri.Split('?');

            builder.Path = builder.Path.TrimEnd('/') + "/" + parts[0].TrimStart('/');
            builder.Query = parts[1];
        }
        else
        {
            builder.Path = builder.Path.TrimEnd('/') + "/" + relativeUri.TrimStart('/');
        }

        return builder.Uri.AbsoluteUri;
    }

    /// <summary>
    /// Logs a navigation event.
    /// </summary>
    /// <param name="viewModel">The ViewModel being navigated to.</param>
    /// <param name="uri">The URI being navigated to.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Navigating to '{ViewModel}' with uri '{Uri}'")]
    private partial void LogNavigationEvent(string? viewModel, string uri);

    [LoggerMessage(LogLevel.Debug, Message = "Navigating to key '{Key}' with uri '{Uri}'")]
    private partial void LogKeyedNavigationEvent(object key, string uri);

    private NavigationOptions CloneNavigationOptions(BrowserNavigationOptions options)
    {
        return new NavigationOptions()
        {
            ForceLoad = options.ForceLoad,
            HistoryEntryState = options.HistoryEntryState,
            ReplaceHistoryEntry = options.ReplaceHistoryEntry
        };
    }

    #endregion Internals
}
