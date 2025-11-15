using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Routing;
using Blazing.Mvvm.Tests.Infrastructure.Common;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Blazing.Mvvm.Tests.ComponentTests;

public class MvvmNavLinkTests : ComponentTestBase
{
    private readonly Mock<IViewModelRouteCache> _routeCacheMock;
    private readonly Mock<ILogger<MvvmNavigationManager>> _loggerMock;

    public MvvmNavLinkTests()
    {
        _routeCacheMock = new Mock<IViewModelRouteCache>();
        _loggerMock = new Mock<ILogger<MvvmNavigationManager>>();

        // Setup default route cache behavior
        var viewModelRoutes = new Dictionary<Type, string>
        {
            [typeof(ITestViewModel)] = "/test"
        };
        var keyedRoutes = new Dictionary<object, string>
        {
            ["TestKey"] = "/keyed-test"
        };

        _routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(viewModelRoutes);
        _routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);

        Services.AddSingleton(_routeCacheMock.Object);
        Services.AddSingleton(_loggerMock.Object);
        Services.AddSingleton<IMvvmNavigationManager, MvvmNavigationManager>();
        Services.AddSingleton(Options.Create(new LibraryConfiguration()));
    }

    [Fact]
    public void MvvmNavigationManager_GivenValidViewModel_ShouldGetCorrectUri()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act
        var uri = mvvmNavigationManager.GetUri<ITestViewModel>();

        // Assert
        uri.Should().Be("/test");
    }

    [Fact]
    public void MvvmNavigationManager_GivenValidKey_ShouldGetCorrectUri()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act
        var uri = mvvmNavigationManager.GetUri("TestKey");

        // Assert
        uri.Should().Be("/keyed-test");
    }

    [Fact]
    public void MvvmNavigationManager_GivenValidViewModel_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act
        mvvmNavigationManager.NavigateTo<ITestViewModel>();

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/test");
    }

    [Fact]
    public void MvvmNavigationManager_GivenValidKey_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act
        mvvmNavigationManager.NavigateTo("TestKey");

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/keyed-test");
    }

    [Fact]
    public void MvvmNavigationManager_GivenViewModelWithRelativeUri_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act
        mvvmNavigationManager.NavigateTo<ITestViewModel>("details/123");

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/test/details/123");
    }

    [Fact]
    public void MvvmNavigationManager_GivenViewModelWithQueryString_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act
        mvvmNavigationManager.NavigateTo<ITestViewModel>("?id=123&name=test");

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/test?id=123&name=test");
    }

    [Fact]
    public void MvvmNavigationManager_GivenViewModelWithRelativeUriAndQuery_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act
        mvvmNavigationManager.NavigateTo<ITestViewModel>("details?id=123");

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/test/details?id=123");
    }

    [Fact]
    public void MvvmNavigationManager_GivenKeyWithRelativeUri_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act
        mvvmNavigationManager.NavigateTo("TestKey", "admin/users");

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/keyed-test/admin/users");
    }

    [Fact]
    public void MvvmNavigationManager_GivenInvalidViewModel_ShouldThrowException()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act & Assert
        var act = () => mvvmNavigationManager.NavigateTo<IInvalidViewModel>();
        act.Should().Throw<ViewModelRouteNotFoundException>()
           .WithMessage("*IInvalidViewModel*");
    }

    [Fact]
    public void MvvmNavigationManager_GivenInvalidKey_ShouldThrowException()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act & Assert
        var act = () => mvvmNavigationManager.NavigateTo("InvalidKey");
        act.Should().Throw<ViewModelRouteNotFoundException>()
           .WithMessage("*InvalidKey*");
    }

    [Fact]
    public void MvvmNavigationManager_GivenNavigationOptions_ShouldNavigateWithOptions()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;
        var options = new BrowserNavigationOptions
        {
            ForceLoad = true,
            ReplaceHistoryEntry = true
        };

        // Act
        mvvmNavigationManager.NavigateTo<ITestViewModel>(options);

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/test");
    }

    [Fact] 
    public void MvvmNavigationManager_GivenKeyWithNavigationOptions_ShouldNavigateWithOptions()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;
        var options = new BrowserNavigationOptions
        {
            ForceLoad = false,
            ReplaceHistoryEntry = false
        };

        // Act
        mvvmNavigationManager.NavigateTo("TestKey", options);

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/keyed-test");
    }

    [Fact]
    public void MvvmNavigationManager_GivenRelativeUriWithNavigationOptions_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;
        var options = new BrowserNavigationOptions { ForceLoad = true };

        // Act
        mvvmNavigationManager.NavigateTo<ITestViewModel>("details/456", options);

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/test/details/456");
    }

    [Fact]
    public void MvvmNavigationManager_GivenKeyWithRelativeUriAndOptions_ShouldNavigateCorrectly()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var fakeNavigationManager = Services.GetService<FakeNavigationManager>()!;
        var options = new BrowserNavigationOptions { ReplaceHistoryEntry = true };

        // Act
        mvvmNavigationManager.NavigateTo("TestKey", "settings", options);

        // Assert
        fakeNavigationManager.Uri.Should().Be("http://localhost/keyed-test/settings");
    }

    // Test interfaces
    public interface ITestViewModel : IViewModelBase { }
    public interface IInvalidViewModel : IViewModelBase { }
}