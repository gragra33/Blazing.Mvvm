# View Models

Blazing.Mvvm provides base classes and conventions that let Blazor components work with ViewModels in a predictable MVVM style.

## Available base classes

Use the base class that matches your ViewModel needs:

- [`ViewModelBase`](xref:Blazing.Mvvm.ComponentModel.ViewModelBase) derives from `ObservableObject`
- [`RecipientViewModelBase`](xref:Blazing.Mvvm.ComponentModel.RecipientViewModelBase) derives from `ObservableRecipient`
- [`ValidatorViewModelBase`](xref:Blazing.Mvvm.ComponentModel.ValidatorViewModelBase) derives from `ObservableValidator`

## Lifecycle methods

The ViewModel base classes mirror common `ComponentBase` lifecycle hooks:

- `OnInitialized`
- `OnInitializedAsync`
- `OnParametersSet`
- `OnParametersSetAsync`
- `OnAfterRender`
- `OnAfterRenderAsync`
- `ShouldRender`

This lets component lifecycle flow into the ViewModel without custom plumbing in every page.

## Disposal behavior

Since v3.2.1, all ViewModel base classes implement `IDisposable`. When a ViewModel is disposed, it automatically unsubscribes from all `IAsyncRelayCommand` `PropertyChanged` events. This matters most for commands with `AllowConcurrentExecutions` set to `false`, where the framework monitors the command's `IsRunning` property to trigger UI updates. Without cleanup, those subscriptions leak.

Automatic disposal gives you:

- **Memory leak prevention**: command event subscriptions are cleaned up without manual tracking
- **Simpler ViewModels**: no need to track and unsubscribe from command events yourself
- **A consistent pattern**: all ViewModels follow the standard .NET dispose pattern
- **Efficient garbage collection**: commands and ViewModels are released promptly

If a derived ViewModel needs to release additional resources, override `Dispose(bool disposing)`:

```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class MyViewModel : ViewModelBase
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        base.Dispose(disposing);
    }
}
```

> [!WARNING]
> If you previously implemented `public void Dispose()` yourself, change that to `protected override void Dispose(bool disposing)` so the base class can keep its cleanup behavior.

### When a ViewModel is disposed

Who disposes a ViewModel depends on its registered lifetime and on the component base type:

| ViewModel lifetime | [`MvvmComponentBase`](xref:Blazing.Mvvm.Components.MvvmComponentBase`1) / [`MvvmLayoutComponentBase`](xref:Blazing.Mvvm.Components.MvvmLayoutComponentBase`1) | [`MvvmOwningComponentBase`](xref:Blazing.Mvvm.Components.MvvmOwningComponentBase`1) |
| --- | --- | --- |
| Transient (the default) | Disposed with the component | Disposed with the component's service scope |
| Scoped | Disposed when the circuit (Server) or application (WebAssembly) scope ends | Disposed with the component's service scope |
| Singleton | Disposed when the application stops | Disposed when the application stops |

The dependency injection container keeps every disposable transient service it creates until its scope ends. On Blazor Server that scope is the whole circuit, and on Blazor WebAssembly it is the whole application, so a transient ViewModel resolved from it would stay in memory after its page closed, and one more would accumulate on every visit. To prevent that, `MvvmComponentBase` and `MvvmLayoutComponentBase` create a transient ViewModel from its registration themselves instead of asking the container for it, and dispose it when the component is disposed.

The ViewModel's constructor dependencies are still resolved from the component's service provider, so scoped services such as authentication state or a per-circuit cache stay shared with the rest of the circuit. This is the difference from `MvvmOwningComponentBase`, which gives the component a service scope of its own, and therefore its own copy of every scoped dependency.

> [!NOTE]
> Only the ViewModel changes owner. A disposable **transient** service injected into the ViewModel is still created and held by the circuit's container. If such a dependency must be released with the page, inherit from `MvvmOwningComponentBase` or inject a factory and dispose what it creates in `Dispose(bool disposing)`.

A ViewModel that receives its own key through `[ServiceKey]` can only be constructed by the container, so it is always resolved from the container and disposed with its scope.

To restore the earlier behavior, in which the container owns transient ViewModels, turn the option off:

```csharp
builder.Services.AddMvvm(options =>
{
    options.DisposeTransientViewModels = false;
});
```

## Service registration

ViewModels are registered as transient services by default. Use [`ViewModelDefinition`](xref:Blazing.Mvvm.ComponentModel.ViewModelDefinitionAttribute) to choose another lifetime:

```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public partial class FetchDataViewModel : ViewModelBase
{
    // ViewModel code
}
```

Then inherit your component from the matching MVVM base type:

```razor
@page "/fetchdata"
@inherits MvvmComponentBase<FetchDataViewModel>
```

## Register with interfaces or abstract types

Use the generic [`ViewModelDefinition`](xref:Blazing.Mvvm.ComponentModel.ViewModelDefinitionAttribute) attribute when you want the ViewModel resolved through an abstraction:

```csharp
[ViewModelDefinition<IFetchDataViewModel>]
public partial class FetchDataViewModel : ViewModelBase, IFetchDataViewModel
{
    // ViewModel code
}
```

The component can then depend on that abstraction:

```razor
@page "/fetchdata"
@inherits MvvmComponentBase<IFetchDataViewModel>
```

## Register keyed ViewModels

Use a key when you need explicit string-based lookup:

```csharp
[ViewModelDefinition(Key = "FetchDataViewModel")]
public partial class FetchDataViewModel : ViewModelBase
{
    // ViewModel code
}
```

Reference the key on the component with `ViewModelKey`:

```razor
@page "/fetchdata"
@attribute [ViewModelKey("FetchDataViewModel")]
@inherits MvvmComponentBase<FetchDataViewModel>
```

## When to use each component base type

- [`MvvmComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmComponentBase`1): default choice for most pages and components
- [`MvvmOwningComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmOwningComponentBase`1): use when the component needs its own scoped dependency lifetime; its ViewModel and every scoped dependency are created in a new scope and disposed with the component
- [`MvvmLayoutComponentBase<TViewModel>`](xref:Blazing.Mvvm.Components.MvvmLayoutComponentBase`1): use when the layout itself owns a ViewModel

## Related topics

- [Parameter Resolution and Two-Way Binding](parameter-resolution.md)
- [Multi-Project ViewModel Registration](multi-project-registration.md)
- [MVVM Navigation](../navigation/mvvm-navigation.md)
