using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HybridSample.WinForms.Services;

/// <summary>
/// Service to handle graceful shutdown of the WinForms application.
/// </summary>
public class ApplicationLifecycleService : IHostedService
{
    private readonly ILogger<ApplicationLifecycleService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationLifecycleService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="applicationLifetime">The application lifetime manager.</param>
    public ApplicationLifecycleService(
        ILogger<ApplicationLifecycleService> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    /// <summary>
    /// Starts the application lifecycle service and registers shutdown handlers.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application lifecycle service started");
        
        // Register shutdown handlers
        _applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
        _applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the application lifecycle service.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application lifecycle service stopping");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handler called when the application is stopping.
    /// </summary>
    private void OnApplicationStopping()
    {
        _logger.LogInformation("Application is stopping gracefully...");
        
        // Give some time for cleanup operations
        Thread.Sleep(100);
    }

    /// <summary>
    /// Handler called when the application has stopped.
    /// </summary>
    private void OnApplicationStopped()
    {
        _logger.LogInformation("Application has stopped");
    }
}