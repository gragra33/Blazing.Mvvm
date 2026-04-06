using Blazing.Mvvm;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HybridSample.Core.Services;
using HybridSample.Core.ViewModels;
using HybridSample.Wpf.Extensions;
using Refit;

namespace HybridSample.Wpf;

/// <summary>
/// Interaction logic for the WPF application.
/// </summary>
public partial class App
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class and configures services.
    /// </summary>
    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        IServiceCollection services = builder.Services;

        services.AddWpfBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        services
            .AddSingleton(RestService.For<IRedditService>("https://www.reddit.com/"))
            .AddServicesWpf()
            .AddMvvm(options =>
            { 
                options.RegisterViewModelsFromAssemblyContaining<SamplePageViewModel>();
                options.RegisterViewModelsFromAssemblyContaining<HybridSample.Blazor.Core.Pages.IntroductionPage>();
            });

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
#endif

        services.AddScoped<MainWindow>();

        Resources.Add("services", services.BuildServiceProvider());

        // will throw an error
        //MainWindow = provider.GetRequiredService<MainWindow>();
        //MainWindow.Show();
    }
}
