using Microsoft.CodeAnalysis.Testing;
using System.Diagnostics.CodeAnalysis;

namespace Blazing.Mvvm.Analyzers.Tests;

/// <summary>
/// Custom verifier that normalizes line endings before comparison
/// </summary>
public class LineEndingNormalizingVerifier : IVerifier
{
    private readonly DefaultVerifier _defaultVerifier = new();

    /// <summary>
    /// Normalizes line endings in a string to LF only
    /// </summary>
    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    public void Empty<T>(string collectionName, IEnumerable<T> collection)
    {
        _defaultVerifier.Empty(collectionName, collection);
    }

    public void Equal<T>(T expected, T actual, string? message = null)
    {
        if (expected is string expectedString && actual is string actualString)
        {
            // Normalize line endings to LF for comparison
            expectedString = NormalizeLineEndings(expectedString);
            actualString = NormalizeLineEndings(actualString);
            _defaultVerifier.Equal(expectedString, actualString, message);
        }
        else
        {
            _defaultVerifier.Equal(expected, actual, message);
        }
    }

    public void False(bool assert, string? message = null)
    {
        _defaultVerifier.False(assert, message);
    }

    [DoesNotReturn]
    public void Fail(string? message = null)
    {
        _defaultVerifier.Fail(message);
    }

    public void LanguageIsSupported(string language)
    {
        _defaultVerifier.LanguageIsSupported(language);
    }

    public void NotEmpty<T>(string collectionName, IEnumerable<T> collection)
    {
        _defaultVerifier.NotEmpty(collectionName, collection);
    }

    public IVerifier PushContext(string context)
    {
        // Return this instance - the pushed context is handled by the default verifier
        _defaultVerifier.PushContext(context);
        return this;
    }

    public void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T>? equalityComparer = null, string? message = null)
    {
        // If comparing strings, normalize line endings
        if (typeof(T) == typeof(string))
        {
            var normalizedExpected = expected.Cast<string>().Select(NormalizeLineEndings).Cast<T>();
            var normalizedActual = actual.Cast<string>().Select(NormalizeLineEndings).Cast<T>();
            _defaultVerifier.SequenceEqual(normalizedExpected, normalizedActual, equalityComparer, message);
        }
        else
        {
            _defaultVerifier.SequenceEqual(expected, actual, equalityComparer, message);
        }
    }

    public void True(bool assert, string? message = null)
    {
        _defaultVerifier.True(assert, message);
    }
}
