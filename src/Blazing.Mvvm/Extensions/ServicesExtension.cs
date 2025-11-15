using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Parameter;
using Blazing.Mvvm.Components.Routing; // Added for ViewModelRouteCache
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // Added for Options.Create

namespace Blazing.Mvvm;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to add the required services for Blazing.Mvvm.
/// </summary>
public static class ServicesExtension
{
    /// <summary>
    /// Adds the required services for Blazing.Mvvm to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to add services to.</param>
    /// <param name="configuration">An optional action to configure the <see cref="LibraryConfiguration"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="LibraryConfiguration.HostingModelType"/> is not within the defined range.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a view model has a generic <see cref="ViewModelDefinitionAttribute{TViewModel}"/> attribute but does not implement the generic type.</exception>
    public static IServiceCollection AddMvvm(this IServiceCollection services, Action<LibraryConfiguration>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var callingAssembly = Assembly.GetCallingAssembly();
        LibraryConfiguration options = new();
        configuration?.Invoke(options);
        AddDependencies(services, options, callingAssembly);
        return services;
    }

    /// <summary>
    /// Adds core Blazing.Mvvm dependencies to the service collection, including configuration, navigation manager, route cache, and view models.
    /// </summary>
    /// <param name="services">The service collection to add dependencies to.</param>
    /// <param name="configuration">The library configuration.</param>
    /// <param name="callingAssembly">The calling assembly for view model registration.</param>
    private static void AddDependencies(IServiceCollection services, LibraryConfiguration configuration, Assembly callingAssembly)
    {
        if (configuration.ViewModelAssemblies.Count == 0)
        {
            configuration.RegisterViewModelsFromAssembly(callingAssembly);
        }

        // Register LibraryConfiguration for IOptions<LibraryConfiguration>
        services.TryAddSingleton(Options.Create(configuration));

        services.TryAddSingleton<IParameterResolver>(_ => new ParameterResolver(configuration.ParameterResolutionMode));
        // Register ViewModelRouteCache as a singleton, ensuring assemblies are captured here.
        services.TryAddSingleton<IViewModelRouteCache>(sp => 
            new ViewModelRouteCache(
                sp.GetRequiredService<ILogger<ViewModelRouteCache>>(), 
                configuration
            )
        );
        AddMvvmNavigationManager(services, configuration); // This now correctly registers MvvmNavigationManager as Scoped for Server/WebApp
        AddViewModels(services, configuration.ViewModelAssemblies);
    }

    /// <summary>
    /// Registers the MVVM navigation manager service according to the hosting model type.
    /// </summary>
    /// <param name="services">The service collection to add the navigation manager to.</param>
    /// <param name="configuration">The library configuration containing the hosting model type.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the hosting model type is invalid.</exception>
    private static void AddMvvmNavigationManager(this IServiceCollection services, LibraryConfiguration configuration)
    {
        var serviceDescriptor = configuration.HostingModelType switch
        {
            BlazorHostingModelType.WebAssembly or BlazorHostingModelType.Hybrid or BlazorHostingModelType.NotSpecified => ServiceDescriptor.Singleton<IMvvmNavigationManager, MvvmNavigationManager>(),
            BlazorHostingModelType.Server or BlazorHostingModelType.WebApp or BlazorHostingModelType.HybridMaui => ServiceDescriptor.Scoped<IMvvmNavigationManager, MvvmNavigationManager>(),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration), $"Invalid hosting model type: {configuration.HostingModelType}")
        };

        services.TryAdd(serviceDescriptor);
    }

    /// <summary>
    /// Registers all non-abstract types implementing <see cref="IViewModelBase"/> from the specified assemblies as services.
    /// Handles generic and keyed view model definitions via <see cref="ViewModelDefinitionAttribute"/>.
    /// </summary>
    /// <param name="services">The service collection to add view models to.</param>
    /// <param name="assemblies">The assemblies to scan for view model types.</param>
    /// <exception cref="InvalidOperationException">Thrown when a view model does not implement its generic type as required by <see cref="ViewModelDefinitionAttribute{TViewModel}"/>.</exception>
    private static void AddViewModels(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        foreach (var vmImplementationType in assemblies.SelectMany(a => a.GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(IViewModelBase)))))
        {
            var vmDefinitionAttributes = vmImplementationType.GetCustomAttributes<ViewModelDefinitionAttribute>();
            var viewModelDefinitionAttributes = vmDefinitionAttributes as ViewModelDefinitionAttribute[] ?? vmDefinitionAttributes.ToArray();

            if (viewModelDefinitionAttributes.Length == 0)
            {
                services.TryAddTransient(vmImplementationType);
                continue;
            }

            foreach (var vmDefinitionAttribute in viewModelDefinitionAttributes)
            {
                ServiceDescriptor serviceDescriptor;

                if (vmDefinitionAttribute is not IViewModelGenericAttributeDefinition vmDefinitionAttributeGeneric)
                {
                    serviceDescriptor = vmDefinitionAttribute.Key is null
                        ? ServiceDescriptor.Describe(vmImplementationType, vmImplementationType, vmDefinitionAttribute.Lifetime)
                        : ServiceDescriptor.DescribeKeyed(vmImplementationType, vmDefinitionAttribute.Key, vmImplementationType, vmDefinitionAttribute.Lifetime);

                    services.TryAdd(serviceDescriptor);
                    continue;
                }

                if (!vmDefinitionAttributeGeneric.ViewModelType.IsAssignableFrom(vmImplementationType))
                {
                    throw new InvalidOperationException($"ViewModel '{vmImplementationType.FullName}' does not implement '{vmDefinitionAttributeGeneric.ViewModelType.FullName}'.");
                }

                serviceDescriptor = vmDefinitionAttribute.Key is null
                   ? ServiceDescriptor.Describe(vmDefinitionAttributeGeneric.ViewModelType, vmImplementationType, vmDefinitionAttributeGeneric.Lifetime)
                   : ServiceDescriptor.DescribeKeyed(vmDefinitionAttributeGeneric.ViewModelType, vmDefinitionAttributeGeneric.Key, vmImplementationType, vmDefinitionAttributeGeneric.Lifetime);

                services.TryAdd(serviceDescriptor);
            }
        }
    }
}
