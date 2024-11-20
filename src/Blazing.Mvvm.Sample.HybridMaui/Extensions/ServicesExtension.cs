using Blazing.Mvvm.Sample.HybridMaui.ViewModels;

namespace Blazing.Mvvm.Sample.HybridMaui;

public static class ServicesExtension
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddScoped<MainLayoutViewModel>();
        services.AddScoped<FetchDataViewModel>();

        services.AddScoped<WeatherViewModel>();
        services.AddScoped<HexEntryViewModel>();
        services.AddScoped<TextEntryViewModel>();
        services.AddScoped<EditContactViewModel>();

        services.AddScoped<HexTranslateViewModel>();
        services.AddScoped<ITestNavigationViewModel, TestNavigationViewModel>();

        return services;
    }
}