# BLAZMVVM0015: Dispose Pattern Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0015
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: ViewModel should implement IDisposable

## Description

This analyzer detects ViewModels that use disposable resources, event subscriptions, or messenger registrations but don't implement `IDisposable` for proper cleanup. Failing to dispose resources can lead to memory leaks, resource exhaustion, and unexpected behavior.

## Problem

ViewModels that don't implement proper disposal patterns may cause:

1. **Memory leaks**: Event handlers keeping objects alive
2. **Resource leaks**: Database connections, file handles, HTTP clients not disposed
3. **Zombie objects**: Objects receiving events after they should be garbage collected
4. **Performance degradation**: Accumulated resources consuming memory

## Solution

Implement `IDisposable` interface and clean up resources in the `Dispose` method:

- Unregister event handlers
- Unregister messenger subscriptions
- Dispose of disposable fields/properties
- Call `GC.SuppressFinalize(this)` if no finalizer

## Examples

### ❌ Incorrect (Missing Disposal)

```csharp
// No IDisposable implementation
public class ProductViewModel : ViewModelBase // ⚠️ Warning BLAZMVVM0015
{
    private readonly HttpClient _httpClient;

    public ProductViewModel()
    {
        _httpClient = new HttpClient();
        WeakReferenceMessenger.Default.Register<ProductUpdated>(this, HandleUpdate);
    }

    private void HandleUpdate(object recipient, ProductUpdated message)
    {
        // Handle message
    }
    
    // Missing Dispose method!
}
```

### ✅ Correct (With Proper Disposal)

```csharp
public class ProductViewModel : ViewModelBase, IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public ProductViewModel()
    {
        _httpClient = new HttpClient();
        WeakReferenceMessenger.Default.Register<ProductUpdated>(this, HandleUpdate);
    }

    private void HandleUpdate(object recipient, ProductUpdated message)
    {
        // Handle message
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        // Unregister messenger
        WeakReferenceMessenger.Default.UnregisterAll(this);
        
        // Dispose resources
        _httpClient?.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
```

## Common Scenarios Requiring Disposal

### 1. Messenger Registrations

```csharp
// ❌ Without disposal
public class MyViewModel : ViewModelBase
{
    public MyViewModel()
    {
        Messenger.Register<MyMessage>(this, HandleMessage);
        // Missing unregister!
    }
}

// ✅ With disposal
public class MyViewModel : ViewModelBase, IDisposable
{
    public MyViewModel()
    {
        Messenger.Register<MyMessage>(this, HandleMessage);
    }

    public void Dispose()
    {
        Messenger.UnregisterAll(this);
        GC.SuppressFinalize(this);
    }
}
```

### 2. Event Subscriptions

```csharp
// ❌ Without disposal
public class MyViewModel : ViewModelBase
{
    public MyViewModel(IDataService service)
    {
        service.DataChanged += OnDataChanged;
        // Missing unsubscribe!
    }
    
    private void OnDataChanged(object sender, EventArgs e) { }
}

// ✅ With disposal
public class MyViewModel : ViewModelBase, IDisposable
{
    private readonly IDataService _service;

    public MyViewModel(IDataService service)
    {
        _service = service;
        _service.DataChanged += OnDataChanged;
    }
    
    private void OnDataChanged(object sender, EventArgs e) { }

    public void Dispose()
    {
        _service.DataChanged -= OnDataChanged;
        GC.SuppressFinalize(this);
    }
}
```

### 3. Disposable Fields

```csharp
// ❌ Without disposal
public class MyViewModel : ViewModelBase
{
    private readonly Timer _timer;
    private readonly HttpClient _httpClient;

    public MyViewModel()
    {
        _timer = new Timer(OnTick, null, 0, 1000);
        _httpClient = new HttpClient();
        // Missing disposal!
    }
}

// ✅ With disposal
public class MyViewModel : ViewModelBase, IDisposable
{
    private readonly Timer _timer;
    private readonly HttpClient _httpClient;

    public MyViewModel()
    {
        _timer = new Timer(OnTick, null, 0, 1000);
        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

### 4. CancellationTokenSource

```csharp
// ❌ Without disposal
public class MyViewModel : ViewModelBase
{
    private CancellationTokenSource _cts = new();

    public async Task LoadDataAsync()
    {
        await _service.LoadAsync(_cts.Token);
        // _cts not disposed!
    }
}

// ✅ With disposal
public class MyViewModel : ViewModelBase, IDisposable
{
    private CancellationTokenSource _cts = new();

    public async Task LoadDataAsync()
    {
        await _service.LoadAsync(_cts.Token);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

## RecipientViewModelBase Alternative

For messenger-only scenarios, consider using `RecipientViewModelBase` which handles messenger cleanup automatically:

```csharp
// ✅ Automatic messenger cleanup
public class MyViewModel : RecipientViewModelBase
{
    protected override void OnActivated()
    {
        // Register messages here
        Messenger.Register<MyMessage>(this, HandleMessage);
        // Automatically unregistered when deactivated!
    }

    private void HandleMessage(object recipient, MyMessage message)
    {
        // Handle message
    }
    
    // No manual Dispose needed for messenger!
}
```

## Full Dispose Pattern

For complex ViewModels with both managed and unmanaged resources:

```csharp
public class ComplexViewModel : ViewModelBase, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Timer _timer;
    private readonly IDataService _service;
    private CancellationTokenSource _cts;
    private bool _disposed;

    public ComplexViewModel(IDataService service)
    {
        _service = service;
        _httpClient = new HttpClient();
        _timer = new Timer(OnTick, null, 0, 1000);
        _cts = new CancellationTokenSource();
        
        _service.DataChanged += OnDataChanged;
        Messenger.Register<MyMessage>(this, HandleMessage);
    }

    private void OnDataChanged(object sender, EventArgs e) { }
    private void HandleMessage(object recipient, MyMessage message) { }
    private void OnTick(object state) { }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources
            _cts?.Cancel();
            _cts?.Dispose();
            _timer?.Dispose();
            _httpClient?.Dispose();
            
            // Unsubscribe events
            _service.DataChanged -= OnDataChanged;
            
            // Unregister messenger
            Messenger.UnregisterAll(this);
        }

        // Free unmanaged resources here (if any)

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~ComplexViewModel()
    {
        Dispose(disposing: false);
    }
}
```

## When Disposal is NOT Needed

✅ No disposal needed for:

- Simple data properties
- Injected services (managed by DI container)
- Value types
- Strings
- Collections (unless they contain disposables)

```csharp
// ✅ No disposal needed
public class SimpleViewModel : ViewModelBase
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<string> Items { get; set; } = new();
    
    // No disposable resources, no IDisposable needed
}
```

## Best Practices

### DO ✅

- Implement `IDisposable` when using disposable resources
- Unregister all event handlers
- Unregister all messenger subscriptions
- Use `using` statements for disposable ViewModels where possible
- Call `GC.SuppressFinalize(this)` in Dispose
- Consider `RecipientViewModelBase` for messenger-only scenarios

### DON'T ❌

- Forget to unregister event handlers
- Leave messenger registrations active
- Keep disposable resources without disposal
- Dispose services injected via DI (container handles it)

## Component Integration

Blazing.MVVM components automatically dispose ViewModels:

```csharp
// MvvmComponentBase automatically calls Dispose on ViewModel
@inherits MvvmComponentBase<ProductViewModel>
@implements IDisposable

// ViewModel is disposed when component is disposed
```

## Related Analyzers

- **[BLAZMVVM0016](BLAZMVVM0016.md)**: Messenger Registration Lifetime Analyzer
- **[BLAZMVVM0013](BLAZMVVM0013.md)**: MvvmOwningComponentBase Usage Analyzer
- **[BLAZMVVM0001](BLAZMVVM0001.md)**: ViewModelBase Inheritance Analyzer

## Additional Resources

- [IDisposable Interface](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)
- [Implement Dispose Pattern](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
- [Disposing Resources](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern)
