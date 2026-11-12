using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
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

    [Theory]
    [InlineData("public new void Dispose() { base.Dispose(); }")]
    [InlineData("public new void Dispose() { Dispose(true); }")]
    [InlineData("void IDisposable.Dispose() { Dispose(); }")]
    [InlineData("void IDisposable.Dispose() => base.Dispose(true);")]
    public async Task ViewReimplementingIDisposableAndCallingBase_NoDiagnostic(string implementation)
    {
        var test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IDisposable
    {
        " + implementation + @"
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task OwningViewReimplementingIDisposableAndCallingBase_NoDiagnostic()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmOwningComponentBase<TestViewModel>, IDisposable
    {
        public void Dispose() => Dispose(true);
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task OwningViewReimplementingIDisposableWithoutCallingBase_ReportsDiagnostic()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmOwningComponentBase<TestViewModel>, IDisposable
    {
        public void {|#0:Dispose|}()
        {
        }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", "IDisposable", "MvvmOwningComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewReimplementingIDisposableAndDisposingOnlyAField_ReportsDiagnostic()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IDisposable
    {
        private readonly IDisposable _timer = null!;

        public new void {|#0:Dispose|}() => _timer.Dispose();
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

    [Theory]
    [InlineData("public new void {|#0:Dispose|}() { Dispose(false); }")]
    [InlineData("public new void {|#0:Dispose|}() { Dispose(1); } private void Dispose(int value) { }")]
    [InlineData("public new void {|#0:Dispose|}() { Dispose(true, 1); } private void Dispose(bool first, int second) { }")]
    public async Task ViewReimplementingIDisposableWithoutManagedCleanup_ReportsDiagnostic(string implementation)
    {
        var test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IDisposable
    {
        " + implementation + @"
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

    [Theory]
    [InlineData("IDisposable", "public new void {|#0:Dispose|}() { }")]
    [InlineData("IAsyncDisposable", "public System.Threading.Tasks.ValueTask {|#0:DisposeAsync|}() => default;")]
    public async Task GeneratedView_ReportsDiagnostic(string interfaceName, string implementation)
    {
        var test = @"// <auto-generated/>
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, " + interfaceName + @"
    {
        " + implementation + @"
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", interfaceName, "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewInheritingIDisposableReimplementation_ReportsOnlyOnDeclaringType()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public abstract class MyViewBase : MvvmComponentBase<TestViewModel>, IDisposable
    {
        public new void {|#0:Dispose|}()
        {
        }
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
            .WithArguments("MyViewBase", "IDisposable", "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewOverridingDisposeAsyncCore_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>
    {
        protected override ValueTask DisposeAsyncCore() => base.DisposeAsyncCore();
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
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

    [Fact]
    public async Task OwningViewReimplementingIAsyncDisposableAndCallingBase_NoDiagnostic()
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
        public new async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
        }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewDerivingFromViewThatRelistsInterface_ReportsOnlyOnRelistingView()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyViewBase : MvvmComponentBase<TestViewModel>
    {
        public new void Dispose()
        {
        }
    }

    public class {|#0:MyView|} : MyViewBase, IDisposable
    {
    }

    public class MyDerivedView : MyView
    {
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

    [Theory]
    [InlineData("IDisposable", "public new void Dispose() { }", "public new void {|#0:Dispose|}() { base.Dispose(); }")]
    [InlineData("IAsyncDisposable", "public ValueTask DisposeAsync() => default;", "public new async ValueTask {|#0:DisposeAsync|}() { await base.DisposeAsync(); }")]
    public async Task ViewCallingBaseThatResolvesToHidingMethod_ReportsDiagnostic(string interfaceName, string hidingMethod, string viewMethod)
    {
        var test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyViewBase : MvvmComponentBase<TestViewModel>
    {
        " + hidingMethod + @"
    }

    public class MyView : MyViewBase, " + interfaceName + @"
    {
        " + viewMethod + @"
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", interfaceName, "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewRelistingInterfaceMappedToIntermediateWithUnresolvedBaseCall_ReportsDiagnostic()
    {
        // While code is being typed the base call may not bind; the analyzer must still run and report the bypass
        const string test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyViewBase : MvvmComponentBase<TestViewModel>
    {
        public async ValueTask DisposeAsync() => await base.{|#1:DisposeAsync|}();
    }

    public class {|#0:MyView|} : MyViewBase, IAsyncDisposable
    {
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new[]
        {
            new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
                .WithLocation(0)
                .WithArguments("MyView", "IAsyncDisposable", "MvvmComponentBase"),
            DiagnosticResult.CompilerError("CS0117").WithLocation(1).WithArguments("Blazing.Mvvm.Components.MvvmComponentBase<TestNamespace.TestViewModel>", "DisposeAsync")
        };

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData("public new void Dispose() => base.Dispose();", "public new void Dispose() => base.Dispose();")]
    [InlineData("public new void Dispose() { }", "public new void Dispose() => base.Dispose();")]
    public async Task ViewCallingBaseThatResolvesToIntermediateInAnotherAssembly_NoDiagnostic(string intermediateMethod, string viewMethod)
    {
        // The intermediate class is compiled into a separate assembly, so the analyzer sees it as metadata with no source.
        // Its body cannot be inspected, so a base call landing on it counts even when it hides the base method.
        var references = await AnalyzerTestReferenceAssemblies.Net80.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
        var library = CSharpCompilation.Create(
            "IntermediateViews",
            [
                CSharpSyntaxTree.ParseText(TestCode.BlazingMvvmStubs),
                CSharpSyntaxTree.ParseText(@"
namespace Library
{
    public class TestViewModel : Blazing.Mvvm.ComponentModel.ViewModelBase
    {
    }

    public class MyViewBase : Blazing.Mvvm.Components.MvvmComponentBase<TestViewModel>
    {
        " + intermediateMethod + @"
    }
}")
            ],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var emitResult = library.Emit(image);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

        var test = new CSharpAnalyzerTest<ViewDisposalBypassAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = AnalyzerTestReferenceAssemblies.Net80,
            TestCode = @"
using System;

namespace TestNamespace
{
    public class MyView : Library.MyViewBase, IDisposable
    {
        " + viewMethod + @"
    }
}"
        };
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromImage(image.ToArray()));

        await test.RunAsync();
    }

    [Fact]
    public async Task ViewCallingBaseThatResolvesToIntermediateCallingBase_NoDiagnostic()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyViewBase : MvvmComponentBase<TestViewModel>
    {
        public new void Dispose() => base.Dispose();
    }

    public class MyView : MyViewBase, IDisposable
    {
        public new void Dispose() => base.Dispose();
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewDeclaringAbstractDisposeAsync_ReportsDiagnostic()
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
        public abstract ValueTask {|#0:DisposeAsync|}();
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
    public async Task CompilationWithoutBlazingMvvm_NoDiagnostic()
    {
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<ViewDisposalBypassAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = AnalyzerTestReferenceAssemblies.Net80,
            TestCode = @"
using System;

public class Plain : IDisposable
{
    public void Dispose()
    {
    }
}"
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task LayoutOverridingDisposeAsyncCore_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyLayout : MvvmLayoutComponentBase<TestViewModel>
    {
        protected override async ValueTask DisposeAsyncCore()
        {
            await base.DisposeAsyncCore();
        }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewReimplementingIDisposableWithBaseCallOnlyInLambda_ReportsDiagnostic()
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
            Action never = () => base.Dispose();
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
    public async Task ViewReimplementingIDisposableWithBaseCallInLocalFunction_NoDiagnostic()
    {
        const string test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyView : MvvmComponentBase<TestViewModel>, IDisposable
    {
        public new void Dispose()
        {
            Release();

            void Release() => base.Dispose();
        }
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("IDisposable", "public new void Dispose() { }")]
    [InlineData("IAsyncDisposable", "public ValueTask DisposeAsync() => default;")]
    public async Task ViewRelistingInterfaceMappedToHidingMethodOnIntermediate_ReportsDiagnostic(string interfaceName, string hidingMethod)
    {
        var test = @"
using System;
using System.Threading.Tasks;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyViewBase : MvvmComponentBase<TestViewModel>
    {
        " + hidingMethod + @"
    }

    public class {|#0:MyView|} : MyViewBase, " + interfaceName + @"
    {
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewDisposalBypassed)
            .WithLocation(0)
            .WithArguments("MyView", interfaceName, "MvvmComponentBase");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Theory]
    [InlineData("public new void Dispose() { base.Dispose(); }")]
    [InlineData("public new void Dispose() { Dispose(true); }")]
    public async Task ViewRelistingIDisposableMappedToIntermediateCallingBase_NoDiagnostic(string hidingMethod)
    {
        var test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyViewBase : MvvmComponentBase<TestViewModel>
    {
        " + hidingMethod + @"
    }

    public class MyView : MyViewBase, IDisposable
    {
    }

    public class TestViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("public new void Dispose() { Dispose(false); }")]
    [InlineData("public new void Dispose() { Action never = () => base.Dispose(); }")]
    [InlineData("private IDisposable _timer; public new void Dispose() { _timer.Dispose(); }")]
    [InlineData("private IDisposable _timer; public new void Dispose() { _timer?.Dispose(); }")]
    [InlineData("public new void Dispose() { Dispose(true, true); } private void Dispose(bool first, bool second) { }")]
    public async Task ViewRelistingIDisposableMappedToIntermediateSkippingCleanup_ReportsDiagnostic(string hidingMethod)
    {
        var test = @"
using System;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyViewBase : MvvmComponentBase<TestViewModel>
    {
        " + hidingMethod + @"
    }

    public class {|#0:MyView|} : MyViewBase, IDisposable
    {
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
}
