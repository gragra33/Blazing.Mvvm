using Blazing.Mvvm.Components;
using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Tests.UnitTests;

public class MvvmNavigationManagerTests
{
    [Fact]
    public void NavigateTo_GivenInvalidViewModelType_ShouldThrowArgumentException()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        var act = () => mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"{typeof(TestViewModel)} has no associated page");
    }

    [Fact]
    public void GetUri_GivenInvalidViewModelType_ShouldThrowArgumentException()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        var act = () => mvvmNavigationManager.GetUri<TestViewModel>();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"{typeof(TestViewModel)} has no associated page");
    }

    [Fact]
    public void GetUri_GivenValidViewModelType_ShouldReturnUri()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri<CounterViewModel>();

        // Assert
        uri.Should().Be("/counter");
    }

    [Fact]
    public void GetUri_GivenValidViewModelTypeHasOwingComponentParent_ShouldReturnUri()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri<FetchDataViewModel>();

        // Assert
        uri.Should().Be("/fetchdata");
    }

    [Fact]
    public void GetUri_GivenValidIViewModelType_ShouldReturnUri()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri<ITestNavigationViewModel>();

        // Assert
        uri.Should().Be("/test");
    }

    [Fact]
    public void NavigateTo_GivenInvalidKey_ShouldThrowArgumentException()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        var act = () => mvvmNavigationManager.NavigateTo("InvalidKey");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("No associated page for key 'InvalidKey'");
    }

    [Fact]
    public void GetUri_GivenInvalidKey_ShouldThrowArgumentException()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        var act = () => mvvmNavigationManager.GetUri("InvalidKey");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("No associated page for key 'InvalidKey'");
    }

    [Theory]
    [InlineData(nameof(SingletonTestViewModel), "/singleton")]
    [InlineData(nameof(SingletonKeyedTestViewModel), "/singleton-keyed")]
    public void GetUri_GivenValidKey_ShouldReturnUri(string key, string expectedRoute)
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri(key);

        // Assert
        uri.Should().Be(expectedRoute);
    }
}
