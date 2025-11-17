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

    /// <summary>
    /// Initializes a new instance of the <see cref="MvvmNavLinkTests"/> class and sets up route cache and navigation manager mocks.
    /// </summary>
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

    /// <summary>
    /// Verifies that GetUri returns the correct URI for a valid ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that GetUri returns the correct URI for a valid key.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI for a valid ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI for a valid key.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI with a relative path for a ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI with a query string for a ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI with a relative path and query string for a ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI with a relative path for a key.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo throws an exception for an invalid ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo throws an exception for an invalid key.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates with browser navigation options for a ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates with browser navigation options for a key.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI with a relative path and browser navigation options for a ViewModel type.
    /// </summary>
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

    /// <summary>
    /// Verifies that NavigateTo navigates to the correct URI with a relative path and browser navigation options for a key.
    /// </summary>
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

    /// <summary>
    /// Test interface for a valid ViewModel.
    /// </summary>
    public interface ITestViewModel : IViewModelBase { }
    /// <summary>
    /// Test interface for an invalid ViewModel.
    /// </summary>
    public interface IInvalidViewModel : IViewModelBase { }
}