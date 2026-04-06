using Blazing.Mvvm.ParentChildSample.ViewModels;

namespace Blazing.Mvvm.ParentChildSample;

// obsolete - now uses the ViewModelDefinition attribute & auto registration
//public static class ServicesExtension
//{
//    public static IServiceCollection AddViewModels(this IServiceCollection services)
//    {
//        services.AddTransient<HomeViewModel>();
//        services.AddTransient<ChildViewModel>();

//        return services;
//    }
//}