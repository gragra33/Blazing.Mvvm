using Blazing.Mvvm.Analyzers.Analyzers;
using Blazing.Mvvm.Analyzers.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpCodeFixVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.DisposePatternAnalyzer,
    Blazing.Mvvm.Analyzers.CodeFixProviders.DisposePatternCodeFixProvider>;

namespace Blazing.Mvvm.Analyzers.Tests.CodeFixTests;

/// <summary>
/// Unit tests for <see cref="DisposePatternCodeFixProvider"/>
/// </summary>
public class DisposePatternCodeFixProviderTests
{
    [Fact]
    public async Task ViewModelWithMessenger_AddsDisposable()
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

        const string fixedCode = @"
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
            WeakReferenceMessenger.Default.UnregisterAll(this);
            GC.SuppressFinalize(this);
        }
    }

    public class MyMessage { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.DisposePatternMissing)
            .WithLocation(0)
            .WithArguments("TestViewModel");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
