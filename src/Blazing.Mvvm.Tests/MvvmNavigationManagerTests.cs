using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Sample.Wasm.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blazing.Mvvm.Tests;

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
        ArgumentException exception = Assert.Throws<ArgumentException>(() => mvvmNavigationManager.NavigateTo<InvalidViewModel>(false));

        // Assert
        exception.Message.Should().Be($"{typeof(InvalidViewModel)} has no associated page");
    }

    [Fact]
    public void GetUri_GivenInvalidViewModelType_ShouldThrowArgumentException()
    {
        // Arrange
        Mock<NavigationManager> navigationManagerMock = new();
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        MvvmNavigationManager mvvmNavigationManager = new(navigationManagerMock.Object, loggerMock.Object);

        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => mvvmNavigationManager.GetUri<InvalidViewModel>());

        // Assert
        exception.Message.Should().Be($"{typeof(InvalidViewModel)} has no associated page");
    }

    [Fact]
    public void GetUri_GivenValidViewModelType_ShouldReturnUri()
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

    public class InvalidViewModel : ViewModelBase
    {
    }
}