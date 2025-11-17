using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A test view model for keyed navigation in Blazor MVVM tests.
/// Registered with the key "TestKeyedNavigationViewModel" for navigation scenarios.
/// </summary>
[ViewModelDefinition<ITestKeyedNavigationViewModel>(Key = "TestKeyedNavigationViewModel")]
public sealed class TestKeyedNavigationViewModel : TestKeyedNavigationBaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestKeyedNavigationViewModel"/> class.
    /// </summary>
    /// <param name="mvvmNavigationManager">The MVVM navigation manager.</param>
    /// <param name="navigationManager">The Blazor navigation manager.</param>
    public TestKeyedNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
        : base(mvvmNavigationManager, navigationManager)
    {
    }
}
