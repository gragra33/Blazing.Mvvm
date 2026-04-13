using HybridSample.MAUI.ViewModels;
using Microsoft.Maui.Controls;

namespace HybridSample.MAUI;

/// <summary>
/// The main page of the MAUI application.
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}