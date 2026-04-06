using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

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
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        QueryString = uri.Query;
        Test = GetQueryParameterValue(uri.Query, "test");
    }

    private static string? GetQueryParameterValue(string? query, string key)
    {
        if (string.IsNullOrEmpty(query))
        {
            return null;
        }

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var segments = pair.Split('=', 2);
            var parameterName = Uri.UnescapeDataString(segments[0]);

            if (!string.Equals(parameterName, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return segments.Length > 1
                ? Uri.UnescapeDataString(segments[1].Replace("+", " "))
                : string.Empty;
        }

        return null;
    }
}
