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
    private readonly RouteTemplateSelector _routeTemplateSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="MvvmNavigationManager"/> class.
    /// </summary>
    /// <param name="navigationManager">The Blazor navigation manager for handling browser navigation.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="routeCache">The ViewModel route cache containing cached ViewModel-to-route mappings.</param>
    /// <param name="libraryConfiguration">The library configuration containing base path and other settings.</param>
    /// <param name="routeTemplateSelector">The route template selector for choosing the best route template based on parameters.</param>
    public MvvmNavigationManager(NavigationManager navigationManager, ILogger<MvvmNavigationManager> logger, IViewModelRouteCache routeCache, IOptions<LibraryConfiguration> libraryConfiguration, RouteTemplateSelector routeTemplateSelector)
    {
        _navigationManager = navigationManager;
        _logger = logger;
        _routeCache = routeCache;
        _libraryConfiguration = libraryConfiguration;
        _routeTemplateSelector = routeTemplateSelector;
    }

    /// <summary>
    /// Gets the cached URI for the specified ViewModel type and resolves it for navigation.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type to get the URI for.</typeparam>
    /// <param name="parameters">Optional route parameters to substitute into the template.</param>
    /// <returns>The resolved navigation URI.</returns>
    /// <exception cref="ViewModelRouteNotFoundException">Thrown when the ViewModel type has no associated route.</exception>
    private string GetResolvedUriForViewModel<TViewModel>(string? parameters = null) where TViewModel : IViewModelBase
    {
        // Try multi-route template support first (v1.0 feature)
        if (_libraryConfiguration.Value.EnableMultiRouteTemplates && 
            _routeCache.ViewModelRouteTemplates.TryGetValue(typeof(TViewModel), out var templates))
        {
            RouteTemplate selectedTemplate = _routeTemplateSelector.SelectBestTemplate(templates, parameters);
            LogRouteTemplateSelected(typeof(TViewModel).Name, selectedTemplate.Pattern, templates.AllRoutes.Count);
            
            string selectedUri = ResolveNavigationUri(selectedTemplate.Pattern, parameters);
            LogResolvedUri(typeof(TViewModel).Name, selectedTemplate.Pattern, selectedUri);
            return selectedUri;
        }
        
        // Fallback to legacy single-route cache (backward compatibility)
        if (!_routeCache.ViewModelRoutes.TryGetValue(typeof(TViewModel), out string? uriFromCache))
        {
            throw new ViewModelRouteNotFoundException(typeof(TViewModel));
        }

        LogUriResolution(typeof(TViewModel).Name, uriFromCache);
        string resolvedUri = ResolveNavigationUri(uriFromCache, parameters);
        LogResolvedUri(typeof(TViewModel).Name, uriFromCache, resolvedUri);
        return resolvedUri;
    }

    /// <summary>
    /// Gets the cached URI for the specified key and resolves it for navigation.
    /// </summary>
    /// <param name="key">The key to get the URI for.</param>
    /// <param name="parameters">Optional route parameters to substitute into the template.</param>
    /// <returns>The resolved navigation URI.</returns>
    /// <exception cref="ViewModelRouteNotFoundException">Thrown when the key has no associated route.</exception>
    private string GetResolvedUriForKey(object key, string? parameters = null)
    {
        // Try multi-route template support first (v1.0 feature)
        if (_libraryConfiguration.Value.EnableMultiRouteTemplates && 
            _routeCache.KeyedViewModelRouteTemplates.TryGetValue(key, out var templates))
        {
            RouteTemplate selectedTemplate = _routeTemplateSelector.SelectBestTemplate(templates, parameters);
            LogKeyedRouteTemplateSelected(key, selectedTemplate.Pattern, templates.AllRoutes.Count);
            
            string selectedUri = ResolveNavigationUri(selectedTemplate.Pattern, parameters);
            LogResolvedKeyedUri(key, selectedTemplate.Pattern, selectedUri);
            return selectedUri;
        }
        
        // Fallback to legacy single-route cache (backward compatibility)
        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ViewModelRouteNotFoundException(key);
        }

        LogKeyedUriResolution(key, uriFromCache);
        string resolvedUri = ResolveNavigationUri(uriFromCache, parameters);
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

        // Check if relativeUri is query string only (starts with ?)
        if (relativeUri.StartsWith('?'))
        {
            // Query string only - get base URI and append query string
            string resolvedUri = GetResolvedUriForViewModel<TViewModel>();
            string finalUri = BuildUri(_navigationManager.ToAbsoluteUri(resolvedUri).AbsoluteUri, relativeUri);
            
            LogNavigationEvent(typeof(TViewModel).FullName, finalUri);
            _navigationManager.NavigateTo(finalUri, forceLoad, replace);
        }
        else
        {
            // Path segments (with or without query string) - let ResolveNavigationUri handle parameter substitution
            string resolvedUri = GetResolvedUriForViewModel<TViewModel>(relativeUri);
            
            LogNavigationEvent(typeof(TViewModel).FullName, resolvedUri);
            _navigationManager.NavigateTo(resolvedUri, forceLoad, replace);
        }
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(string relativeUri, BrowserNavigationOptions options)
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        // Check if relativeUri is query string only (starts with ?)
        if (relativeUri.StartsWith('?'))
        {
            // Query string only - get base URI and append query string
            string resolvedUri = GetResolvedUriForViewModel<TViewModel>();
            string finalUri = BuildUri(_navigationManager.ToAbsoluteUri(resolvedUri).AbsoluteUri, relativeUri);

            LogNavigationEvent(typeof(TViewModel).FullName, finalUri);
            _navigationManager.NavigateTo(finalUri, CloneNavigationOptions(options));
        }
        else
        {
            // Path segments (with or without query string) - let ResolveNavigationUri handle parameter substitution
            string resolvedUri = GetResolvedUriForViewModel<TViewModel>(relativeUri);

            LogNavigationEvent(typeof(TViewModel).FullName, resolvedUri);
            _navigationManager.NavigateTo(resolvedUri, CloneNavigationOptions(options));
        }
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

        return ResolveNavigationUri(uriFromCache); //uriFromCache;
    }

    /// <inheritdoc/>
    public string GetUri(object key)
    {
        if (!_routeCache.KeyedViewModelRoutes.TryGetValue(key, out string? uriFromCache))
        {
            throw new ViewModelRouteNotFoundException(key);
        }

        return ResolveNavigationUri(uriFromCache); //uriFromCache;
    }

    #region Internals

    /// <summary>
    /// Gets the base path for URI resolution, supporting both configured and dynamic detection.
    /// </summary>
    /// <returns>The base path to use for route resolution, or null if hosting at root.</returns>
    private string? GetBasePathForResolution()
    {
        // 1. Check for explicitly configured BasePath (backward compatibility)
#pragma warning disable CS0618 // Type or member is obsolete
        string? configuredBasePath = _libraryConfiguration.Value.BasePath?.Trim('/');
#pragma warning restore CS0618 // Type or member is obsolete
        if (!string.IsNullOrEmpty(configuredBasePath))
        {
            LogConfiguredBasePath(configuredBasePath);
            return configuredBasePath;
        }
        
        // 2. Extract from NavigationManager.BaseUri (dynamic YARP support)
        // Guard: Check if NavigationManager is initialized before accessing BaseUri
        try
        {
            var baseUri = new Uri(_navigationManager.BaseUri);
            string localPath = baseUri.LocalPath.Trim('/');
            string? dynamicBasePath = string.IsNullOrEmpty(localPath) ? null : localPath;
            
            if (dynamicBasePath != null)
            {
                LogDynamicBasePath(dynamicBasePath);
            }
            
            return dynamicBasePath;
        }
        catch (InvalidOperationException)
        {
            // NavigationManager not yet initialized - return null to use default behavior
            LogNavigationManagerNotInitialized();
            return null;
        }
    }

    /// <summary>
    /// Resolves a route template to a navigation URI at runtime, handling base path and root path scenarios.
    /// </summary>
    /// <param name="routeTemplate">The route template from the cache to resolve.</param>
    /// <param name="parameters">Optional route parameters to substitute into the template.</param>
    /// <returns>The resolved navigation URI ready for browser navigation.</returns>
    private string ResolveNavigationUri(string routeTemplate, string? parameters = null)
    {
        LogRouteTemplateResolution(routeTemplate);
        
        // Substitute route parameters if provided
        string workingTemplate = routeTemplate;
        if (!string.IsNullOrWhiteSpace(parameters))
        {
            workingTemplate = SubstituteRouteParameters(routeTemplate, parameters);
            LogRouteParameterSubstitution(routeTemplate, parameters, workingTemplate);
        }
        
        // Handle root path
        if (workingTemplate == "/")
        {
            string rootPath = new Uri(_navigationManager.BaseUri).LocalPath;
            LogRootPathResolution(workingTemplate, rootPath);
            return rootPath;
        }

        // Get base path for subpath hosting scenarios (configured or dynamic)
        string? basePath = GetBasePathForResolution();

        // Handle absolute paths
        if (workingTemplate.StartsWith("/"))
        {
            string workingUri = workingTemplate;
            
            // If we have a BasePath and the route template starts with it, remove it
            if (!string.IsNullOrEmpty(basePath))
            {
                string basePathWithSlash = "/" + basePath;
                if (workingUri.StartsWith(basePathWithSlash + "/", StringComparison.OrdinalIgnoreCase))
                {
                    // Remove BasePath prefix: "/test/counter" -> "/counter"
                    workingUri = workingUri.Substring(basePathWithSlash.Length);
                    LogBasePathRemoval(workingTemplate, basePathWithSlash, workingUri);
                }
                else if (workingUri.Equals(basePathWithSlash, StringComparison.OrdinalIgnoreCase))
                {
                    // BasePath root: "/test" -> "/"
                    workingUri = "/";
                    LogBasePathRootRemoval(workingTemplate, basePathWithSlash, workingUri);
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
        
        LogUnchangedRouteTemplate(workingTemplate);
        return workingTemplate;
    }

    /// <summary>
    /// Substitutes route parameters in a route template with actual values.
    /// Supports both single and multiple route parameters, with optional query string.
    /// </summary>
    /// <param name="routeTemplate">The route template containing parameter placeholders (e.g., "/accessions/{AccessionNumber}").</param>
    /// <param name="parameterValues">The parameter value(s) to substitute. Can be a single value, slash-separated values, or values with query string.</param>
    /// <returns>The route template with parameters substituted and query string appended if present.</returns>
    /// <remarks>
    /// Examples:
    /// - Template: "/accessions/{id}", Value: "123" → "/accessions/123"
    /// - Template: "/users/{userId}/posts/{postId}", Value: "42/789" → "/users/42/posts/789"
    /// - Template: "/items/{id}", Value: "123?test=value" → "/items/123?test=value"
    /// - Template: "/users/{userId}/posts/{postId}", Value: "42/789?test=value" → "/users/42/posts/789?test=value"
    /// </remarks>
    private static string SubstituteRouteParameters(string routeTemplate, string parameterValues)
    {
        // Separate path parameters from query string if present
        string pathPart = parameterValues;
        string? queryPart = null;
        
        int queryIndex = parameterValues.IndexOf('?');
        if (queryIndex >= 0)
        {
            pathPart = parameterValues.Substring(0, queryIndex);
            queryPart = parameterValues.Substring(queryIndex); // includes the '?'
        }
        
        // Find all parameter placeholders in the template
        var parameterPattern = new System.Text.RegularExpressions.Regex(@"\{([^}]+)\}");
        var matches = parameterPattern.Matches(routeTemplate);
        
        if (matches.Count == 0)
        {
            // No parameters in template, return as-is (with query string if present)
            return queryPart != null ? routeTemplate + queryPart : routeTemplate;
        }

        // Split parameter values by '/' for multiple parameters
        string[] values = pathPart.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        string result = routeTemplate;
        int valueIndex = 0;
        
        // Replace each parameter placeholder with its corresponding value
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (valueIndex < values.Length)
            {
                // Replace the entire placeholder including braces
                result = result.Replace(match.Value, values[valueIndex]);
                valueIndex++;
            }
            else
            {
                // Not enough parameter values provided - log warning but continue
                break;
            }
        }
        
        // Append query string if present
        if (queryPart != null)
        {
            result += queryPart;
        }
        
        return result;
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
    /// Logs configured BasePath being used for resolution.
    /// </summary>
    /// <param name="basePath">The configured BasePath value.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Using configured BasePath: '{BasePath}'")]
    private partial void LogConfiguredBasePath(string basePath);

    /// <summary>
    /// Logs dynamically detected BasePath from NavigationManager.
    /// </summary>
    /// <param name="basePath">The dynamically detected BasePath value.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Detected dynamic BasePath from NavigationManager: '{BasePath}'")]
    private partial void LogDynamicBasePath(string basePath);

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

    /// <summary>
    /// Logs route parameter substitution.
    /// </summary>
    /// <param name="originalTemplate">The original route template with placeholders.</param>
    /// <param name="parameters">The parameter values being substituted.</param>
    /// <param name="result">The resulting route after substitution.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Substituted route parameters in '{OriginalTemplate}' with values '{Parameters}' resulting in '{Result}'")]
    private partial void LogRouteParameterSubstitution(string originalTemplate, string parameters, string result);

    /// <summary>
    /// Logs when NavigationManager is not yet initialized - using default behavior as fallback.
    /// </summary>
    [LoggerMessage(LogLevel.Debug, Message = "NavigationManager not yet initialized - using default behavior")]
    private partial void LogNavigationManagerNotInitialized();

    /// <summary>
    /// Logs route template selection for ViewModel navigation.
    /// </summary>
    /// <param name="viewModelName">The ViewModel name being navigated to.</param>
    /// <param name="selectedTemplate">The selected route template pattern.</param>
    /// <param name="totalTemplates">The total number of available templates.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Selected route template '{SelectedTemplate}' for ViewModel '{ViewModelName}' from {TotalTemplates} available templates")]
    private partial void LogRouteTemplateSelected(string viewModelName, string selectedTemplate, int totalTemplates);

    /// <summary>
    /// Logs route template selection for keyed navigation.
    /// </summary>
    /// <param name="key">The key being navigated to.</param>
    /// <param name="selectedTemplate">The selected route template pattern.</param>
    /// <param name="totalTemplates">The total number of available templates.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Selected route template '{SelectedTemplate}' for key '{Key}' from {TotalTemplates} available templates")]
    private partial void LogKeyedRouteTemplateSelected(object key, string selectedTemplate, int totalTemplates);

    #endregion Logging Methods

    #endregion Internals
}
