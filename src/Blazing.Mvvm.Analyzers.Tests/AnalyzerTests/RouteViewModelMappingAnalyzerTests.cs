using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.RouteViewModelMappingAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="RouteViewModelMappingAnalyzer"/>
/// </summary>
public class RouteViewModelMappingAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PageWithoutViewModel_ReportsDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    [Route(""/products"")]
    public class {|#0:ProductsPage|} : ComponentBase
    {
        public string Title { get; set; }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.PageMissingViewModel)
            .WithLocation(0)
            .WithArguments("ProductsPage");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task PageWithViewModel_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    [Route(""/products"")]
    public class ProductsPage : MvvmComponentBase<ProductsViewModel>
    {
    }

    [ViewModelDefinition]
    public class ProductsViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ComponentWithoutRoute_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [Parameter]
        public string Title { get; set; }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PageInheritingFromMvvmOwningComponentBase_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    [Route(""/products"")]
    public class ProductsPage : MvvmOwningComponentBase<ProductsViewModel>
    {
    }

    [ViewModelDefinition]
    public class ProductsViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task LayoutComponent_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MainLayout : LayoutComponentBase
    {
        public string Title { get; set; }
    }
}";

        // Layout components don't need ViewModels
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleRoutesWithoutViewModel_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    [Route(""/products"")]
    public class {|#0:ProductsPage|} : ComponentBase
    {
    }

    [Route(""/orders"")]
    public class {|#1:OrdersPage|} : ComponentBase
    {
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.PageMissingViewModel)
            .WithLocation(0)
            .WithArguments("ProductsPage");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.PageMissingViewModel)
            .WithLocation(1)
            .WithArguments("OrdersPage");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }
}
