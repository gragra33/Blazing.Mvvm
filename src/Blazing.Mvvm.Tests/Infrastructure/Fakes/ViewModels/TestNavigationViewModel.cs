using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition<ITestNavigationViewModel>]
public sealed class TestNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    : TestNavigationBaseViewModel(mvvmNavigationManager, navigationManager);
