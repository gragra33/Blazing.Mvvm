using Blazing.Mvvm.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm;

public static class ServicesExtension
{
    public static IServiceCollection AddMvvmNavigation(this IServiceCollection services)
    {
        services.AddScoped<IMvvmNavigationManager, MvvmNavigationManager>();
        return services;
    }
}