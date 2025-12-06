using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Blazing.Mvvm.Analyzers.Tests;

/// <summary>
/// Base class for analyzer test helpers
/// </summary>
public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Creates a new analyzer test instance
    /// </summary>
    public static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateTest()
    {
        return new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
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
            }
        };
    }

    /// <summary>
    /// Verifies analyzer diagnostics
    /// </summary>
    public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = CreateTest();
        test.TestCode = source;
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }
}
