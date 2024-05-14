using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Components;

/// <summary>
/// A base class that creates a service provider scope, and resolves a ViewModel of type <typeparamref name="TViewModel"/>.
/// </summary>
/// <typeparam name="TViewModel">The ViewModel.</typeparam>
/// <remarks>
/// Use the <see cref="MvvmComponentBase{TViewModel}" /> class as a base class to author components that control
/// the lifetime of a ViewModel and of a service provider scope. This is useful when using a transient or scoped service that
/// requires disposal such as a repository or database abstraction. Using <see cref="MvvmComponentBase{TViewModel}" />
/// as a base class ensures that the ViewModel and service and relates services that share its scope are disposed with the component.
/// </remarks>
public abstract class MvvmOwningComponentBase<TViewModel> : OwningComponentBase, IView<TViewModel>, IAsyncDisposable
    where TViewModel : IViewModelBase
{
    private TViewModel _viewModel = default!;

    protected virtual TViewModel ViewModel
    {
        get
        {
            _viewModel ??= ScopedServices.GetRequiredService<TViewModel>();
            return _viewModel;
        }

        set => _viewModel = value;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        // The is to call the base class implementation as it won't be called when this is being disposed
        // https://github.com/dotnet/aspnetcore/issues/25873
        // https://github.com/dotnet/aspnetcore/discussions/25817
        ((IDisposable)this).Dispose();

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

    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        base.Dispose(disposing);
    }

    protected async virtual ValueTask DisposeAsyncCore()
    {
        if (ScopedServices is not IAsyncDisposable asyncDisposable)
        {
            return;
        }

        await asyncDisposable.DisposeAsync();
    }

    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);
}
