using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.StateHasChangedOveruseAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="StateHasChangedOveruseAnalyzer"/>
/// </summary>
public class StateHasChangedOveruseAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task StateHasChangedWithObservableProperty_ReportsDiagnostic()
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

        // Manually declare the generated property for testing
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public void UpdateName(string newName)
        {
            Name = newName;
            {|#0:StateHasChanged()|};
        }

        private void StateHasChanged()
        {
            // Custom implementation or inherited
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.StateHasChangedUnnecessary)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task StateHasChangedWithSetProperty_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public void UpdateName(string newName)
        {
            Name = newName;
            {|#0:StateHasChanged()|};
        }

        private void StateHasChanged()
        {
            // Custom implementation
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.StateHasChangedUnnecessary)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task StateHasChangedWithoutPropertyNotification_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        private string _name;

        public void UpdateName(string newName)
        {
            _name = newName; // Direct field assignment
            StateHasChanged();
        }

        private void StateHasChanged()
        {
            // May be necessary here
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task StateHasChangedInComponent_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        private string _name;

        public void UpdateName(string newName)
        {
            _name = newName;
            StateHasChanged();
        }
    }
}";

        // Analyzer only checks ViewModels
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task StateHasChangedAfterAsyncOperation_MayBeNecessary_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public async Task LoadDataAsync()
        {
            await Task.Delay(100);
            StateHasChanged();
        }

        private void StateHasChanged()
        {
            // May be necessary after async operations
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleStateHasChangedCalls_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestNamespace
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _firstName;

        [ObservableProperty]
        private string _lastName;

        // Manually declare generated properties for testing
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public void UpdateName(string first, string last)
        {
            FirstName = first;
            {|#0:StateHasChanged()|};
            
            LastName = last;
            {|#1:StateHasChanged()|};
        }

        private void StateHasChanged()
        {
        }
    }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.StateHasChangedUnnecessary)
            .WithLocation(0);

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.StateHasChangedUnnecessary)
            .WithLocation(1);

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task InvokeAsync_NotStateHasChanged_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public async Task DoWorkAsync()
        {
            await InvokeAsync(() => 
            {
                // Some work
            });
        }

        private Task InvokeAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
