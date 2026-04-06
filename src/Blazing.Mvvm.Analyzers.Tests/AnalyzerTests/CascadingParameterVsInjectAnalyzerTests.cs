using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.CascadingParameterVsInjectAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="CascadingParameterVsInjectAnalyzer"/>
/// </summary>
public class CascadingParameterVsInjectAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task CascadingParameterForService_ReportsDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [CascadingParameter]
        public IMyService {|#0:MyService|} { get; set; }
    }

    public interface IMyService { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.InjectPreferredOverCascading)
            .WithLocation(0)
            .WithArguments("MyService");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task InjectAttributeForService_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [Inject]
        public IMyService MyService { get; set; }
    }

    public interface IMyService { }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task CascadingParameterForNonService_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [CascadingParameter]
        public ThemeInfo Theme { get; set; }
    }

    public class ThemeInfo
    {
        public string PrimaryColor { get; set; }
    }
}";

        // ThemeInfo is not a service interface
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task CascadingParameterWithName_ForState_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [CascadingParameter(Name = ""AppState"")]
        public AppState State { get; set; }
    }

    public class AppState
    {
        public string CurrentUser { get; set; }
    }
}";

        // Named cascading parameters are typically for component state
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task CascadingParameterForNavigation_ReportsDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [CascadingParameter]
        public NavigationManager {|#0:Navigation|} { get; set; }
    }

    public class NavigationManager { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.InjectPreferredOverCascading)
            .WithLocation(0)
            .WithArguments("Navigation");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MultipleServicesWithCascadingParameter_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [CascadingParameter]
        public IFirstService {|#0:FirstService|} { get; set; }

        [CascadingParameter]
        public ISecondService {|#1:SecondService|} { get; set; }
    }

    public interface IFirstService { }
    public interface ISecondService { }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.InjectPreferredOverCascading)
            .WithLocation(0)
            .WithArguments("FirstService");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.InjectPreferredOverCascading)
            .WithLocation(1)
            .WithArguments("SecondService");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task CascadingParameterInNonComponent_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyClass
    {
        [CascadingParameter]
        public IMyService MyService { get; set; }
    }

    public interface IMyService { }
}";

        // Not a component, analyzer doesn't apply
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
