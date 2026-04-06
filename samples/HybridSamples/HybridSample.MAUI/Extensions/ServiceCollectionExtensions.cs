using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;
using HybridSample.Blazor.Core.Services;
using HybridSample.MAUI.ViewModels;
using FilesService = HybridSample.MAUI.Services.FilesService;

namespace HybridSample.MAUI.Extensions;

/// <summary>
/// Extension methods for registering MAUI services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MAUI-specific services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddServicesMaui(this IServiceCollection services)
    {
        services.AddScoped<IFilesService, FilesService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        
        // Register ViewModels
        services.AddTransient<MainPageViewModel>();

        return services;
    }
}
