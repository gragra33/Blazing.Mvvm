using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.RelayCommandAsyncPatternAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="RelayCommandAsyncPatternAnalyzer"/>
/// </summary>
public class RelayCommandAsyncPatternAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AsyncVoidRelayCommand_ReportsDiagnostic()
    {
        const string test = @"
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private async void {|#0:LoadData|}()
        {
            await Task.Delay(100);
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.AsyncVoidRelayCommand)
            .WithLocation(0)
            .WithArguments("LoadData");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task AsyncTaskRelayCommand_NoDiagnostic()
    {
        const string test = @"
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private async Task LoadDataAsync()
        {
            await Task.Delay(100);
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task SyncRelayCommand_NoDiagnostic()
    {
        const string test = @"
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private void Execute()
        {
            // Synchronous operation
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AsyncVoidWithoutAttribute_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private async void LoadData()
        {
            await Task.Delay(100);
        }
    }
}";

        // Analyzer only checks methods with [RelayCommand]
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task RelayCommandWithCancellationToken_NoDiagnostic()
    {
        const string test = @"
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private async Task LoadDataAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleAsyncVoidRelayCommands_ReportsMultipleDiagnostics()
    {
        const string test = @"
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private async void {|#0:LoadData|}()
        {
            await Task.Delay(100);
        }

        [RelayCommand]
        private async void {|#1:SaveData|}()
        {
            await Task.Delay(100);
        }
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.AsyncVoidRelayCommand)
            .WithLocation(0)
            .WithArguments("LoadData");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.AsyncVoidRelayCommand)
            .WithLocation(1)
            .WithArguments("SaveData");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }
}
