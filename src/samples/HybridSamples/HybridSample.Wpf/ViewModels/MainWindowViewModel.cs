using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HybridSample.Core.ViewModels;
using HybridSample.Wpf.States;

namespace HybridSample.Wpf.ViewModels;

/// <summary>
/// ViewModel for the main window, providing navigation logic and actions.
/// </summary>
internal class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    public MainWindowViewModel()
        => NavigateToCommand = new RelayCommand<string>(arg => NavigationActions[arg!].Action.Invoke());

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
        => AppState.Navigation.NavigateTo(url);

    /// <summary>
    /// Navigates to the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to.</typeparam>
    private static void NavigateTo<TViewModel>() where TViewModel : IViewModelBase
        => AppState.Navigation.NavigateTo<TViewModel>();
}
