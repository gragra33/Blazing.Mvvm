using HybridSample.MAUI.ViewModels;

namespace HybridSample.MAUI;

/// <summary>
/// The main MAUI application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="mainPageViewModel">The view model for the main page, injected via DI.</param>
    public App(MainPageViewModel mainPageViewModel)
    {
        InitializeComponent();
        
        // Wrap MainPage in NavigationPage to enable push/pop navigation
        MainPage = new NavigationPage(new MainPage(mainPageViewModel));
    }

    /// <summary>
    /// Override CreateWindow to use the modern MAUI window creation approach.
    /// </summary>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        
        // Optional: Set window properties here if needed
        // window.Title = "Blazing MVVM - MAUI Hybrid Sample";
        
        return window;
    }
}