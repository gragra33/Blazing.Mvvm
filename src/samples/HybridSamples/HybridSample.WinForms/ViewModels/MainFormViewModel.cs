using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HybridSample.Core.ViewModels;
using HybridSample.WinForms.States;
using Microsoft.Extensions.Logging;

namespace HybridSample.WinForms.ViewModels;

/// <summary>
/// ViewModel for the main form of the WinForms application.
/// </summary>
[ViewModelDefinition]
public partial class MainFormViewModel : ViewModelBase
{
    private readonly ILogger<MainFormViewModel> _logger;

    /// <summary>
    /// Source-generated logging methods for the MainFormViewModel class.
    /// </summary>
    [LoggerMessage(LogLevel.Debug, "NavigateToCommand executed with key: {NavigationKey}")]
    private partial void LogNavigateToCommandExecuted(string? navigationKey);

    [LoggerMessage(LogLevel.Debug, "Executing navigation action for key: {NavigationKey}")]
    private partial void LogExecutingNavigationAction(string navigationKey);

    [LoggerMessage(LogLevel.Error, "Error in navigation action: {ErrorMessage}")]
    private partial void LogNavigationActionError(string errorMessage);

    [LoggerMessage(LogLevel.Warning, "Navigation key '{NavigationKey}' not found in NavigationActions")]
    private partial void LogNavigationKeyNotFound(string? navigationKey);

    [LoggerMessage(LogLevel.Debug, "NavigateTo URL called: {Url}")]
    private static partial void LogNavigateToUrl(ILogger logger, string url);

    [LoggerMessage(LogLevel.Error, "AppState.Navigation is null!")]
    private static partial void LogAppStateNavigationNull(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Calling AppState.Navigation.NavigateTo...")]
    private static partial void LogCallingAppStateNavigation(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Navigation to {Url} completed successfully")]
    private static partial void LogNavigationToUrlCompleted(ILogger logger, string url);

    [LoggerMessage(LogLevel.Error, "Error navigating to {Url}: {ErrorMessage}")]
    private static partial void LogNavigationToUrlError(ILogger logger, string url, string errorMessage);

    [LoggerMessage(LogLevel.Debug, "NavigateTo ViewModel called: {ViewModelName}")]
    private static partial void LogNavigateToViewModel(ILogger logger, string viewModelName);

    [LoggerMessage(LogLevel.Debug, "Calling AppState.Navigation.NavigateTo<TViewModel>...")]
    private static partial void LogCallingAppStateViewModelNavigation(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Navigation to {ViewModelName} completed successfully")]
    private static partial void LogNavigationToViewModelCompleted(ILogger logger, string viewModelName);

    [LoggerMessage(LogLevel.Error, "Error navigating to {ViewModelName}: {ErrorMessage}")]
    private static partial void LogNavigationToViewModelError(ILogger logger, string viewModelName, string errorMessage);

    /// <summary>
    /// Initializes a new instance of the <see cref="MainFormViewModel"/> class.
    /// </summary>
    public MainFormViewModel(ILogger<MainFormViewModel> logger)
    {
        _logger = logger;
        Title = "Blazing MVVM - WinForms Hybrid Sample";
        NavigateToCommand = new RelayCommand<string>(arg => 
        {
            LogNavigateToCommandExecuted(arg);
            if (arg != null && NavigationActions.TryGetValue(arg, out NavigationAction? navigationAction))
            {
                try
                {
                    LogExecutingNavigationAction(arg);
                    navigationAction.Action.Invoke();
                }
                catch (Exception ex)
                {
                    LogNavigationActionError(ex.Message);
                }
            }
            else
            {
                LogNavigationKeyNotFound(arg);
            }
        });
    }

    /// <summary>
    /// Gets or sets the title of the main form.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Gets or sets the command used to navigate to a page by key.
    /// </summary>
    public IRelayCommand<string> NavigateToCommand { get; set; }

    /// <summary>
    /// Gets the dictionary of navigation actions mapped by key.
    /// </summary>
    public Dictionary<string, NavigationAction> NavigationActions { get; } = new()
    {
        ["home"] = new("Introduction", () => NavigateTo("/")),
        ["observeObj"] = new("ObservableObject", NavigateTo<ObservableObjectPageViewModel>),
        ["relayCommand"] = new("Relay Commands", NavigateTo<RelayCommandPageViewModel>),
        ["asyncCommand"] = new("Async Commands", NavigateTo<AsyncRelayCommandPageViewModel>),
        ["msg"] = new("Messenger", NavigateTo<MessengerPageViewModel>),
        ["sendMsg"] = new("Sending Messages", NavigateTo<MessengerSendPageViewModel>),
        ["ReqMsg"] = new("Request Messages", NavigateTo<MessengerRequestPageViewModel>),
        ["ioc"] = new("Inversion of Control", NavigateTo<IocPageViewModel>),
        ["vmSetup"] = new("ViewModel Setup", NavigateTo<ISettingUpTheViewModelsPageViewModel>),
        ["SettingsSvc"] = new("Settings Service", NavigateTo<ISettingsServicePageViewModel>),
        ["redditSvc"] = new("Reddit Service", NavigateTo<IRedditServicePageViewModel>),
        ["buildUI"] = new("Building the UI", NavigateTo<IBuildingTheUIPageViewModel>),
        ["reddit"] = new("The Final Result", NavigateTo<IRedditBrowserPageViewModel>),
    };

    /// <summary>
    /// Navigates to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    private static void NavigateTo(string url)
    {
        // Create a simple logger for static methods - in production, you'd use a global logger
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<MainFormViewModel>();
        
        LogNavigateToUrl(logger, url);
        try
        {
            if (AppState.Navigation == null)
            {
                LogAppStateNavigationNull(logger);
                return;
            }
            
            LogCallingAppStateNavigation(logger);
            AppState.Navigation.NavigateTo(url);
            LogNavigationToUrlCompleted(logger, url);
        }
        catch (Exception ex)
        {
            LogNavigationToUrlError(logger, url, ex.Message);
        }
    }

    /// <summary>
    /// Navigates to the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to.</typeparam>
    private static void NavigateTo<TViewModel>() where TViewModel : IViewModelBase
    {
        // Create a simple logger for static methods - in production, you'd use a global logger
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<MainFormViewModel>();
        
        LogNavigateToViewModel(logger, typeof(TViewModel).Name);
        try
        {
            if (AppState.Navigation == null)
            {
                LogAppStateNavigationNull(logger);
                return;
            }
            
            LogCallingAppStateViewModelNavigation(logger);
            AppState.Navigation.NavigateTo<TViewModel>();
            LogNavigationToViewModelCompleted(logger, typeof(TViewModel).Name);
        }
        catch (Exception ex)
        {
            LogNavigationToViewModelError(logger, typeof(TViewModel).Name, ex.Message);
        }
    }
}