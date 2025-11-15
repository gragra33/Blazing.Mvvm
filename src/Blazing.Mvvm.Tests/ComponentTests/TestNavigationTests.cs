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
    public TestNavigationTests()
    {
        // Configure route cache with necessary route mappings
        var routeCacheMock = new Mock<IViewModelRouteCache>();
        var viewModelRoutes = new Dictionary<Type, string>
        {
            [typeof(ITestNavigationViewModel)] = "/test",
            [typeof(IHexTranslateViewModel)] = "/hextranslate"
        };
        var keyedRoutes = new Dictionary<object, string>
        {
            ["TestKeyedNavigationViewModel"] = "/keyedtest"
        };
        
        routeCacheMock.Setup(x => x.ViewModelRoutes).Returns(viewModelRoutes);
        routeCacheMock.Setup(x => x.KeyedViewModelRoutes).Returns(keyedRoutes);
        
        // Add services to the DI container. AutoMocker will not resolve these services.
        Services.AddSingleton(routeCacheMock.Object);
        Services.AddSingleton<IMvvmNavigationManager, MvvmNavigationManager>();
        Services.AddSingleton<ITestNavigationViewModel, TestNavigationViewModel>();
        Services.AddSingleton<IHexTranslateViewModel, HexTranslateViewModel>();
        Services.AddKeyedSingleton<ITestKeyedNavigationViewModel, TestKeyedNavigationViewModel>("TestKeyedNavigationViewModel");
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewModel));
    }

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

    [Fact]
    public void GivenComponentRendered_WhenTestButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test";
        const string testButtonSelector = "#test";

        var fakeNavigationManager = GetService<FakeNavigationManager>();
        var cut = RenderComponent<TestNavigation>();

        // Act
        cut.Find(testButtonSelector).Click();

        // Assert
        fakeNavigationManager.Uri.Should().Be(expectedUri);
    }

    [Fact]
    public void GivenComponentRendered_WhenTestRelativePathButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test/this%20is%20a%20MvvmNavLink%20test";
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

        // Assert
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().BeEmpty();
        cutViewModel.Test.Should().BeNull();
    }

    [Fact]
    public void GivenComponentRendered_WhenTestQueryStringButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test?test=this%20is%20a%20MvvmNavLink%20querystring%20test";
        const string expectedQueryString = "?test=this%20is%20a%20MvvmNavLink%20querystring%20test";
        const string expectedQueryParameterValue = "this is a MvvmNavLink querystring test";
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

    [Fact]
    public void GivenComponentRendered_WhenTestRelativePathQueryStringButtonClicked_ThenShouldNavigateToTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/test/this%20is%20a%20MvvmNavLink%20test/?test=this%20is%20a%20MvvmNavLink%20querystring%20test";
        const string expectedEcho = "this is a MvvmNavLink test";
        const string expectedQueryString = "?test=this%20is%20a%20MvvmNavLink%20querystring%20test";
        const string expectedQueryParameterValue = "this is a MvvmNavLink querystring test";
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

        // Assert
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedRelativePathParagraphContent);
        cut.FindByLabelText(queryStringParagraphAriaLabel).TextContent.Should().Be(expectedQueryStringParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().Be(expectedQueryString);
        cutViewModel.Test.Should().Be(expectedQueryParameterValue);
    }

    [Fact]
    public void GivenComponentRendered_WhenKeyedTestButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest";
        const string testButtonSelector = "#keyedtest";

        var fakeNavigationManager = GetService<FakeNavigationManager>();
        var cut = RenderComponent<TestKeyedNavigation>();

        // Act
        cut.Find(testButtonSelector).Click();

        // Assert
        fakeNavigationManager.Uri.Should().Be(expectedUri);
    }

    [Fact]
    public void GivenComponentRendered_WhenKeyedTestRelativePathButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest/this%20is%20a%20MvvmKeyNavLink%20test";
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

        // Assert
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().BeEmpty();
        cutViewModel.Test.Should().BeNull();
    }

    [Fact]
    public void GivenComponentRendered_WhenKeyedTestQueryStringButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";
        const string expectedQueryString = "?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";
        const string expectedQueryParameterValue = "this is a MvvmKeyNavLink querystring test";
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

    [Fact]
    public void GivenComponentRendered_WhenKeyedTestRelativePathQueryStringButtonClicked_ThenShouldNavigateToKeyedTestNavigationPage()
    {
        // Arrange
        const string expectedUri = "http://localhost/keyedtest/this%20is%20a%20MvvmKeyNavLink%20test/?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";
        const string expectedEcho = "this is a MvvmKeyNavLink test";
        const string expectedQueryString = "?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test";
        const string expectedQueryParameterValue = "this is a MvvmKeyNavLink querystring test";
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

        // Assert
        using var _ = new AssertionScope();
        fakeNavigationManager.Uri.Should().Be(expectedUri);
        cut.FindByLabelText(relativePathParagraphAriaLabel).TextContent.Should().Be(expectedRelativePathParagraphContent);
        cut.FindByLabelText(queryStringParagraphAriaLabel).TextContent.Should().Be(expectedQueryStringParagraphContent);
        cutViewModel.Echo.Should().Be(expectedEcho);
        cutViewModel.QueryString.Should().Be(expectedQueryString);
        cutViewModel.Test.Should().Be(expectedQueryParameterValue);
    }
}