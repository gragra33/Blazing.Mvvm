using Blazing.Mvvm.Analyzers.Analyzers;
using Blazing.Mvvm.Analyzers.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpCodeFixVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ViewModelBaseInheritanceAnalyzer,
    Blazing.Mvvm.Analyzers.CodeFixProviders.ViewModelBaseInheritanceCodeFixProvider>;

namespace Blazing.Mvvm.Analyzers.Tests.CodeFixTests;

/// <summary>
/// Unit tests for <see cref="ViewModelBaseInheritanceCodeFixProvider"/>
/// </summary>
public class ViewModelBaseInheritanceCodeFixProviderTests
{
    [Fact]
    public async Task ViewModelWithoutBase_AddsViewModelBase()
    {
        const string test = @"
namespace TestNamespace
{
    public class {|#0:TestViewModel|}
    {
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelBaseMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task ViewModelWithExistingUsing_DoesNotDuplicateUsing()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class {|#0:TestViewModel|}
    {
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelBaseMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task ViewModelWithOtherUsings_PreservesOrder()
    {
        const string test = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class {|#0:TestViewModel|}
    {
    }
}";

        const string fixedCode = @"
using System;
using System.Collections.Generic;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelBaseMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
