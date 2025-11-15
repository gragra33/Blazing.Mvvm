using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

/// <summary>
/// A base class for Blazor components that create a service provider scope and resolve a ViewModel of type <typeparamref name="TViewModel"/>.
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel associated with this component. Must implement <see cref="IViewModelBase"/>.</typeparam>
/// <remarks>
/// Use <see cref="MvvmOwningComponentBase{TViewModel}" /> as a base class to author components that control
/// the lifetime of a ViewModel and a service provider scope. This is useful when using a transient or scoped service that
/// requires disposal, such as a repository or database abstraction. Using <see cref="MvvmOwningComponentBase{TViewModel}" />
/// ensures that the ViewModel and related services sharing its scope are disposed with the component.
/// </remarks>
public abstract class MvvmOwningComponentBase<TViewModel> : OwningComponentBase, IView<TViewModel>, IAsyncDisposable
    where TViewModel : IViewModelBase
{
    private TViewModel? _viewModel;

    /// <summary>
    /// Gets or sets the parameter resolver used to set parameters on the View and ViewModel.
    /// </summary>
    [Inject]
    protected IParameterResolver ParameterResolver { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <c>ViewModel</c> associated with this component, resolved from the <see cref="OwningComponentBase.ScopedServices"/>.
    /// </summary>
    protected virtual TViewModel ViewModel
    {
        get => _viewModel ??= ViewModelResolver.Resolve(this, ScopedServices);
        set => _viewModel = value;
    }

    /// <summary>
    /// Disposes the component asynchronously and releases unmanaged resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        // The is to call the base class implementation as it won't be called when this is being disposed
        // https://github.com/dotnet/aspnetcore/issues/25873
        // https://github.com/dotnet/aspnetcore/discussions/25817
        ((IDisposable)this).Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Invoked after the component has rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first time <see cref="OnAfterRender(bool)"/> is called; otherwise, false.</param>
    protected override void OnAfterRender(bool firstRender)
        => ViewModel.OnAfterRender(firstRender);

    /// <summary>
    /// Asynchronously invoked after the component has rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first time <see cref="OnAfterRenderAsync(bool)"/> is called; otherwise, false.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override Task OnAfterRenderAsync(bool firstRender)
        => ViewModel.OnAfterRenderAsync(firstRender);

    /// <summary>
    /// Initializes the component and subscribes to ViewModel property changes.
    /// </summary>
    protected override void OnInitialized()
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.OnInitialized();
    }

    /// <summary>
    /// Asynchronously initializes the component.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override Task OnInitializedAsync()
        => ViewModel.OnInitializedAsync();

    /// <summary>
    /// Sets parameters on the component and ViewModel.
    /// </summary>
    protected override void OnParametersSet()
        => ViewModel.OnParametersSet();

    /// <summary>
    /// Asynchronously sets parameters on the component and ViewModel.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override Task OnParametersSetAsync()
        => ViewModel.OnParametersSetAsync();

    /// <summary>
    /// Sets parameters from the given <see cref="ParameterView"/> on both the View and its ViewModel using the <see cref="ParameterResolver"/>.
    /// </summary>
    /// <param name="parameters">The parameters to set.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        return ParameterResolver.SetParameters(this, ViewModel, parameters)
            ? base.SetParametersAsync(ParameterView.Empty)
            : base.SetParametersAsync(parameters);
    }

    /// <summary>
    /// Determines whether the component should render.
    /// </summary>
    /// <returns><c>true</c> if the component should render; otherwise, <c>false</c>.</returns>
    protected override bool ShouldRender()
        => ViewModel.ShouldRender();

    /// <summary>
    /// Disposes the component and releases unmanaged resources.
    /// </summary>
    /// <param name="disposing">True if called from the public Dispose method; false if called from a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
            if (ViewModel is ObservableRecipient observableRecipient)
            {
                observableRecipient.IsActive = false;
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Disposes the component asynchronously and releases unmanaged resources for the scoped services.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (ScopedServices is not IAsyncDisposable asyncDisposable)
        {
            return;
        }

        await asyncDisposable.DisposeAsync();
    }

    /// <summary>
    /// Handles property change notifications from the ViewModel and triggers a UI update.
    /// </summary>
    /// <param name="o">The sender object.</param>
    /// <param name="propertyChangedEventArgs">The event data.</param>
    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);
}
