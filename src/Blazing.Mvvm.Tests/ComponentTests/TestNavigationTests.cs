using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Parameter;
using Blazing.Mvvm.Components.Routing;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Blazing.Mvvm.Tests.ComponentTests;

public class TestNavigationTests : ComponentTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestNavigationTests"/> class and configures route cache and services.
    /// </summary>
    public TestNavigationTests()
    {
        // Configure route cache with necessary route mappings
        var routeCacheMock = new Mock<IViewModelRouteCache>();
        var routeTemplateParser = new RouteTemplateParser();
        
        var viewModelRoutes = new Dictionary<Type, string>
        {
            [typeof(ITestNavigationViewModel)] = "/test/{echo}",  // Route with parameter matching actual page
            [typeof(IHexTranslateViewModel)] = "/hextranslate"
        };
        var keyedRoutes = new Dictionary<object, string>
        {
            ["TestKeyedNavigationViewModel"] = "/keyedtest/{echo}"  // Route with parameter matching actual page
        };
        
        // Setup multi-route templates for all routes
        var viewModelRouteTemplates = new Dictionary<Type, RouteTemplateCollection>();
        foreach (var (type, route) in viewModelRoutes)
        {
            var template = routeTemplateParser.Parse(route);
            viewModelRouteTemplates[type] = new RouteTemplateCollection
            {
                PrimaryRoute = route,
                AllRoutes = new List<RouteTemplate> { template }
            };
        }
        
        var keyedRouteTemplates = new Dictionary<object, RouteTemplateCollection>();
        foreach (var (key, route) in keyedRoutes)
        {
            var template = routeTemplateParser.Parse(route);
            keyedRouteTemplates[key] = new RouteTemplateCollection
            {
                PrimaryRoute = route,
                AllRoutes = new List<RouteTemplate> { template }
            };
        }
        
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(viewModelRoutes);
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);
        routeCacheMock.Setup(x => x.ViewModelRouteTemplates).Returns(viewModelRouteTemplates);
        routeCacheMock.Setup(x => x.KeyedViewModelRouteTemplates).Returns(keyedRouteTemplates);
        
        // Add services to the DI container. AutoMocker will not resolve these services.
        Services.AddSingleton(routeCacheMock.Object);
        Services.AddSingleton<RouteTemplateSelector>();
        Services.AddSingleton<IMvvmNavigationManager, MvvmNavigationManager>();
        Services.AddSingleton<ITestNavigationViewModel, TestNavigationViewModel>();
        Services.AddSingleton<IHexTranslateViewModel, HexTranslateViewModel>();
        Services.AddKeyedSingleton<ITestKeyedNavigationViewModel, TestKeyedNavigationViewModel>("TestKeyedNavigationViewModel");
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewModel));
    }

    /// <summary>
    /// Verifies navigation to the HexTranslator page when the HexTranslate button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenHexTranslateButtonClicked_ThenShouldNavigateToHexTranslatorPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/hextranslate";
        const string hexTranslateButtonSelector = "#hex-translate";

        var fakeNavigationManager = GetService<FakeNavigationManager>();
        var cut = RenderComponent<TestNavigation>();

        // Act
        cut.Find(hexTranslateButtonSelector).Click();

        // Assert
        fakeNavigationManager.Uri.Should().Match(expectedUri);
    }

    /// <summary>
    /// Verifies navigation to the TestNavigation page when the Test button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenTestButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test/{echo}";  // {echo} not URL-encoded in NavigationManager
        const string testButtonSelector = "#test";

        var fakeNavigationManager = GetService<FakeNavigationManager>();
        var cut = RenderComponent<TestNavigation>();

        // Act
        cut.Find(testButtonSelector).Click();

        // Assert - Navigates without parameter, so {echo} remains unsubstituted
        fakeNavigationManager.Uri.Should().Be(expectedUri);
    }

    /// <summary>
    /// Verifies navigation to the TestNavigation page with a relative path when the corresponding button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenTestRelativePathButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test/this is a MvvmNavLink test";  // Not URL-encoded
        const string expectedEcho = "this is a MvvmNavLink test";
        const string expectedParagraphContent = "Relative Path: " + expectedEcho;
        const string testRelativePathButtonSelector = "#test-relative-path";
        const string relativePathParagraphAriaLabel = "relative path";

        var cut = RenderComponent<TestNavigation>();
        var cutViewModel = GetViewModel<ITestNavigationViewModel>();
        var fakeNavigationManager = GetService<FakeNavigationManager>();
        /* Simulating setting Echo after navigation */
        fakeNavigationManager.LocationChanged += (_, _)
            => cut.SetParametersAndRender(parameters => parameters.Add(p => p.Echo, expectedEcho));

        // Act
        cut.Find(testRelativePathButtonSelector).Click();

        // Assert - Parameter substitution: {echo} replaced with the parameter value
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().BeEmpty();
        cutViewModel.Test.Should().BeNull();
    }

    /// <summary>
    /// Verifies navigation to the TestNavigation page with a query string when the corresponding button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenTestQueryStringButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test/%7Becho%7D?test=this%20is%20a%20MvvmNavLink%20querystring%20test";  // URL-encoded
        const string expectedQueryString = "?test=this%20is%20a%20MvvmNavLink%20querystring%20test";  // URL-encoded
        const string expectedQueryParameterValue = "this is a MvvmNavLink querystring test";  // Decoded value
        const string expectedParagraphContent = "QueryString: " + expectedQueryString;
        const string queryStringParagraphAriaLabel = "query string";
        const string testQueryStringButtonSelector = "#test-query-string";

        var fakeNavigationManager = GetService<FakeNavigationManager>();
        var cut = RenderComponent<TestNavigation>();
        var cutViewModel = GetViewModel<ITestNavigationViewModel>();

        // Act
        cut.Find(testQueryStringButtonSelector).Click();

        // Assert
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(queryStringParagraphAriaLabel).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.Echo.Should().BeEmpty();
        cutViewModel.QueryString.Should().Be(expectedQueryString);
        cutViewModel.Test.Should().Be(expectedQueryParameterValue);
    }

    /// <summary>
    /// Verifies navigation to the TestNavigation page with a relative path and query string when the corresponding button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenTestRelativePathQueryStringButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test/this is a MvvmNavLink test?test=this%20is%20a%20MvvmNavLink%20querystring%20test";  // Path not encoded, query encoded
        const string expectedEcho = "this is a MvvmNavLink test";
        const string expectedQueryString = "?test=this%20is%20a%20MvvmNavLink%20querystring%20test";  // URL-encoded
        const string expectedQueryParameterValue = "this is a MvvmNavLink querystring test";  // Decoded value
        const string expectedRelativePathParagraphContent = "Relative Path: " + expectedEcho;
        const string expectedQueryStringParagraphContent = "QueryString: " + expectedQueryString;
        const string queryStringParagraphAriaLabel = "query string";
        const string relativePathParagraphAriaLabel = "relative path";
        const string testRelativePathQueryStringButtonSelector = "#test-relpath-qstring";

        var cut = RenderComponent<TestNavigation>();
        var cutViewModel = GetViewModel<ITestNavigationViewModel>();
        var fakeNavigationManager = GetService<FakeNavigationManager>();
        /* Simulating setting Echo after navigation */
        fakeNavigationManager.LocationChanged += (_, _) =>
            cut.SetParametersAndRender(parameters => parameters.Add(p => p.Echo, expectedEcho));

        // Act
        cut.Find(testRelativePathQueryStringButtonSelector).Click();

        // Assert - Parameter substitution with query string
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedRelativePathParagraphContent);
        cut.FindByLabelText(queryStringParagraphAriaLabel).TextContent.Should().Be(expectedQueryStringParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().Be(expectedQueryString);
        cutViewModel.Test.Should().Be(expectedQueryParameterValue);
    }

    /// <summary>
    /// Verifies navigation to the KeyedTestNavigation page when the KeyedTest button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenKeyedTestButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest/{echo}";  // Not URL-encoded when navigating to template
        const string testButtonSelector = "#keyedtest";

        var fakeNavigationManager = GetService<FakeNavigationManager>();
        var cut = RenderComponent<TestKeyedNavigation>();

        // Act
        cut.Find(testButtonSelector).Click();

        // Assert - Navigates without parameter, so {echo} remains unsubstituted
        fakeNavigationManager.Uri.Should().Be(expectedUri);
    }

    /// <summary>
    /// Verifies navigation to the KeyedTestNavigation page with a relative path when the corresponding button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenKeyedTestRelativePathButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest/%7Becho%7D/this%20is%20a%20MvvmKeyNavLink%20test";  // URL-encoded
        const string expectedEcho = "this is a MvvmKeyNavLink test";
        const string expectedParagraphContent = "Relative Path: " + expectedEcho;
        const string testRelativePathButtonSelector = "#keyedtest-relative-path";
        const string relativePathParagraphAriaLabel = "relative path";

        var cut = RenderComponent<TestKeyedNavigation>();
        var cutViewModel = GetViewModel<ITestKeyedNavigationViewModel>("TestKeyedNavigationViewModel");
        var fakeNavigationManager = GetService<FakeNavigationManager>();
        /* Simulating setting Echo after navigation */
        fakeNavigationManager.LocationChanged += (_, _)
            => cut.SetParametersAndRender(parameters => parameters.Add(p => p.Echo, expectedEcho));

        // Act
        cut.Find(testRelativePathButtonSelector).Click();

        // Assert - Keyed navigation appends relative URI instead of parameter substitution
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().BeEmpty();
        cutViewModel.Test.Should().BeNull();
    }

    /// <summary>
    /// Verifies navigation to the KeyedTestNavigation page with a query string when the corresponding button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenKeyedTestQueryStringButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest/%7Becho%7D?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";  // URL-encoded
        const string expectedQueryString = "?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";  // URL-encoded
        const string expectedQueryParameterValue = "this is a MvvmKeyNavLink querystring test";  // Decoded value
        const string expectedParagraphContent = "QueryString: " + expectedQueryString;
        const string queryStringParagraphAriaLabel = "query string";
        const string testQueryStringButtonSelector = "#keyedtest-query-string";

        var fakeNavigationManager = GetService<FakeNavigationManager>();
        var cut = RenderComponent<TestKeyedNavigation>();
        var cutViewModel = GetViewModel<ITestKeyedNavigationViewModel>("TestKeyedNavigationViewModel");

        // Act
        cut.Find(testQueryStringButtonSelector).Click();

        // Assert
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(queryStringParagraphAriaLabel).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.Echo.Should().BeEmpty();
        cutViewModel.QueryString.Should().Be(expectedQueryString);
        cutViewModel.Test.Should().Be(expectedQueryParameterValue);
    }

    /// <summary>
    /// Verifies navigation to the KeyedTestNavigation page with a relative path and query string when the corresponding button is clicked.
    /// </summary>
    [Fact]
    public void GivenComponentRendered_WhenKeyedTestRelativePathQueryStringButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest/%7Becho%7D/this%20is%20a%20MvvmKeyNavLink%20test/?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";  // URL-encoded
        const string expectedEcho = "this is a MvvmKeyNavLink test";
        const string expectedQueryString = "?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";  // URL-encoded
        const string expectedQueryParameterValue = "this is a MvvmKeyNavLink querystring test";  // Decoded value
        const string expectedRelativePathParagraphContent = "Relative Path: " + expectedEcho;
        const string expectedQueryStringParagraphContent = "QueryString: " + expectedQueryString;
        const string queryStringParagraphAriaLabel = "query string";
        const string relativePathParagraphAriaLabel = "relative path";
        const string testRelativePathQueryStringButtonSelector = "#keyedtest-relpath-qstring";

        var cut = RenderComponent<TestKeyedNavigation>();
        var cutViewModel = GetViewModel<ITestKeyedNavigationViewModel>("TestKeyedNavigationViewModel");
        var fakeNavigationManager = GetService<FakeNavigationManager>();
        /* Simulating setting Echo after navigation */
        fakeNavigationManager.LocationChanged += (_, _) =>
            cut.SetParametersAndRender(parameters => parameters.Add(p => p.Echo, expectedEcho));

        // Act
        cut.Find(testRelativePathQueryStringButtonSelector).Click();

        // Assert - Keyed navigation appends relative URI with query string
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedRelativePathParagraphContent);
        cut.FindByLabelText(queryStringParagraphAriaLabel).TextContent.Should().Be(expectedQueryStringParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().Be(expectedQueryString);
        cutViewModel.Test.Should().Be(expectedQueryParameterValue);
    }
}