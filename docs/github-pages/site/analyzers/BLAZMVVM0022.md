# BLAZMVVM0022: View Disposal Bypass

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0022
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: View bypasses the MVVM base class disposal

## Description

A View that derives from [`MvvmComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmComponentBase`1), [`MvvmOwningComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmOwningComponentBase`1) or [`MvvmLayoutComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmLayoutComponentBase`1) should not implement `IDisposable` or `IAsyncDisposable` itself.

When a component implements a disposal interface, Blazor calls the component's own implementation, and when it implements both, Blazor calls only `DisposeAsync` ([ASP.NET Core Razor component disposal](https://learn.microsoft.com/aspnet/core/blazor/components/component-disposal)). A View that adds `@implements IDisposable` or `@implements IAsyncDisposable` therefore replaces the base class disposal instead of extending it, and the base class cleanup never runs:

- the View stays subscribed to its ViewModel's `PropertyChanged` event;
- a recipient ViewModel stays active, so a strong-reference messenger keeps it alive;
- a transient ViewModel or service scope the View owns may never be released.

The rule reports the method that implements the interface, on the type that declares it.

## Examples

### ❌ Incorrect

```razor
@page "/orders"
@inherits MvvmComponentBase<OrdersViewModel>
@implements IAsyncDisposable

@code {
    private IJSObjectReference? _module;

    public async ValueTask DisposeAsync() // BLAZMVVM0022
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
```

### ✅ Correct

Extend the base class cleanup by overriding `Dispose(bool)`:

```razor
@page "/orders"
@inherits MvvmComponentBase<OrdersViewModel>

@code {
    private Timer? _timer;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Dispose();
        }

        base.Dispose(disposing);
    }
}
```

When cleanup must be asynchronous, keep `IAsyncDisposable` and call `Dispose()` at the end of `DisposeAsync` so the base class cleanup still runs. Suppress the diagnostic for that method once it does:

```razor
@page "/orders"
@inherits MvvmComponentBase<OrdersViewModel>
@implements IAsyncDisposable

@code {
    private IJSObjectReference? _module;

#pragma warning disable BLAZMVVM0022 // Dispose() below runs the base class cleanup
    public async ValueTask DisposeAsync()
#pragma warning restore BLAZMVVM0022
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }

        Dispose();
    }
}
```

> [!NOTE]
> A View that implements `IAsyncDisposable` itself keeps its transient ViewModel owned by the dependency injection container, because the base class cannot be sure its disposal will run. See [When a ViewModel is disposed](../configuration/view-models.md#when-a-viewmodel-is-disposed).

## Code Fix

No code fix is provided. Moving cleanup into `Dispose(bool)` changes when and how it runs, so the change is left to the author.

## Related

- **[BLAZMVVM0013](BLAZMVVM0013.md)**: MvvmOwningComponentBase Usage
- **[BLAZMVVM0015](BLAZMVVM0015.md)**: Dispose Pattern
