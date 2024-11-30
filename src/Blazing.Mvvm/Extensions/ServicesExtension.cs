using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Parameter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blazing.Mvvm;

/// <summary>
/// An extension class for <see cref="IServiceCollection"/> to add the required services for Blazing.Mvvm.
/// </summary>
public static class ServicesExtension
{
    /// <summary>
    /// Adds the required services for Blazing.Mvvm.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">An action that configures the <see cref="LibraryConfiguration"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Throws when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Throws when <see cref="LibraryConfiguration.HostingModelType"/> is not within the defined range.</exception>
    /// <exception cref="InvalidOperationException">Throws an when a <c>view model</c> has a generic <see cref="ViewModelDefinitionAttribute{TViewModel}"/> attribute but does not implement the generic type.</exception>
    public static IServiceCollection AddMvvm(this IServiceCollection services, Action<LibraryConfiguration>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var callingAssembly = Assembly.GetCallingAssembly();
        LibraryConfiguration options = new();
        configuration?.Invoke(options);
        AddDependencies(services, options, callingAssembly);
        return services;
    }

    private static void AddDependencies(IServiceCollection services, LibraryConfiguration configuration, Assembly callingAssembly)
    {
        if (configuration.ViewModelAssemblies.Count == 0)
        {
            configuration.RegisterViewModelsFromAssembly(callingAssembly);
        }

        services.TryAddSingleton<IParameterResolver>(_ => new ParameterResolver(configuration.ParameterResolutionMode));
        AddMvvmNavigationManager(services, configuration);
        AddViewModels(services, configuration.ViewModelAssemblies);
    }

    private static void AddMvvmNavigationManager(this IServiceCollection services, LibraryConfiguration configuration)
    {
        var serviceDescriptor = configuration.HostingModelType switch
        {
            BlazorHostingModelType.WebAssembly or BlazorHostingModelType.Hybrid or BlazorHostingModelType.NotSpecified => ServiceDescriptor.Singleton<IMvvmNavigationManager, MvvmNavigationManager>(),
            BlazorHostingModelType.Server or BlazorHostingModelType.WebApp => ServiceDescriptor.Scoped<IMvvmNavigationManager, MvvmNavigationManager>(),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration), $"Invalid hosting model type: {configuration.HostingModelType}")
        };

        services.TryAdd(serviceDescriptor);
    }

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
