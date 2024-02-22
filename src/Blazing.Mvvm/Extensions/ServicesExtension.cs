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

    public static void AddMvvmNavigationManager(this IServiceCollection services, LibraryConfiguration configuration)
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
            var vmDefinitionAttributes = vmImplementationType.GetCustomAttributes<ViewModelDefinitionAttributeBase>();
            var vmAttributeCount = vmDefinitionAttributes.Count();

            if (vmAttributeCount == 0)
            {
                services.TryAddTransient(vmImplementationType);
                continue;
            }

            if (vmAttributeCount > 1)
            {
                // TODO: Create a custom exception type
                throw new InvalidOperationException($"ViewModel {vmImplementationType.FullName} cannot have more than one {nameof(ViewModelDefinitionAttribute)} defined.");
            }

            var vmAttribute = vmDefinitionAttributes.First();
            ServiceDescriptor serviceDescriptor;

            if (vmAttribute is ViewModelDefinitionAttribute vmDefinitionAttribute)
            {
                serviceDescriptor = ServiceDescriptor.Describe(vmImplementationType, vmImplementationType, vmDefinitionAttribute.Lifetime);
                services.TryAdd(serviceDescriptor);
                continue;
            }

            if (vmAttribute is not IViewModelDefinition vmDefinitionAttributeGeneric)
            {
                // TODO: Create a custom exception type
                throw new InvalidOperationException($"ViewModel {vmImplementationType.FullName} does not implement {nameof(IViewModelDefinition)}.");
            }

            if (!vmDefinitionAttributeGeneric.ViewModelType.IsAssignableFrom(vmImplementationType))
            {
                // TODO: Create a custom exception type
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
