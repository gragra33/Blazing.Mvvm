using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Provides an abstraction for querying and managing navigation via ViewModel (class/interface).
/// </summary>
public class MvvmNavigationManager : IMvvmNavigationManager
{

    private readonly NavigationManager _navigationManager;
    private readonly ILogger<MvvmNavigationManager> _logger;

    private readonly Dictionary<Type, string> _references = new();

    public MvvmNavigationManager(NavigationManager navigationManager, ILogger<MvvmNavigationManager> logger)
    {
        _navigationManager = navigationManager;
        _logger = logger;
             
        GenerateReferenceCache();
    }

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
    public void NavigateTo<TViewModel>(bool? forceLoad = false, bool? replace = false)
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"Navigating '{typeof(TViewModel).FullName}' to uri '{uri}'");

        _navigationManager.NavigateTo(uri, (bool)forceLoad!, (bool)replace!);

    }

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
    public void NavigateTo<TViewModel>(NavigationOptions options)
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"Navigating '{typeof(TViewModel).FullName}' to uri '{uri}'");

        _navigationManager.NavigateTo(uri, options);
    }

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="relativeUri">relative URI &/or QueryString appended to the navigation Uri.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
    public void NavigateTo<TViewModel>(string? relativeUri = null, bool? forceLoad = false, bool? replace = false)
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");

        //string navUri = relativeUri is null ? uri : uri + relativeUri;
        
        uri = BuildUri(_navigationManager.ToAbsoluteUri(uri).AbsoluteUri, relativeUri);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"Navigating '{typeof(TViewModel).FullName}' to uri '{uri}'");

        _navigationManager.NavigateTo(uri, (bool)forceLoad!, (bool)replace!);
    }

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="relativeUri">relative URI &/or QueryString appended to the navigation Uri.</param>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
    public void NavigateTo<TViewModel>(string relativeUri, NavigationOptions options)
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");

        uri = BuildUri(_navigationManager.ToAbsoluteUri(uri).AbsoluteUri, relativeUri);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"Navigating '{typeof(TViewModel).FullName}' to uri '{uri}'");

        _navigationManager.NavigateTo(uri, options);
    }

    /// <summary>
    /// Get the <see cref="IViewModelBase"/> associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <returns>A relative URI path.</returns>
    /// <exception cref="ArgumentException"></exception>
    public string GetUri<TViewModel>()
        where TViewModel : IViewModelBase
    {
        if (!_references.TryGetValue(typeof(TViewModel), out string? uri))
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");

        return uri;
    }

    #region Internals
    
    private static string BuildUri(string uri, string? relativeUri)
    {
        if (string.IsNullOrWhiteSpace(relativeUri))
            return uri;

        UriBuilder builder = new(uri);

        if (relativeUri.StartsWith('?'))
            builder.Query = relativeUri.TrimStart('?');

        else if (relativeUri.Contains('?'))
        {
            string[] parts = relativeUri.Split('?');

            builder.Path = builder.Path.TrimEnd('/') + "/" + parts[0].TrimStart('/');
            builder.Query = parts[1];
        }

        else
            builder.Path = builder.Path.TrimEnd('/') + "/" + relativeUri.TrimStart('/');

        return builder.ToString();
    }

    private void GenerateReferenceCache()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (_logger.IsEnabled(LogLevel.Debug))
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
            if (!items.Any())
                continue;

            foreach ((Type Type, Type? Argument) item in items)
            {
                Attribute? attribute = item.Type.GetCustomAttributes().FirstOrDefault(a => a is RouteAttribute);

                // is this a page or a component?
                if (attribute is null)
                    continue;

                // we have a page, let's reference it!
                string uri = ((RouteAttribute)attribute).Template;
                _references.Add(item.Argument!, uri);

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Caching navigation reference '{item.Argument!}' with uri '{uri}' for '{item.Type.FullName}'");
            }
        }

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Completed generating the Reference Cache");
    }

    private static (Type Type, Type? Argument) GetViewArgumentType(Type type)
    {
        Type viewInterfaceType = typeof(IView<>);
        Type viewModelType = typeof(IViewModelBase);
        Type ComponentBaseGenericType = typeof(MvvmComponentBase<>);
        Type? ComponentBaseType = null;
        Type? typeArgument = null;

        // Find the generic type definition for MvvmComponentBase<> with the correct type argument
        foreach (Type interfaceType in type.GetInterfaces())
        {
            if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != viewInterfaceType)
                continue;

            typeArgument = interfaceType.GetGenericArguments()[0];
            ComponentBaseType = ComponentBaseGenericType.MakeGenericType(typeArgument);
            break;
        }

        if (ComponentBaseType == null)
            return default;

        // Check if the type constraint is a subtype of MvvmComponentBase<>
        if (!ComponentBaseType.IsAssignableFrom(type))
            return default;

        // get all interfaces
        Type[] interfaces = ComponentBaseType
            .GetGenericArguments()[0]
            .GetInterfaces();

        // Check if the type argument of IView<> implements IViewModel
        if (interfaces.FirstOrDefault(i => i.Name == $"{viewModelType.Name}") is null)
            return default;

        // all checks passed, so return the type with the argument type declared 
        return (type, typeArgument);
    }

    #endregion
}