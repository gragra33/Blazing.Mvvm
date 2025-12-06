using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.DisposePatternAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="DisposePatternAnalyzer"/>
/// </summary>
public class DisposePatternAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithMessengerRegistration_WithoutDispose_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace TestNamespace
{
    public class {|#0:TestViewModel|} : ViewModelBase
    {
        public TestViewModel()
        {
            WeakReferenceMessenger.Default.Register<MyMessage>(this, HandleMessage);
        }

        private void HandleMessage(object recipient, MyMessage message)
        {
        }
    }

    public class MyMessage { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.DisposePatternMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelWithMessengerRegistration_WithDispose_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase, IDisposable
    {
        public TestViewModel()
        {
            WeakReferenceMessenger.Default.Register<MyMessage>(this, HandleMessage);
        }

        private void HandleMessage(object recipient, MyMessage message)
        {
        }

        public void Dispose()
        {
            WeakReferenceMessenger.Default.Unregister<MyMessage>(this);
            GC.SuppressFinalize(this);
        }
    }

    public class MyMessage { }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithDisposableField_WithoutDispose_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Net.Http;

namespace TestNamespace
{
    public class {|#0:TestViewModel|} : ViewModelBase
    {
        private readonly HttpClient _httpClient = new HttpClient();
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.DisposePatternMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelWithDisposableField_WithDispose_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System;
using System.Net.Http;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase, IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ViewModelWithEventSubscription_WithoutDispose_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System;

namespace TestNamespace
{
    public class {|#0:TestViewModel|} : ViewModelBase
    {
        public TestViewModel(IMyService service)
        {
            service.DataChanged += OnDataChanged;
        }

        private void OnDataChanged(object sender, EventArgs e)
        {
        }
    }

    public interface IMyService
    {
        event EventHandler DataChanged;
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.DisposePatternMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelWithTimer_WithoutDispose_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using System.Threading;

namespace TestNamespace
{
    public class {|#0:TestViewModel|} : ViewModelBase
    {
        private readonly Timer _timer;

        public TestViewModel()
        {
            _timer = new Timer(OnTick, null, 0, 1000);
        }

        private void OnTick(object state)
        {
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.DisposePatternMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ViewModelWithoutDisposableResources_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class TestViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task RecipientViewModelBase_WithOnActivated_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace TestNamespace
{
    public class TestViewModel : RecipientViewModelBase
    {
        protected override void OnActivated()
        {
            Messenger.Register<MyMessage>(this, HandleMessage);
        }

        private void HandleMessage(object recipient, MyMessage message)
        {
        }
    }

    public class MyMessage { }
}";

        // RecipientViewModelBase handles cleanup automatically
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
