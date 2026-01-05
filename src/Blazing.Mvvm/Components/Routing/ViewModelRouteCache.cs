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
public sealed class ViewModelRouteCache : IViewModelRouteCache
{
    private readonly ILogger<ViewModelRouteCache> _logger;
    private readonly LibraryConfiguration _configuration;
    private readonly RouteTemplateParser _parser;
    private readonly Dictionary<Type, string> _viewModelRoutes = [];
    private readonly Dictionary<object, string> _keyedViewModelRoutes = [];
    private readonly Dictionary<Type, RouteTemplateCollection> _viewModelRouteTemplates = [];
    private readonly Dictionary<object, RouteTemplateCollection> _keyedViewModelRouteTemplates = [];

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

    /// <inheritdoc />
    /// <summary>
    /// Gets the cached route template collections for ViewModels.
    /// </summary>
    public IReadOnlyDictionary<Type, RouteTemplateCollection> ViewModelRouteTemplates => _viewModelRouteTemplates;

    /// <inheritdoc />
    /// <summary>
    /// Gets the cached route template collections for keyed ViewModels.
    /// </summary>
    public IReadOnlyDictionary<object, RouteTemplateCollection> KeyedViewModelRouteTemplates => _keyedViewModelRouteTemplates;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelRouteCache"/> class, sets up logging and configuration, and generates the reference cache.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic messages.</param>
    /// <param name="configuration">The library configuration containing assemblies and base path.</param>
    public ViewModelRouteCache(ILogger<ViewModelRouteCache> logger, LibraryConfiguration configuration) 
    {
        _logger = logger;
        _configuration = configuration;
        _parser = new RouteTemplateParser();
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
                // Collect ALL RouteAttributes for multi-route template support
                var routeAttributes = item.Type.GetCustomAttributes<RouteAttribute>().ToList();

                if (routeAttributes.Count == 0)
                {
                    _logger.LogDebug("View {ViewType} does not have a RouteAttribute", item.Type.FullName);
                    continue;
                }

                // Process ViewModel type routes
                if (item.Argument != null)
                {
                    ProcessViewModelRoutes(item.Type, item.Argument, routeAttributes);
                }

                // Process keyed ViewModel routes
                ViewModelKeyAttribute? vmKeyAttribute = item.Type.GetCustomAttribute<ViewModelKeyAttribute>();
                if (vmKeyAttribute?.Key != null)
                {
                    ProcessKeyedViewModelRoutes(item.Type, vmKeyAttribute.Key, routeAttributes);
                }
            }
        }
        
        _logger.LogDebug("Completed generating the Reference Cache for ViewModelRouteCache. Total ViewModels cached: {ViewModelCount}, Total keyed ViewModels cached: {KeyedViewModelCount}", 
            _viewModelRoutes.Count, _keyedViewModelRoutes.Count);
        _logger.LogDebug("Multi-template cache: {TemplateCount} ViewModel templates, {KeyedTemplateCount} keyed ViewModel templates",
            _viewModelRouteTemplates.Count, _keyedViewModelRouteTemplates.Count);
    }

    /// <summary>
    /// Processes route attributes for a ViewModel type and populates both legacy and multi-template caches.
    /// </summary>
    private void ProcessViewModelRoutes(Type viewType, Type viewModelType, List<RouteAttribute> routeAttributes)
    {
        // Apply BasePath to all routes
        var processedRoutes = routeAttributes
            .Select(ra => ApplyBasePath(ra.Template))
            .ToList();

        // Parse all route templates
        var templates = _parser.ParseMany(processedRoutes);

        // Select primary route (simplest: fewest parameters, then shortest)
        var primaryTemplate = templates
            .OrderBy(t => t.ParameterCount)
            .ThenBy(t => t.Pattern.Length)
            .First();

        // Populate legacy cache (backward compatibility)
        if (_viewModelRoutes.TryAdd(viewModelType, primaryTemplate.Pattern))
        {
            _logger.LogDebug("Caching navigation reference '{ViewModelType}' with uri '{Uri}' for '{ViewType}'", 
                viewModelType, primaryTemplate.Pattern, viewType.FullName);
        }
        else
        {
            _logger.LogWarning("Duplicate ViewModel type {ViewModelType} found. Existing route: '{ExistingRoute}', Ignored route: '{NewRoute}'", 
                viewModelType.FullName, _viewModelRoutes[viewModelType], primaryTemplate.Pattern);
            return; // Skip multi-template caching for duplicates
        }

        // Populate multi-template cache
        var collection = new RouteTemplateCollection
        {
            PrimaryRoute = primaryTemplate.Pattern,
            AllRoutes = templates
        };

        _viewModelRouteTemplates[viewModelType] = collection;

        if (templates.Count > 1)
        {
            _logger.LogDebug("Cached {RouteCount} route templates for ViewModel '{ViewModelType}'. Primary: '{PrimaryRoute}'",
                templates.Count, viewModelType, primaryTemplate.Pattern);
        }
    }

    /// <summary>
    /// Processes route attributes for a keyed ViewModel and populates both legacy and multi-template caches.
    /// </summary>
    private void ProcessKeyedViewModelRoutes(Type viewType, object key, List<RouteAttribute> routeAttributes)
    {
        // Apply BasePath to all routes
        var processedRoutes = routeAttributes
            .Select(ra => ApplyBasePath(ra.Template))
            .ToList();

        // Parse all route templates
        var templates = _parser.ParseMany(processedRoutes);

        // Select primary route (simplest: fewest parameters, then shortest)
        var primaryTemplate = templates
            .OrderBy(t => t.ParameterCount)
            .ThenBy(t => t.Pattern.Length)
            .First();

        // Populate legacy cache (backward compatibility)
        if (_keyedViewModelRoutes.TryAdd(key, primaryTemplate.Pattern))
        {
            _logger.LogDebug("Caching keyed navigation reference '{Key}' with uri '{Uri}' for '{ViewType}'", 
                key, primaryTemplate.Pattern, viewType.FullName);
        }
        else
        {
            _logger.LogWarning("Duplicate ViewModel key {ViewModelKey} found. Existing route: '{ExistingRoute}', Ignored route: '{NewRoute}'", 
                key, _keyedViewModelRoutes[key], primaryTemplate.Pattern);
            return; // Skip multi-template caching for duplicates
        }

        // Populate multi-template cache
        var collection = new RouteTemplateCollection
        {
            PrimaryRoute = primaryTemplate.Pattern,
            AllRoutes = templates
        };

        _keyedViewModelRouteTemplates[key] = collection;

        if (templates.Count > 1)
        {
            _logger.LogDebug("Cached {RouteCount} route templates for keyed ViewModel '{Key}'. Primary: '{PrimaryRoute}'",
                templates.Count, key, primaryTemplate.Pattern);
        }
    }

    /// <summary>
    /// Applies the configured BasePath to a route template if configured.
    /// </summary>
    private string ApplyBasePath(string template)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (!string.IsNullOrWhiteSpace(_configuration.BasePath))
        {
            return $"{_configuration.BasePath.TrimEnd('/')}/{template.TrimStart('/')}";
        }
#pragma warning restore CS0618 // Type or member is obsolete
        
        return template;
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
