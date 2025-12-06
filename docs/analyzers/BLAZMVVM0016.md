# BLAZMVVM0016: Messenger Registration Lifetime Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0016
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: Messenger registration without unregistration

## Description

Detects messenger registrations (Register calls) without corresponding unregistration (Unregister calls), which can lead to memory leaks as messenger keeps strong references to recipients.

## Examples

### ❌ Incorrect

```csharp
public class MyViewModel : ViewModelBase
{
    public MyViewModel()
    {
        Messenger.Register<MyMessage>(this, HandleMessage); // ⚠️ Warning
    }
}
```

### ✅ Correct (With Dispose)

```csharp
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

### ✅ Correct (Using RecipientViewModelBase)

```csharp
public class MyViewModel : RecipientViewModelBase
{
    protected override void OnActivated()
    {
        Messenger.Register<MyMessage>(this, HandleMessage);
        // Automatically unregistered on deactivation
    }
}
```

## Related

- **[BLAZMVVM0015](BLAZMVVM0015.md)**: Dispose Pattern Analyzer
