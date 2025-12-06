using Blazing.Mvvm.Analyzers.Analyzers;
using Blazing.Mvvm.Analyzers.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpCodeFixVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.StateHasChangedOveruseAnalyzer,
    Blazing.Mvvm.Analyzers.CodeFixProviders.StateHasChangedOveruseCodeFixProvider>;

namespace Blazing.Mvvm.Analyzers.Tests.CodeFixTests;

/// <summary>
/// Unit tests for <see cref="StateHasChangedOveruseCodeFixProvider"/>
/// </summary>
public class StateHasChangedOveruseCodeFixProviderTests
{
    [Fact]
    public async Task StateHasChangedWithObservableProperty_RemovesCall()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name;

        public void UpdateName(string newName)
        {
            Name = newName;
            {|#0:StateHasChanged()|};
        }

        private void StateHasChanged()
        {
        }
    }
}";

        const string fixedCode = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name;

        public void UpdateName(string newName)
        {
            Name = newName;
        }

        private void StateHasChanged()
        {
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.StateHasChangedUnnecessary)
            .WithLocation(0);

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
