using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.TwoWayBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Provides a base Blazor component that resolves and manages a ViewModel of type <typeparamref name="TViewModel"/>.
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel associated with this component.</typeparam>
public abstract class MvvmComponentBase<TViewModel> : ComponentBase, IView<TViewModel>
    where TViewModel : IViewModelBase
{
    /// <summary>
    /// Backing field for the <see cref="ViewModel"/> property.
    /// </summary>
    private TViewModel? _viewModel;

    /// <summary>
    /// Helper for automatic two-way binding between View parameters and ViewModel properties.
    /// </summary>
    private TwoWayBindingHelper<TViewModel>? _bindingHelper;

    /// <summary>
    /// Gets a value indicating whether the component has been disposed.
    /// </summary>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets or sets the parameter resolver used to set parameters on the View and ViewModel.
    /// </summary>
    [Inject]
    protected IParameterResolver ParameterResolver { get; set; } = null!;

    /// <summary>
    /// Gets or sets the service provider used for dependency resolution.
    /// </summary>
    [Inject]
    private IServiceProvider Services { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <c>ViewModel</c> associated with this component, resolved from the dependency injection container.
    /// </summary>
    protected virtual TViewModel ViewModel
    {
        get => _viewModel ??= ViewModelResolver.Resolve(this, Services);
        set => _viewModel = value;
    }

    /// <summary>
    /// Disposes the component and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
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

        // Automatically initialize two-way binding helper
        // It will detect and wire up EventCallback<T> parameters that follow the {PropertyName}Changed convention
        _bindingHelper = new TwoWayBindingHelper<TViewModel>(this, ViewModel);
        _bindingHelper.Initialize();
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
    /// Sets parameters from the given <see cref="ParameterView"/> on both the View and its ViewModel using the <see cref="ParameterResolver"/>
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
    /// <param name="disposing">True if called from <see cref="Dispose()"/>; false if called from a finalizer.</param>
    protected virtual void Dispose(bool disposing)
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

            // Dispose two-way binding helper if it was initialized
            _bindingHelper?.Dispose();
            _bindingHelper = null;
        }

        IsDisposed = true;
    }

    /// <summary>
    /// Handles property change notifications from the ViewModel and triggers a UI update.
    /// </summary>
    /// <param name="o">The sender object.</param>
    /// <param name="propertyChangedEventArgs">The event data.</param>
    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);
}
