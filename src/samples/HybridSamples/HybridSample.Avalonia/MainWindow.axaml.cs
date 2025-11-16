using Avalonia;
using Avalonia.Controls;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using System;

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

        Resources.Add("services", services);

        InitializeComponent();

        // Configure root components after the BlazorWebView is created
        var blazorWebView = this.Find<Baksteen.Avalonia.Blazor.BlazorWebView>("BlazorWebView");
        if (blazorWebView != null)
        {
            blazorWebView.RootComponents.Add(new RootComponent("#app", typeof(HybridApp), null));
        }
    }
}
