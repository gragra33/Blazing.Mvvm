using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HybridSample.Core.ViewModels;
using HybridSample.MAUI.States;
using System.Diagnostics;

namespace HybridSample.MAUI.ViewModels;

/// <summary>
/// ViewModel for the main page of the MAUI application.
/// </summary>
[ViewModelDefinition]
public partial class MainPageViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the title of the main page.
    /// </summary>
    [ObservableProperty]
    private string _title = "Blazing MVVM - MAUI Hybrid Sample";

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
    /// </summary>
    public MainPageViewModel()
    {
        Debug.WriteLine("=== MainPageViewModel Constructor ===");

        NavigateToCommand = new RelayCommand<string>(arg =>
        {
            Debug.WriteLine($"=== NavigateToCommand Executed with arg: '{arg}' ===");

            if (string.IsNullOrEmpty(arg))
            {
                Debug.WriteLine("ERROR: arg is null or empty");
                return;
            }

            if (!NavigationActions.ContainsKey(arg))
            {
                Debug.WriteLine($"ERROR: NavigationActions does not contain key '{arg}'");
                return;
            }

            Debug.WriteLine($"Invoking action for key '{arg}' with Title: '{NavigationActions[arg].Title}'");
            NavigationActions[arg].Action.Invoke();
        });

        // Log all navigation actions
        Debug.WriteLine($"=== NavigationActions Count: {NavigationActions.Count} ===");
        foreach (var kvp in NavigationActions)
        {
            Debug.WriteLine($"  Key: '{kvp.Key}', Title: '{kvp.Value.Title}'");
        }
    }

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
        Debug.WriteLine($"=== NavigateTo(string) called with url: '{url}' ===");
        Debug.WriteLine($"AppState.Navigation is null: {AppState.Navigation == null}");

        if (AppState.Navigation == null)
        {
            Debug.WriteLine("ERROR: AppState.Navigation is null! Blazor WebView may not be initialized yet.");
            // Optionally show a message to the user
            return;
        }

        try
        {
            AppState.Navigation.NavigateTo(url);
            Debug.WriteLine($"Navigation completed to: '{url}'");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to.</typeparam>
    private static void NavigateTo<TViewModel>() where TViewModel : IViewModelBase
    {
        Debug.WriteLine($"=== NavigateTo<TViewModel> called with type: {typeof(TViewModel).Name} ===");
        Debug.WriteLine($"AppState.Navigation is null: {AppState.Navigation == null}");

        if (AppState.Navigation == null)
        {
            Debug.WriteLine("ERROR: AppState.Navigation is null! Blazor WebView may not be initialized yet.");
            return;
        }

        try
        {
            AppState.Navigation.NavigateTo<TViewModel>();
            Debug.WriteLine($"Navigation completed to: {typeof(TViewModel).Name}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }
}