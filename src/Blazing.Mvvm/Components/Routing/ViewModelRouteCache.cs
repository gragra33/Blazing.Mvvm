using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Singleton store for ViewModel to route mappings.
/// </summary>
public class ViewModelRouteCache : IViewModelRouteCache
{
    private readonly ILogger<ViewModelRouteCache> _logger;
    private readonly LibraryConfiguration _configuration; 
    private readonly Dictionary<Type, string> _viewModelRoutes = [];
    private readonly Dictionary<object, string> _keyedViewModelRoutes = [];

    /// <inheritdoc />
    public IReadOnlyDictionary<Type, string> ViewModelRoutes => _viewModelRoutes;

    /// <inheritdoc />
    public IReadOnlyDictionary<object, string> KeyedViewModelRoutes => _keyedViewModelRoutes;

    
    /// <summary>
    /// The constructor for ViewModelRouteCache, initializes the logger and configuration, and generates the reference cache.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    public ViewModelRouteCache(ILogger<ViewModelRouteCache> logger, LibraryConfiguration configuration) 
    {
        _logger = logger;
        _configuration = configuration; 
        GenerateReferenceCache(configuration.ViewModelAssemblies);
    }

    private void GenerateReferenceCache(IEnumerable<Assembly> assemblies)
    {
        _logger.LogDebug("Starting generation of a new Reference Cache for ViewModelRouteCache");

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
            catch (Exception ex)
            {
                // Log and continue, or handle as appropriate for your library
                _logger.LogWarning(ex, "Error processing assembly {AssemblyName} for route caching.", assembly.FullName);
                continue;
            }

            if (items.Count == 0)
            {
                continue;
            }

            foreach ((Type Type, Type? Argument) item in items)
            {
                RouteAttribute? routeAttribute = item.Type.GetCustomAttributes<RouteAttribute>().FirstOrDefault();

                if (routeAttribute is null)
                {
                    continue;
                }

                string uri = routeAttribute.Template;
                if (!string.IsNullOrWhiteSpace(_configuration.BasePath))
                {
                    uri = $"{_configuration.BasePath.TrimEnd('/')}/{uri.TrimStart('/')}";
                }
                
                if (item.Argument != null && _viewModelRoutes.TryAdd(item.Argument, uri))
                {
                    _logger.LogDebug("Caching navigation reference '{Argument}' with uri '{Uri}' for '{FullName}'", item.Argument, uri, item.Type.FullName);
                }

                ViewModelKeyAttribute? vmKeyAttribute = item.Type.GetCustomAttribute<ViewModelKeyAttribute>();
                if (vmKeyAttribute?.Key != null && _keyedViewModelRoutes.TryAdd(vmKeyAttribute.Key, uri))
                {
                    _logger.LogDebug("Caching keyed navigation reference '{Key}' with uri '{Uri}' for '{FullName}'", vmKeyAttribute.Key, uri, item.Type.FullName);
                }
            }
        }
        _logger.LogDebug("Completed generating the Reference Cache for ViewModelRouteCache");
    }

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

        if (componentBaseType is null || typeArgument is null) // Added null check for typeArgument
        {
            return default;
        }

        Type[] interfaces = typeArgument // Changed from componentBaseType to typeArgument
            .GetInterfaces();


        return interfaces.FirstOrDefault(i => viewModelType.IsAssignableFrom(i)) is null
            ? default
            : (type, typeArgument);
    }
}
