using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
public sealed partial class MainLayoutViewModel : ViewModelBase, IDisposable
{
    public MainLayoutViewModel(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        Counter++;
    }

    private readonly NavigationManager _navigationManager;

    [ObservableProperty]
    private int _counter;

    public void Dispose()
        => _navigationManager.LocationChanged -= OnLocationChanged;
}