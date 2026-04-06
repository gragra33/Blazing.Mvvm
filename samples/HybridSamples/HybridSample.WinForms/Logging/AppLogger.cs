using Microsoft.Extensions.Logging;

namespace HybridSample.WinForms.Logging;

/// <summary>
/// Source-generated logging for App Razor component.
/// </summary>
public static partial class AppLogger
{
    [LoggerMessage(LogLevel.Information, "App.razor OnInitialized() called - setting AppState.Navigation")]
    public static partial void LogAppOnInitialized(ILogger logger);

    [LoggerMessage(LogLevel.Information, "AppState.Navigation set to: {AppState}")]
    public static partial void LogAppStateNavigationSet(ILogger logger, object appState);

    [LoggerMessage(LogLevel.Debug, "NavigationManager BaseUri: {BaseUri}")]
    public static partial void LogNavigationManagerBaseUri(ILogger logger, string baseUri);

    [LoggerMessage(LogLevel.Debug, "NavigationManager Uri: {Uri}")]
    public static partial void LogNavigationManagerUri(ILogger logger, string uri);

    [LoggerMessage(LogLevel.Information, "App.razor OnAfterRender (first render) - navigation should now be fully initialized")]
    public static partial void LogAppAfterFirstRender(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "App.razor NavigateTo(string) called with: {Page}")]
    public static partial void LogAppNavigateToString(ILogger logger, string page);

    [LoggerMessage(LogLevel.Debug, "Current NavManager.Uri: {Uri}")]
    public static partial void LogCurrentNavManagerUri(ILogger logger, string uri);

    [LoggerMessage(LogLevel.Debug, "Navigating to: {Page}")]
    public static partial void LogNavigatingTo(ILogger logger, string page);

    [LoggerMessage(LogLevel.Debug, "NavigationManager.NavigateTo({Page}) completed")]
    public static partial void LogNavigationManagerCompleted(ILogger logger, string page);

    [LoggerMessage(LogLevel.Debug, "New NavManager.Uri: {Uri}")]
    public static partial void LogNewNavManagerUri(ILogger logger, string uri);

    [LoggerMessage(LogLevel.Error, "Error in NavigationManager.NavigateTo: {ErrorMessage}")]
    public static partial void LogNavigationManagerError(ILogger logger, string errorMessage);

    [LoggerMessage(LogLevel.Debug, "Error stack trace: {StackTrace}")]
    public static partial void LogNavigationManagerErrorStackTrace(ILogger logger, string? stackTrace);

    [LoggerMessage(LogLevel.Debug, "App.razor NavigateTo<TViewModel>() called with: {ViewModelName}")]
    public static partial void LogAppNavigateToViewModel(ILogger logger, string viewModelName);

    [LoggerMessage(LogLevel.Debug, "Current NavManager.Uri before MVVM navigation: {Uri}")]
    public static partial void LogCurrentUriBeforeMvvmNavigation(ILogger logger, string uri);

    [LoggerMessage(LogLevel.Debug, "MvvmNavigationManager.NavigateTo<{ViewModelName}>() completed successfully")]
    public static partial void LogMvvmNavigationCompleted(ILogger logger, string viewModelName);

    [LoggerMessage(LogLevel.Debug, "New NavManager.Uri after MVVM navigation: {Uri}")]
    public static partial void LogNewUriAfterMvvmNavigation(ILogger logger, string uri);

    [LoggerMessage(LogLevel.Warning, "MVVM Navigation failed: {ErrorMessage}")]
    public static partial void LogMvvmNavigationFailed(ILogger logger, string errorMessage);

    [LoggerMessage(LogLevel.Information, "Trying fallback navigation to: {FallbackRoute}")]
    public static partial void LogTryingFallbackNavigation(ILogger logger, string fallbackRoute);

    [LoggerMessage(LogLevel.Information, "Fallback navigation to {FallbackRoute} completed")]
    public static partial void LogFallbackNavigationCompleted(ILogger logger, string fallbackRoute);

    [LoggerMessage(LogLevel.Error, "Error in NavigateTo<TViewModel>: {ErrorMessage}")]
    public static partial void LogNavigateToViewModelError(ILogger logger, string errorMessage);
}