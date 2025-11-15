using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Manages navigation via ViewModel using the route cache, supporting both type-based and key-based navigation in Blazor MVVM applications.
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
    /// <param name="navigationManager">The Blazor navigation manager for handling browser navigation.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="routeCache">The ViewModel route cache containing cached ViewModel-to-route mappings.</param>
    /// <param name="libraryConfiguration">The library configuration containing base path and other settings.</param>
    public MvvmNavigationManager(NavigationManager navigationManager, ILogger<MvvmNavigationManager> logger, IViewModelRouteCache routeCache, IOptions<LibraryConfiguration> libraryConfiguration)
    {
        _navigationManager = navigationManager;
        _logger = logger;
        _routeCache = routeCache;
        _libraryConfiguration = libraryConfiguration;
    }

    /// <summary>
    /// Gets the cached URI for the specified ViewModel type and resolves it for navigation.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type to get the URI for.</typeparam>
    /// <returns>The resolved navigation URI.</returns>
    /// <exception cref="ViewModelRouteNotFoundException">Thrown when the ViewModel type has no associated route.</exception>
    private string GetResolvedUriForViewModel<TViewModel>() where TViewModel : IViewModelBase
    {
        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ViewModelRouteNotFoundException(typeof(TViewModel));
        }

        LogUriResolution(typeof(TViewModel).Name, uriFromCache);
        string resolvedUri = ResolveNavigationUri(uriFromCache);
        LogResolvedUri(typeof(TViewModel).Name, uriFromCache, resolvedUri);
        return resolvedUri;
    }

    /// <summary>
    /// Gets the cached URI for the specified key and resolves it for navigation.
    /// </summary>
    /// <param name="key">The key to get the URI for.</param>
    /// <returns>The resolved navigation URI.</returns>
    /// <exception cref="ViewModelRouteNotFoundException">Thrown when the key has no associated route.</exception>
    private string GetResolvedUriForKey(object key)
    {
        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ViewModelRouteNotFoundException(key);
        }

        LogKeyedUriResolution(key, uriFromCache);
        string resolvedUri = ResolveNavigationUri(uriFromCache);
        LogResolvedKeyedUri(key, uriFromCache, resolvedUri);
        return resolvedUri;
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase
    {
        string resolvedUri = GetResolvedUriForViewModel<TViewModel>();
        LogNavigationEvent(typeof(TViewModel).FullName, resolvedUri);
        _navigationManager.NavigateTo(resolvedUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(BrowserNavigationOptions options)
        where TViewModel : IViewModelBase
    {
        string resolvedUri = GetResolvedUriForViewModel<TViewModel>();
        LogNavigationEvent(typeof(TViewModel).FullName, resolvedUri);
        _navigationManager.NavigateTo(resolvedUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(string relativeUri, bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        string resolvedUri = GetResolvedUriForViewModel<TViewModel>();
        string finalUri = BuildUri(_navigationManager.ToAbsoluteUri(resolvedUri).AbsoluteUri, relativeUri);

        LogNavigationEvent(typeof(TViewModel).FullName, finalUri);
        _navigationManager.NavigateTo(finalUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(string relativeUri, BrowserNavigationOptions options)
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        string resolvedUri = GetResolvedUriForViewModel<TViewModel>();
        string finalUri = BuildUri(_navigationManager.ToAbsoluteUri(resolvedUri).AbsoluteUri, relativeUri);

        LogNavigationEvent(typeof(TViewModel).FullName, finalUri);
        _navigationManager.NavigateTo(finalUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(key);

        string resolvedUri = GetResolvedUriForKey(key);
        LogKeyedNavigationEvent(key, resolvedUri);
        _navigationManager.NavigateTo(resolvedUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, BrowserNavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);

        string resolvedUri = GetResolvedUriForKey(key);
        LogKeyedNavigationEvent(key, resolvedUri);
        _navigationManager.NavigateTo(resolvedUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, string relativeUri, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(relativeUri);

        string resolvedUri = GetResolvedUriForKey(key);
        string finalUri = BuildUri(_navigationManager.ToAbsoluteUri(resolvedUri).AbsoluteUri, relativeUri);

        LogKeyedNavigationEvent(key, finalUri);
        _navigationManager.NavigateTo(finalUri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo(object key, string relativeUri, BrowserNavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(relativeUri);

        string resolvedUri = GetResolvedUriForKey(key);
        string finalUri = BuildUri(_navigationManager.ToAbsoluteUri(resolvedUri).AbsoluteUri, relativeUri);

        LogKeyedNavigationEvent(key, finalUri);
        _navigationManager.NavigateTo(finalUri, CloneNavigationOptions(options));
    }

    /// <inheritdoc/>
    public string GetUri<TViewModel>()
        where TViewModel : IViewModelBase
    {
        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ViewModelRouteNotFoundException(typeof(TViewModel));
        }

        return uriFromCache;
    }

    /// <inheritdoc/>
    public string GetUri(object key)
    {
        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ViewModelRouteNotFoundException(key);
        }

        return uriFromCache;
    }

    #region Internals

    /// <summary>
    /// Resolves a route template to a navigation URI at runtime, handling base path and root path scenarios.
    /// </summary>
    /// <param name="routeTemplate">The route template from the cache to resolve.</param>
    /// <returns>The resolved navigation URI ready for browser navigation.</returns>
    private string ResolveNavigationUri(string routeTemplate)
    {
        LogRouteTemplateResolution(routeTemplate);
        
        // Handle root path
        if (routeTemplate == "/")
        {
            string rootPath = new Uri(_navigationManager.BaseUri).LocalPath;
            LogRootPathResolution(routeTemplate, rootPath);
            return rootPath;
        }

        // Get configured BasePath for subpath hosting scenarios
        string? configuredBasePath = _libraryConfiguration.Value.BasePath?.Trim('/');
        LogBasePath(configuredBasePath);

        // Handle absolute paths
        if (routeTemplate.StartsWith("/"))
        {
            string workingUri = routeTemplate;
            
            // If we have a configured BasePath and the route template starts with it, remove it
            if (!string.IsNullOrEmpty(configuredBasePath))
            {
                string basePathWithSlash = "/" + configuredBasePath;
                if (workingUri.StartsWith(basePathWithSlash + "/", StringComparison.OrdinalIgnoreCase))
                {
                    // Remove BasePath prefix: "/test/counter" -> "/counter"
                    workingUri = workingUri.Substring(basePathWithSlash.Length);
                    LogBasePathRemoval(routeTemplate, basePathWithSlash, workingUri);
                }
                else if (workingUri.Equals(basePathWithSlash, StringComparison.OrdinalIgnoreCase))
                {
                    // BasePath root: "/test" -> "/"
                    workingUri = "/";
                    LogBasePathRootRemoval(routeTemplate, basePathWithSlash, workingUri);
                }
            }
            
            // Convert to relative path by removing leading slash
            if (workingUri.Length > 1 && workingUri.StartsWith("/"))
            {
                string relativePath = workingUri[1..];
                LogRelativePathConversion(workingUri, relativePath);
                return relativePath;
            }
            else if (workingUri == "/")
            {
                LogEmptyRelativePathConversion(workingUri);
                return string.Empty;
            }
        }
        
        LogUnchangedRouteTemplate(routeTemplate);
        return routeTemplate;
    }

    /// <summary>
    /// Builds a complete URI from a base URI and a relative URI or query string.
    /// </summary>
    /// <param name="uri">The base URI to build upon.</param>
    /// <param name="relativeUri">The relative URI or query string to append.</param>
    /// <returns>The complete URI combining the base and relative parts.</returns>
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
    /// Clones browser navigation options for use with the underlying NavigationManager.
    /// </summary>
    /// <param name="options">The browser navigation options to clone.</param>
    /// <returns>A NavigationOptions instance compatible with the NavigationManager.</returns>
    private NavigationOptions CloneNavigationOptions(BrowserNavigationOptions options)
    {
        return new NavigationOptions()
        {
            ForceLoad = options.ForceLoad,
            HistoryEntryState = options.HistoryEntryState,
            ReplaceHistoryEntry = options.ReplaceHistoryEntry
        };
    }

    #region Logging Methods

    /// <summary>
    /// Logs a navigation event for diagnostic purposes.
    /// </summary>
    /// <param name="viewModel">The ViewModel type name being navigated to.</param>
    /// <param name="uri">The URI being navigated to.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Navigating to '{ViewModel}' with uri '{Uri}'")]
    private partial void LogNavigationEvent(string? viewModel, string uri);

    /// <summary>
    /// Logs a keyed navigation event for diagnostic purposes.
    /// </summary>
    /// <param name="key">The key being used for navigation.</param>
    /// <param name="uri">The URI being navigated to.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Navigating to key '{Key}' with uri '{Uri}'")]
    private partial void LogKeyedNavigationEvent(object key, string uri);

    /// <summary>
    /// Logs URI resolution start for ViewModel navigation.
    /// </summary>
    /// <param name="viewModelName">The ViewModel name being resolved.</param>
    /// <param name="cachedUri">The cached URI from route cache.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Resolving URI for ViewModel '{ViewModelName}' from cached route '{CachedUri}'")]
    private partial void LogUriResolution(string viewModelName, string cachedUri);

    /// <summary>
    /// Logs final resolved URI for ViewModel navigation.
    /// </summary>
    /// <param name="viewModelName">The ViewModel name being resolved.</param>
    /// <param name="cachedUri">The cached URI from route cache.</param>
    /// <param name="resolvedUri">The final resolved URI for navigation.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Resolved ViewModel '{ViewModelName}' from '{CachedUri}' to '{ResolvedUri}'")]
    private partial void LogResolvedUri(string viewModelName, string cachedUri, string resolvedUri);

    /// <summary>
    /// Logs URI resolution start for keyed navigation.
    /// </summary>
    /// <param name="key">The key being resolved.</param>
    /// <param name="cachedUri">The cached URI from route cache.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Resolving URI for key '{Key}' from cached route '{CachedUri}'")]
    private partial void LogKeyedUriResolution(object key, string cachedUri);

    /// <summary>
    /// Logs final resolved URI for keyed navigation.
    /// </summary>
    /// <param name="key">The key being resolved.</param>
    /// <param name="cachedUri">The cached URI from route cache.</param>
    /// <param name="resolvedUri">The final resolved URI for navigation.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Resolved key '{Key}' from '{CachedUri}' to '{ResolvedUri}'")]
    private partial void LogResolvedKeyedUri(object key, string cachedUri, string resolvedUri);

    /// <summary>
    /// Logs route template resolution start.
    /// </summary>
    /// <param name="routeTemplate">The route template being resolved.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Resolving route template '{RouteTemplate}'")]
    private partial void LogRouteTemplateResolution(string routeTemplate);

    /// <summary>
    /// Logs configured BasePath for diagnostics.
    /// </summary>
    /// <param name="basePath">The configured BasePath value.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Configured BasePath: '{BasePath}'")]
    private partial void LogBasePath(string? basePath);

    /// <summary>
    /// Logs root path resolution.
    /// </summary>
    /// <param name="routeTemplate">The original route template.</param>
    /// <param name="rootPath">The resolved root path.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Root path '{RouteTemplate}' resolved to '{RootPath}'")]
    private partial void LogRootPathResolution(string routeTemplate, string rootPath);

    /// <summary>
    /// Logs BasePath removal from route template.
    /// </summary>
    /// <param name="originalRoute">The original route template.</param>
    /// <param name="basePath">The BasePath being removed.</param>
    /// <param name="resultingRoute">The route after BasePath removal.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Removed BasePath '{BasePath}' from '{OriginalRoute}' resulting in '{ResultingRoute}'")]
    private partial void LogBasePathRemoval(string originalRoute, string basePath, string resultingRoute);

    /// <summary>
    /// Logs BasePath root removal.
    /// </summary>
    /// <param name="originalRoute">The original route template.</param>
    /// <param name="basePath">The BasePath being removed.</param>
    /// <param name="resultingRoute">The route after BasePath removal.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Removed BasePath root '{BasePath}' from '{OriginalRoute}' resulting in '{ResultingRoute}'")]
    private partial void LogBasePathRootRemoval(string originalRoute, string basePath, string resultingRoute);

    /// <summary>
    /// Logs conversion to relative path.
    /// </summary>
    /// <param name="absolutePath">The absolute path being converted.</param>
    /// <param name="relativePath">The resulting relative path.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Converted absolute path '{AbsolutePath}' to relative path '{RelativePath}'")]
    private partial void LogRelativePathConversion(string absolutePath, string relativePath);

    /// <summary>
    /// Logs conversion of root to empty relative path.
    /// </summary>
    /// <param name="rootPath">The root path being converted.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Converted root path '{RootPath}' to empty relative path")]
    private partial void LogEmptyRelativePathConversion(string rootPath);

    /// <summary>
    /// Logs when route template remains unchanged.
    /// </summary>
    /// <param name="routeTemplate">The unchanged route template.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Route template '{RouteTemplate}' returned unchanged")]
    private partial void LogUnchangedRouteTemplate(string routeTemplate);

    #endregion Logging Methods

    #endregion Internals
}
