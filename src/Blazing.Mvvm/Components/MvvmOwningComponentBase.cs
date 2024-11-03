using System.ComponentModel;
using System.Reflection;
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
    private TViewModel? _viewModel;

    /// <summary>
    /// The <c>ViewModel</c> associated with this component, resolved from the <see cref="OwningComponentBase.ScopedServices"/>.
    /// </summary>
    protected virtual TViewModel ViewModel
    {
        get => SetViewModel();
        set => _viewModel = value;
    }

    /// <summary>
    /// Resolves parameters in the <c>View</c> and <c>ViewModel</c>.
    /// </summary>
    [Inject]
    protected IParameterResolver ParameterResolver { get; set; } = default!;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        // The is to call the base class implementation as it won't be called when this is being disposed
        // https://github.com/dotnet/aspnetcore/issues/25873
        // https://github.com/dotnet/aspnetcore/discussions/25817
        ((IDisposable)this).Dispose();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
        => ViewModel.OnAfterRender(firstRender);

    /// <inheritdoc/>
    protected override Task OnAfterRenderAsync(bool firstRender)
        => ViewModel.OnAfterRenderAsync(firstRender);

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.OnInitialized();
    }

    /// <inheritdoc/>
    protected override Task OnInitializedAsync()
        => ViewModel.OnInitializedAsync();

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => ViewModel.OnParametersSet();

    /// <inheritdoc/>
    protected override Task OnParametersSetAsync()
        => ViewModel.OnParametersSetAsync();

    /// <inheritdoc/>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        return ParameterResolver.SetParameters(this, ViewModel, parameters)
            ? base.SetParametersAsync(ParameterView.Empty)
            : base.SetParametersAsync(parameters);
    }

    /// <inheritdoc/>
    protected override bool ShouldRender()
        => ViewModel.ShouldRender();

    /// <summary>
    /// Disposes the component and releases unmanaged resources.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method was called from the dispose method or from a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Disposes the component asynchronously and releases unmanaged resources.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (ScopedServices is not IAsyncDisposable asyncDisposable)
        {
            return;
        }

        await asyncDisposable.DisposeAsync();
    }

    private TViewModel SetViewModel()
    {
        if (_viewModel != null) return
            _viewModel;

        ViewModelDefinitionAttribute? viewModelDefinition = GetType().GetCustomAttribute<ViewModelDefinitionAttribute>();

        _viewModel = viewModelDefinition != null
            ? ScopedServices.GetRequiredKeyedService<TViewModel>(viewModelDefinition.Key)
            : ScopedServices.GetRequiredService<TViewModel>();

        return _viewModel;
    }

    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);
}
