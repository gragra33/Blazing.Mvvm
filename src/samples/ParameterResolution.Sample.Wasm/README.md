# Parameter Resolution Sample (WASM)

This sample project demonstrates the **Parameter Resolution** and **Automatic Two-Way Binding** features in Blazing.Mvvm with interactive commands and MVVM navigation.

## What is Parameter Resolution?

Parameter Resolution allows you to pass parameter values from a Blazor View component to its ViewModel automatically. Properties in the ViewModel marked with the `[ViewParameter]` attribute will be resolved from parameters defined in the View with the `[Parameter]` attribute.

## What is Automatic Two-Way Binding?

Automatic Two-Way Binding eliminates the need for manual `PropertyChanged` event handling in components. When a component has:
- An `EventCallback<T>` parameter following the `{PropertyName}Changed` naming convention (e.g., `CounterChanged`)
- A corresponding ViewModel property marked with `[ViewParameter]` (e.g., `Counter`)

The binding is automatically wired up - when the ViewModel property changes, the EventCallback is invoked automatically. No manual event subscription or disposal code is needed!

## Key Features Demonstrated

1. **Property Name Mapping**: Using `[ViewParameter]` to map by property name
2. **Custom Name Mapping**: Using `[ViewParameter("ParamName")]` to map to a different parameter name
3. **ObservableProperty Support**: Integration with `[ObservableProperty]` from CommunityToolkit.Mvvm
4. **RelayCommand Support**: Using `[RelayCommand]` to generate command properties for View interaction
5. **MVVM Navigation**: Using `MvvmNavigationManager` with `RelayCommand` for type-safe, ViewModel-first navigation
6. **Nullable Support**: Handling nullable parameter types
7. **Query String Parameters**: Resolving parameters from URL query strings using `[SupplyParameterFromQuery]`
8. **Interactive UI**: Increment/Decrement buttons demonstrating ViewModel-to-View binding
9. **Automatic Two-Way Binding**: `CounterComponent` demonstrates zero-configuration two-way binding between View and ViewModel

## How to Run

1. Set `ParameterResolution.Sample.Wasm` as the startup project
2. Run the application (F5 or Ctrl+F5)
3. Navigate through the samples to see parameter resolution in action
4. Use the increment/decrement buttons on the Parameter Demo page to interact with the ViewModel
5. Observe how the counter in the `CounterComponent` updates automatically via two-way binding

## Project Structure

- **ViewModels/ParameterDemoViewModel.cs** - Demonstrates ViewParameter usage with different mapping strategies and RelayCommand
- **ViewModels/CounterComponentViewModel.cs** - Simple ViewModel with `[ViewParameter]` for two-way binding
- **ViewModels/MainLayoutViewModel.cs** - Tracks navigation count
- **ViewModels/HomeViewModel.cs** - Demonstrates MVVM navigation with MvvmNavigationManager and RelayCommand
- **Components/CounterComponent.razor** - Demonstrates automatic two-way binding (no manual event handling!)
- **Pages/Home.razor** - Landing page with MVVM navigation examples
- **Pages/ParameterDemo.razor** - Shows parameter resolution with interactive counter buttons and child component
- **Layout/MainLayout.razor** - Main layout with navigation counter
- **Layout/NavMenu.razor** - Navigation menu
- **Program.cs** - Configuration with `ParameterResolutionMode.ViewAndViewModel`

## Configuration

The key configuration in `Program.cs`:

```csharp
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.WebAssembly;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
});
```

## Automatic Two-Way Binding Example

**ViewModel (CounterComponentViewModel.cs):**

```csharp
public partial class CounterComponentViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter;
}
```

**Child Component (CounterComponent.razor):**

```razor
@inherits MvvmComponentBase<CounterComponentViewModel>

<p role="status">Current count: <strong>@ViewModel.Counter</strong></p>

@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }
}
```

**Parent Component Usage:**

```razor
<CounterComponent @bind-Counter="ViewModel.Counter" />
```

That's it! No manual event subscription, no disposal code, no boilerplate. The two-way binding is automatically detected and wired up because:
- ✅ Component has `Counter` parameter with `CounterChanged` EventCallback
- ✅ ViewModel has `Counter` property marked with `[ViewParameter]`
- ✅ Follows Blazor's `@bind-` naming convention

When `ViewModel.Counter` changes in the child component, the `CounterChanged` callback is automatically invoked, updating the parent's ViewModel. Memory leaks are prevented through automatic cleanup when the component is disposed.

## MVVM Navigation Pattern

The Home page demonstrates the recommended MVVM navigation pattern:

**ViewModel (HomeViewModel.cs):**

```csharp
[RelayCommand]
private void NavigateWithParams(string queryString)
{
    _navigationManager.NavigateTo<ParameterDemoViewModel>(queryString);
}
```

**View (Home.razor):**

```razor
<button class="btn btn-primary" 
        @onclick="@(() => ViewModel.NavigateWithParamsCommand.Execute("?Title=Hello%20World&Count=42&Content=Sample%20Content"))">
    Navigate with Basic Parameters
</button>
```

This approach:

- ✅ Keeps navigation logic in the ViewModel
- ✅ Uses strongly-typed navigation with `MvvmNavigationManager`
- ✅ Leverages `RelayCommand` for command pattern
- ✅ Passes query string parameters from View to ViewModel
- ✅ Maintains proper separation of concerns

## Interactive Features

The Parameter Demo page includes:

- **Increment Button**: Uses `IncrementCounterCommand` to increase the counter
- **Decrement Button**: Uses `DecrementCounterCommand` to decrease the counter
- **Real-time Display**: Shows the current counter value and all resolved parameters
- **Counter Component**: Demonstrates automatic two-way binding with child component
- **Code Examples**: Displays both ViewModel and View code for reference

## Learn More

For more information about Parameter Resolution and Automatic Two-Way Binding, see the main [Blazing.Mvvm README](../../README.md#parameter-resolution).
