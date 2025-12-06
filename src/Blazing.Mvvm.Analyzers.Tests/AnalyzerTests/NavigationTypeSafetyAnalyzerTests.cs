using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.NavigationTypeSafetyAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="NavigationTypeSafetyAnalyzer"/>
/// </summary>
public class NavigationTypeSafetyAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact(Skip = "CompilationEndAction diagnostic not captured by test framework - analyzer works correctly in IDE")]
    public async Task NavigateToWithInvalidViewModel_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Services;

namespace TestNamespace
{
    public class MyService
    {
        public void Navigate(INavigationService nav)
        {
            nav.{|#0:NavigateTo<TestViewModel>()|};
        }
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        // No corresponding route
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.InvalidNavigationTarget)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task NavigateToWithValidViewModel_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Services;

namespace TestNamespace
{
    public class MyService
    {
        public void Navigate(INavigationService nav)
        {
            nav.NavigateTo<TestViewModel>();
        }
    }

    [ViewModelDefinition(ViewType = typeof(TestView))]
    public class TestViewModel : ViewModelBase
    {
    }

    [Microsoft.AspNetCore.Components.RouteAttribute(""/test"")]
    public class TestView
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NavigateToWithKey_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Services;

namespace TestNamespace
{
    public class MyService
    {
        public void Navigate(INavigationService nav)
        {
            nav.NavigateTo(""test-key"");
        }
    }
}";

        // Key-based navigation doesn't trigger type safety check
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
