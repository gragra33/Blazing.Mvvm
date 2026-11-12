# BLAZMVVM0022: View Disposal Bypass

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0022
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: View bypasses the MVVM base class disposal

## Description

A View that derives from [`MvvmComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmComponentBase`1), [`MvvmOwningComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmOwningComponentBase`1) or [`MvvmLayoutComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmLayoutComponentBase`1) should extend the base class disposal, not replace it. When a component implements both disposal interfaces, Blazor calls only `DisposeAsync` ([ASP.NET Core Razor component disposal](https://learn.microsoft.com/aspnet/core/blazor/components/component-disposal)), and the base classes implement it. The rule reports two patterns:

**A View that implements `IAsyncDisposable` itself.** The View's method receives Blazor's only disposal call, so the base class cleanup never runs:

- the View stays subscribed to its ViewModel's `PropertyChanged` event;
- a recipient ViewModel stays active, so a strong-reference messenger keeps it alive;
- the service scope of an `MvvmOwningComponentBase` View is not released;
- a transient ViewModel stays with the container until its scope ends.

**A View that re-implements `IDisposable` without calling the base disposal.** The base class still calls the View's method and still releases what it owns, but the View stays subscribed to its ViewModel and a recipient ViewModel stays active. With a scoped or singleton ViewModel, that keeps the closed View alive.

A re-implementation is not reported when it calls the base disposal: `base.Dispose()` or `Dispose(true)` for `IDisposable`, or `await base.DisposeAsync()` for `IAsyncDisposable`, which only `MvvmOwningComponentBase` exposes. A parameterless `base.Dispose()` or `base.DisposeAsync()` counts only when it reaches the MVVM base class, directly or through an intermediate class whose method calls the base in turn; a call that lands on an intermediate method hiding the base one does not. An intermediate class from another assembly cannot be inspected, so a call that lands on it always counts. The call counts in the method body or in a local function, including under an `if`, but not when it appears only inside a lambda or anonymous method, which may never run. A call made through a separate helper method is not recognised, so suppress the diagnostic with `#pragma warning disable BLAZMVVM0022` in that case.

The rule reports the method that implements the interface, on the type that declares it. It also reports a View that re-lists `IDisposable` or `IAsyncDisposable` when the interface then maps to a method an intermediate class declared to hide the base method, such as `public new void Dispose() { }`, because re-listing turns that hidden method into the one Blazor calls. That diagnostic is on the View that re-lists the interface, not on the classes that derive from it.

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

```razor
@page "/orders"
@inherits MvvmComponentBase<OrdersViewModel>
@implements IDisposable

@code {
    private Timer? _timer;

    public new void Dispose() // BLAZMVVM0022
    {
        _timer?.Dispose();
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

When cleanup must be asynchronous, override `DisposeAsyncCore`. It runs before the base class cleanup, while the ViewModel can still be called:

```razor
@page "/orders"
@inherits MvvmComponentBase<OrdersViewModel>

@code {
    private IJSObjectReference? _module;

    protected override async ValueTask DisposeAsyncCore()
    {
        try
        {
            if (_module is not null)
            {
                await _module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // JS interop is unavailable once the circuit has disconnected
        }

        await base.DisposeAsyncCore();
    }
}
```

> [!NOTE]
> On `MvvmOwningComponentBase`, the base `DisposeAsyncCore` releases the service scope, so call it last and do not omit it.

## Code Fix

No code fix is provided. Moving cleanup into `Dispose(bool)` or `DisposeAsyncCore` changes when and how it runs, so the change is left to the author.

## Related

- **[BLAZMVVM0013](BLAZMVVM0013.md)**: MvvmOwningComponentBase Usage
- **[BLAZMVVM0015](BLAZMVVM0015.md)**: Dispose Pattern
- **[When a ViewModel is disposed](../configuration/view-models.md#when-a-viewmodel-is-disposed)**
