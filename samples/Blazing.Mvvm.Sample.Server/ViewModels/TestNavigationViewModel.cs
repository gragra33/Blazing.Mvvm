using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

[ViewModelDefinition<ITestNavigationViewModel>(Lifetime = ServiceLifetime.Scoped)]
public sealed class TestNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    : TestNavigationBaseViewModel(mvvmNavigationManager, navigationManager)
{ /* skipped */ }