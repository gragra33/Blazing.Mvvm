using Microsoft.Extensions.Logging;

namespace HybridSample.WinForms.Logging;

/// <summary>
/// Source-generated logging for Program class.
/// </summary>
public static partial class ProgramLogger
{
    [LoggerMessage(LogLevel.Information, "Starting WinForms MVVM Blazor Hybrid Sample Application")]
    public static partial void LogApplicationStarting(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Console window is now visible for debug output")]
    public static partial void LogConsoleWindowVisible(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Debug logging enabled with console output")]
    public static partial void LogDebugLoggingEnabled(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Service provider created, getting MainForm...")]
    public static partial void LogServiceProviderCreated(ILogger logger);

    [LoggerMessage(LogLevel.Information, "MainForm obtained, starting application...")]
    public static partial void LogMainFormObtained(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Click buttons in the WinForms window to see navigation debug output")]
    public static partial void LogInstructionsForUser(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Application exited. Press any key to close console...")]
    public static partial void LogApplicationExited(ILogger logger);
}