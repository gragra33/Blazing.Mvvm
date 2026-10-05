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

The dependency injection container keeps every disposable transient service it creates until its scope ends ([Disposable transient services captured by container](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/guidelines#disposable-transient-services-captured-by-container)). On Blazor Server that scope is the whole circuit, and on Blazor WebAssembly it is the whole application, so a transient ViewModel resolved from it would stay in memory after its page closed, and one more would accumulate on every visit.

To prevent that, `MvvmComponentBase` and `MvvmLayoutComponentBase` follow Microsoft's guidance for a transient disposable that must be released before its scope ends: they create the ViewModel outside the container with `ActivatorUtilities.CreateInstance`, while still using the container for its dependencies ([IDisposable guidance for transient and shared instances](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/guidelines#idisposable-guidance-for-transient-and-shared-instances)). The component owns that ViewModel and disposes it when the component is disposed, awaiting `DisposeAsync` when the ViewModel implements `IAsyncDisposable`. Assigning another ViewModel through the `ViewModel` setter does not change this: the component still disposes the ViewModel it created, and never disposes one it was given.

The ViewModel's constructor dependencies are resolved from the component's service provider, so scoped services such as authentication state or a per-circuit cache stay shared with the rest of the circuit. This is the difference from `MvvmOwningComponentBase`, which gives the component a service scope of its own, and therefore its own copy of every scoped dependency.

> [!NOTE]
> Only the ViewModel changes owner. A disposable **transient** service injected into the ViewModel is still created and held by the circuit's container. If such a dependency must be released with the page, inherit from `MvvmOwningComponentBase` or inject a factory and dispose what it creates in `Dispose(bool disposing)`.

#### Transient ViewModels the container still owns

The component leaves these transient ViewModels to the container, which disposes them when its scope ends, exactly as when the option is off:

- **Factory registrations**, such as `AddTransient<IMyViewModel>(sp => ...)`. A factory can return an instance that other code shares, for example `sp => sp.GetRequiredService<MyViewModel>()` forwarding to a singleton, and disposing it with one page would break every other user. The container offers no way to release an instance it is tracking, and Microsoft's own design work on this for Blazor Server remains open ([dotnet/aspnetcore #26676](https://github.com/dotnet/aspnetcore/issues/26676)). Register ViewModels by type, as `AddMvvm` and `[ViewModelDefinition]` do, to have the component own them.
- **ViewModels that receive their own key through `[ServiceKey]`**, which only the container can construct.
- **ViewModels of a View that implements `IAsyncDisposable` itself.** Blazor calls only the View's own `DisposeAsync` ([ASP.NET Core Razor component disposal](https://learn.microsoft.com/aspnet/core/blazor/components/component-disposal)), so the base class cannot be sure its disposal runs. Analyzer [BLAZMVVM0022](../analyzers/BLAZMVVM0022.md) reports this pattern; extend cleanup by overriding `Dispose(bool disposing)` instead.

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
