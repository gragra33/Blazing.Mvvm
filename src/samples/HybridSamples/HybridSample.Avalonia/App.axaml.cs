using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HybridSample.Avalonia.ViewModels;

namespace HybridSample.Avalonia;

/// <summary>
/// The main application class for the Avalonia app.
/// </summary>
public class App : Application
{
    /// <summary>
    /// Gets the application's service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="services">The service provider for dependency injection.</param>
    public App(IServiceProvider services)
        => Services = services;

    /// <summary>
    /// Loads Avalonia XAML resources for the application.
    /// </summary>
    public override void Initialize()
        => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Called when Avalonia framework initialization is completed.
    /// Sets up the main window and its data context.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
        
        base.OnFrameworkInitializationCompleted();
    }
}
