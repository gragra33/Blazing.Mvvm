using HybridSample.Wpf.ViewModels;

namespace HybridSample.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }
}
