using Avalonia;
using Avalonia.Controls;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace HybridSample.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        IServiceProvider? services = (Application.Current as App)?.Services;
        RootComponentsCollection rootComponents = new() { new("#app", typeof(HybridApp), null) };

        Resources.Add("services", services);
        Resources.Add("rootComponents", rootComponents);

        InitializeComponent();
    }
}
