using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HybridSample.Core.ViewModels;
using HybridSample.Avalonia.States;

namespace HybridSample.Avalonia.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
        => NavigateToCommand = new RelayCommand<string>(arg => NavigationActions[arg!].Action.Invoke());

    public IRelayCommand<string> NavigateToCommand { get; set; }

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

    private static void NavigateTo(string url)
        => AppState.Navigation.NavigateTo(url);

    private static void NavigateTo<TViewModel>() where TViewModel : IViewModelBase
        => AppState.Navigation.NavigateTo<TViewModel>();
}
