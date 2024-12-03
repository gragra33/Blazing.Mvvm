using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

// By default in HybridMaui hosting model, ViewModels are
// registered with Lifetime = ServiceLifetime.Scoped
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class MainLayoutViewModel : ViewModelBase, IDisposable
{
    private readonly NavigationManager _navigationManager;

    [ObservableProperty]
    private int _counter;

    public MainLayoutViewModel(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    public void Dispose()
        => _navigationManager.LocationChanged -= OnLocationChanged;

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => Counter++;
}
