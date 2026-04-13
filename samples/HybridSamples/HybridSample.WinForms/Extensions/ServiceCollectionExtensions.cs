using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;
using HybridSample.WinForms.Services;

namespace HybridSample.WinForms.Extensions;

/// <summary>
/// Extension methods for service collection to register WinForms-specific services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds WinForms-specific services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServicesWinForms(this IServiceCollection services)
    {
        return services
            .AddScoped<IFilesService, FilesService>()
            .AddScoped<ISettingsService, SettingsService>()
            .AddHostedService<ApplicationLifecycleService>();
    }
}