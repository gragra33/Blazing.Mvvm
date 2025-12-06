using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.LifecycleMethodOverrideAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="LifecycleMethodOverrideAnalyzer"/>
/// </summary>
public class LifecycleMethodOverrideAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithConstructorLogic_SuggestsOnInitializedAsync()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public {|#0:TestViewModel|}()
        {
            // Initialization logic that could be async
            LoadData();
        }

        private void LoadData()
        {
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelOverridingOnInitializedAsync_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            await base.OnInitializedAsync();
        }

        private Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithParameterizedConstructor_SuggestsOnParametersSetAsync()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public {|#0:TestViewModel|}(IMyService service)
        {
            // Parameter-dependent initialization
            service.Initialize();
        }
    }

    public interface IMyService
    {
        void Initialize();
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.LifecycleMethodSuggestion)
            .WithLocation(0)
            .WithArguments("OnInitializedAsync");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelWithEmptyConstructor_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel()
        {
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithoutConstructor_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public string Name { get; set; }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithDependencyInjectionOnly_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private readonly IMyService _service;

        public TestViewModel(IMyService service)
        {
            _service = service; // Only assignment, no logic
        }
    }

    public interface IMyService { }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
