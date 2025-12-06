using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ViewModelKeyConsistencyAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="ViewModelKeyConsistencyAnalyzer"/>
/// </summary>
public class ViewModelKeyConsistencyAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelKeyNotUsedInNavigation_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Services;

namespace TestNamespace
{
    [ViewModelDefinition]
    [ViewModelKey(""{|#0:product-details|}"")]
    public class ProductViewModel : ViewModelBase
    {
    }

    public class NavigationService
    {
        public void Navigate(INavigationService nav)
        {
            nav.NavigateTo(""product-info""); // Different key
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ViewModelKeyInconsistent)
            .WithLocation(0)
            .WithArguments("product-details");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelKeyUsedInNavigation_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Services;

namespace TestNamespace
{
    [ViewModelDefinition]
    [ViewModelKey(""product-details"")]
    public class ProductViewModel : ViewModelBase
    {
    }

    public class NavigationService
    {
        public void Navigate(INavigationService nav)
        {
            nav.NavigateTo(""product-details""); // Matching key
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithoutKey_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class ProductViewModel : ViewModelBase
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleViewModelsWithKeys_AllUsed_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Services;

namespace TestNamespace
{
    [ViewModelDefinition]
    [ViewModelKey(""products"")]
    public class ProductsViewModel : ViewModelBase
    {
    }

    [ViewModelDefinition]
    [ViewModelKey(""orders"")]
    public class OrdersViewModel : ViewModelBase
    {
    }

    public class NavigationService
    {
        public void Navigate(INavigationService nav)
        {
            nav.NavigateTo(""products"");
            nav.NavigateTo(""orders"");
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelKeyUsedMultipleTimes_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Services;

namespace TestNamespace
{
    [ViewModelDefinition]
    [ViewModelKey(""product-details"")]
    public class ProductViewModel : ViewModelBase
    {
    }

    public class NavigationService
    {
        public void NavigateToProduct(INavigationService nav)
        {
            nav.NavigateTo(""product-details"");
        }

        public void GoToProduct(INavigationService nav)
        {
            nav.NavigateTo(""product-details""); // Multiple usages OK
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
