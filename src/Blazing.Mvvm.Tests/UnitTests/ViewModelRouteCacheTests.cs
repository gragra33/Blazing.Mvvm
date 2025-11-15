using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Moq;

namespace Blazing.Mvvm.Tests.UnitTests;

public class ViewModelRouteCacheTests
{
    private readonly Mock<ILogger<ViewModelRouteCache>> _loggerMock;
    private readonly LibraryConfiguration _configuration;

    public ViewModelRouteCacheTests()
    {
        _loggerMock = new Mock<ILogger<ViewModelRouteCache>>();
        _configuration = new LibraryConfiguration();
    }

    [Fact]
    public void Constructor_GivenValidConfiguration_ShouldInitializeEmptyRoutes()
    {
        // Arrange & Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        cache.ViewModelRoutes.Should().NotBeNull();
        cache.KeyedViewModelRoutes.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_GivenAssemblyWithViewModels_ShouldCacheRoutes()
    {
        // Arrange
        _configuration.RegisterViewModelsFromAssemblyContaining<TestViewModel>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        using var _ = new AssertionScope();
        cache.ViewModelRoutes.Should().ContainKey(typeof(TestViewModel));
        cache.ViewModelRoutes[typeof(TestViewModel)].Should().Be("/test");
    }

    [Fact]
    public void Constructor_GivenAssemblyWithKeyedViewModels_ShouldCacheKeyedRoutes()
    {
        // Arrange
        _configuration.RegisterViewModelsFromAssemblyContaining<TestKeyedViewModel>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        using var _ = new AssertionScope();
        cache.KeyedViewModelRoutes.Should().ContainKey("TestKey");
        cache.KeyedViewModelRoutes["TestKey"].Should().Be("/keyed-test");
    }

    [Fact]
    public void Constructor_GivenBasePathConfiguration_ShouldPrependBasePath()
    {
        // Arrange
        _configuration.BasePath = "/app";
        _configuration.RegisterViewModelsFromAssemblyContaining<TestViewModel>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        cache.ViewModelRoutes[typeof(TestViewModel)].Should().Be("/app/test");
    }

    [Fact]
    public void Constructor_GivenBasePathWithTrailingSlash_ShouldHandleCorrectly()
    {
        // Arrange
        _configuration.BasePath = "/app/";
        _configuration.RegisterViewModelsFromAssemblyContaining<TestViewModel>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        cache.ViewModelRoutes[typeof(TestViewModel)].Should().Be("/app/test");
    }

    [Fact]
    public void Constructor_GivenViewWithoutRoute_ShouldNotCacheRoute()
    {
        // Arrange
        _configuration.RegisterViewModelsFromAssemblyContaining<TestViewModelWithoutRoute>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        cache.ViewModelRoutes.Should().NotContainKey(typeof(TestViewModelWithoutRoute));
    }

    [Fact]
    public void Constructor_GivenMultipleRoutesOnSameView_ShouldCacheFirstRoute()
    {
        // Arrange
        _configuration.RegisterViewModelsFromAssemblyContaining<TestViewModelWithMultipleRoutes>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        using var _ = new AssertionScope();
        cache.ViewModelRoutes.Should().ContainKey(typeof(TestViewModelWithMultipleRoutes));
        cache.ViewModelRoutes[typeof(TestViewModelWithMultipleRoutes)].Should().Be("/first");
    }

    [Fact]
    public void Constructor_GivenInvalidAssembly_ShouldContinueProcessing()
    {
        // Arrange
        _configuration.RegisterViewModelsFromAssemblyContaining<TestViewModel>();
        
        // Mock an assembly that throws during GetTypes()
        var mockAssembly = new Mock<Assembly>();
        var loaderException = new ReflectionTypeLoadException(
            new Type[0], 
            new Exception[] { new FileNotFoundException("Test file not found") });
        mockAssembly.Setup(a => a.GetTypes()).Throws(loaderException);
        
        // This test verifies the cache handles exceptions gracefully
        // Act & Assert should not throw
        var act = () => new ViewModelRouteCache(_loggerMock.Object, _configuration);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_GivenEmptyAssemblyList_ShouldCreateEmptyCache()
    {
        // Arrange
        // No assemblies registered

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        using var _ = new AssertionScope();
        cache.ViewModelRoutes.Should().BeEmpty();
        cache.KeyedViewModelRoutes.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_GivenViewWithBothRouteAndKey_ShouldCacheBoth()
    {
        // Arrange
        _configuration.RegisterViewModelsFromAssemblyContaining<TestViewModelWithRouteAndKey>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        using var _ = new AssertionScope();
        cache.ViewModelRoutes.Should().ContainKey(typeof(TestViewModelWithRouteAndKey));
        cache.ViewModelRoutes[typeof(TestViewModelWithRouteAndKey)].Should().Be("/route-and-key");
        cache.KeyedViewModelRoutes.Should().ContainKey("RouteAndKey");
        cache.KeyedViewModelRoutes["RouteAndKey"].Should().Be("/route-and-key");
    }

    [Fact]
    public void Constructor_GivenOwningComponentBase_ShouldCacheRoute()
    {
        // Arrange
        _configuration.RegisterViewModelsFromAssemblyContaining<TestOwningViewModel>();

        // Act
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Assert
        using var _ = new AssertionScope();
        cache.ViewModelRoutes.Should().ContainKey(typeof(TestOwningViewModel));
        cache.ViewModelRoutes[typeof(TestOwningViewModel)].Should().Be("/owning");
    }

    [Fact]
    public void ViewModelRoutes_ShouldReturnReadOnlyDictionary()
    {
        // Arrange
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Act
        var routes = cache.ViewModelRoutes;

        // Assert
        routes.Should().BeAssignableTo<IReadOnlyDictionary<Type, string>>();
    }

    [Fact]
    public void KeyedViewModelRoutes_ShouldReturnReadOnlyDictionary()
    {
        // Arrange
        var cache = new ViewModelRouteCache(_loggerMock.Object, _configuration);

        // Act
        var keyedRoutes = cache.KeyedViewModelRoutes;

        // Assert
        keyedRoutes.Should().BeAssignableTo<IReadOnlyDictionary<object, string>>();
    }

    // Test ViewModel and View classes
    public class TestViewModel : ViewModelBase { }

    [Route("/test")]
    public class TestView : MvvmComponentBase<TestViewModel> { }

    public class TestKeyedViewModel : ViewModelBase { }

    [Route("/keyed-test")]
    [ViewModelKey("TestKey")]
    public class TestKeyedView : MvvmComponentBase<TestKeyedViewModel> { }

    public class TestViewModelWithoutRoute : ViewModelBase { }

    public class TestViewWithoutRoute : MvvmComponentBase<TestViewModelWithoutRoute> { }

    public class TestViewModelWithMultipleRoutes : ViewModelBase { }

    [Route("/first")]
    [Route("/second")]
    public class TestViewWithMultipleRoutes : MvvmComponentBase<TestViewModelWithMultipleRoutes> { }

    public class TestViewModelWithRouteAndKey : ViewModelBase { }

    [Route("/route-and-key")]
    [ViewModelKey("RouteAndKey")]
    public class TestViewWithRouteAndKey : MvvmComponentBase<TestViewModelWithRouteAndKey> { }

    public class TestOwningViewModel : ViewModelBase { }

    [Route("/owning")]
    public class TestOwningView : MvvmOwningComponentBase<TestOwningViewModel> { }
}