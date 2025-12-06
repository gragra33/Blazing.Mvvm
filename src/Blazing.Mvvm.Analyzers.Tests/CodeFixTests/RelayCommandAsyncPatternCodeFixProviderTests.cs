using Blazing.Mvvm.Analyzers.Analyzers;
using Blazing.Mvvm.Analyzers.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpCodeFixVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.RelayCommandAsyncPatternAnalyzer,
    Blazing.Mvvm.Analyzers.CodeFixProviders.RelayCommandAsyncPatternCodeFixProvider>;

namespace Blazing.Mvvm.Analyzers.Tests.CodeFixTests;

/// <summary>
/// Unit tests for <see cref="RelayCommandAsyncPatternCodeFixProvider"/>
/// </summary>
public class RelayCommandAsyncPatternCodeFixProviderTests
{
    [Fact]
    public async Task AsyncVoidRelayCommand_ConvertsToAsyncTask()
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

        const string fixedCode = @"
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

        var expected = new DiagnosticResult(DiagnosticDescriptors.AsyncVoidRelayCommand)
            .WithLocation(0)
            .WithArguments("LoadData");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task AsyncVoidWithoutAsyncSuffix_AddsAsyncSuffix()
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
        private async void {|#0:Save|}()
        {
            await Task.Delay(100);
        }
    }
}";

        const string fixedCode = @"
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        [RelayCommand]
        private async Task SaveAsync()
        {
            await Task.Delay(100);
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.AsyncVoidRelayCommand)
            .WithLocation(0)
            .WithArguments("Save");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
