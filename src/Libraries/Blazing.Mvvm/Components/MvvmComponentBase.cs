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
/// <remarks>
/// Extend cleanup by overriding <see cref="Dispose(bool)"/>, or <see cref="DisposeAsyncCore"/> when it must be asynchronous.
/// A derived component that implements <see cref="IAsyncDisposable"/> itself receives the renderer's only disposal call,
/// which bypasses this class's cleanup.
/// </remarks>
public abstract class MvvmComponentBase<TViewModel> : ComponentBase, IView<TViewModel>, IAsyncDisposable
    where TViewModel : IViewModelBase
{
    /// <summary>
    /// Backing field for the <see cref="ViewModel"/> property.
    /// </summary>
    private TViewModel? _viewModel;

    /// <summary>
    /// The ViewModel this component created and must dispose, kept apart from <see cref="_viewModel"/> so that assigning another ViewModel does not lose it.
    /// </summary>
    private IViewModelBase? _ownedViewModel;

    /// <summary>
    /// The ViewModel whose <see cref="INotifyPropertyChanged.PropertyChanged"/> event this component subscribed to.
    /// </summary>
    private IViewModelBase? _subscribedViewModel;

    /// <summary>
    /// Helper for automatic two-way binding between View parameters and ViewModel properties.
    /// </summary>
    private TwoWayBindingHelper<TViewModel>? _bindingHelper;

    /// <summary>
    /// Whether asynchronous disposal has started, so that a re-entrant or repeated call does nothing.
    /// </summary>
    private bool _isDisposingAsync;

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
    /// <remarks>
    /// When <see cref="LibraryConfiguration.DisposeTransientViewModels"/> is enabled, a ViewModel registered as transient is
    /// created for this component rather than tracked by the container, and is disposed when this component is disposed,
    /// asynchronously when it implements <see cref="IAsyncDisposable"/>. A ViewModel assigned through the setter is never
    /// disposed by this component, and assigning one does not release the component's claim on the ViewModel it created.
    /// A component owns at most one ViewModel, and none once disposal has started; any other comes from the container.
    /// </remarks>
    protected virtual TViewModel ViewModel
    {
        get
        {
            if (_viewModel is null)
            {
                var canOwnViewModel = _ownedViewModel is null && !_isDisposingAsync && !IsDisposed;
                _viewModel = ViewModelResolver.Resolve(this, Services, canOwnViewModel, out var isOwnedByView);
                if (isOwnedByView)
                {
                    _ownedViewModel = _viewModel;
                }
            }

            return _viewModel;
        }
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
    /// Disposes the component and awaits asynchronous disposal of a ViewModel this component owns.
    /// </summary>
    /// <remarks>
    /// The renderer calls only this method when a component implements both disposal interfaces. It awaits
    /// <see cref="DisposeAsyncCore"/>, then runs the synchronous disposal path through <see cref="IDisposable"/>, so a derived
    /// component that re-implements <see cref="IDisposable"/> still receives the call. The owned ViewModel is released last,
    /// even when that re-implementation skips the base cleanup or either step throws.
    /// </remarks>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (IsDisposed || _isDisposingAsync)
        {
            return;
        }

        _isDisposingAsync = true;

        // Detach an async-disposable ViewModel before any derived code runs, so it is disposed once, asynchronously
        var asyncDisposableViewModel = _ownedViewModel as IAsyncDisposable;
        if (asyncDisposableViewModel is not null)
        {
            _ownedViewModel = null;
        }

        try
        {
            try
            {
                await DisposeAsyncCore();
            }
            finally
            {
                ((IDisposable)this).Dispose();
            }
        }
        finally
        {
            ReleaseOwnedViewModel();

            if (asyncDisposableViewModel is not null)
            {
                await asyncDisposableViewModel.DisposeAsync();
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Override to add asynchronous cleanup, such as disposing an <c>IJSObjectReference</c>, and call the base implementation.
    /// </summary>
    /// <remarks>
    /// This runs while the ViewModel is still usable, before <see cref="Dispose(bool)"/>. It runs only when the renderer disposes
    /// the component, or when <see cref="IAsyncDisposable.DisposeAsync"/> is called, and not when <see cref="Dispose()"/> is called directly.
    /// </remarks>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous cleanup.</returns>
    protected virtual ValueTask DisposeAsyncCore()
        => ValueTask.CompletedTask;

    /// <summary>
    /// Disposes the ViewModel this component owns, clearing the reference first so that a re-entrant call cannot dispose it again.
    /// </summary>
    private void ReleaseOwnedViewModel()
    {
        var ownedViewModel = _ownedViewModel;
        _ownedViewModel = null;
        ownedViewModel?.Dispose();
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
        _subscribedViewModel = ViewModel;
        _subscribedViewModel.PropertyChanged += OnPropertyChanged;
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

        IsDisposed = true;

        if (!disposing)
        {
            return;
        }

        try
        {
            // The base class activates a ViewModel only after subscribing, so without a subscription it has nothing to undo and must not resolve one
            if (_subscribedViewModel is { } subscribedViewModel)
            {
                _subscribedViewModel = null;
                subscribedViewModel.PropertyChanged -= OnPropertyChanged;
                if (subscribedViewModel is ObservableRecipient observableRecipient)
                {
                    observableRecipient.IsActive = false;
                }
            }

            // Dispose two-way binding helper if it was initialized
            _bindingHelper?.Dispose();
            _bindingHelper = null;
        }
        finally
        {
            // A ViewModel this component created is not tracked by the container, so nothing else will dispose it
            ReleaseOwnedViewModel();
        }
    }

    /// <summary>
    /// Handles property change notifications from the ViewModel and triggers a UI update.
    /// </summary>
    /// <param name="o">The sender object.</param>
    /// <param name="propertyChangedEventArgs">The event data.</param>
    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);
}
