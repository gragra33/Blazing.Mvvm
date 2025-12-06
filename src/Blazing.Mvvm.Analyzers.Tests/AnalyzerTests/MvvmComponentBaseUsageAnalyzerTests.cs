using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.MvvmComponentBaseUsageAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="MvvmComponentBaseUsageAnalyzer"/>
/// </summary>
public class MvvmComponentBaseUsageAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ComponentBaseWithViewModelProperty_ReportsDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class {|#0:MyComponent|} : ComponentBase
    {
        public TestViewModel ViewModel { get; set; }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MvvmComponentBaseMissing)
            .WithLocation(0)
            .WithArguments("MyComponent");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ComponentBaseWithInjectAttribute_ReportsDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class {|#0:MyComponent|} : ComponentBase
    {
        [Microsoft.AspNetCore.Components.Inject]
        public TestViewModel ViewModel { get; set; }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MvvmComponentBaseMissing)
            .WithLocation(0)
            .WithArguments("MyComponent");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MvvmComponentBase_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyComponent : MvvmComponentBase<TestViewModel>
    {
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MvvmOwningComponentBase_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyComponent : MvvmOwningComponentBase<TestViewModel>
    {
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ComponentBaseWithoutViewModel_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        public string Message { get; set; }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ComponentBaseWithNonViewModelProperty_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        public MyService Service { get; set; }
    }

    public class MyService
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ComponentWithMultipleViewModelProperties_ReportsDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class {|#0:MyComponent|} : ComponentBase
    {
        public FirstViewModel FirstVm { get; set; }
        public SecondViewModel SecondVm { get; set; }
    }

    public class FirstViewModel : ViewModelBase { }
    public class SecondViewModel : ViewModelBase { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MvvmComponentBaseMissing)
            .WithLocation(0)
            .WithArguments("MyComponent");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }
}
