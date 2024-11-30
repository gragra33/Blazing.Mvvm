using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Sample.HybridMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });


            builder.Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
            builder.Services.AddMvvm(options =>
            {
                options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
                options.HostingModelType = BlazorHostingModelType.HybridMaui;
            });
            
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
