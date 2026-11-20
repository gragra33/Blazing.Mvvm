using System.ComponentModel;
using System.Runtime.ExceptionServices;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components.TwoWayBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

/// <summary>
///  A base class for components that represent a layout and resolves a ViewModel of type <typeparamref name="TViewModel"/>.
/// </summary>
/// <typeparam name="TViewModel">The ViewModel.</typeparam>
/// <remarks>
/// Extend cleanup by overriding <see cref="Dispose(bool)"/>, or <see cref="DisposeAsyncCore"/> when it must be asynchronous.
/// A derived layout that implements <see cref="IAsyncDisposable"/> itself receives the renderer's only disposal call,
/// which bypasses this class's cleanup.
/// </remarks>
public abstract class MvvmLayoutComponentBase<TViewModel> : LayoutComponentBase, IView<TViewModel>, IAsyncDisposable
    where TViewModel : IViewModelBase
{
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
    /// Whether disposal has started through either interface, so that lifecycle calls stop and a re-entrant or repeated
    /// asynchronous call does nothing.
    /// </summary>
    private bool _isReleasing;

    /// <summary>
    /// Whether <see cref="DisposeAsyncCore"/> is running, so that a direct call to <see cref="Dispose()"/> waits for the
    /// asynchronous disposal to finish the cleanup rather than disposing the ViewModel underneath it.
    /// </summary>
    private bool _isRunningDisposeAsyncCore;

    /// <summary>
    /// Indicates if the component has been disposed.
    /// </summary>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether disposal has started, after which lifecycle calls are no longer forwarded to the ViewModel.
    /// </summary>
    /// <remarks>
    /// Both flags are needed, because <see cref="IsDisposed"/> stays <see langword="false"/> when a derived component
    /// re-implements <see cref="IDisposable"/>, or overrides <see cref="Dispose(bool)"/>, without calling the base disposal.
    /// </remarks>
    private bool IsReleased => IsDisposed || _isReleasing;

    /// <summary>
    /// Resolves parameters in the <c>View</c> and <c>ViewModel</c>.
    /// </summary>
    [Inject]
    protected IParameterResolver ParameterResolver { get; set; } = null!;

    [Inject]
    private IServiceProvider Services { get; set; } = null!;

    /// <summary>
    /// The <c>ViewModel</c> associated with this component, resolved from the dependency injection container.
    /// </summary>
    /// <remarks>
    /// When <see cref="LibraryConfiguration.DisposeTransientViewModels"/> is enabled, a ViewModel registered as transient is
    /// created for this component rather than tracked by the container, and is disposed when this component is disposed,
    /// asynchronously when it implements <see cref="IAsyncDisposable"/>. A ViewModel assigned through the setter is never
    /// disposed by this component, and assigning one does not release the component's claim on the ViewModel it created.
    /// A component owns at most one ViewModel, and none once disposal has started or while it renders statically; any other
    /// comes from the container.
    /// </remarks>
    protected virtual TViewModel ViewModel
    {
        get
        {
            if (_viewModel is null)
            {
                var canOwnViewModel = _ownedViewModel is null && !IsReleased && !IsStaticRendering();
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
    /// <remarks>
    /// A ViewModel this component owns is disposed synchronously, through its <see cref="IDisposable.Dispose"/> method only,
    /// even when it also implements <see cref="IAsyncDisposable"/>. The renderer disposes the component asynchronously, so
    /// call <see cref="IAsyncDisposable.DisposeAsync"/> instead when disposing it from code and the ViewModel has asynchronous
    /// cleanup. While <see cref="DisposeAsyncCore"/> is running this method does nothing, and the asynchronous disposal finishes
    /// the cleanup once it completes.
    /// </remarks>
    public void Dispose()
    {
        if (_isRunningDisposeAsyncCore)
        {
            return;
        }

        // Stop lifecycle forwarding and new ownership even when a derived Dispose(bool) skips the base
        _isReleasing = true;

        // A derived Dispose(bool) that skips the base would otherwise leave an owned ViewModel undisposed, as nothing else tracks it
        RunThenReleaseOwnedViewModel(() => Dispose(true));
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the component and awaits asynchronous disposal of a ViewModel this component owns.
    /// </summary>
    /// <remarks>
    /// The renderer calls only this method when a component implements both disposal interfaces. It awaits
    /// <see cref="DisposeAsyncCore"/>, then runs the synchronous disposal path through <see cref="IDisposable"/>, so a derived
    /// component that re-implements <see cref="IDisposable"/> still receives the call. The owned ViewModel is released last,
    /// even when that re-implementation skips the base cleanup or a step throws. Every step runs even when an earlier one
    /// throws; a single failure is rethrown as it is, and several are reported together in an <see cref="AggregateException"/>.
    /// </remarks>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (IsReleased)
        {
            return;
        }

        _isReleasing = true;

        // Detach an async-disposable ViewModel before any derived code runs, so it is disposed once, asynchronously
        var asyncDisposableViewModel = _ownedViewModel as IAsyncDisposable;
        if (asyncDisposableViewModel is not null)
        {
            _ownedViewModel = null;
        }

        List<Exception>? exceptions = null;

        _isRunningDisposeAsyncCore = true;
        try
        {
            await DisposeAsyncCore();
        }
        catch (Exception exception)
        {
            (exceptions ??= []).Add(exception);
        }

        _isRunningDisposeAsyncCore = false;

        try
        {
            ((IDisposable)this).Dispose();
        }
        catch (Exception exception)
        {
            (exceptions ??= []).Add(exception);
        }

        try
        {
            ReleaseOwnedViewModel();
        }
        catch (Exception exception)
        {
            (exceptions ??= []).Add(exception);
        }

        if (asyncDisposableViewModel is not null)
        {
            try
            {
                await asyncDisposableViewModel.DisposeAsync();
            }
            catch (Exception exception)
            {
                (exceptions ??= []).Add(exception);
            }
        }

        GC.SuppressFinalize(this);

        if (exceptions is [var singleException])
        {
            ExceptionDispatchInfo.Throw(singleException);
        }

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }

    /// <summary>
    /// Override to add asynchronous cleanup, such as disposing an <c>IJSObjectReference</c>, and call the base implementation.
    /// </summary>
    /// <remarks>
    /// This runs before <see cref="Dispose(bool)"/>, while the ViewModel can still be called, although the component no longer
    /// renders or forwards lifecycle calls to it. It runs only when the renderer disposes the component, or when
    /// <see cref="IAsyncDisposable.DisposeAsync"/> is called, and not when <see cref="Dispose()"/> is called directly.
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
    /// Runs <paramref name="cleanup"/>, then releases the ViewModel this component owns even when the cleanup throws, reporting
    /// both failures together rather than letting the second hide the first.
    /// </summary>
    /// <param name="cleanup">The cleanup to run before the owned ViewModel is released.</param>
    private void RunThenReleaseOwnedViewModel(Action cleanup)
    {
        try
        {
            cleanup();
        }
        catch (Exception cleanupException)
        {
            try
            {
                ReleaseOwnedViewModel();
            }
            catch (Exception releaseException)
            {
                throw new AggregateException(cleanupException, releaseException);
            }

            throw;
        }

        ReleaseOwnedViewModel();
    }

    /// <summary>
    /// Determines whether the component is rendering statically, during static server-side rendering or prerendering.
    /// </summary>
    /// <remarks>
    /// The request scope ends with the response in those modes, so the container can dispose a transient ViewModel without
    /// leaking it, and in the order its scoped dependencies need: the renderer is a scoped service created before them, so a
    /// ViewModel the component owned would otherwise be disposed after them. Before .NET 9 the render mode is not exposed, so
    /// the component owns its ViewModel in every mode.
    /// </remarks>
    /// <returns><see langword="true"/> if the renderer reports that it is not interactive; otherwise, <see langword="false"/>.</returns>
    private bool IsStaticRendering()
    {
#if NET9_0_OR_GREATER
        try
        {
            return !RendererInfo.IsInteractive;
        }
        catch (Exception)
        {
            // A renderer that does not report its render mode throws, and the type varies: bUnit's MissingRendererInfoException
            // derives from Exception directly. Such a renderer is treated as interactive, so the component keeps owning its ViewModel
            return false;
        }
#else
        return false;
#endif
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Not forwarded once disposal has started, which happens when the ViewModel's own after-render work causes the
    /// component to be removed.
    /// </remarks>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!IsReleased)
        {
            ViewModel.OnAfterRender(firstRender);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Not forwarded once disposal has started, which happens when <see cref="OnAfterRender(bool)"/> causes the component
    /// to be removed.
    /// </remarks>
    protected override Task OnAfterRenderAsync(bool firstRender)
        => IsReleased ? Task.CompletedTask : ViewModel.OnAfterRenderAsync(firstRender);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override Task OnInitializedAsync()
        => ViewModel.OnInitializedAsync();

    /// <inheritdoc/>
    /// <remarks>
    /// Not forwarded once disposal has started, which happens when the component is removed while
    /// <see cref="OnInitializedAsync"/> is still running.
    /// </remarks>
    protected override void OnParametersSet()
    {
        if (!IsReleased)
        {
            ViewModel.OnParametersSet();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Not forwarded once disposal has started, which happens when the component is removed while
    /// <see cref="OnInitializedAsync"/> is still running.
    /// </remarks>
    protected override Task OnParametersSetAsync()
        => IsReleased ? Task.CompletedTask : ViewModel.OnParametersSetAsync();

    /// <inheritdoc/>
    /// <remarks>
    /// Once disposal has started no parameters are set, on the layout or on the ViewModel.
    /// </remarks>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (IsReleased)
        {
            return base.SetParametersAsync(ParameterView.Empty);
        }

        return ParameterResolver.SetParameters(this, ViewModel, parameters)
            ? base.SetParametersAsync(ParameterView.Empty)
            : base.SetParametersAsync(parameters);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <see langword="false"/> without asking the ViewModel once disposal has started.
    /// </remarks>
    protected override bool ShouldRender()
        => !IsReleased && ViewModel.ShouldRender();

    /// <summary>
    /// Disposes the component and releases unmanaged resources.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method was called from the dispose method or from a finalizer.</param>
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

        // A ViewModel this component created is not tracked by the container, so nothing else will dispose it
        RunThenReleaseOwnedViewModel(() =>
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
        });
    }

    private void OnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        => InvokeAsync(StateHasChanged);
}
