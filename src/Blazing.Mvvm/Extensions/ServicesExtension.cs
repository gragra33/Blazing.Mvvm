using Blazing.Mvvm.Components;
using Blazing.Mvvm.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm;

public static class ServicesExtension
{
    public static IServiceCollection AddMvvmNavigation(this IServiceCollection services, LibraryConfiguration? configuration = null)
    {
        if (configuration is not null)
        {
            switch (configuration.HostingModelType)
            {
                case BlazorHostingModelType.WebAssembly:
                case BlazorHostingModelType.Hybrid:
                case BlazorHostingModelType.NotSpecified:
                    services.AddSingleton<IMvvmNavigationManager, MvvmNavigationManager>();
                    break;
                case BlazorHostingModelType.Server:
                case BlazorHostingModelType.WebApp:
                    services.AddScoped<IMvvmNavigationManager, MvvmNavigationManager>();
                    break;
            }
        }
        else
        {
            services.AddSingleton<IMvvmNavigationManager, MvvmNavigationManager>();
        }
        return services;
    }

    public static IServiceCollection AddMvvmNavigation(this IServiceCollection services, Action<LibraryConfiguration> configuration)
    {
        LibraryConfiguration options = new();
        configuration.Invoke(options);

        return AddMvvmNavigation(services, options);
    }


}