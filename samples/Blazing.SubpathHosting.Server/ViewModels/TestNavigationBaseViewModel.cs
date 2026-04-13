using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.SubpathHosting.Server.ViewModels;

public abstract partial class TestNavigationBaseViewModel : ViewModelBase, ITestNavigationViewModel
{
    internal readonly IMvvmNavigationManager MvvmNavigationManager;
    internal readonly NavigationManager NavigationManager;

    private RelayCommand? _hexTranslateNavigateCommand;
    private bool isDisposed;

    internal RelayCommand<string>? TestNavigateCommandImpl;

    [ObservableProperty]
    private string? _queryString;

    [ObservableProperty]
    private string? _test;

    protected TestNavigationBaseViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    {
        MvvmNavigationManager = mvvmNavigationManager;
        NavigationManager = navigationManager;
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    [ViewParameter] // populated by MvvmComponentBase
    public string? Echo { get; set; } = string.Empty;

    public RelayCommand HexTranslateNavigateCommand
        => _hexTranslateNavigateCommand ??= new RelayCommand(() => Navigate<HexTranslateViewModel>());

    public virtual RelayCommand<string> TestNavigateCommand
        => TestNavigateCommandImpl ??= new RelayCommand<string>(Navigate<ITestNavigationViewModel>);

    public override void OnInitialized()
        => ProcessQueryString();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        isDisposed = true;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => ProcessQueryString();

    private void Navigate<T>(string? @params = null)
        where T : IViewModelBase
    {
        if (string.IsNullOrWhiteSpace(@params))
        {
            MvvmNavigationManager.NavigateTo<T>();
            return;
        }

        MvvmNavigationManager.NavigateTo<T>(@params);
    }

    private void ProcessQueryString()
    {
        QueryString = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;
        NavigationManager.TryGetQueryString("test", out string temp);
        Test = temp;
    }
}
