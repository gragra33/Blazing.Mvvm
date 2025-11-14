using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;
using HybridSample.Avalonia.Services;
using HybridSample.Blazor.Core.Services;
using FilesService = HybridSample.Avalonia.Services.FilesService;

namespace HybridSample.Avalonia.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesWpf(this IServiceCollection services)
    {
        services.AddTransient<IFilesService>(sp => new FilesService());
        services.AddSingleton<ISettingsService, SettingsService>();

        return services;
    }
}
