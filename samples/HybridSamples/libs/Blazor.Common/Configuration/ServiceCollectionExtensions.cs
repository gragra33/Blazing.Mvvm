using Blazing.Common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blazing.Common.Configuration;

/// <summary>
/// Provides extension methods for registering Blazor page history services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IPageHistoryService"/> to the service collection with default buffer capacity.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBlazorPageHistory(this IServiceCollection services)
        => services.AddScoped<IPageHistoryService, PageHistoryService>();
 
    /// <summary>
    /// Adds the <see cref="IPageHistoryService"/> to the service collection with a specified buffer capacity.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="bufferCapacity">The maximum number of pages to keep in history.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBlazorPageHistory(this IServiceCollection services, int bufferCapacity)
    {
        services.TryAddScoped<IPageHistoryService>(builder =>
            new PageHistoryService(builder.GetService<NavigationManager>()!, bufferCapacity));

        return services;
    }
}
