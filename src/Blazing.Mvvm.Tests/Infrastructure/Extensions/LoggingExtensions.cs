using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Tests.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for verifying log invocations on mocked <see cref="ILogger"/> instances.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Verifies that a log invocation with the specified log level and message occurred on the mocked logger.
    /// </summary>
    /// <typeparam name="TLogger">The type of the logger.</typeparam>
    /// <param name="logger">The mocked logger.</param>
    /// <param name="logLevel">The log level to verify.</param>
    /// <param name="logMessage">The log message to verify.</param>
    /// <param name="times">The number of times the log invocation should have occurred.</param>
    /// <returns>The mocked logger.</returns>
    public static Mock<TLogger> VerifyLog<TLogger>(
        this Mock<TLogger> logger,
        LogLevel logLevel,
        string logMessage,
        Times? times = null)
        where TLogger : class, ILogger
    {
        times ??= Times.AtLeastOnce();

        logger.Verify(
            l => l.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => CompareMessage(v.ToString(), logMessage)),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times.Value,
            $"No invocation matched for {nameof(logMessage)}: '{logMessage}'.");

        return logger;
    }

    /// <summary>
    /// Verifies that a log invocation with the specified log level, message, exception type, and exception message occurred on the mocked logger.
    /// </summary>
    /// <typeparam name="TLogger">The type of the logger.</typeparam>
    /// <param name="logger">The mocked logger.</param>
    /// <param name="logLevel">The log level to verify.</param>
    /// <param name="logMessage">The log message to verify.</param>
    /// <param name="exceptionType">The type of the exception to verify.</param>
    /// <param name="exceptionMessage">The exception message to verify.</param>
    /// <param name="times">The number of times the log invocation should have occurred.</param>
    /// <returns>The mocked logger.</returns>
    public static Mock<TLogger> VerifyLog<TLogger>(
        this Mock<TLogger> logger,
        LogLevel logLevel,
        string logMessage,
        Type exceptionType,
        string exceptionMessage,
        Times? times = null)
        where TLogger : class, ILogger
    {
        times ??= Times.AtLeastOnce();

        logger.Verify(
            l => l.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => CompareMessage(v.ToString(), logMessage)),
                It.Is<Exception?>((e) => VerifyException(e, exceptionType, exceptionMessage)),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times.Value,
            $"No invocation matched for {nameof(logMessage)}: '{logMessage}', {nameof(exceptionType)}: '{exceptionType.Name}' and {nameof(exceptionMessage)}: '{exceptionMessage}'.");

        return logger;
    }

    /// <summary>
    /// Verifies that a log invocation with the specified log level, message, and exception occurred on the mocked logger.
    /// </summary>
    /// <typeparam name="TLogger">The type of the logger.</typeparam>
    /// <param name="logger">The mocked logger.</param>
    /// <param name="logLevel">The log level to verify.</param>
    /// <param name="logMessage">The log message to verify.</param>
    /// <param name="exception">The exception to verify.</param>
    /// <param name="times">The number of times the log invocation should have occurred.</param>
    /// <returns>The mocked logger.</returns>
    public static Mock<TLogger> VerifyLog<TLogger>(
        this Mock<TLogger> logger,
        LogLevel logLevel,
        string logMessage,
        Exception exception,
        Times? times = null)
        where TLogger : class, ILogger
    {
        times ??= Times.AtLeastOnce();

        logger.Verify(
            l => l.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => CompareMessage(v.ToString(), logMessage)),
                It.Is<Exception?>((e) => e == exception),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times.Value,
            $"No invocation matched for {nameof(logMessage)}: '{logMessage}' and {nameof(exception)}: '{exception.GetType().Name}'.");

        return logger;
    }

    /// <summary>
    /// Verifies that the exception matches the expected type and message.
    /// </summary>
    /// <param name="exception">The exception to verify.</param>
    /// <param name="expectedExceptionType">The expected exception type.</param>
    /// <param name="expectedExceptionMessage">The expected exception message.</param>
    /// <returns><c>true</c> if the exception matches; otherwise, <c>false</c>.</returns>
    private static bool VerifyException(Exception? exception, Type expectedExceptionType, string expectedExceptionMessage)
    {
        if (exception is null)
        {
            return false;
        }

        if (exception.GetType() != expectedExceptionType)
        {
            return false;
        }

        return CompareMessage(exception.Message, expectedExceptionMessage);
    }

    /// <summary>
    /// Compares a message string to an expected message, supporting wildcards.
    /// </summary>
    /// <param name="message">The actual message string.</param>
    /// <param name="expectedMessage">The expected message string, which may contain wildcards.</param>
    /// <returns><c>true</c> if the messages match; otherwise, <c>false</c>.</returns>
    private static bool CompareMessage(string? message, string expectedMessage)
    {
        if (message is null)
        {
            return false;
        }

        if (expectedMessage.Length == 0)
        {
            return message.Length == 0;
        }

        if (expectedMessage[0] == '*' && expectedMessage[^1] == '*')
        {
            return message.Contains(expectedMessage[1..^1], StringComparison.Ordinal);
        }

        if (expectedMessage[0] == '*')
        {
            return message.EndsWith(expectedMessage[1..], StringComparison.Ordinal);
        }

        if (expectedMessage[^1] == '*')
        {
            return message.StartsWith(expectedMessage[..^1], StringComparison.Ordinal);
        }

        return message.Equals(expectedMessage, StringComparison.Ordinal);
    }
}
