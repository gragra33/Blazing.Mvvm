using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Infrastructure;
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

            builder.Services.AddMauiBlazorWebView();

            // register ViewModels
            builder.Services.AddViewModels();

            builder.Services.AddMvvmNavigation(new LibraryConfiguration()
            {
                HostingModel = BlazorHostingModel.HybridMaui,
            });            

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
