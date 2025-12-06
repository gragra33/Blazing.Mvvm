# BLAZMVVM0012: Command Pattern

## Summary

Methods invoked from UI should be exposed as RelayCommands instead of direct method calls.

## Severity

**Info** - Suggestion to follow command pattern best practices.

## Why This Rule Exists

-   Enables proper async command execution with cancellation
-   Provides automatic CanExecute support
-   Improves testability
-   Supports command state management (IsRunning, etc.)
-   Follows MVVM command pattern
-   Better separation of concerns

## ✅ DO: Correct Usage

```csharp
// ✅ Correct: Using [RelayCommand]
[ViewModelDefinition]
public partial class ProductViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    public ProductViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _productService.SaveAsync(Product);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync()
    {
        await _productService.DeleteAsync(Product.Id);
    }

    private bool CanDelete() => Product.Id > 0;

    public Product Product { get; set; } = new();
}

// View usage
<button @onclick="ViewModel.SaveCommand">Save</button>
<button @onclick="ViewModel.DeleteCommand" disabled="@(!ViewModel.DeleteCommand.CanExecute(null))">
    Delete
</button>
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: Public methods called directly from UI
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    public async Task SaveAsync()  // Called directly - no command support!
    {
        await _productService.SaveAsync(Product);
    }

    public async Task DeleteAsync()  // No CanExecute support
    {
        await _productService.DeleteAsync(Product.Id);
    }
}

// View - direct method calls
<button @onclick="ViewModel.SaveAsync">Save</button>

// ❌ Wrong: Event handlers in code-behind
@code {
    private async Task OnSaveClicked()
    {
        await ViewModel.SaveAsync();  // Bypasses command infrastructure
    }
}

<button @onclick="OnSaveClicked">Save</button>

// ❌ Wrong: Mixing patterns
[ViewModelDefinition]
public partial class ProductViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task SaveAsync() { }  // Command

    public async Task DeleteAsync() { }  // Direct method - inconsistent!
}
```

## How to Fix

### Convert Method to Command

```csharp
// Before
public async Task SaveProductAsync()
{
    await _productService.SaveAsync(Product);
}

// In View
<button @onclick="ViewModel.SaveProductAsync">Save</button>

// After
[RelayCommand]
private async Task SaveProductAsync()
{
    await _productService.SaveAsync(Product);
}

// In View
<button @onclick="ViewModel.SaveProductCommand">Save</button>
```

### Add CanExecute Logic

```csharp
// Before
public async Task DeleteAsync()
{
    if (Product.Id > 0)
    {
        await _productService.DeleteAsync(Product.Id);
    }
}

// After
[RelayCommand(CanExecute = nameof(CanDelete))]
private async Task DeleteAsync()
{
    await _productService.DeleteAsync(Product.Id);
}

private bool CanDelete() => Product.Id > 0;

// In View - button automatically disables
<button @onclick="ViewModel.DeleteCommand"
        disabled="@(!ViewModel.DeleteCommand.CanExecute(null))">
    Delete
</button>
```

## Benefits

### Automatic Command Infrastructure

```csharp
[RelayCommand]
private async Task SaveAsync()
{
    await _productService.SaveAsync(Product);
}

// Generated:
// public IAsyncRelayCommand SaveCommand { get; }
// - Automatic IsRunning support
// - Cancellation token support
// - Exception handling
// - CanExecute support
```

### CanExecute Support

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync()
{
    await _productService.SaveAsync(Product);
}

private bool CanSave() => !string.IsNullOrWhiteSpace(Product.Name);

// View
<button @onclick="ViewModel.SaveCommand"
        disabled="@(!ViewModel.SaveCommand.CanExecute(null))">
    Save
</button>
```

### Cancellation Support

```csharp
[RelayCommand]
private async Task LoadDataAsync(CancellationToken cancellationToken)
{
    await _productService.LoadAsync(cancellationToken);
}

// Can be cancelled
ViewModel.LoadDataCommand.Cancel();
```

### IsRunning State

```razor
<button @onclick="ViewModel.LoadCommand" disabled="@ViewModel.LoadCommand.IsRunning">
    @(ViewModel.LoadCommand.IsRunning ? "Loading..." : "Load Data")
</button>
```

## Command Patterns

### Simple Command

```csharp
[RelayCommand]
private void DoSomething()
{
    // Synchronous action
}
```

### Async Command

```csharp
[RelayCommand]
private async Task LoadAsync()
{
    await Task.Delay(1000);
}
```

### With Parameter

```csharp
[RelayCommand]
private async Task DeleteAsync(int id)
{
    await _productService.DeleteAsync(id);
}

// View
<button @onclick="() => ViewModel.DeleteCommand.Execute(productId)">Delete</button>
```

### With CanExecute

```csharp
[RelayCommand(CanExecute = nameof(CanExecute))]
private async Task SaveAsync()
{
    await _productService.SaveAsync(Product);
}

private bool CanExecute() => Product.IsValid;
```

## Code Fix

This analyzer provides an automatic code fix that adds the `[RelayCommand]` attribute and makes the method private.

## Related Analyzers

-   **[BLAZMVVM0017](BLAZMVVM0017.md)**: RelayCommand Async Pattern
-   **[BLAZMVVM0001](BLAZMVVM0001.md)**: ViewModelBase Inheritance

## Additional Resources

-   [RelayCommand Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/relaycommand)
-   [Command Pattern in MVVM](https://github.com/gragra33/Blazing.Mvvm#commands)
