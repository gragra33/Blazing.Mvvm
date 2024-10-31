using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Manages navigation via ViewModel.
/// </summary>
public partial class MvvmNavigationManager : IMvvmNavigationManager
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<MvvmNavigationManager> _logger;

    private readonly Dictionary<Type, string> _references = [];
    private readonly Dictionary<string, string> _keyedReferences = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MvvmNavigationManager"/> class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="logger">The logger.</param>
    public MvvmNavigationManager(NavigationManager navigationManager, ILogger<MvvmNavigationManager> logger)
    {
        _navigationManager = navigationManager;
        _logger = logger;

        GenerateReferenceCache();
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        LogNavigationEvent(typeof(TViewModel).FullName, uri);
        _navigationManager.NavigateTo(uri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(NavigationOptions options)
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        LogNavigationEvent(typeof(TViewModel).FullName, uri);
        _navigationManager.NavigateTo(uri, options);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(string relativeUri, bool forceLoad = false, bool replace = false)
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        uri = BuildUri(_navigationManager.ToAbsoluteUri(uri).AbsoluteUri, relativeUri);

        LogNavigationEvent(typeof(TViewModel).FullName, uri);
        _navigationManager.NavigateTo(uri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(string relativeUri, NavigationOptions options)
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        uri = BuildUri(_navigationManager.ToAbsoluteUri(uri).AbsoluteUri, relativeUri);

        LogNavigationEvent(typeof(TViewModel).FullName, uri);
        _navigationManager.NavigateTo(uri, options);
    }

    /// <inheritdoc/>
    public void NavigateTo(string key, bool forceLoad = false, bool replace = false)
    {
        if (!_keyedReferences.TryGetValue(key, out string? uri))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        LogNavigationEvent(key, uri);
        _navigationManager.NavigateTo(uri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo(string key, NavigationOptions options)
    {
        if (!_keyedReferences.TryGetValue(key, out string? uri))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        LogNavigationEvent(key, uri);
        _navigationManager.NavigateTo(uri, options);
    }

    /// <inheritdoc/>
    public void NavigateTo(string key, string relativeUri, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_keyedReferences.TryGetValue(key, out string? uri))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        uri = BuildUri(_navigationManager.ToAbsoluteUri(uri).AbsoluteUri, relativeUri);

        LogNavigationEvent(key, uri);
        _navigationManager.NavigateTo(uri, forceLoad, replace);
    }

    /// <inheritdoc/>
    public void NavigateTo(string key, string relativeUri, NavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(relativeUri);

        if (!_keyedReferences.TryGetValue(key, out string? uri))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        uri = BuildUri(_navigationManager.ToAbsoluteUri(uri).AbsoluteUri, relativeUri);

        LogNavigationEvent(key, uri);
        _navigationManager.NavigateTo(uri, options);
    }

    /// <inheritdoc/>
    public string GetUri<TViewModel>()
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        return uri;
    }

    /// <inheritdoc/>
    public string GetUri(string key)
    {
        if (!_keyedReferences.TryGetValue(key, out string? uri))
        {
            throw new ArgumentException($"No associated page for key '{key}'");
        }

        return uri;
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
    /// Generates a cache of references for navigation.
    /// </summary>
    private void GenerateReferenceCache()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        _logger.LogDebug("Starting generation of a new Reference Cache");

        foreach (Assembly assembly in assemblies)
        {
            List<(Type Type, Type? Argument)> items;

            try
            {
                items = assembly
                    .GetTypes()
                    .Select(GetViewArgumentType)
                    .Where(t => t.Argument is not null)
                    .ToList();
            }
            catch (Exception)
            {
                // avoid issue with unit tests
                continue;
            }

            // does the assembly contain the required types?
            if (items.Count == 0)
            {
                continue;
            }

            foreach ((Type Type, Type? Argument) item in items)
            {
                IEnumerable<ViewModelDefinitionAttribute> vmDefinitionAttributes = item.Type.GetCustomAttributes<ViewModelDefinitionAttribute>();
                ViewModelDefinitionAttribute[] viewModelDefinitionAttributes = vmDefinitionAttributes as ViewModelDefinitionAttribute[] ?? vmDefinitionAttributes.ToArray();

                if (viewModelDefinitionAttributes.Length == 0)
                {
                    Attribute? routeAttribute = item.Type.GetCustomAttributes().FirstOrDefault(a => a is RouteAttribute);
                    if (routeAttribute is null)
                    {
                        continue;
                    }

                    string uri = ((RouteAttribute)routeAttribute).Template;
                    _references.Add(item.Argument!, uri);
                    _logger.LogDebug("Caching navigation reference '{Argument}' with uri '{Uri}' for '{FullName}'", item.Argument, uri, item.Type.FullName);
                    continue;
                }

                foreach (ViewModelDefinitionAttribute vmDefinitionAttribute in viewModelDefinitionAttributes)
                {
                    Attribute? routeAttribute = item.Type.GetCustomAttributes().FirstOrDefault(a => a is RouteAttribute);
 
                    // is this a page or a component?
                    if (routeAttribute is null)
                    {
                        continue;
                    }

                    // we have a page, let's reference it!
                    string uri = ((RouteAttribute)routeAttribute).Template;

                    if (vmDefinitionAttribute.Key is not null)
                    {
                        _keyedReferences.Add(vmDefinitionAttribute.Key!.ToString()!, uri);
                        _logger.LogDebug("Caching keyed navigation reference '{Key}' with uri '{Uri}' for '{FullName}'", vmDefinitionAttribute.Key, uri, item.Type.FullName);
                    }
                    else
                    {
                        _references.Add(item.Argument!, uri);
                        _logger.LogDebug("Caching navigation reference '{Argument}' with uri '{Uri}' for '{FullName}'", item.Argument, uri, item.Type.FullName);
                    }

                }
            }
        }

        _logger.LogDebug("Completed generating the Reference Cache");
    }

    /// <summary>
    /// Logs a navigation event.
    /// </summary>
    /// <param name="viewModel">The ViewModel being navigated to.</param>
    /// <param name="uri">The URI being navigated to.</param>
    [LoggerMessage(LogLevel.Debug, Message = "Navigating '{ViewModel}' to uri '{Uri}'")]
    private partial void LogNavigationEvent(string? viewModel, string uri);

    /// <summary>
    /// Gets the ViewModel argument type for a given type.
    /// </summary>
    /// <param name="type">The type to get the ViewModel argument type for.</param>
    /// <returns>A tuple containing the type and its ViewModel argument type.</returns>
    private static (Type Type, Type? Argument) GetViewArgumentType(Type type)
    {
        Type viewInterfaceType = typeof(IView<>);
        Type viewModelType = typeof(IViewModelBase);
        Type componentBaseGenericType = typeof(MvvmComponentBase<>);
        Type owingComponentBaseGenericType = typeof(MvvmOwningComponentBase<>);
        Type? componentBaseType = null;
        Type? typeArgument = null;

        foreach (Type interfaceType in type.GetInterfaces())
        {
            if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != viewInterfaceType)
            {
                continue;
            }

            typeArgument = interfaceType.GetGenericArguments()[0];
            componentBaseType = componentBaseGenericType.MakeGenericType(typeArgument);

            // Check if the type constraint is a subtype of MvvmComponentBase<>
            if (componentBaseType.IsAssignableFrom(type))
            {
                break;
            }

            // Check if the type constraint is a subtype of MvvmOwningComponentBase<>
            componentBaseType = owingComponentBaseGenericType.MakeGenericType(typeArgument);

            if (componentBaseType.IsAssignableFrom(type))
            {
                break;
            }

            return default;
        }

        if (componentBaseType is null)
        {
            return default;
        }

        // get all interfaces
        Type[] interfaces = componentBaseType
            .GetGenericArguments()[0]
            .GetInterfaces();

        // Check if the type argument of IView<> implements IViewModel
        if (interfaces.FirstOrDefault(i => i.Name == $"{viewModelType.Name}") is null)
        {
            return default;
        }

        // all checks passed, so return the type with the argument type declared
        return (type, typeArgument);
    }

    #endregion Internals
}