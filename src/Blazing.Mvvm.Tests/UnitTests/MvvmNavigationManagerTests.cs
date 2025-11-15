using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Routing;
using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blazing.Mvvm.Tests.UnitTests;

// Test double for NavigationManager that allows verification of navigation calls
public class TestNavigationManager : NavigationManager
{
    private readonly List<NavigationCall> _navigationCalls = new();
    
    public TestNavigationManager(string baseUri)
    {
        Initialize(baseUri, baseUri);
    }
    
    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        _navigationCalls.Add(new NavigationCall(uri, forceLoad, false));
    }
    
    public void SetBaseUri(string baseUri)
    {
        Initialize(baseUri, baseUri);
    }
    
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
    
    public void VerifyNoNavigationCalls()
    {
        if (_navigationCalls.Any())
        {
            throw new InvalidOperationException($"Expected no navigation calls, but found {_navigationCalls.Count}: {string.Join(", ", _navigationCalls.Select(c => $"'{c.Uri}'"))}");
        }
    }
    
    public void ClearNavigationCalls()
    {
        _navigationCalls.Clear();
    }
    
    public record NavigationCall(string Uri, bool ForceLoad, bool Replace);
}

public class MvvmNavigationManagerTests
{
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
        uri.Should().Be("/counter");
    }

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
        uri.Should().Be("/fetchdata");
    }

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
        uri.Should().Be("/test");
    }

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

    [Theory]
    [InlineData(nameof(SingletonTestViewModel), "/singleton")]
    [InlineData(nameof(SingletonKeyedTestViewModel), "/singleton-keyed")]
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
}
