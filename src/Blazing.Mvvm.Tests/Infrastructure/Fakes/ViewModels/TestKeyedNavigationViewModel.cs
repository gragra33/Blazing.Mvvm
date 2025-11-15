using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

[ViewModelDefinition<ITestKeyedNavigationViewModel>(Key = "TestKeyedNavigationViewModel")]
public sealed class TestKeyedNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    : TestKeyedNavigationBaseViewModel(mvvmNavigationManager, navigationManager);
