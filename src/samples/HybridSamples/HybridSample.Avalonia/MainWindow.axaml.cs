using Avalonia;
using Avalonia.Controls;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace HybridSample.Avalonia;

/// <summary>
/// Interaction logic for the main window of the Avalonia application.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        IServiceProvider? services = (Application.Current as App)?.Services;
        RootComponentsCollection rootComponents = new() { new("#app", typeof(HybridApp), null) };

        Resources.Add("services", services);
        Resources.Add("rootComponents", rootComponents);

        InitializeComponent();
    }
}
