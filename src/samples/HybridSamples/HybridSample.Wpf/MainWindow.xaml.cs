using HybridSample.Wpf.ViewModels;

namespace HybridSample.Wpf;

/// <summary>
/// Interaction logic for the main window of the WPF application.
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }
}
