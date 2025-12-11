# Parameter Resolution Sample (WASM)

This sample project demonstrates the **Parameter Resolution** feature in Blazing.Mvvm with interactive commands and MVVM navigation.

## What is Parameter Resolution?

Parameter Resolution allows you to pass parameter values from a Blazor View component to its ViewModel automatically. Properties in the ViewModel marked with the `[ViewParameter]` attribute will be resolved from parameters defined in the View with the `[Parameter]` attribute.

## Key Features Demonstrated

1. **Property Name Mapping**: Using `[ViewParameter]` to map by property name
2. **Custom Name Mapping**: Using `[ViewParameter("ParamName")]` to map to a different parameter name
3. **ObservableProperty Support**: Integration with `[ObservableProperty]` from CommunityToolkit.Mvvm
4. **RelayCommand Support**: Using `[RelayCommand]` to generate command properties for View interaction
5. **MVVM Navigation**: Using `MvvmNavigationManager` with `RelayCommand` for type-safe, ViewModel-first navigation
6. **Nullable Support**: Handling nullable parameter types
7. **Query String Parameters**: Resolving parameters from URL query strings using `[SupplyParameterFromQuery]`
8. **Interactive UI**: Increment/Decrement buttons demonstrating ViewModel-to-View binding

## How to Run

1. Set `ParameterResolution.Sample.Wasm` as the startup project
2. Run the application (F5 or Ctrl+F5)
3. Navigate through the samples to see parameter resolution in action
4. Use the increment/decrement buttons on the Parameter Demo page to interact with the ViewModel

## Project Structure

- **ViewModels/ParameterDemoViewModel.cs** - Demonstrates ViewParameter usage with different mapping strategies and RelayCommand
- **ViewModels/MainLayoutViewModel.cs** - Tracks navigation count
- **ViewModels/HomeViewModel.cs** - Demonstrates MVVM navigation with MvvmNavigationManager and RelayCommand
- **Pages/Home.razor** - Landing page with MVVM navigation examples
- **Pages/ParameterDemo.razor** - Shows parameter resolution with interactive counter buttons
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

- ? Keeps navigation logic in the ViewModel
- ? Uses strongly-typed navigation with `MvvmNavigationManager`
- ? Leverages `RelayCommand` for command pattern
- ? Passes query string parameters from View to ViewModel
- ? Maintains proper separation of concerns

## Interactive Features

The Parameter Demo page includes:

- **Increment Button**: Uses `IncrementCounterCommand` to increase the counter
- **Decrement Button**: Uses `DecrementCounterCommand` to decrease the counter
- **Real-time Display**: Shows the current counter value and all resolved parameters
- **Code Examples**: Displays both ViewModel and View code for reference

## Learn More

For more information about Parameter Resolution, see the main [Blazing.Mvvm README](../../README.md#parameter-resolution).
