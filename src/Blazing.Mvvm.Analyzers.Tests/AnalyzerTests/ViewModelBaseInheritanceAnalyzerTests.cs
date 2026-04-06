using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ViewModelBaseInheritanceAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="ViewModelBaseInheritanceAnalyzer"/>
/// </summary>
public class ViewModelBaseInheritanceAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithoutBaseClass_ReportsDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public class {|#0:TestViewModel|}
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelBaseMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelWithViewModelBaseSuffix_WithoutBaseClass_ReportsDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public class {|#0:MyCustomViewModel|}
    {
        public string Name { get; set; }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelBaseMissing)
            .WithLocation(0)
            .WithArguments("MyCustomViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelInheritingFromViewModelBase_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelInheritingFromRecipientViewModelBase_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : RecipientViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelInheritingFromValidatorViewModelBase_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ValidatorViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ClassNotEndingWithViewModel_NoDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public class MyClass
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AbstractViewModel_NoDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public abstract class BaseViewModel
    {
    }
}";

        // Abstract classes might be intentionally not inheriting
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NestedViewModel_ReportsDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public class OuterClass
    {
        public class {|#0:InnerViewModel|}
        {
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelBaseMissing)
            .WithLocation(0)
            .WithArguments("InnerViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }
}
