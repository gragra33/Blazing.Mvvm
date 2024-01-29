using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

public abstract class MvvmComponentBase<TViewModel> : ComponentBase, IAsyncDisposable, IView<TViewModel> where TViewModel : IViewModelBase
{
    private bool _disposed;

    [Inject]
    protected TViewModel? ViewModel { get; set; }

    protected override void OnInitialized()
    {
        // Cause changes to the ViewModel to make Blazor re-render
        ViewModel!.PropertyChanged += OnPropertyChanged;
        base.OnInitialized();
    }

    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);

    protected override Task OnInitializedAsync()
        => ViewModel!.OnInitializedAsync();

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            ViewModel!.PropertyChanged -= OnPropertyChanged;

#if DEBUG
        Console.WriteLine($"..Disposing: {GetType().FullName}");
#endif
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        TViewModel? viewModel = ViewModel;

        if(viewModel is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();

        if(viewModel is IDisposable disposable)
            disposable.Dispose();
        
        Dispose();
    }
}