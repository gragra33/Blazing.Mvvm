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

public class NavigationIntegrationTests : ComponentTestBase
{
    private readonly Mock<ILogger<ViewModelRouteCache>> _routeCacheLoggerMock;
    private readonly Mock<ILogger<MvvmNavigationManager>> _navManagerLoggerMock;
    private readonly Mock<IViewModelRouteCache> _routeCacheMock;

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
}