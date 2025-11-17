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
    /// <param name="appShell">The app shell, injected via DI.</param>
    public App(AppShell appShell)
    {
        InitializeComponent();
#pragma warning disable CS0618 // Type or member is obsolete
        MainPage = appShell;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Override CreateWindow to use the modern MAUI window creation approach.
    /// </summary>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return base.CreateWindow(activationState);
    }
}