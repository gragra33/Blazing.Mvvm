# BLAZMVVM0007: Lifecycle Method Override

## Summary

Lifecycle methods in ViewModels should use the `override` keyword.

## Severity

**Info** - Helps catch common mistakes in lifecycle method declarations.

## Why This Rule Exists

-   Ensures lifecycle methods properly override base methods
-   Catches typos in method names (e.g., `OnInitializeAsync` vs `OnInitializedAsync`)
-   Provides IntelliSense support for lifecycle methods
-   Prevents methods from not being called due to missing `override`

## ✅ DO: Correct Usage

```csharp
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await RefreshDataAsync();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // First render logic
        }
        return Task.CompletedTask;
    }
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: Missing 'override' keyword
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    protected async Task OnInitializedAsync()  // Won't be called!
    {
        await LoadProductsAsync();
    }
}

// ❌ Wrong: Typo in method name
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()  // Wrong name!
    {
        await LoadProductsAsync();
    }
}

// ❌ Wrong: Incorrect signature
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    protected override void OnInitializedAsync()  // Should return Task!
    {
        LoadProducts();
    }
}
```

## How to Fix

### Add Override Keyword

```csharp
// Before
protected async Task OnInitializedAsync()
{
    await LoadDataAsync();
}

// After
protected override async Task OnInitializedAsync()
{
    await LoadDataAsync();
}
```

### Fix Method Name

```csharp
// Before
protected override async Task OnInitializeAsync()  // Wrong name
{
    await LoadDataAsync();
}

// After
protected override async Task OnInitializedAsync()  // Correct
{
    await LoadDataAsync();
}
```

## Benefits

### IntelliSense Support

```csharp
// With 'override', IDE shows available methods
protected override  // <-- IntelliSense shows: OnInitializedAsync, OnParametersSetAsync, etc.
```

### Compile-Time Validation

```csharp
// Compiler catches signature mismatches
protected override void OnInitializedAsync()  // Error: wrong return type
```

### Prevents Silent Failures

```csharp
// Without 'override', method never called - no warning!
protected async Task OnInitializedAsync() { }  // Analyzer catches this

// With 'override', proper behavior
protected override async Task OnInitializedAsync() { }  // ✅ Called correctly
```

## Available Lifecycle Methods

```csharp
public class MyViewModel : ViewModelBase
{
    // Initialization
    protected override async Task OnInitializedAsync() { }

    // Parameters changed
    protected override async Task OnParametersSetAsync() { }

    // After rendering
    protected override Task OnAfterRenderAsync(bool firstRender) { }

    // Navigation
    protected override Task OnNavigatedToAsync(NavigationContext context) { }
    protected override Task OnNavigatedFromAsync() { }
}
```

## Code Fix

This analyzer does not provide an automatic code fix. Add the `override` keyword manually.

## Related Analyzers

-   **[BLAZMVVM0001](BLAZMVVM0001.md)**: ViewModelBase Inheritance
-   **[BLAZMVVM0017](BLAZMVVM0017.md)**: RelayCommand Async Pattern

## Additional Resources

-   [ViewModel Lifecycle Documentation](https://github.com/gragra33/Blazing.Mvvm#viewmodel-lifecycle)
-   [Blazor Component Lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)
