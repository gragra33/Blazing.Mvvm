using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Blazing.Mvvm.Analyzers.Tests;

/// <summary>
/// Verifies diagnostics produced from compilation end analyzer actions.
/// </summary>
public static class CompilationEndAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Verifies analyzer diagnostics for a markup source document.
    /// </summary>
    public static async Task VerifyAnalyzerAsync(string markupSource, params ExpectedDiagnostic[] expectedDiagnostics)
    {
        await VerifyAnalyzerAsync(markupSource, additionalFiles: [], expectedDiagnostics).ConfigureAwait(false);
    }

    /// <summary>
    /// Verifies analyzer diagnostics for a markup source document with additional files.
    /// </summary>
    public static async Task VerifyAnalyzerAsync(
        string markupSource,
        IEnumerable<(string Path, string Content)> additionalFiles,
        params ExpectedDiagnostic[] expectedDiagnostics)
    {
        var (source, spans) = ParseMarkup(markupSource);
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(SourceText.From(TestCode.BlazingMvvmStubs, Encoding.UTF8), new CSharpParseOptions(LanguageVersion.CSharp12)),
            CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), new CSharpParseOptions(LanguageVersion.CSharp12))
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: $"{typeof(TAnalyzer).Name}.Tests",
            syntaxTrees: syntaxTrees,
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilerErrors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

        var analyzer = new TAnalyzer();
        var analyzerOptions = new AnalyzerOptions(
            additionalFiles.Select(static file => (AdditionalText)new InMemoryAdditionalText(file.Path, file.Content)).ToImmutableArray());
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            [analyzer],
            new CompilationWithAnalyzersOptions(analyzerOptions, onAnalyzerException: null, concurrentAnalysis: true, logAnalyzerExecutionTime: false, reportSuppressedDiagnostics: false));
        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);

        Assert.Equal(expectedDiagnostics.Length, diagnostics.Length);

        foreach (var expected in expectedDiagnostics)
        {
            Assert.True(spans.TryGetValue(expected.MarkerId, out var expectedSpan), $"Marker '{expected.MarkerId}' not found.");

            var matchingDiagnostic = diagnostics.SingleOrDefault(d =>
                d.Id == expected.DiagnosticId &&
                SpansOverlap(d.Location.SourceSpan, expectedSpan));

            Assert.True(
                matchingDiagnostic is not null,
                $"Expected diagnostic '{expected.DiagnosticId}' at marker '{expected.MarkerId}' was not found.{Environment.NewLine}Compiler errors:{Environment.NewLine}{string.Join(Environment.NewLine, compilerErrors.Select(d => d.ToString()))}{Environment.NewLine}Actual diagnostics:{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()))}");

            foreach (var argument in expected.Arguments)
            {
                var message = matchingDiagnostic.GetMessage();
                if (!message.Contains("{0}", StringComparison.Ordinal) &&
                    !message.Contains("{1}", StringComparison.Ordinal) &&
                    !message.Contains("{2}", StringComparison.Ordinal))
                {
                    Assert.Contains(argument, message, StringComparison.Ordinal);
                }
            }
        }
    }

    /// <summary>
    /// Represents an expected diagnostic tied to a markup marker.
    /// </summary>
    public sealed record ExpectedDiagnostic(string MarkerId, string DiagnosticId, params string[] Arguments);

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var trustedPlatformAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        var additionalAssemblies = new[]
        {
            ResolveFrameworkAssembly("Microsoft.AspNetCore.App.Ref", "net8.0", "Microsoft.AspNetCore.Components.dll"),
            ResolvePackageAssembly("microsoft.extensions.dependencyinjection.abstractions", "8.0.0", "lib", "net8.0", "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
            ResolvePackageAssembly("communitytoolkit.mvvm", "8.3.2", "lib", "net8.0", "CommunityToolkit.Mvvm.dll"),
            typeof(Enumerable).Assembly.Location,
            typeof(PropertyChangedEventHandler).Assembly.Location,
        };

        return trustedPlatformAssemblies
            .Concat(additionalAssemblies)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path));
    }

    private static string ResolveFrameworkAssembly(string packName, string targetFramework, string assemblyName)
    {
        var packRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "packs", packName);
        var latestPack = Directory.GetDirectories(packRoot)
            .OrderByDescending(static directory => directory, StringComparer.OrdinalIgnoreCase)
            .First(directory => File.Exists(Path.Combine(directory, "ref", targetFramework, assemblyName)));

        return Path.Combine(latestPack, "ref", targetFramework, assemblyName);
    }

    private static string ResolvePackageAssembly(string packageId, string version, params string[] relativePath)
    {
        var packageRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", packageId, version);
        return Path.Combine(new[] { packageRoot }.Concat(relativePath).ToArray());
    }

    private static (string Source, Dictionary<string, TextSpan> Spans) ParseMarkup(string markupSource)
    {
        var builder = new StringBuilder(markupSource.Length);
        var spans = new Dictionary<string, TextSpan>(StringComparer.Ordinal);

        for (var index = 0; index < markupSource.Length;)
        {
            if (markupSource.AsSpan(index).StartsWith("{|#", StringComparison.Ordinal))
            {
                var markerStart = index + 3;
                var colonIndex = markupSource.IndexOf(':', markerStart);
                var endIndex = markupSource.IndexOf("|}", colonIndex + 1, StringComparison.Ordinal);

                Assert.True(colonIndex >= 0 && endIndex >= 0, "Invalid markup syntax.");

                var markerId = markupSource[markerStart..colonIndex];
                var content = markupSource[(colonIndex + 1)..endIndex];
                var spanStart = builder.Length;

                builder.Append(content);
                spans[markerId] = new TextSpan(spanStart, content.Length);
                index = endIndex + 2;
                continue;
            }

            builder.Append(markupSource[index]);
            index++;
        }

        return (builder.ToString(), spans);
    }

    private static bool SpansOverlap(TextSpan left, TextSpan right)
    {
        return left.IntersectsWith(right) || left == right || left.Contains(right.Start) || right.Contains(left.Start);
    }

    private sealed class InMemoryAdditionalText(string path, string content) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => SourceText.From(content, Encoding.UTF8);
    }
}