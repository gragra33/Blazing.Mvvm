using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.CommandPatternAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="CommandPatternAnalyzer"/>
/// </summary>
public class CommandPatternAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PublicMethodInViewModel_SuggestsRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public void {|#0:Save|}()
        {
            // Save logic
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("Save");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MethodWithRelayCommand_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private void Save()
        {
            // Save logic
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PrivateMethod_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private void Save()
        {
            // Private helper method
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ProtectedMethod_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        protected virtual void Save()
        {
            // Protected for inheritance
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PropertyAccessor_NoDiagnostic()
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
    public async Task OverrideMethod_NoDiagnostic()
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
            await base.OnInitializedAsync();
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PublicAsyncMethod_SuggestsRelayCommand()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public async Task {|#0:LoadDataAsync|}()
        {
            await Task.Delay(100);
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("LoadDataAsync");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MethodInNonViewModel_NoDiagnostic()
    {
        const string test = @"
namespace TestNamespace
{
    public class MyService
    {
        public void Execute()
        {
            // Service method, not a command
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultiplePublicMethods_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public void {|#0:Save|}()
        {
        }

        public void {|#1:Delete|}()
        {
        }
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(0)
            .WithArguments("Save");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.MethodShouldBeCommand)
            .WithLocation(1)
            .WithArguments("Delete");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }
}
