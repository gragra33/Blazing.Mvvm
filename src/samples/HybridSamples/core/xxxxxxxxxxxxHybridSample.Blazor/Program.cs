using Blazing.Mvvm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HybridSample.Core.Services;
using HybridSample.Core.ViewModels;
using HybridSample.Blazor;
using HybridSample.Blazor.Core.Extensions;
using Refit;

/// <summary>
/// Configures and starts the Blazor WebAssembly application.
/// </summary>
WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddSingleton(RestService.For<IRedditService>("https://www.reddit.com/"))
    //.AddViewModels() // obsolete - now uses the ViewModelDefinition attribute & auto registration
    .AddServices()
    .AddMvvm(options =>
    {
        // Register ViewModels from HybridSample.Core assembly
        options.RegisterViewModelsFromAssemblyContaining<SamplePageViewModel>();
        // Register Views/Pages from HybridSample.Blazor.Core assembly for route mapping
        options.RegisterViewModelsFromAssemblyContaining<HybridSample.Blazor.Core.Pages.IntroductionPage>();
    });

#if DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Trace);
#endif

/// <summary>
/// Builds and runs the Blazor WebAssembly host.
/// </summary>
await builder.Build().RunAsync();
