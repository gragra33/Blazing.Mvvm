using Blazing.Mvvm.Sample.WebApp.Client.Data;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using Blazing.Mvvm.Sample.WebApp.Client.Pages;
using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Tests.ComponentTests;

public class FetchDataTests : ComponentTestBase
{
    private const string TableSelector = "table";

    private readonly FakePersistentComponentState _fakeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="FetchDataTests"/> class and registers the <see cref="FetchDataViewModel"/>.
    /// </summary>
    public FetchDataTests()
    {
        _fakeState = this.AddFakePersistentComponentState();

        // Add a view model to Services because the IScopedFactory created by BUnit does not fall back to AutoMocker.
        Services.AddScoped(_ => CreateInstance<FetchDataViewModel>(true));
    }

    /// <summary>
    /// Verifies that the loading text is shown when the weather forecast data is being fetched.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenFetchingWeatherForecastData_ThenShouldShowLoadingText()
    {
        // Arrange
        const string loadingParagraphAriaLabel = "loading";
        const string expectedParagraphContent = "Loading...";

        var taskCompletionSource = new TaskCompletionSource<IEnumerable<WeatherForecast>?>();
        var weatherServiceMock = GetMock<IWeatherService>();
        weatherServiceMock.Setup(x => x.GetForecastAsync(It.Is<CancellationToken>(x => x != default)))
            .Returns(taskCompletionSource.Task);

        // Act
        var cut = RenderComponent<FetchData>();

        // Assert
        cut.FindByLabelText(loadingParagraphAriaLabel).TextContent.Should().Be(expectedParagraphContent);
    }

    /// <summary>
    /// Verifies that an empty table is shown when the fetched weather forecast data is empty.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenFetchedWeatherForecastDataIsEmpty_ThenShouldShowEmptyTable()
    {
        // Arrange
        const string expectedTableHtml = """
            <table diff:ignoreAttributes>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                </tbody>
            </table>
            """;

        var weatherServiceMock = GetMock<IWeatherService>();
        weatherServiceMock.Setup(x => x.GetForecastAsync(It.Is<CancellationToken>(x => x != default)))
            .ReturnsAsync([]);

        // Act
        var cut = RenderComponent<FetchData>();

        // Assert
        cut.WaitForAssertion(() => cut.Find(TableSelector).MarkupMatches(expectedTableHtml));
    }

    /// <summary>
    /// Verifies that the table is shown with weather forecast data when returned from the weather service.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenWeatherForecastDataReturnedFromWeatherService_ThenShouldShowTableWithData()
    {
        // Arrange
        var weatherForecast = new WeatherForecast
        {
            Date = DateTime.Now.AddDays(1),
            TemperatureC = 23,
            Summary = "Warm"
        };

        string expectedTableHtml = $"""
            <table diff:ignoreAttributes>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>{weatherForecast.Date.ToShortDateString()}</td>
                        <td>{weatherForecast.TemperatureC}</td>
                        <td>{weatherForecast.TemperatureF}</td>
                        <td>{weatherForecast.Summary}</td>
                    </tr>
                </tbody>
            </table>
            """;

        var cutViewModel = GetViewModel<FetchDataViewModel>();
        var weatherServiceMock = GetMock<IWeatherService>();
        weatherServiceMock.Setup(x => x.GetForecastAsync(It.Is<CancellationToken>(x => x != default)))
            .ReturnsAsync([weatherForecast]);

        // Act
        var cut = RenderComponent<FetchData>();

        // Assert
        using var _ = new AssertionScope();
        cut.WaitForAssertion(() => cut.Find(TableSelector).MarkupMatches(expectedTableHtml));
        cutViewModel.WeatherForecasts.Should().BeEquivalentTo([weatherForecast]);
    }

    /// <summary>
    /// Verifies that the table is shown with weather forecast data when returned from persistent component state.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenWeatherForecastDataReturnedFromPersistentComponentState_ThenShouldShowTableWithData()
    {
        // Arrange
        var weatherForecast = new WeatherForecast
        {
            Date = DateTime.Now.AddDays(5),
            TemperatureC = 22,
            Summary = "Cool"
        };

        string expectedTableHtml = $"""
            <table diff:ignoreAttributes>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>{weatherForecast.Date.ToShortDateString()}</td>
                        <td>{weatherForecast.TemperatureC}</td>
                        <td>{weatherForecast.TemperatureF}</td>
                        <td>{weatherForecast.Summary}</td>
                    </tr>
                </tbody>
            </table>
            """;

        var cutViewModel = GetViewModel<FetchDataViewModel>();
        var weatherServiceMock = GetMock<IWeatherService>();
        _fakeState.Persist<IEnumerable<WeatherForecast>>(nameof(cutViewModel.WeatherForecasts), [weatherForecast]);

        // Act
        var cut = RenderComponent<FetchData>();

        // Assert
        using var _ = new AssertionScope();
        cut.WaitForAssertion(() => cut.Find(TableSelector).MarkupMatches(expectedTableHtml));
        cutViewModel.WeatherForecasts.Should().BeEquivalentTo([weatherForecast]);
        weatherServiceMock.Verify(x => x.GetForecastAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    /// <summary>
    /// Verifies that the state is persisted when OnPersisting is triggered.
    /// </summary>
    [Fact]
    public void GivenComponent_WhenRegisterOnPersistingIsTriggered_ThenShouldPersistState()
    {
        // Arrange
        RenderComponent<FetchData>();
        var cutViewModel = GetViewModel<FetchDataViewModel>();
        cutViewModel.WeatherForecasts = [new WeatherForecast { Date = DateTime.Now, TemperatureC = 30, Summary = "Hot" }];

        // Act
        _fakeState.TriggerOnPersisting();

        // Assert
        using var _ = new AssertionScope();
        _fakeState.TryTake<IEnumerable<WeatherForecast>>(nameof(cutViewModel.WeatherForecasts), out var weatherForecasts).Should().BeTrue();
        weatherForecasts.Should().BeEquivalentTo(cutViewModel.WeatherForecasts);
    }

    /// <summary>
    /// Verifies that the view model is disposed when the component is disposed.
    /// </summary>
    [Fact]
    public void GivenComponent_WhenDisposed_ThenShouldDisposeViewModel()
    {
        // Arrange
        const string expectedLogMessage = "Disposing FetchDataViewModel.";

        RenderComponent<FetchData>();
        var loggerMock = GetMock<ILogger<FetchDataViewModel>>();

        // Act
        DisposeComponents();

        // Assert
        loggerMock.VerifyLog(LogLevel.Information, expectedLogMessage, Times.Once());
    }
}
