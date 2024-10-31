using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Tests.Infrastructure.Extensions;

public static class LoggingExtensions
{
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
