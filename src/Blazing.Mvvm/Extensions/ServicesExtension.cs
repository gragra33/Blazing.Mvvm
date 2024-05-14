using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blazing.Mvvm;

public static class ServicesExtension
{
    /// <summary>
    /// Adds the required services for Blazing.Mvvm.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">The <see cref="LibraryConfiguration"/> instance.</param>
    /// <returns></returns>
    public static IServiceCollection AddMvvm(this IServiceCollection services, LibraryConfiguration? configuration = null)
    {
        configuration ??= new();
        RegisterDependencies(services, configuration);

        return services;
    }

    /// <summary>
    /// Adds the required services for Blazing.Mvvm.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">An action that configures the <see cref="LibraryConfiguration"/>.</param>
    /// <returns></returns>
    public static IServiceCollection AddMvvm(this IServiceCollection services, Action<LibraryConfiguration> configuration)
    {
        LibraryConfiguration options = new();
        configuration.Invoke(options);

        return AddMvvm(services, options);
    }

    private static void RegisterDependencies(IServiceCollection services, LibraryConfiguration configuration)
    {
        AddMvvmNavigationManager(services, configuration);
        AddViewModels(services, configuration.GetScanAssemblies());
    }

    private static void AddMvvmNavigationManager(this IServiceCollection services, LibraryConfiguration configuration)
    {
        if (!configuration.EnableMvvmNavigationManager)
        {
            return;
        }

        var serviceDescriptor = configuration.HostingModelType switch
        {
            BlazorHostingModelType.WebAssembly or BlazorHostingModelType.Hybrid or BlazorHostingModelType.NotSpecified => ServiceDescriptor.Singleton<IMvvmNavigationManager, MvvmNavigationManager>(),
            BlazorHostingModelType.Server or BlazorHostingModelType.WebApp => ServiceDescriptor.Scoped<IMvvmNavigationManager, MvvmNavigationManager>(),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration.HostingModelType))
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

                if (vmDefinitionAttribute is not IViewModelGenericAttributeDefinition)
                {
#if NET8_0_OR_GREATER
                serviceDescriptor = string.IsNullOrWhiteSpace(vmDefinitionAttribute.Key)
                    ? ServiceDescriptor.Describe(vmImplementationType, vmImplementationType, vmDefinitionAttribute.Lifetime)
                    : ServiceDescriptor.DescribeKeyed(vmImplementationType, vmDefinitionAttribute.Key, vmImplementationType, vmDefinitionAttribute.Lifetime);
#else
                    serviceDescriptor = ServiceDescriptor.Describe(vmImplementationType, vmImplementationType, vmDefinitionAttribute.Lifetime);
#endif
                    services.TryAdd(serviceDescriptor);
                    continue;
                }

                var vmDefinitionAttributeGeneric = (IViewModelGenericAttributeDefinition)vmDefinitionAttribute;

                if (!vmDefinitionAttributeGeneric.ViewModelType.IsAssignableFrom(vmImplementationType))
                {
                    throw new InvalidOperationException($"ViewModel {vmImplementationType.FullName} does not implement {vmDefinitionAttributeGeneric.ViewModelType.FullName}.");
                }

#if NET8_0_OR_GREATER
            serviceDescriptor = string.IsNullOrWhiteSpace(vmDefinitionAttributeGeneric.Key)
               ? ServiceDescriptor.Describe(vmDefinitionAttributeGeneric.ViewModelType, vmImplementationType, vmDefinitionAttributeGeneric.Lifetime)
               : ServiceDescriptor.DescribeKeyed(vmDefinitionAttributeGeneric.ViewModelType, vmDefinitionAttributeGeneric.Key, vmImplementationType, vmDefinitionAttributeGeneric.Lifetime);
#else
                serviceDescriptor = ServiceDescriptor.Describe(vmDefinitionAttributeGeneric.ViewModelType, vmImplementationType, vmDefinitionAttributeGeneric.Lifetime);
#endif
                services.TryAdd(serviceDescriptor);
            }
        }
    }
}
