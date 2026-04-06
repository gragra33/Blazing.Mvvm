using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;
using HybridSample.Blazor.Core.Services;
using FilesService = HybridSample.Avalonia.Services.FilesService;

namespace HybridSample.Avalonia.Extensions;

/// <summary>
/// Extension methods for registering Avalonia services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Avalonia-specific services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddServicesWpf(this IServiceCollection services)
    {
        services.AddTransient<IFilesService>(_ => new FilesService());
        services.AddSingleton<ISettingsService, SettingsService>();

        return services;
    }
}
