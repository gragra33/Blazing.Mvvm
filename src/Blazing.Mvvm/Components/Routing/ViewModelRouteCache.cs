using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Provides a singleton cache for mapping ViewModel types and keys to their associated route URIs in a Blazor MVVM application.
/// </summary>
/// <remarks>
/// This cache is used to efficiently resolve navigation routes for ViewModels and keyed ViewModels, supporting both type-based and key-based navigation.
/// </remarks>
public class ViewModelRouteCache : IViewModelRouteCache
{
    private readonly ILogger<ViewModelRouteCache> _logger;
    private readonly LibraryConfiguration _configuration; 
    private readonly Dictionary<Type, string> _viewModelRoutes = [];
    private readonly Dictionary<object, string> _keyedViewModelRoutes = [];

    /// <inheritdoc />
    /// <summary>
    /// Gets the cached routes for ViewModels, mapping ViewModel types to route URIs.
    /// </summary>
    public IReadOnlyDictionary<Type, string> ViewModelRoutes => _viewModelRoutes;

    /// <inheritdoc />
    /// <summary>
    /// Gets the cached routes for keyed ViewModels, mapping keys to route URIs.
    /// </summary>
    public IReadOnlyDictionary<object, string> KeyedViewModelRoutes => _keyedViewModelRoutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelRouteCache"/> class, sets up logging and configuration, and generates the reference cache.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic messages.</param>
    /// <param name="configuration">The library configuration containing assemblies and base path.</param>
    public ViewModelRouteCache(ILogger<ViewModelRouteCache> logger, LibraryConfiguration configuration) 
    {
        _logger = logger;
        _configuration = configuration; 
        GenerateReferenceCache(configuration.ViewModelAssemblies);
    }

    /// <summary>
    /// Generates the cache of ViewModel to route mappings by scanning the provided assemblies for Views and their associated route attributes.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for ViewModel route mappings.</param>
    private void GenerateReferenceCache(IEnumerable<Assembly> assemblies)
    {
        _logger.LogDebug("Starting generation of a new Reference Cache for ViewModelRouteCache");
        
        var assemblyList = assemblies.ToList();
        _logger.LogDebug("Scanning {AssemblyCount} assemblies for ViewModel route mappings", assemblyList.Count);
        
        foreach (var assembly in assemblyList)
        {
            _logger.LogDebug("Scanning assembly: {AssemblyName}", assembly.FullName);
        }

        foreach (Assembly assembly in assemblyList)
        {
            List<(Type Type, Type? Argument)> items;

            try
            {
                items = assembly
                    .GetTypes()
                    .Select(GetViewArgumentType)
                    .Where(t => t.Argument is not null)
                    .ToList();
                    
                _logger.LogDebug("Found {ViewCount} Views with ViewModels in assembly {AssemblyName}", items.Count, assembly.GetName().Name);
            }
            catch (Exception ex)
            {
                // Log and continue, or handle as appropriate for your library
                _logger.LogWarning(ex, "Error processing assembly {AssemblyName} for route caching.", assembly.FullName);
                continue;
            }

            if (items.Count == 0)
            {
                _logger.LogDebug("No Views with ViewModels found in assembly {AssemblyName}", assembly.GetName().Name);
                continue;
            }

            foreach ((Type Type, Type? Argument) item in items)
            {
                RouteAttribute? routeAttribute = item.Type.GetCustomAttributes<RouteAttribute>().FirstOrDefault();

                if (routeAttribute is null)
                {
                    _logger.LogDebug("View {ViewType} does not have a RouteAttribute", item.Type.FullName);
                    continue;
                }

                string uri = routeAttribute.Template;
                
                if (item.Argument != null && _viewModelRoutes.TryAdd(item.Argument, uri))
                {
                    _logger.LogDebug("Caching navigation reference '{Argument}' with uri '{Uri}' for '{FullName}'", item.Argument, uri, item.Type.FullName);
                }
                else if (item.Argument != null && _viewModelRoutes.ContainsKey(item.Argument))
                {
                    _logger.LogWarning("Duplicate ViewModel type {ViewModelType} found. Existing route: '{ExistingRoute}', Ignored route: '{NewRoute}'", 
                        item.Argument.FullName, _viewModelRoutes[item.Argument], uri);
                }

                ViewModelKeyAttribute? vmKeyAttribute = item.Type.GetCustomAttribute<ViewModelKeyAttribute>();
                if (vmKeyAttribute?.Key != null && _keyedViewModelRoutes.TryAdd(vmKeyAttribute.Key, uri))
                {
                    _logger.LogDebug("Caching keyed navigation reference '{Key}' with uri '{Uri}' for '{FullName}'", vmKeyAttribute.Key, uri, item.Type.FullName);
                }
                else if (vmKeyAttribute?.Key != null && _keyedViewModelRoutes.ContainsKey(vmKeyAttribute.Key))
                {
                    _logger.LogWarning("Duplicate ViewModel key {ViewModelKey} found. Existing route: '{ExistingRoute}', Ignored route: '{NewRoute}'", 
                        vmKeyAttribute.Key, _keyedViewModelRoutes[vmKeyAttribute.Key], uri);
                }
            }
        }
        
        _logger.LogDebug("Completed generating the Reference Cache for ViewModelRouteCache. Total ViewModels cached: {ViewModelCount}, Total keyed ViewModels cached: {KeyedViewModelCount}", 
            _viewModelRoutes.Count, _keyedViewModelRoutes.Count);
    }

    /// <summary>
    /// Gets the ViewModel type argument from a View type if it implements <see cref="IView{TViewModel}"/> and is assignable to a supported component base type.
    /// </summary>
    /// <param name="type">The type to inspect for ViewModel argument.</param>
    /// <returns>
    /// A tuple containing the View type and its ViewModel type argument, or <c>default</c> if not applicable.
    /// </returns>
    private (Type Type, Type? Argument) GetViewArgumentType(Type type)
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

            if (componentBaseType.IsAssignableFrom(type))
            {
                break;
            }

            componentBaseType = owingComponentBaseGenericType.MakeGenericType(typeArgument);
            if (componentBaseType.IsAssignableFrom(type))
            {
                break;
            }
            return default;
        }

        if (componentBaseType is null || typeArgument is null)
        {
            return default;
        }

        Type[] interfaces = typeArgument
            .GetInterfaces();

        return interfaces.FirstOrDefault(i => viewModelType.IsAssignableFrom(i)) is null
            ? default
            : (type, typeArgument);
    }
}
