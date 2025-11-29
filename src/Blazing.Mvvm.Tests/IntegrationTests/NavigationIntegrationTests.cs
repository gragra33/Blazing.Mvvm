using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Routing;
using Blazing.Mvvm.Tests.Infrastructure.Common;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Blazing.Mvvm.Tests.IntegrationTests;

/// <summary>
/// Integration tests for Blazing.Mvvm navigation scenarios, including route discovery, navigation flows, parameter binding, and error handling.
/// </summary>
public class NavigationIntegrationTests : ComponentTestBase
{
    private readonly Mock<ILogger<ViewModelRouteCache>> _routeCacheLoggerMock;
    private readonly Mock<ILogger<MvvmNavigationManager>> _navManagerLoggerMock;
    private readonly Mock<IViewModelRouteCache> _routeCacheMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationIntegrationTests"/> class and sets up test services and route cache mocks.
    /// </summary>
    public NavigationIntegrationTests()
    {
        _routeCacheLoggerMock = new Mock<ILogger<ViewModelRouteCache>>();
        _navManagerLoggerMock = new Mock<ILogger<MvvmNavigationManager>>();
        _routeCacheMock = new Mock<IViewModelRouteCache>();

        // Setup mock route cache with test routes
        var viewModelRoutes = new Dictionary<Type, string>
        {
            [typeof(IHomeViewModel)] = "/",
            [typeof(IProductViewModel)] = "/products",
            [typeof(IAdminViewModel)] = "/admin"
        };
        var keyedRoutes = new Dictionary<object, string>
        {
            ["Admin"] = "/admin"
        };

        _routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(viewModelRoutes);
        _routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);

        // Setup services
        Services.AddSingleton(Options.Create(new LibraryConfiguration()));
        Services.AddSingleton(_routeCacheLoggerMock.Object);
        Services.AddSingleton(_navManagerLoggerMock.Object);
        Services.AddSingleton(_routeCacheMock.Object);
        Services.AddSingleton<IMvvmNavigationManager, MvvmNavigationManager>();
        Services.AddSingleton<IHomeViewModel, HomeViewModel>();
        Services.AddSingleton<IProductViewModel, ProductViewModel>();
        Services.AddKeyedSingleton<IAdminViewModel, AdminViewModel>("Admin");
    }

    /// <summary>
    /// Tests that full navigation flow works for a complete setup and navigates to the expected URI.
    /// </summary>
    [Fact]
    public void FullNavigationFlow_GivenCompleteSetup_ShouldNavigateSuccessfully()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act - Navigate using MVVM navigation manager
        mvvmNavigationManager.NavigateTo<IHomeViewModel>();

        // Assert
        navigationManager.Uri.Should().Be("http://localhost/");
    }

    /// <summary>
    /// Tests navigation with a parameterized route and verifies the resulting URI.
    /// </summary>
    [Fact]
    public void FullNavigationFlow_GivenParameterizedRoute_ShouldNavigateWithParameters()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act - Navigate with relative URI
        mvvmNavigationManager.NavigateTo<IProductViewModel>("123");

        // Assert
        navigationManager.Uri.Should().Be("http://localhost/products/123");
    }

    /// <summary>
    /// Tests navigation with query parameters and verifies the resulting URI.
    /// </summary>
    [Fact]
    public void FullNavigationFlow_GivenQueryParameters_ShouldNavigateWithQuery()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act - Navigate with query string
        mvvmNavigationManager.NavigateTo<IProductViewModel>("?category=electronics&sort=name");

        // Assert
        navigationManager.Uri.Should().Be("http://localhost/products?category=electronics&sort=name");
    }

    /// <summary>
    /// Tests keyed navigation and verifies the resulting URI.
    /// </summary>
    [Fact]
    public void FullNavigationFlow_GivenKeyedNavigation_ShouldNavigateSuccessfully()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act - Navigate using key
        mvvmNavigationManager.NavigateTo("Admin");

        // Assert
        navigationManager.Uri.Should().Be("http://localhost/admin");
    }

    /// <summary>
    /// Tests NavLink integration with the navigation manager and verifies URIs.
    /// </summary>
    [Fact]
    public void NavLinkIntegration_NavigationManagerIntegration_ShouldWork()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var navigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act - Test that the navigation manager can get URIs that NavLink would use
        var homeUri = mvvmNavigationManager.GetUri<IHomeViewModel>();
        var productUri = mvvmNavigationManager.GetUri<IProductViewModel>();
        var adminUri = mvvmNavigationManager.GetUri("Admin");

        // Assert
        using var _ = new AssertionScope();
        homeUri.Should().Be("/");
        productUri.Should().Be("/products");
        adminUri.Should().Be("/admin");
    }

    /// <summary>
    /// Tests navigation flow using NavLink and verifies the resulting URIs.
    /// </summary>
    [Fact]
    public void NavLinkIntegration_NavigationFlow_ShouldWork()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();
        var navigationManager = Services.GetService<FakeNavigationManager>()!;

        // Act - Test the navigation that NavLink would perform
        mvvmNavigationManager.NavigateTo<IHomeViewModel>();
        var homeUri = navigationManager.Uri;

        mvvmNavigationManager.NavigateTo<IProductViewModel>();
        var productUri = navigationManager.Uri;

        // Assert
        using var _ = new AssertionScope();
        homeUri.Should().Be("http://localhost/");
        productUri.Should().Be("http://localhost/products");
    }

    /// <summary>
    /// Simulates a complete user journey with multiple navigations and verifies the final state.
    /// </summary>
    [Fact]
    public void CompleteUserJourney_MultipleNavigations_ShouldWorkCorrectly()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act - Simulate user journey
        // 1. Start at home
        mvvmNavigationManager.NavigateTo<IHomeViewModel>();
        navigationManager.Uri.Should().Be("http://localhost/");

        // 2. Navigate to products
        mvvmNavigationManager.NavigateTo<IProductViewModel>();
        navigationManager.Uri.Should().Be("http://localhost/products");

        // 3. Navigate to specific product
        mvvmNavigationManager.NavigateTo<IProductViewModel>("123");
        navigationManager.Uri.Should().Be("http://localhost/products/123");

        // 4. Navigate to admin (keyed)
        mvvmNavigationManager.NavigateTo("Admin");
        navigationManager.Uri.Should().Be("http://localhost/admin");

        // 5. Back to home
        mvvmNavigationManager.NavigateTo<IHomeViewModel>();

        // Assert final state
        navigationManager.Uri.Should().Be("http://localhost/");
    }

    /// <summary>
    /// Tests route discovery using the mock route cache and verifies all expected routes are present.
    /// </summary>
    [Fact]
    public void RouteDiscovery_GivenMockRouteCache_ShouldContainAllRoutes()
    {
        // Arrange
        var routeCache = Services.GetRequiredService<IViewModelRouteCache>();

        // Act & Assert
        using var _ = new AssertionScope();
        routeCache.ViewModelRoutes.Should().ContainKey(typeof(IHomeViewModel));
        routeCache.ViewModelRoutes.Should().ContainKey(typeof(IProductViewModel));
        routeCache.ViewModelRoutes.Should().ContainKey(typeof(IAdminViewModel));
        routeCache.KeyedViewModelRoutes.Should().ContainKey("Admin");
    }

    /// <summary>
    /// Tests that the ViewModel lifecycle methods are called during component integration.
    /// </summary>
    [Fact]
    public void ComponentIntegration_ViewModelLifecycle_ShouldWork()
    {
        // Arrange
        Services.AddSingleton<TestIntegrationViewModel>();

        // Act
        var cut = RenderComponent<TestIntegrationView>();
        var viewModel = Services.GetRequiredService<TestIntegrationViewModel>();

        // Assert
        using var _ = new AssertionScope();
        viewModel.OnInitializedCalled.Should().BeTrue();
        viewModel.OnParametersSetCalled.Should().BeTrue();
        viewModel.OnAfterRenderCalled.Should().BeTrue();
    }

    /// <summary>
    /// Tests parameter binding integration and verifies that parameters are resolved correctly.
    /// </summary>
    [Fact]
    public void ParameterBinding_Integration_ShouldResolveCorrectly()
    {
        // Arrange
        Services.AddSingleton<TestParameterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new Components.Parameter.ParameterResolver(ParameterResolutionMode.ViewModel));

        // Act
        var cut = RenderComponent<TestParameterView>(parameters => parameters
            .Add(p => p.TestParameter, "TestValue"));

        var viewModel = Services.GetRequiredService<TestParameterViewModel>();

        // Assert
        viewModel.TestParameter.Should().Be("TestValue");
    }

    /// <summary>
    /// Tests error handling for invalid ViewModel navigation and verifies that a meaningful exception is thrown.
    /// </summary>
    [Fact]
    public void ErrorHandling_InvalidViewModel_ShouldThrowMeaningfulException()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act
        var act = () => mvvmNavigationManager.NavigateTo<IInvalidViewModel>();

        // Assert
        act.Should().Throw<ViewModelRouteNotFoundException>()
           .WithMessage("*IInvalidViewModel*no associated page*");
    }

    /// <summary>
    /// Tests error handling for invalid key navigation and verifies that a meaningful exception is thrown.
    /// </summary>
    [Fact]
    public void ErrorHandling_InvalidKey_ShouldThrowMeaningfulException()
    {
        // Arrange
        var mvvmNavigationManager = Services.GetRequiredService<IMvvmNavigationManager>();

        // Act
        var act = () => mvvmNavigationManager.NavigateTo("InvalidKey");

        // Assert
        act.Should().Throw<ViewModelRouteNotFoundException>()
           .WithMessage("*InvalidKey*");
    }

    // Test ViewModels and Views
    public interface IHomeViewModel : IViewModelBase { }
    public class HomeViewModel : ViewModelBase, IHomeViewModel { }

    public interface IProductViewModel : IViewModelBase { }
    public class ProductViewModel : ViewModelBase, IProductViewModel { }

    public interface IAdminViewModel : IViewModelBase { }
    public class AdminViewModel : ViewModelBase, IAdminViewModel { }

    public class TestIntegrationViewModel : ViewModelBase
    {
        public bool OnInitializedCalled { get; private set; }
        public bool OnParametersSetCalled { get; private set; }
        public bool OnAfterRenderCalled { get; private set; }

        public override void OnInitialized()
        {
            OnInitializedCalled = true;
        }

        public override void OnParametersSet()
        {
            OnParametersSetCalled = true;
        }

        public override void OnAfterRender(bool firstRender)
        {
            OnAfterRenderCalled = true;
        }
    }

    public class TestIntegrationView : MvvmComponentBase<TestIntegrationViewModel>
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.AddContent(0, "Test View");
        }
    }

    public class TestParameterViewModel : ViewModelBase
    {
        [ViewParameter]
        public string TestParameter { get; set; } = string.Empty;
    }

    public class TestParameterView : MvvmComponentBase<TestParameterViewModel>
    {
        [Parameter]
        public string TestParameter { get; set; } = string.Empty;

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.AddContent(0, "Parameter View");
        }
    }

    public interface IInvalidViewModel : IViewModelBase { }

    #region YARP and Dynamic Base Path Integration Tests

    /// <summary>
    /// Tests that dynamic base path detection works in integration scenario with YARP-style subpath.
    /// </summary>
    [Fact]
    public void DynamicBasePath_YarpStyleHosting_ShouldNavigateCorrectly()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        navigationManager.NavigateTo("http://localhost/api/v1/", false);
        
        var routeCache = new Mock<IViewModelRouteCache>();
        var routes = new Dictionary<Type, string> { [typeof(IProductViewModel)] = "/api/v1/products" };
        routeCache.Setup(x => x.ViewModelRoutes).Returns(routes);
        
        var config = Options.Create(new LibraryConfiguration()); // No configured BasePath
        var logger = new Mock<ILogger<MvvmNavigationManager>>();
        var mvvmNavManager = new MvvmNavigationManager(navigationManager, logger.Object, routeCache.Object, config);

        // Act
        mvvmNavManager.NavigateTo<IProductViewModel>();

        // Assert - Should detect "api/v1" from NavigationManager and navigate to "products"
        navigationManager.Uri.Should().Be("http://localhost/api/v1/products");
    }

    /// <summary>
    /// Tests that dynamic base path detection works with deeply nested paths.
    /// </summary>
    [Fact]
    public void DynamicBasePath_DeeplyNestedPath_ShouldNavigateCorrectly()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        navigationManager.NavigateTo("http://localhost/app/tenant/123/", false);
        
        var routeCache = new Mock<IViewModelRouteCache>();
        var routes = new Dictionary<Type, string> { [typeof(IHomeViewModel)] = "/app/tenant/123/dashboard" };
        routeCache.Setup(x => x.ViewModelRoutes).Returns(routes);
        
        var config = Options.Create(new LibraryConfiguration()); // No configured BasePath
        var logger = new Mock<ILogger<MvvmNavigationManager>>();
        var mvvmNavManager = new MvvmNavigationManager(navigationManager, logger.Object, routeCache.Object, config);

        // Act
        mvvmNavManager.NavigateTo<IHomeViewModel>();

        // Assert - Should detect "app/tenant/123" and navigate to "dashboard"
        navigationManager.Uri.Should().Be("http://localhost/app/tenant/123/dashboard");
    }

    /// <summary>
    /// Tests that configured BasePath takes priority over dynamically detected base path.
    /// </summary>
    [Fact]
    public void DynamicBasePath_ConfiguredTakesPriority_ShouldUseConfigured()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        navigationManager.NavigateTo("http://localhost/detected/path/", false);
        
        var routeCache = new Mock<IViewModelRouteCache>();
        var routes = new Dictionary<Type, string> { [typeof(IProductViewModel)] = "/configured/path/products" };
        routeCache.Setup(x => x.ViewModelRoutes).Returns(routes);
        
#pragma warning disable CS0618 // Type or member is obsolete
        var config = Options.Create(new LibraryConfiguration { BasePath = "/configured/path/" });
#pragma warning restore CS0618 // Type or member is obsolete
        var logger = new Mock<ILogger<MvvmNavigationManager>>();
        var mvvmNavManager = new MvvmNavigationManager(navigationManager, logger.Object, routeCache.Object, config);

        // Act
        mvvmNavManager.NavigateTo<IProductViewModel>();

        // Assert - Should use configured BasePath to strip "/configured/path" prefix, resulting in "products" relative navigation
        navigationManager.Uri.Should().Be("http://localhost/products");
    }

    /// <summary>
    /// Tests complete user journey with dynamic base path detection.
    /// </summary>
    [Fact]
    public void DynamicBasePath_CompleteUserJourney_ShouldWorkCorrectly()
    {
        // Arrange - Simulating YARP reverse proxy scenario
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        navigationManager.NavigateTo("http://localhost/myapp/", false);
        
        var routeCache = new Mock<IViewModelRouteCache>();
        var routes = new Dictionary<Type, string>
        {
            [typeof(IHomeViewModel)] = "/myapp",
            [typeof(IProductViewModel)] = "/myapp/products",
            [typeof(IAdminViewModel)] = "/myapp/admin"
        };
        routeCache.Setup(x => x.ViewModelRoutes).Returns(routes);
        routeCache.Setup(x => x.KeyedViewModelRoutes).Returns(new Dictionary<object, string>());
        
        var config = Options.Create(new LibraryConfiguration()); // No configured BasePath
        var logger = new Mock<ILogger<MvvmNavigationManager>>();
        var mvvmNavManager = new MvvmNavigationManager(navigationManager, logger.Object, routeCache.Object, config);

        // Act & Assert - Simulate user journey
        // 1. Navigate to home
        mvvmNavManager.NavigateTo<IHomeViewModel>();
        navigationManager.Uri.Should().Be("http://localhost/myapp");

        // 2. Navigate to products
        mvvmNavManager.NavigateTo<IProductViewModel>();
        navigationManager.Uri.Should().Be("http://localhost/myapp/products");

        // 3. Navigate with query parameters
        mvvmNavManager.NavigateTo<IProductViewModel>("?filter=active");
        navigationManager.Uri.Should().Be("http://localhost/myapp/products?filter=active");

        // 4. Navigate to admin
        mvvmNavManager.NavigateTo<IAdminViewModel>();
        navigationManager.Uri.Should().Be("http://localhost/myapp/admin");
    }

    /// <summary>
    /// Tests that dynamic base path detection handles root hosting correctly in integration scenario.
    /// </summary>
    [Fact]
    public void DynamicBasePath_RootHosting_ShouldNavigateCorrectly()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        // NavigationManager already at root: http://localhost/
        
        var routeCache = new Mock<IViewModelRouteCache>();
        var routes = new Dictionary<Type, string> { [typeof(IProductViewModel)] = "/products" };
        routeCache.Setup(x => x.ViewModelRoutes).Returns(routes);
        
        var config = Options.Create(new LibraryConfiguration()); // No configured BasePath
        var logger = new Mock<ILogger<MvvmNavigationManager>>();
        var mvvmNavManager = new MvvmNavigationManager(navigationManager, logger.Object, routeCache.Object, config);

        // Act
        mvvmNavManager.NavigateTo<IProductViewModel>();

        // Assert - Should detect no base path and navigate normally
        navigationManager.Uri.Should().Be("http://localhost/products");
    }

    /// <summary>
    /// Tests that keyed navigation works with dynamic base path detection.
    /// </summary>
    [Fact]
    public void DynamicBasePath_KeyedNavigation_ShouldNavigateCorrectly()
    {
        // Arrange
        var navigationManager = Services.GetService<FakeNavigationManager>()!;
        navigationManager.NavigateTo("http://localhost/app/", false);
        
        var routeCache = new Mock<IViewModelRouteCache>();
        var keyedRoutes = new Dictionary<object, string> { ["AdminKey"] = "/app/admin" };
        routeCache.Setup(x => x.ViewModelRoutes).Returns(new Dictionary<Type, string>());
        routeCache.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);
        
        var config = Options.Create(new LibraryConfiguration()); // No configured BasePath
        var logger = new Mock<ILogger<MvvmNavigationManager>>();
        var mvvmNavManager = new MvvmNavigationManager(navigationManager, logger.Object, routeCache.Object, config);

        // Act
        mvvmNavManager.NavigateTo("AdminKey");

        // Assert - Should detect "app" and navigate to "admin"
        navigationManager.Uri.Should().Be("http://localhost/app/admin");
    }

    #endregion
}