using System.ComponentModel;
using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components.TwoWayBinding;

/// <summary>
/// Provides automatic two-way binding between View component parameters and ViewModel properties
/// that are marked with the <see cref="ViewParameterAttribute"/>.
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel. Must implement <see cref="IViewModelBase"/>.</typeparam>
/// <remarks>
/// This helper automatically detects EventCallback&lt;T&gt; parameters that follow the Blazor naming convention
/// (e.g., {PropertyName}Changed) and wires them up to the corresponding ViewModel property changes.
/// It handles proper subscription and disposal to prevent memory leaks.
/// </remarks>
public sealed class TwoWayBindingHelper<TViewModel> : IDisposable
    where TViewModel : IViewModelBase
{
    private readonly TViewModel _viewModel;
    private readonly ComponentBase _component;
    private readonly Dictionary<string, EventCallbackInfo> _eventCallbacks = new();
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwoWayBindingHelper{TViewModel}"/> class.
    /// </summary>
    /// <param name="component">The component that owns this helper.</param>
    /// <param name="viewModel">The ViewModel to bind to.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> or <paramref name="viewModel"/> is null.</exception>
    public TwoWayBindingHelper(ComponentBase component, TViewModel viewModel)
    {
        _component = component ?? throw new ArgumentNullException(nameof(component));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>
    /// Initializes the two-way binding by scanning for EventCallback parameters and subscribing to ViewModel property changes.
    /// </summary>
    /// <remarks>
    /// This method should be called during the component's OnInitialized lifecycle method.
    /// It scans the component for EventCallback&lt;T&gt; properties that match the {PropertyName}Changed pattern
    /// and sets up automatic invocation when the corresponding ViewModel property changes.
    /// </remarks>
    public void Initialize()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(TwoWayBindingHelper<TViewModel>));

        // Scan component for EventCallback<T> properties
        var componentType = _component.GetType();
        var properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Check if property is an EventCallback<T>
            if (!property.PropertyType.IsGenericType)
                continue;

            var genericType = property.PropertyType.GetGenericTypeDefinition();
            if (genericType != typeof(EventCallback<>))
                continue;

            // Check if it follows the {PropertyName}Changed naming convention
            if (!property.Name.EndsWith("Changed"))
                continue;

            var propertyName = property.Name[..^7]; // Remove "Changed" suffix
            var valueType = property.PropertyType.GetGenericArguments()[0];

            // Check if corresponding ViewModel property exists and has [ViewParameter]
            var viewModelProperty = _viewModel.GetType().GetProperty(propertyName);
            if (viewModelProperty == null)
                continue;

            var hasViewParameter = viewModelProperty.GetCustomAttribute<ViewParameterAttribute>() != null;
            if (!hasViewParameter)
                continue;

            // Store the EventCallback info
            var eventCallback = property.GetValue(_component);
            if (eventCallback != null)
            {
                _eventCallbacks[propertyName] = new EventCallbackInfo(
                    propertyName,
                    eventCallback,
                    valueType,
                    property.PropertyType
                );
            }
        }

        // Subscribe to ViewModel property changes
        if (_eventCallbacks.Count > 0)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Handles property change notifications from the ViewModel and invokes the corresponding EventCallback.
    /// </summary>
    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName))
            return;

        if (!_eventCallbacks.TryGetValue(e.PropertyName, out var callbackInfo))
            return;

        // Get the current value from the ViewModel
        var viewModelProperty = _viewModel.GetType().GetProperty(e.PropertyName);
        if (viewModelProperty == null)
            return;

        var currentValue = viewModelProperty.GetValue(_viewModel);

        // Get the corresponding component parameter value to check if it's different
        var componentProperty = _component.GetType().GetProperty(e.PropertyName);
        if (componentProperty != null)
        {
            var componentValue = componentProperty.GetValue(_component);
            
            // Only invoke if the values are different
            if (Equals(currentValue, componentValue))
                return;
        }

        // Invoke the EventCallback
        await InvokeEventCallbackAsync(callbackInfo, currentValue);
    }

    /// <summary>
    /// Invokes an EventCallback with the specified value.
    /// </summary>
    private async Task InvokeEventCallbackAsync(EventCallbackInfo callbackInfo, object? value)
    {
        try
        {
            // Use reflection to call InvokeAsync on the EventCallback<T>
            var invokeMethod = callbackInfo.EventCallbackType.GetMethod("InvokeAsync", new[] { callbackInfo.ValueType });
            if (invokeMethod != null)
            {
                var task = invokeMethod.Invoke(callbackInfo.EventCallback, new[] { value }) as Task;
                if (task != null)
                {
                    await task;
                }
            }
        }
        catch (Exception)
        {
            // Silently ignore errors during callback invocation to prevent disrupting the component
            // In production, you might want to log this
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="TwoWayBindingHelper{TViewModel}"/>.
    /// </summary>
    /// <remarks>
    /// This method unsubscribes from all ViewModel property change events to prevent memory leaks.
    /// It should be called during the component's Dispose method.
    /// </remarks>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _eventCallbacks.Clear();
        _isDisposed = true;
    }

    /// <summary>
    /// Holds information about an EventCallback that should be invoked when a property changes.
    /// </summary>
    private sealed class EventCallbackInfo
    {
        public string PropertyName { get; }
        public object EventCallback { get; }
        public Type ValueType { get; }
        public Type EventCallbackType { get; }

        public EventCallbackInfo(string propertyName, object eventCallback, Type valueType, Type eventCallbackType)
        {
            PropertyName = propertyName;
            EventCallback = eventCallback;
            ValueType = valueType;
            EventCallbackType = eventCallbackType;
        }
    }
}
