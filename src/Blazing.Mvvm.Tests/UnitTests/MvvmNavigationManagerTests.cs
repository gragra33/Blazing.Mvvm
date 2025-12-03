using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Routing;
using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable CS0618 // Type or member is obsolete - Testing obsolete BasePath property

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Test double for <see cref="NavigationManager"/> that allows verification of navigation calls.
/// </summary>
public class TestNavigationManager : NavigationManager
{
    private readonly List<NavigationCall> _navigationCalls = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TestNavigationManager"/> class with the specified base URI.
    /// </summary>
    /// <param name="baseUri">The base URI for navigation.</param>
    public TestNavigationManager(string baseUri)
    {
        Initialize(baseUri, baseUri);
    }
    
    /// <summary>
    /// Records navigation calls for verification.
    /// </summary>
    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        _navigationCalls.Add(new NavigationCall(uri, forceLoad, false));
    }
    
    /// <summary>
    /// Sets the base URI for navigation.
    /// </summary>
    /// <param name="baseUri">The new base URI.</param>
    public void SetBaseUri(string baseUri)
    {
        Initialize(baseUri, baseUri);
    }
    
    /// <summary>
    /// Verifies that a navigation call was made to the expected URI with the specified options.
    /// </summary>
    public void VerifyNavigateTo(string expectedUri, bool expectedForceLoad = false, bool expectedReplace = false)
    {
        var matching = _navigationCalls.Where(call => call.Uri == expectedUri && 
                                                    call.ForceLoad == expectedForceLoad && 
                                                    call.Replace == expectedReplace).ToList();
        if (matching.Count != 1)
        {
            throw new InvalidOperationException($"Expected exactly one navigation call to '{expectedUri}' with forceLoad={expectedForceLoad}, replace={expectedReplace}, but found {matching.Count}. All calls: {string.Join(", ", _navigationCalls.Select(c => $"'{c.Uri}' (forceLoad={c.ForceLoad}, replace={c.Replace})"))}");
        }
    }
    
    /// <summary>
    /// Verifies that no navigation calls were made.
    /// </summary>
    public void VerifyNoNavigationCalls()
    {
        if (_navigationCalls.Any())
        {
            throw new InvalidOperationException($"Expected no navigation calls, but found {_navigationCalls.Count}: {string.Join(", ", _navigationCalls.Select(c => $"'{c.Uri}'"))}");
        }
    }
    
    /// <summary>
    /// Clears all recorded navigation calls.
    /// </summary>
    public void ClearNavigationCalls()
    {
        _navigationCalls.Clear();
    }
    
    /// <summary>
    /// Represents a navigation call for verification.
    /// </summary>
    public record NavigationCall(string Uri, bool ForceLoad, bool Replace);
}

/// <summary>
/// Unit tests for <see cref="MvvmNavigationManager"/> covering navigation, URI resolution, and base path handling scenarios.
/// </summary>
public class MvvmNavigationManagerTests
{
    /// <summary>
    /// Tests that navigating to an invalid view model type throws <see cref="ViewModelRouteNotFoundException"/>.
    /// </summary>
    [Fact]
    public void NavigateTo_GivenInvalidViewModelType_ShouldThrowViewModelRouteNotFoundException()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(new Dictionary<Type, string>());
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        var act = () => mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        act.Should().Throw<ViewModelRouteNotFoundException>()
            .WithMessage($"{typeof(TestViewModel)} has no associated page");
    }

    /// <summary>
    /// Tests that getting a URI for an invalid view model type throws <see cref="ViewModelRouteNotFoundException"/>.
    /// </summary>
    [Fact]
    public void GetUri_GivenInvalidViewModelType_ShouldThrowViewModelRouteNotFoundException()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(new Dictionary<Type, string>());
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        var act = () => mvvmNavigationManager.GetUri<TestViewModel>();

        // Assert
        act.Should().Throw<ViewModelRouteNotFoundException>()
            .WithMessage($"{typeof(TestViewModel)} has no associated page");
    }

    /// <summary>
    /// Tests that getting a URI for a valid view model type returns the expected URI.
    /// </summary>
    [Fact]
    public void GetUri_GivenValidViewModelType_ShouldReturnUri()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(CounterViewModel)] = "/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri<CounterViewModel>();

        // Assert
        uri.Should().Be("counter");
    }

    /// <summary>
    /// Tests that getting a URI for a valid view model type with an owning component parent returns the expected URI.
    /// </summary>
    [Fact]
    public void GetUri_GivenValidViewModelTypeHasOwingComponentParent_ShouldReturnUri()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(FetchDataViewModel)] = "/fetchdata" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri<FetchDataViewModel>();

        // Assert
        uri.Should().Be("fetchdata");
    }

    /// <summary>
    /// Tests that getting a URI for a valid interface view model type returns the expected URI.
    /// </summary>
    [Fact]
    public void GetUri_GivenValidIViewModelType_ShouldReturnUri()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(Blazing.Mvvm.Tests.Infrastructure.Fakes.ITestNavigationViewModel)] = "/test" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri<Blazing.Mvvm.Tests.Infrastructure.Fakes.ITestNavigationViewModel>();

        // Assert
        uri.Should().Be("test");
    }

    /// <summary>
    /// Tests that navigating to an invalid key throws <see cref="ViewModelRouteNotFoundException"/>.
    /// </summary>
    [Fact]
    public void NavigateTo_GivenInvalidKey_ShouldThrowViewModelRouteNotFoundException()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(new Dictionary<object, string>());
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        var act = () => mvvmNavigationManager.NavigateTo("InvalidKey");

        // Assert
        act.Should().Throw<ViewModelRouteNotFoundException>()
            .WithMessage("No associated page for key 'InvalidKey'");
    }

    /// <summary>
    /// Tests that getting a URI for an invalid key throws <see cref="ViewModelRouteNotFoundException"/>.
    /// </summary>
    [Fact]
    public void GetUri_GivenInvalidKey_ShouldThrowViewModelRouteNotFoundException()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(new Dictionary<object, string>());
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        var act = () => mvvmNavigationManager.GetUri("InvalidKey");

        // Assert
        act.Should().Throw<ViewModelRouteNotFoundException>()
            .WithMessage("No associated page for key 'InvalidKey'");
    }

    /// <summary>
    /// Tests that getting a URI for a valid key returns the expected route.
    /// </summary>
    [Theory]
    [InlineData(nameof(SingletonTestViewModel), "singleton")]
    [InlineData(nameof(SingletonKeyedTestViewModel), "singleton-keyed")]
    public void GetUri_GivenValidKey_ShouldReturnUri(string key, string expectedRoute)
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var keyedRoutes = new Dictionary<object, string> { [key] = expectedRoute };
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        string uri = mvvmNavigationManager.GetUri(key);

        // Assert
        uri.Should().Be(expectedRoute);
    }

    #region URI Resolution Tests for Regular Hosting (No BasePath)

    /// <summary>
    /// Tests that navigating to the root route in regular hosting navigates to the root path.
    /// </summary>
    [Fact]
    public void NavigateTo_RegularHosting_RootRoute_ShouldNavigateToRootPath()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = null });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("/", false, false);
    }

    /// <summary>
    /// Tests that navigating to an absolute route in regular hosting navigates to the relative path.
    /// </summary>
    [Fact]
    public void NavigateTo_RegularHosting_AbsoluteRoute_ShouldNavigateToRelativePath()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = null });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that navigating to a nested route in regular hosting navigates to the relative path.
    /// </summary>
    [Fact]
    public void NavigateTo_RegularHosting_NestedRoute_ShouldNavigateToRelativePath()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/test/nested" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = null });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("test/nested", false, false);
    }

    #endregion

    #region URI Resolution Tests for Subpath Hosting (With BasePath)

    /// <summary>
    /// Tests that navigating to the base path root in subpath hosting navigates to the empty path.
    /// </summary>
    [Fact]
    public void NavigateTo_SubpathHosting_BasePathRoot_ShouldNavigateToEmptyPath()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("", false, false);
    }

    /// <summary>
    /// Tests that navigating to a base path route in subpath hosting navigates to the relative path.
    /// </summary>
    [Fact]
    public void NavigateTo_SubpathHosting_BasePathRoute_ShouldNavigateToRelativePath()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that navigating to a hex translate route in subpath hosting works correctly.
    /// </summary>
    [Fact]
    public void NavigateTo_SubpathHosting_HexTranslateRoute_ShouldNavigateCorrectly()
    {
        // Arrange - This tests the specific failing scenario from user's description
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/hextranslate" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should navigate to "hextranslate", NOT "test/hextranslate"
        navigationManager.VerifyNavigateTo("hextranslate", false, false);
    }

    /// <summary>
    /// Tests that navigating to a test route in subpath hosting works correctly.
    /// </summary>
    [Fact]
    public void NavigateTo_SubpathHosting_TestRoute_ShouldNavigateCorrectly()
    {
        // Arrange - This tests the specific failing scenario from user's description
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/test" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should navigate to "test", NOT "test/test"
        navigationManager.VerifyNavigateTo("test", false, false);
    }

    /// <summary>
    /// Tests that navigating to a nested route in subpath hosting navigates to the relative path.
    /// </summary>
    [Fact]
    public void NavigateTo_SubpathHosting_NestedRoute_ShouldNavigateToRelativePath()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/nested/route" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("nested/route", false, false);
    }

    #endregion

    #region Keyed Navigation URI Resolution Tests

    /// <summary>
    /// Tests that keyed navigation in regular hosting navigates to the relative path.
    /// </summary>
    [Fact]
    public void NavigateTo_KeyedNavigation_RegularHosting_ShouldNavigateToRelativePath()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var keyedRoutes = new Dictionary<object, string> { ["TestKey"] = "/keyedtest" };
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = null });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo("TestKey");

        // Assert
        navigationManager.VerifyNavigateTo("keyedtest", false, false);
    }

    /// <summary>
    /// Tests that keyed navigation in subpath hosting navigates correctly.
    /// </summary>
    [Fact]
    public void NavigateTo_KeyedNavigation_SubpathHosting_ShouldNavigateCorrectly()
    {
        // Arrange - This tests keyed navigation in subpath hosting scenarios
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var keyedRoutes = new Dictionary<object, string> { ["TestKey"] = "/fu/bar/keyedtest" };
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo("TestKey");

        // Assert - Should navigate to "keyedtest", NOT "test/keyedtest"
        navigationManager.VerifyNavigateTo("keyedtest", false, false);
    }

    #endregion

    #region Edge Cases for BasePath Handling

    /// <summary>
    /// Tests that navigating with a base path without a trailing slash works.
    /// </summary>
    [Fact]
    public void NavigateTo_SubpathHosting_BasePathWithoutTrailingSlash_ShouldWork()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar" }); // No trailing slash
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that navigating with a base path without a leading slash works.
    /// </summary>
    [Fact]
    public void NavigateTo_SubpathHosting_BasePathWithoutLeadingSlash_ShouldWork()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "fu/bar/" }); // No leading slash
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that navigating when the route doesn't match the base path navigates as a relative path.
    /// </summary>
    [Fact]
    public void NavigateTo_RouteNotMatchingBasePath_ShouldNavigateAsRelativePath()
    {
        // Arrange - Route doesn't match configured BasePath (configuration mismatch scenario)
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/different/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/fu/bar/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should still strip leading slash and navigate to relative path
        navigationManager.VerifyNavigateTo("different/counter", false, false);
    }

    #endregion

    #region Dynamic Base Path Detection Tests

    /// <summary>
    /// Tests that dynamic base path is detected from NavigationManager.BaseUri when no BasePath is configured.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_NoConfiguredBasePath_ShouldDetectFromNavigationManager()
    {
        // Arrange - NavigationManager has base path, but config doesn't
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = null }); // No configured base path
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should detect "fu/bar" from NavigationManager and navigate to "counter"
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that dynamic base path detection works with YARP-style subpath hosting.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_YarpStyleHosting_ShouldDetectCorrectly()
    {
        // Arrange - Simulating YARP reverse proxy with subpath
        var navigationManager = new TestNavigationManager("https://localhost:7037/api/v1/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/api/v1/users" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration()); // No configured base path
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should detect "api/v1" and navigate to "users"
        navigationManager.VerifyNavigateTo("users", false, false);
    }

    /// <summary>
    /// Tests that configured BasePath takes priority over dynamically detected base path.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_ConfiguredBasePathTakesPriority_ShouldUseConfigured()
    {
        // Arrange - Both NavigationManager and config have base paths (config should win)
        var navigationManager = new TestNavigationManager("https://localhost:7037/detected/path/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/configured/path/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "/configured/path/" });
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should use configured path, resulting in "counter"
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that dynamic base path detection handles root hosting correctly.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_RootHosting_ShouldNavigateCorrectly()
    {
        // Arrange - Root hosting (no subpath)
        var navigationManager = new TestNavigationManager("https://localhost:7037/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration()); // No configured base path
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should detect no base path and navigate to "counter"
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that dynamic base path detection works with deeply nested paths.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_DeeplyNestedPath_ShouldDetectCorrectly()
    {
        // Arrange - Multi-level base path
        var navigationManager = new TestNavigationManager("https://localhost:7037/app/tenant/123/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/app/tenant/123/dashboard" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration()); // No configured base path
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should detect "app/tenant/123" and navigate to "dashboard"
        navigationManager.VerifyNavigateTo("dashboard", false, false);
    }

    /// <summary>
    /// Tests that dynamic base path detection works with keyed navigation.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_KeyedNavigation_ShouldDetectCorrectly()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var keyedRoutes = new Dictionary<object, string> { ["TestKey"] = "/fu/bar/keyed-route" };
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration()); // No configured base path
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo("TestKey");

        // Assert - Should detect "fu/bar" and navigate to "keyed-route"
        navigationManager.VerifyNavigateTo("keyed-route", false, false);
    }

    /// <summary>
    /// Tests that dynamic base path detection handles empty configured BasePath as null.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_EmptyConfiguredBasePath_ShouldDetectFromNavigationManager()
    {
        // Arrange - Empty string BasePath should be treated as not configured
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/fu/bar/counter" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration { BasePath = "" }); // Empty string
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should fall back to dynamic detection
        navigationManager.VerifyNavigateTo("counter", false, false);
    }

    /// <summary>
    /// Tests that dynamic base path detection handles the root route correctly.
    /// </summary>
    [Fact]
    public void NavigateTo_DynamicBasePath_RootRoute_ShouldNavigateToBaseUri()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost:7037/fu/bar/");
        Mock<ILogger<MvvmNavigationManager>> loggerMock = new();
        Mock<IViewModelRouteCache> routeCacheMock = new();
        Mock<IOptions<LibraryConfiguration>> configMock = new();
        
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration()); // No configured base path
        
        MvvmNavigationManager mvvmNavigationManager = new(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>();

        // Assert - Should navigate to the base path
        navigationManager.VerifyNavigateTo("/fu/bar/", false, false);
    }

    #endregion
}
