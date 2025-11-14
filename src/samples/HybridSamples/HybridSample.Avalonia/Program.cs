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

internal class Program
{
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
                options.RegisterViewModelsFromAssemblyContaining<SamplePageViewModel>();
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
            Task.Run(async () => await host.StopAsync()).Wait();
        }
    }

    private static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .LogToTrace();
}
