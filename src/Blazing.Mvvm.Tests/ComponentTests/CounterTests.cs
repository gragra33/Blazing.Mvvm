using Blazing.Mvvm.Sample.WebApp.Client.Pages;
using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Tests.ComponentTests;

public class CounterTests : ComponentTestBase
{
    private const string ParagraphSelector = "p";

    /// <summary>
    /// Initializes a new instance of the <see cref="CounterTests"/> class and registers the <see cref="CounterViewModel"/>.
    /// </summary>
    public CounterTests()
    {
        Services.AddSingleton(_ => CreateInstance<CounterViewModel>(true));
    }

    /// <summary>
    /// Verifies that all life-cycle methods are called on the <see cref="CounterViewModel"/> when the <see cref="Counter"/> component is rendered.
    /// </summary>
    /// <param name="expectedLogMessage">The expected log message for the life-cycle event.</param>
    [Theory]
    [InlineData("CounterViewModel => Life-cycle event: OnInitialized.")]
    [InlineData("CounterViewModel => Life-cycle event: OnInitializedAsync.")]
    [InlineData("CounterViewModel => Life-cycle event: OnParametersSet.")]
    [InlineData("CounterViewModel => Life-cycle event: OnParametersSetAsync.")]
    [InlineData("CounterViewModel => Life-cycle event: OnAfterRender.")]
    [InlineData("CounterViewModel => Life-cycle event: OnAfterRenderAsync.")]
    public void GivenComponent_WhenRendering_ThenLifeCycleMethodsCalledOnViewModel(string expectedLogMessage)
    {
        // Arrange
        var logger = GetMock<ILogger<CounterViewModel>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Act
        RenderComponent<Counter>();

        // Assert
        logger.VerifyLog(LogLevel.Information, expectedLogMessage, Times.Once());
    }

    /// <summary>
    /// Verifies that the ShouldRender life-cycle method is called on the <see cref="CounterViewModel"/> when the component is re-rendered.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenRerenderIsTriggered_ThenShouldRenderLifeCycleMethodCalledOnViewModel()
    {
        // Arrange
        const string expectedLogMessage = "CounterViewModel => Life-cycle event: ShouldRender.";

        var logger = GetMock<ILogger<CounterViewModel>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var cut = RenderComponent<Counter>();

        // Act
        cut.Render();

        // Assert
        logger.VerifyLog(LogLevel.Information, expectedLogMessage, Times.Once());
    }

    /// <summary>
    /// Verifies that clicking the Click Me button increments the current count in the <see cref="CounterViewModel"/> and updates the UI.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenClickMeButtonClicked_ThenCurrentCountShouldBeIncremented()
    {
        // Arrange
        const string clickMeButtonSelector = "#click-me";
        const int expectedCount = 1;
        var expectedParagraphContent = $"Current count: {expectedCount}";

        var cut = RenderComponent<Counter>();
        var cutViewModel = GetViewModel<CounterViewModel>();

        // Act
        cut.Find(clickMeButtonSelector).Click();

        // Assert
        using var _ = new AssertionScope();
        cut.Find(ParagraphSelector).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.CurrentCount.Should().Be(expectedCount);
    }

    /// <summary>
    /// Verifies that clicking the Reset button sets the current count to zero in the <see cref="CounterViewModel"/> and updates the UI.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenResetButtonClicked_ThenCurrentCountShouldBeZero()
    {
        // Arrange
        const string resetButtonSelector = "#reset";
        const int expectedCount = 0;
        var expectedParagraphContent = $"Current count: {expectedCount}";

        var cut = RenderComponent<Counter>();
        var cutViewModel = GetViewModel<CounterViewModel>();
        cutViewModel.CurrentCount = 5;

        // Act
        cut.Find(resetButtonSelector).Click();

        // Assert
        using var _ = new AssertionScope();
        cut.Find(ParagraphSelector).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.CurrentCount.Should().Be(expectedCount);
    }
}
