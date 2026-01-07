using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.EventCallbackTwoWayBindingAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="EventCallbackTwoWayBindingAnalyzer"/>
/// </summary>
public class EventCallbackTwoWayBindingAnalyzerTests
{
    private static string Normalize(string code) => code.Replace("\r\n", "\n").Replace("\r", "\n");

    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    #region Manual Two-Way Binding Detection Tests

    [Fact]
    public async Task ManualPropertyChangedSubscription_ReportsDiagnostic()
    {
        var test = Normalize(@"
using System.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public partial class CounterComponentViewModel : ViewModelBase
    {
        [ViewParameter]
        public int Counter { get; set; }
    }

    public class CounterComponent : MvvmComponentBase<CounterComponentViewModel>
    {
        [Parameter]
        public int Counter { get; set; }

        [Parameter]
        public EventCallback<int> CounterChanged { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            {|#0:ViewModel.PropertyChanged += OnViewModelPropertyChanged|};
        }

        private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Counter) && ViewModel.Counter != Counter)
            {
                await CounterChanged.InvokeAsync(ViewModel.Counter);
            }
        }
    }
}");

        var expected = new DiagnosticResult(DiagnosticDescriptors.ManualTwoWayBindingObsolete)
            .WithLocation(0)
            .WithArguments("Counter");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion

    #region Missing EventCallback Tests

    [Fact]
    public async Task ParameterWithViewParameterButMissingEventCallback_ReportsDiagnostic()
    {
        var test = Normalize(@"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public partial class CounterComponentViewModel : ViewModelBase
    {
        [ViewParameter]
        public int Counter { get; set; }
    }

    public class CounterComponent : MvvmComponentBase<CounterComponentViewModel>
    {
        [Parameter]
        public int {|#0:Counter|} { get; set; }
    }
}");

        var expected = new DiagnosticResult(DiagnosticDescriptors.EventCallbackMissing)
            .WithLocation(0)
            .WithArguments("Counter", "int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ParameterWithEventCallbackPresent_NoDiagnostic()
    {
        var test = Normalize(@"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public partial class CounterComponentViewModel : ViewModelBase
    {
        [ViewParameter]
        public int Counter { get; set; }
    }

    public class CounterComponent : MvvmComponentBase<CounterComponentViewModel>
    {
        [Parameter]
        public int Counter { get; set; }

        [Parameter]
        public EventCallback<int> CounterChanged { get; set; }
    }
}");

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ParameterWithoutViewParameter_NoDiagnostic()
    {
        var test = Normalize(@"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public partial class CounterComponentViewModel : ViewModelBase
    {
        public int Counter { get; set; }
    }

    public class CounterComponent : MvvmComponentBase<CounterComponentViewModel>
    {
        [Parameter]
        public int Counter { get; set; }
    }
}");

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    #endregion

    #region EventCallback Type Mismatch Tests

    [Fact]
    public async Task EventCallbackTypeMismatch_ReportsDiagnostic()
    {
        var test = Normalize(@"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public partial class CounterComponentViewModel : ViewModelBase
    {
        [ViewParameter]
        public int Counter { get; set; }
    }

    public class CounterComponent : MvvmComponentBase<CounterComponentViewModel>
    {
        [Parameter]
        public int Counter { get; set; }

        [Parameter]
        public EventCallback<string> {|#0:CounterChanged|} { get; set; }
    }
}");

        var expected = new DiagnosticResult(DiagnosticDescriptors.EventCallbackTypeMismatch)
            .WithLocation(0)
            .WithArguments("string", "int", "Counter");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task EventCallbackMatchingType_NoDiagnostic()
    {
        var test = Normalize(@"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public partial class CounterComponentViewModel : ViewModelBase
    {
        [ViewParameter]
        public int Counter { get; set; }
    }

    public class CounterComponent : MvvmComponentBase<CounterComponentViewModel>
    {
        [Parameter]
        public int Counter { get; set; }

        [Parameter]
        public EventCallback<int> CounterChanged { get; set; }
    }
}");

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    #endregion

    #region Non-MVVM Component Tests

    [Fact]
    public async Task RegularComponentWithoutMvvmBase_NoDiagnostic()
    {
        var test = Normalize(@"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class RegularComponent : ComponentBase
    {
        [Parameter]
        public int Counter { get; set; }
    }
}");

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    #endregion
}
