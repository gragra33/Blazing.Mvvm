using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.MvvmNavLinkTypeSafetyAnalyzer>;
using VerifyCompilationEndCS = Blazing.Mvvm.Analyzers.Tests.CompilationEndAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.MvvmNavLinkTypeSafetyAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="MvvmNavLinkTypeSafetyAnalyzer"/>
/// </summary>
public class MvvmNavLinkTypeSafetyAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MvvmNavLinkWithInvalidViewModel_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.Routing;

namespace TestNamespace
{
    public class MyComponent
    {
        public void RenderLink()
        {
            var link = new MvvmNavLink<{|#0:UnregisteredViewModel|}>();
        }
    }

    public class UnregisteredViewModel : ViewModelBase
    {
        // No [ViewModelDefinition] attribute
    }
}";

        await VerifyCompilationEndCS.VerifyAnalyzerAsync(
            test,
            new VerifyCompilationEndCS.ExpectedDiagnostic("0", DiagnosticDescriptors.MvvmNavLinkInvalidViewModel.Id, "UnregisteredViewModel"));
    }

    [Fact]
    public async Task MvvmNavLinkWithValidViewModel_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TestNamespace
{
    public class MyComponent
    {
        public void RenderLink()
        {
            var link = new MvvmNavLink<ProductViewModel>();
        }
    }

    [ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
    public class ProductViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MvvmNavLinkWithViewType_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TestNamespace
{
    public class MyComponent
    {
        public void RenderLink()
        {
            var link = new MvvmNavLink<ProductViewModel>();
        }
    }

    [ViewModelDefinition(Lifetime = ServiceLifetime.Transient, ViewType = typeof(ProductView))]
    public class ProductViewModel : ViewModelBase
    {
    }

    public class ProductView : Blazing.Mvvm.Components.MvvmComponentBase<ProductViewModel>
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MvvmNavLinkWithNonViewModel_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components.Routing;

namespace TestNamespace
{
    public class MyComponent
    {
        public void RenderLink()
        {
            var link = new MvvmNavLink<{|#0:NotAViewModel|}>();
        }
    }

    public class NotAViewModel
    {
        // Not a ViewModel at all
    }
}";

        await VerifyCompilationEndCS.VerifyAnalyzerAsync(
            test,
            new VerifyCompilationEndCS.ExpectedDiagnostic("0", DiagnosticDescriptors.MvvmNavLinkInvalidViewModel.Id, "NotAViewModel"));
    }

    [Fact]
    public async Task MvvmNavLinkWithAbstractViewModel_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.Routing;

namespace TestNamespace
{
    public class MyComponent
    {
        public void RenderLink()
        {
            var link = new MvvmNavLink<{|#0:BaseViewModel|}>();
        }
    }

    public abstract class BaseViewModel : ViewModelBase
    {
    }
}";

        await VerifyCompilationEndCS.VerifyAnalyzerAsync(
            test,
            new VerifyCompilationEndCS.ExpectedDiagnostic("0", DiagnosticDescriptors.MvvmNavLinkInvalidViewModel.Id, "BaseViewModel"));
    }
}
