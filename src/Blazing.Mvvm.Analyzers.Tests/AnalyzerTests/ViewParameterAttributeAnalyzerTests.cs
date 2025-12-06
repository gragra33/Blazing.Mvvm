using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ViewParameterAttributeAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="ViewParameterAttributeAnalyzer"/>
/// </summary>
public class ViewParameterAttributeAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewParameterWithoutCorrespondingParameter_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class TestView : MvvmComponentBase<TestViewModel>
    {
        // Missing [Parameter] for ViewModel's ViewParameter
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        [ViewParameter]
        public string {|#0:Name|} { get; set; }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewParameterMismatch)
            .WithLocation(0)
            .WithArguments("Name");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewParameterWithMatchingParameter_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class TestView : MvvmComponentBase<TestViewModel>
    {
        [Parameter]
        public string Name { get; set; }
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        [ViewParameter]
        public string Name { get; set; }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewParameterWithoutView_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        [ViewParameter]
        public string Name { get; set; }
    }
}";

        // No diagnostic if View doesn't exist yet
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleViewParameters_SomeMatching_ReportsPartialDiagnostics()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class TestView : MvvmComponentBase<TestViewModel>
    {
        [Parameter]
        public string Name { get; set; }
        
        // Missing Age parameter
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        [ViewParameter]
        public string Name { get; set; }

        [ViewParameter]
        public int {|#0:Age|} { get; set; }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewParameterMismatch)
            .WithLocation(0)
            .WithArguments("Age");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }
}
