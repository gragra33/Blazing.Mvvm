using Blazing.Mvvm.Sample.WebApp.Client.Layout;
using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.ComponentTests;

public class MainLayoutTests : ComponentTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainLayoutTests"/> class and registers the <see cref="MainLayoutViewModel"/>.
    /// </summary>
    public MainLayoutTests()
    {
        Services.AddSingleton<MainLayoutViewModel>();
    }

    /// <summary>
    /// Verifies that the counter is incremented and the navigation count is updated when the location changes.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenLocationChanged_ThenCounterShouldBeIncremented()
    {
        // Arrange
        const int expectedCounter = 1;
        const string spanArialLabel = "navigation count";
        var expectedSpanContent = $"Navigation Count: {expectedCounter}";

        var cut = RenderComponent<MainLayout>();
        var cutViewModel = GetViewModel<MainLayoutViewModel>();
        var fakeNavigationManager = Services.GetRequiredService<FakeNavigationManager>();

        // Act
        fakeNavigationManager.NavigateTo("test");

        // Assert
        using var _ = new AssertionScope();
        cutViewModel.Counter.Should().Be(expectedCounter);
        cut.FindByLabelText(spanArialLabel).TextContent.Should().Be(expectedSpanContent);
    }
}
