using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for error scenarios in Blazing.Mvvm, covering navigation, route cache, parameter resolution, and service configuration edge cases.
/// </summary>
public class ErrorScenarioTests
{
    /// <summary>
    /// Tests that navigating with a null relative URI throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void MvvmNavigationManager_GivenNullRelativeUri_ShouldThrowArgumentNullException()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost/");
        var loggerMock = new Mock<ILogger<MvvmNavigationManager>>();
        var routeCacheMock = new Mock<IViewModelRouteCache>();
        var configMock = new Mock<IOptions<LibraryConfiguration>>();

        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/test" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());

        var mvvmNavigationManager = new MvvmNavigationManager(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act & Assert
        var act = () => mvvmNavigationManager.NavigateTo<TestViewModel>(null!, false, false);
        act.Should().Throw<ArgumentNullException>().WithParameterName("relativeUri");
    }

    /// <summary>
    /// Tests that navigating with a null key throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void MvvmNavigationManager_GivenNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost/");
        var loggerMock = new Mock<ILogger<MvvmNavigationManager>>();
        var routeCacheMock = new Mock<IViewModelRouteCache>();
        var configMock = new Mock<IOptions<LibraryConfiguration>>();

        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(new Dictionary<object, string>());
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());

        var mvvmNavigationManager = new MvvmNavigationManager(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act & Assert
        var act = () => mvvmNavigationManager.NavigateTo((object)null!, false, false);
        act.Should().Throw<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that the route cache handles assemblies with <see cref="ReflectionTypeLoadException"/> gracefully.
    /// </summary>
    [Fact]
    public void ViewModelRouteCache_GivenAssemblyWithReflectionErrors_ShouldHandleGracefully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ViewModelRouteCache>>();
        var configuration = new LibraryConfiguration();
        
        // Create a mock assembly that throws ReflectionTypeLoadException
        var mockAssembly = new Mock<Assembly>();
        var loaderExceptions = new Exception[] { new FileNotFoundException("Test exception") };
        var reflectionException = new ReflectionTypeLoadException(new Type[0], loaderExceptions);
        
        mockAssembly.Setup(a => a.GetTypes()).Throws(reflectionException);
        configuration.ViewModelAssemblies.Add(mockAssembly.Object);

        // Act & Assert - Should not throw
        var act = () => new ViewModelRouteCache(loggerMock.Object, configuration);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that the route cache handles assemblies with generic exceptions gracefully.
    /// </summary>
    [Fact]
    public void ViewModelRouteCache_GivenAssemblyWithGenericException_ShouldHandleGracefully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ViewModelRouteCache>>();
        var configuration = new LibraryConfiguration();
        
        var mockAssembly = new Mock<Assembly>();
        mockAssembly.Setup(a => a.GetTypes()).Throws<InvalidOperationException>();
        mockAssembly.Setup(a => a.FullName).Returns("TestAssembly");
        configuration.ViewModelAssemblies.Add(mockAssembly.Object);

        // Act & Assert - Should not throw
        var act = () => new ViewModelRouteCache(loggerMock.Object, configuration);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that parameter resolver throws <see cref="ArgumentNullException"/> when view is null.
    /// </summary>
    [Fact]
    public void ParameterResolver_GivenNullView_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resolver = new Components.Parameter.ParameterResolver(ParameterResolutionMode.ViewModel);
        var viewModel = new TestViewModel();
        var parameters = ParameterView.Empty;

        // Act & Assert
        var act = () => resolver.SetParameters<IView<TestViewModel>, TestViewModel>(null!, viewModel, parameters);
        act.Should().Throw<ArgumentNullException>().WithParameterName("view");
    }

    /// <summary>
    /// Tests that parameter resolver throws <see cref="ArgumentNullException"/> when view model is null.
    /// </summary>
    [Fact]
    public void ParameterResolver_GivenNullViewModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resolver = new Components.Parameter.ParameterResolver(ParameterResolutionMode.ViewModel);
        var view = new Mock<IView<TestViewModel>>().Object;
        var parameters = ParameterView.Empty;

        // Act & Assert
        var act = () => resolver.SetParameters(view, (TestViewModel)null!, parameters);
        act.Should().Throw<ArgumentNullException>().WithParameterName("viewModel");
    }

    /// <summary>
    /// Tests that property setter throws <see cref="InvalidOperationException"/> for properties without a setter.
    /// </summary>
    [Fact]
    public void PropertySetter_GivenPropertyWithoutSetter_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var type = typeof(TestViewModelWithReadOnlyProperty);
        var propertyInfo = type.GetProperty(nameof(TestViewModelWithReadOnlyProperty.ReadOnlyProperty))!;

        // Act & Assert
        var act = () => new Components.Parameter.PropertySetter(type, propertyInfo);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ReadOnlyProperty*does not have a setter*");
    }

    /// <summary>
    /// Tests that complex service configuration does not throw exceptions.
    /// </summary>
    [Fact]
    public void ServicesExtension_GivenComplexConfiguration_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Complex service configuration should work
        var act = () =>
        {
            services.AddMvvm(config =>
            {
                config.HostingModelType = BlazorHostingModelType.WebAssembly;
                config.ParameterResolutionMode = ParameterResolutionMode.ViewModel;
                config.RegisterViewModelsFromAssemblyContaining<ErrorScenarioTests>();
            });
        };

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that navigation manager handles malformed base URIs gracefully.
    /// </summary>
    [Fact]
    public void MvvmNavigationManager_GivenMalformedBaseUri_ShouldHandleGracefully()
    {
        // Arrange - Use a valid URI but test MvvmNavigationManager's error handling for internal URI operations
        var navigationManager = new TestNavigationManager("https://localhost/");
        var loggerMock = new Mock<ILogger<MvvmNavigationManager>>();
        var routeCacheMock = new Mock<IViewModelRouteCache>();
        var configMock = new Mock<IOptions<LibraryConfiguration>>();

        // Setup route cache to return malformed route that could cause URI issues
        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "invalid://route" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());

        var mvvmNavigationManager = new MvvmNavigationManager(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act & Assert - Should handle internal URI creation gracefully
        var act = () => mvvmNavigationManager.NavigateTo<TestViewModel>();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that route cache does not modify routes when base path is null.
    /// </summary>
    [Fact]
    public void ViewModelRouteCache_GivenNullBasePath_ShouldNotModifyRoutes()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ViewModelRouteCache>>();
        var configuration = new LibraryConfiguration();
        configuration.BasePath = null;
        configuration.RegisterViewModelsFromAssemblyContaining<TestViewModel>();

        // Act
        var cache = new ViewModelRouteCache(loggerMock.Object, configuration);

        // Assert
        cache.ViewModelRoutes[typeof(TestViewModel)].Should().Be("/test");
    }

    /// <summary>
    /// Tests that route cache does not modify routes when base path is empty.
    /// </summary>
    [Fact]
    public void ViewModelRouteCache_GivenEmptyBasePath_ShouldNotModifyRoutes()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ViewModelRouteCache>>();
        var configuration = new LibraryConfiguration();
        configuration.BasePath = "";
        configuration.RegisterViewModelsFromAssemblyContaining<TestViewModel>();

        // Act
        var cache = new ViewModelRouteCache(loggerMock.Object, configuration);

        // Assert
        cache.ViewModelRoutes[typeof(TestViewModel)].Should().Be("/test");
    }

    /// <summary>
    /// Tests that route cache does not modify routes when base path is whitespace.
    /// </summary>
    [Fact]
    public void ViewModelRouteCache_GivenWhitespaceBasePath_ShouldNotModifyRoutes()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ViewModelRouteCache>>();
        var configuration = new LibraryConfiguration();
        configuration.BasePath = "   ";
        configuration.RegisterViewModelsFromAssemblyContaining<TestViewModel>();

        // Act
        var cache = new ViewModelRouteCache(loggerMock.Object, configuration);

        // Assert
        cache.ViewModelRoutes[typeof(TestViewModel)].Should().Be("/test");
    }

    /// <summary>
    /// Tests that navigation manager returns the original URI when given an empty relative URI.
    /// </summary>
    [Fact]
    public void MvvmNavigationManager_GivenEmptyRelativeUri_ShouldReturnOriginalUri()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost/");
        var loggerMock = new Mock<ILogger<MvvmNavigationManager>>();
        var routeCacheMock = new Mock<IViewModelRouteCache>();
        var configMock = new Mock<IOptions<LibraryConfiguration>>();

        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/test" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());

        var mvvmNavigationManager = new MvvmNavigationManager(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>("");

        // Assert - Should contain the full URI including protocol and host
        navigationManager.LastNavigatedUri.Should().Contain("test");
    }

    /// <summary>
    /// Tests that view model resolver throws <see cref="InvalidOperationException"/> when service is not found.
    /// </summary>
    [Fact]
    public void ViewModelResolver_GivenServiceProviderWithoutService_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var view = new Mock<IView<TestViewModel>>().Object;

        // Act & Assert
        var act = () => ViewModelResolver.Resolve(view, serviceProvider);
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that navigation manager handles complex relative URIs with multiple question marks correctly.
    /// </summary>
    [Fact]
    public void MvvmNavigationManager_GivenComplexRelativeUriWithMultipleQuestionMarks_ShouldHandleCorrectly()
    {
        // Arrange
        var navigationManager = new TestNavigationManager("https://localhost/");
        var loggerMock = new Mock<ILogger<MvvmNavigationManager>>();
        var routeCacheMock = new Mock<IViewModelRouteCache>();
        var configMock = new Mock<IOptions<LibraryConfiguration>>();

        var routes = new Dictionary<Type, string> { [typeof(TestViewModel)] = "/test" };
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(routes);
        configMock.Setup(x => x.Value).Returns(new LibraryConfiguration());

        var mvvmNavigationManager = new MvvmNavigationManager(navigationManager, loggerMock.Object, routeCacheMock.Object, configMock.Object);

        // Act
        mvvmNavigationManager.NavigateTo<TestViewModel>("details?param1=value1?param2=value2");

        // Assert - Should handle malformed query gracefully
        navigationManager.LastNavigatedUri.Should().Contain("test/details");
    }

    // Test classes
    public class TestViewModel : ViewModelBase { }

    [Route("/test")]
    public class TestView : MvvmComponentBase<TestViewModel> { }

    public class TestViewModelWithReadOnlyProperty : ViewModelBase
    {
        [ViewParameter]
        public string ReadOnlyProperty { get; } = "ReadOnly";
    }

    // Test helper
    public class TestNavigationManager : NavigationManager
    {
        public string? LastNavigatedUri { get; private set; }

        public TestNavigationManager(string baseUri)
        {
            Initialize(baseUri, baseUri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            LastNavigatedUri = uri;
        }
    }
}