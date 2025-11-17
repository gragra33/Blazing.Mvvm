using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A test view model for navigation scenarios in Blazor MVVM tests.
/// Implements <see cref="ITestNavigationViewModel"/> and is registered for navigation testing.
/// </summary>
[ViewModelDefinition<ITestNavigationViewModel>]
public sealed class TestNavigationViewModel : TestNavigationBaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestNavigationViewModel"/> class.
    /// </summary>
    /// <param name="mvvmNavigationManager">The MVVM navigation manager.</param>
    /// <param name="navigationManager">The Blazor navigation manager.</param>
    public TestNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
        : base(mvvmNavigationManager, navigationManager)
    {
    }
}
