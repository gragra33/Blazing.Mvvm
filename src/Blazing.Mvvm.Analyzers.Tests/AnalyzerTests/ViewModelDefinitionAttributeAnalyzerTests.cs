using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ViewModelDefinitionAttributeAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="ViewModelDefinitionAttributeAnalyzer"/>
/// </summary>
public class ViewModelDefinitionAttributeAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithoutAttribute_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class {|#0:TestViewModel|} : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelDefinitionMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelWithAttribute_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace TestNamespace
{
    [ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task RecipientViewModelWithAttribute_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace TestNamespace
{
    [ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
    public class TestViewModel : RecipientViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidatorViewModelWithAttribute_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace TestNamespace
{
    [ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
    public class TestViewModel : ValidatorViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ClassNotInheritingFromViewModelBase_NoDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public class TestViewModel
    {
    }
}";

        // Analyzer only checks classes inheriting from ViewModelBase
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AbstractViewModelWithoutAttribute_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public abstract class BaseViewModel : ViewModelBase
    {
    }
}";

        // Abstract ViewModels don't need the attribute
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleViewModelsWithoutAttribute_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class {|#0:FirstViewModel|} : ViewModelBase
    {
    }

    public class {|#1:SecondViewModel|} : ViewModelBase
    {
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.ViewModelDefinitionMissing)
            .WithLocation(0)
            .WithArguments("FirstViewModel");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.ViewModelDefinitionMissing)
            .WithLocation(1)
            .WithArguments("SecondViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }
}
