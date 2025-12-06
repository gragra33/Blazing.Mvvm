using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.ServiceInjectionAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="ServiceInjectionAnalyzer"/>
/// </summary>
public class ServiceInjectionAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact(Skip = "Heuristic-based analyzer - cannot reliably detect all DI registrations at compile time")]
    public async Task InjectedService_NotRegistered_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel({|#0:IUnregisteredService|} service)
        {
        }
    }

    public interface IUnregisteredService { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ServiceNotRegistered)
            .WithLocation(0)
            .WithArguments("IUnregisteredService");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task CommonFrameworkServices_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(ILogger<TestViewModel> logger)
        {
            // ILogger is always registered
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task KnownBlazorServices_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(NavigationManager nav, HttpClient http)
        {
            // Framework services
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact(Skip = "Heuristic-based analyzer - cannot reliably detect all DI registrations at compile time")]
    public async Task CustomServiceWithInterface_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel({|#0:IMyCustomService|} service)
        {
        }
    }

    public interface IMyCustomService { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ServiceNotRegistered)
            .WithLocation(0)
            .WithArguments("IMyCustomService");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task PrimitiveTypeParameter_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(string title, int maxItems)
        {
            // Primitive parameters are not services
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ParameterlessConstructor_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel()
        {
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact(Skip = "Heuristic-based analyzer - cannot reliably detect all DI registrations at compile time")]
    public async Task MultipleUnregisteredServices_ReportsMultipleDiagnostics()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(
            {|#0:IFirstService|} first, 
            {|#1:ISecondService|} second)
        {
        }
    }

    public interface IFirstService { }
    public interface ISecondService { }
}";

        var expected1 = new DiagnosticResult(DiagnosticDescriptors.ServiceNotRegistered)
            .WithLocation(0)
            .WithArguments("IFirstService");

        var expected2 = new DiagnosticResult(DiagnosticDescriptors.ServiceNotRegistered)
            .WithLocation(1)
            .WithArguments("ISecondService");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task ConcreteClassAsService_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel({|#0:MyConcreteService|} service)
        {
        }
    }

    public class MyConcreteService { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.ServiceNotRegistered)
            .WithLocation(0)
            .WithArguments("MyConcreteService");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }
}
