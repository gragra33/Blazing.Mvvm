using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

/// <summary>
///  A base class for components that represent a layout and resolves a ViewModel of type <typeparamref name="TViewModel"/>.
/// </summary>
/// <typeparam name="TViewModel">The ViewModel.</typeparam>
public abstract class MvvmLayoutComponentBase<TViewModel> : LayoutComponentBase, IView<TViewModel>
    where TViewModel : IViewModelBase
{
    protected bool IsDisposed { get; private set; }

    [Inject]
    protected virtual TViewModel ViewModel { get; set; } = default!;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        ViewModel.OnAfterRender(firstRender);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await ViewModel.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ViewModel.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ViewModel.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await ViewModel.OnParametersSetAsync();
    }

    protected override bool ShouldRender()
        => ViewModel.ShouldRender();

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        IsDisposed = true;
    }

    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);
}
