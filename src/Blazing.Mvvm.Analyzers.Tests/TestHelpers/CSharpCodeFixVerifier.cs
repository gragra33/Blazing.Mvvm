using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Blazing.Mvvm.Analyzers.Tests;

/// <summary>
/// Base class for code fix provider test helpers
/// </summary>
public static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    /// <summary>
    /// Creates a new code fix test instance
    /// </summary>
    public static CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> CreateTest()
    {
        return new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
                .AddPackages(ImmutableArray.Create(
                    new PackageIdentity("Microsoft.AspNetCore.Components", "8.0.0"),
                    new PackageIdentity("Microsoft.AspNetCore.Components.Web", "8.0.0"),
                    new PackageIdentity("CommunityToolkit.Mvvm", "8.3.2"),
                    new PackageIdentity("Microsoft.Extensions.DependencyInjection.Abstractions", "8.0.0"),
                    new PackageIdentity("Microsoft.EntityFrameworkCore", "8.0.0")
                )),
            TestState =
            {
                // Add Blazing.Mvvm type stubs to every test
                Sources = { TestCode.BlazingMvvmStubs }
            },
            FixedState =
            {
                // Add Blazing.Mvvm type stubs to fixed code as well
                Sources = { TestCode.BlazingMvvmStubs }
            }
        };
    }

    /// <summary>
    /// Verifies code fix diagnostics and fix
    /// </summary>
    public static Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        var test = CreateTest();
        test.TestCode = source;
        test.FixedCode = fixedSource;
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    /// <summary>
    /// Verifies code fix diagnostics only
    /// </summary>
    public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = CreateTest();
        test.TestCode = source;
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }
}
