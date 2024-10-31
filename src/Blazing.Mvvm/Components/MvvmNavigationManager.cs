using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Provides an abstraction for querying and managing navigation via ViewModel (class/interface).
/// </summary>
public partial class MvvmNavigationManager : IMvvmNavigationManager
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<MvvmNavigationManager> _logger;

    private readonly Dictionary<Type, string> _references = [];

    /// <summary>
    /// Initializes a new instance of <see cref="MvvmNavigationManager"/>.
    /// </summary>
    /// <param name="navigationManager">The</param>
    /// <param name="logger">The logger</param>
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
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
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

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="relativeUri">relative URI or QueryString appended to the navigation Uri.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Throw when <typeparamref name="TViewModel"/> has no associated <c>page</c>.</exception>
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

    /// <summary>
    /// Navigates to the specified associated URI.
    /// </summary>
    /// <typeparam name="TViewModel">The type <see cref="IViewModelBase"/> to use to determine the URI to navigate to.</typeparam>
    /// <param name="relativeUri">relative URI or QueryString appended to the navigation Uri.</param>
    /// <param name="options">Provides additional <see cref="NavigationOptions"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Throw when <typeparamref name="TViewModel"/> has no associated <c>page</c>.</exception>
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
        {
            throw new ArgumentException($"{typeof(TViewModel)} has no associated page");
        }

        return uri;
    }

    #region Internals

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
                Attribute? attribute = item.Type.GetCustomAttributes().FirstOrDefault(a => a is RouteAttribute);

                // is this a page or a component?
                if (attribute is null)
                {
                    continue;
                }

                // we have a page, let's reference it!
                string uri = ((RouteAttribute)attribute).Template;
                _references.Add(item.Argument!, uri);
                _logger.LogDebug("Caching navigation reference '{Argument}' with uri '{Uri}' for '{FullName}'", item.Argument, uri, item.Type.FullName);
            }
        }

        _logger.LogDebug("Completed generating the Reference Cache");
    }

    [LoggerMessage(LogLevel.Debug, Message = "Navigating '{ViewModel}' to uri '{Uri}'")]
    private partial void LogNavigationEvent(string? viewModel, string uri);

    private static (Type Type, Type? Argument) GetViewArgumentType(Type type)
    {
        Type viewInterfaceType = typeof(IView<>);
        Type viewModelType = typeof(IViewModelBase);
        Type componentBaseGenericType = typeof(MvvmComponentBase<>);
        Type owingComponentBaseGenericType = typeof(MvvmOwningComponentBase<>);
        Type? componentBaseType = null;
        Type? typeArgument = null;

        // Find the generic type definition for MvvmComponentBase<> with the correct type argument
        foreach (Type interfaceType in type.GetInterfaces())
        {
            if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != viewInterfaceType)
            {
                continue;
            }

            typeArgument = interfaceType.GetGenericArguments()[0];
            componentBaseType = componentBaseGenericType.MakeGenericType(typeArgument);

            // Check if the type constraint is a subtype of MvvmComponentBase<>
            if (componentBaseType?.IsAssignableFrom(type) == true)
            {
                break;
            }

            // Check if the type constraint is a subtype of MvvmOwningComponentBase<>
            componentBaseType = owingComponentBaseGenericType.MakeGenericType(typeArgument);

            if (componentBaseType?.IsAssignableFrom(type) == true)
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
