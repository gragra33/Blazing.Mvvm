using HybridSample.MAUI.ViewModels;
using Microsoft.Maui.Controls;

namespace HybridSample.MAUI;

/// <summary>
/// The Shell-based navigation for the MAUI application.
/// </summary>
public partial class AppShell : Shell
{
    private readonly MainPageViewModel _viewModel;
    private readonly Dictionary<string, string> _menuTitles = new()
    {
        ["home"] = "Introduction",
        ["observeObj"] = "ObservableObject",
        ["relayCommand"] = "Relay Commands",
        ["asyncCommand"] = "Async Commands",
        ["msg"] = "Messenger",
        ["sendMsg"] = "Sending Messages",
        ["ReqMsg"] = "Request Messages",
        ["ioc"] = "Inversion of Control",
        ["vmSetup"] = "ViewModel Setup",
        ["SettingsSvc"] = "Settings Service",
        ["redditSvc"] = "Reddit Service",
        ["buildUI"] = "Building the UI",
        ["reddit"] = "The Final Result"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    /// <param name="viewModel">The main page view model.</param>
    /// <param name="mainPage">The main page instance (singleton).</param>
    public AppShell(MainPageViewModel viewModel, MainPage mainPage)
    {
        _viewModel = viewModel;
        InitializeComponent();
        
        // Set the MainPage as the content programmatically to maintain singleton instance
        // Reference the ShellContent directly using x:Name from XAML
        MainShellContent.Content = mainPage;
    }

    private void OnMenuItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not string selectedKey)
            return;

        // Execute the navigation command in the ViewModel
        if (_viewModel.NavigateToCommand.CanExecute(selectedKey))
        {
            _viewModel.NavigateToCommand.Execute(selectedKey);
        }

        // Close the flyout on phones
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
        {
            FlyoutIsPresented = false;
        }

        // Clear selection to allow re-selection
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }
}
