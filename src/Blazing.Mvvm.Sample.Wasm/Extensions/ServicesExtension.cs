using Blazing.Mvvm.Sample.Wasm.ViewModels;

namespace Blazing.Mvvm.Sample.Wasm;

public static class ServicesExtension
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainLayoutViewModel>();

        services.AddTransient<FetchDataViewModel>();
        services.AddTransient<HexEntryViewModel>();
        services.AddTransient<TextEntryViewModel>();
        services.AddTransient<EditContactViewModel>();

        services.AddTransient<HexTranslateViewModel>();
        services.AddTransient<ITestNavigationViewModel, TestNavigationViewModel>();

        return services;
    }
}