using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ViewDisposalBypassAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="ViewDisposalBypassAnalyzer"/>
/// </summary>
public class ViewDisposalBypassAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewImplementingIAsyncDisposable_ReportsDiagnostic()
    {
        const string test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IAsyncDisposable
    {
        public ValueTask {|#0:DisposeAsync|}() => default;
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", "IAsyncDisposable", "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewImplementingIAsyncDisposableExplicitly_ReportsDiagnostic()
    {
        const string test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IAsyncDisposable
    {
        ValueTask IAsyncDisposable.{|#0:DisposeAsync|}() => default;
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", "IAsyncDisposable", "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewReimplementingIDisposable_ReportsDiagnostic()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IDisposable
    {
        public new void {|#0:Dispose|}()
        {
        }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", "IDisposable", "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task LayoutImplementingIAsyncDisposable_ReportsDiagnostic()
    {
        const string test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyLayout : MvvmLayoutComponentBase<TestViewModel>, IAsyncDisposable
    {
        public ValueTask {|#0:DisposeAsync|}() => default;
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyLayout", "IAsyncDisposable", "MvvmLayoutComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task OwningViewImplementingIAsyncDisposable_ReportsDiagnostic()
    {
        const string test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmOwningComponentBase<TestViewModel>, IAsyncDisposable
    {
        public ValueTask {|#0:DisposeAsync|}() => default;
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", "IAsyncDisposable", "MvvmOwningComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewInheritingBypassFromIntermediateBase_ReportsOnlyOnDeclaringType()
    {
        const string test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public abstract class MyViewBase : MvvmComponentBase<TestViewModel>, IAsyncDisposable
    {
        public ValueTask {|#0:DisposeAsync|}() => default;
    }

    public class MyView : MyViewBase
    {
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyViewBase", "IAsyncDisposable", "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewOverridingDisposeBool_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewListingIDisposableWithoutOwnImplementation_NoDiagnostic()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IDisposable
    {
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NonMvvmComponentImplementingIAsyncDisposable_NoDiagnostic()
    {
        const string test = @"
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase, IAsyncDisposable
    {
        public ValueTask DisposeAsync() => default;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
