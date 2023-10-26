using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;

namespace Blazing.Mvvm.Sample.WebApp.Client;

public static class ServicesExtension
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainLayoutViewModel>();

        services.AddTransient<WeatherViewModel>();
        services.AddTransient<HexEntryViewModel>();
        services.AddTransient<TextEntryViewModel>();
        services.AddTransient<EditContactViewModel>();

        services.AddTransient<HexTranslateViewModel>();
        services.AddTransient<ITestNavigationViewModel, TestNavigationViewModel>();

        return services;
    }
}