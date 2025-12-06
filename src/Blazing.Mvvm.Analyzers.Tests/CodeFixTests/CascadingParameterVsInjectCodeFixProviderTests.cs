using Blazing.Mvvm.Analyzers.Analyzers;
using Blazing.Mvvm.Analyzers.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Blazing.Mvvm.Analyzers.Tests.CSharpCodeFixVerifier<
    Blazing.Mvvm.Analyzers.Analyzers.CascadingParameterVsInjectAnalyzer,
    Blazing.Mvvm.Analyzers.CodeFixProviders.CascadingParameterVsInjectCodeFixProvider>;

namespace Blazing.Mvvm.Analyzers.Tests.CodeFixTests;

/// <summary>
/// Unit tests for <see cref="CascadingParameterVsInjectCodeFixProvider"/>
/// </summary>
public class CascadingParameterVsInjectCodeFixProviderTests
{
    [Fact]
    public async Task CascadingParameterForService_ReplacesWithInject()
    {
        const string test = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [CascadingParameter]
        public {|#0:IMyService|} MyService { get; set; }
    }

    public interface IMyService { }
}";

        const string fixedCode = @"
using Microsoft.AspNetCore.Components;

namespace TestNamespace
{
    public class MyComponent : ComponentBase
    {
        [Inject]
        public IMyService MyService { get; set; }
    }

    public interface IMyService { }
}";

        var expected = new DiagnosticResult(DiagnosticDescriptors.InjectPreferredOverCascading)
            .WithLocation(0)
            .WithArguments("MyService");

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
