using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

/// <summary>
///  Optional base class for components that represent a layout and resolves a ViewModel of type <typeparam name="TViewModel"></typeparam>/>.
/// </summary>
/// <typeparam name="TViewModel">The ViewModel.</typeparam>
/// <remarks>
/// Use the <see cref="T:Blazing.Mvvm.Components.MvvmLayoutComponentBase" /> class as a base layout to author layout components that control
/// the lifetime of a ViewModel. This is useful when using a transient or scoped service that
/// requires disposal. Using <see cref="T:Blazing.Mvvm.Components.MvvmLayoutComponentBase" />
/// as a base class ensures that the ViewModel is disposed with the component.
/// </remarks>
public abstract class MvvmLayoutComponentBase<TViewModel>
    : LayoutComponentBase, IAsyncDisposable, IDisposable, IView<TViewModel>
    where TViewModel : IViewModelBase
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

    #region Dispose

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
        if (_disposed)
            return;

        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_disposed)
            return;

        TViewModel? viewModel = ViewModel;

        if (viewModel is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();

        if (viewModel is IDisposable disposable)
            disposable.Dispose();

        Dispose();
    }

    #endregion
}