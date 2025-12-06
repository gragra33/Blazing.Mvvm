using Blazing.Mvvm.Analyzers.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpAnalyzerVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.MvvmOwningComponentBaseUsageAnalyzer>;

namespace Blazing.Mvvm.Analyzers.Tests.AnalyzerTests;

/// <summary>
/// Unit tests for <see cref="MvvmOwningComponentBaseUsageAnalyzer"/>
/// </summary>
public class MvvmOwningComponentBaseUsageAnalyzerTests
{
    [Fact]
    public async Task EmptyCode_NoDiagnostic()
    {
        const string test = "";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MvvmComponentBaseWithScopedService_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public class {|#0:MyComponent|} : MvvmComponentBase<TestViewModel>
    {
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(MyDbContext context)
        {
        }
    }

    public class MyDbContext : DbContext
    {
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MvvmOwningComponentBaseSuggested)
            .WithLocation(0)
            .WithArguments("MyComponent");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MvvmOwningComponentBaseWithScopedService_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public class MyComponent : MvvmOwningComponentBase<TestViewModel>
    {
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(MyDbContext context)
        {
        }
    }

    public class MyDbContext : DbContext
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MvvmComponentBaseWithoutScopedService_NoDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;

namespace TestNamespace
{
    public class MyComponent : MvvmComponentBase<TestViewModel>
    {
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(IMyService service)
        {
        }
    }

    public interface IMyService { }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MvvmComponentBaseWithDbConnection_ReportsDiagnostic()
    {
        const string test = @"
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using System.Data;

namespace TestNamespace
{
    public class {|#0:MyComponent|} : MvvmComponentBase<TestViewModel>
    {
    }

    [ViewModelDefinition]
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(IDbConnection connection)
        {
        }
    }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.MvvmOwningComponentBaseSuggested)
            .WithLocation(0)
            .WithArguments("MyComponent");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ComponentNotUsingMvvmBase_NoDiagnostic()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [Microsoft.AspNetCore.Components.Inject]
        public MyDbContext Context { get; set; }
    }

    public class MyDbContext : DbContext
    {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
