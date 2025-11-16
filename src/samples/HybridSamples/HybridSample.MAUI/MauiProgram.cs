using Blazing.Mvvm;
using Microsoft.Extensions.Logging;
using HybridSample.Core.Services;
using HybridSample.Core.ViewModels;
using HybridSample.MAUI.Extensions;
using Refit;

namespace HybridSample.MAUI;

/// <summary>
/// MAUI application program configuration.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application.
    /// </summary>
    /// <returns>The configured MAUI application.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register services
        builder.Services
            .AddSingleton(RestService.For<IRedditService>("https://www.reddit.com/"))
            .AddServicesMaui()
            .AddMvvm(options =>
            {
                options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
                options.HostingModelType = BlazorHostingModelType.HybridMaui;
                options.RegisterViewModelsFromAssemblyContaining<SamplePageViewModel>();
                options.RegisterViewModelsFromAssemblyContaining<HybridSample.Blazor.Core.Pages.IntroductionPage>();
            });

        return builder.Build();
    }
}