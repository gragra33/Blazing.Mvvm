using Avalonia;
using Blazing.Mvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HybridSample.Core.Services;
using HybridSample.Avalonia.Extensions;
using Refit;
using HybridSample.Core.ViewModels;

namespace HybridSample.Avalonia;

/// <summary>
/// Entry point and configuration logic for the Avalonia application.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point for the Avalonia application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        HostApplicationBuilder appBuilder = Host.CreateApplicationBuilder(args);
        appBuilder.Logging.AddDebug();
        
        appBuilder.Services.AddWindowsFormsBlazorWebView();
#if DEBUG
        appBuilder.Services.AddBlazorWebViewDeveloperTools();
#endif
        appBuilder.Services
            .AddSingleton(RestService.For<IRedditService>("https://www.reddit.com/"))
            // .AddViewModels() // obsolete - now uses the ViewModelDefinition attribute & auto registration
            .AddServicesWpf()
            .AddMvvm(options =>
            { 
                // Register ViewModels from HybridSample.Core assembly
                options.RegisterViewModelsFromAssemblyContaining<SamplePageViewModel>();
                // Register Views/Pages from HybridSample.Blazor.Core assembly for route mapping
                options.RegisterViewModelsFromAssemblyContaining<HybridSample.Blazor.Core.Pages.IntroductionPage>();
            });

        using IHost host = appBuilder.Build();

        host.Start();

        try
        {
            BuildAvaloniaApp(host.Services)
                .StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            host.StopAsync().GetAwaiter().GetResult();
            //Task.Run(async () => { await host.StopAsync(); }).Wait();
        }
    }

    /// <summary>
    /// Configures the Avalonia application with the provided service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <returns>The configured Avalonia <see cref="AppBuilder"/>.</returns>
    private static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .LogToTrace();
}
