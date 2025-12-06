# BLAZMVVM0005: Navigation Type Safety

## Summary

Navigation calls must reference registered ViewModels with proper routes.

## Description

This analyzer validates that `NavigateTo` calls reference ViewModels that are properly registered with `[ViewModelDefinition]` and have associated routable components. This prevents runtime navigation failures.

## Severity

**Warning** - Invalid navigation targets cause runtime errors.

## Why This Rule Exists

-   Prevents runtime navigation failures
-   Ensures ViewModels are properly registered
-   Validates route existence at compile time
-   Improves type safety in navigation
-   Catches broken navigation links early

## ✅ DO: Correct Usage

```csharp
// ViewModel with proper registration
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ProductDetailsViewModel : ViewModelBase
{
    [ViewParameter]
    public int ProductId { get; set; }
}

// Component with route
@page "/products/{ProductId:int}"
@inherits MvvmComponentBase<ProductDetailsViewModel>

// Navigation - Type-safe
public class ProductListViewModel : ViewModelBase
{
    private readonly MvvmNavigationManager _navigation;

    public ProductListViewModel(MvvmNavigationManager navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void ViewProduct(int productId)
    {
        // ✅ Correct: Type-safe navigation to registered ViewModel
        _navigation.NavigateTo<ProductDetailsViewModel>(
            parameters: new { ProductId = productId });
    }
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: Navigating to unregistered ViewModel
public class ProductListViewModel : ViewModelBase
{
    [RelayCommand]
    private void ViewProduct()
    {
        // Error: UnregisteredViewModel not decorated with [ViewModelDefinition]
        _navigation.NavigateTo<UnregisteredViewModel>();
    }
}

public class UnregisteredViewModel : ViewModelBase { }

// ❌ Wrong: ViewModel without associated route
[ViewModelDefinition]
public class NoRouteViewModel : ViewModelBase { }

// Missing: No component with @page directive for this ViewModel

// ❌ Wrong: Using string-based navigation (type-unsafe)
[RelayCommand]
private void Navigate()
{
    // Avoid: No compile-time validation
    _navigationManager.NavigateTo("/some/path");
}
```

## How to Fix

### Register the ViewModel

```csharp
// Before
public class MyViewModel : ViewModelBase { }

_navigation.NavigateTo<MyViewModel>();  // Error

// After
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class MyViewModel : ViewModelBase { }

_navigation.NavigateTo<MyViewModel>();  // ✅ Valid
```

### Create Associated Component

```csharp
// ViewModel
[ViewModelDefinition]
public class MyViewModel : ViewModelBase { }

// Component with route
@page "/mypage"
@inherits MvvmComponentBase<MyViewModel>

<h3>My Page</h3>
```

### Use Type-Safe Navigation

```csharp
// Before
_navigationManager.NavigateTo("/products/123");

// After
_navigation.NavigateTo<ProductDetailsViewModel>(
    parameters: new { ProductId = 123 });
```

## Benefits

### Compile-Time Validation

```csharp
// Catches errors at compile time
_navigation.NavigateTo<InvalidViewModel>();  // Won't compile if not registered
```

### Refactoring Safety

```csharp
// Renaming ViewModel updates all navigation calls
_navigation.NavigateTo<ProductDetailsViewModel>();  // IDE refactoring works
```

### IntelliSense Support

```csharp
// Auto-completion shows only valid ViewModels
_navigation.NavigateTo<|>  // Lists registered ViewModels
```

## Navigation Patterns

### Simple Navigation

```csharp
[RelayCommand]
private void GoToHome()
{
    _navigation.NavigateTo<HomeViewModel>();
}
```

### With Parameters

```csharp
[RelayCommand]
private void ViewProduct(int id)
{
    _navigation.NavigateTo<ProductDetailsViewModel>(
        parameters: new { ProductId = id });
}
```

### With Query String

```csharp
[RelayCommand]
private void Search(string query)
{
    _navigation.NavigateTo<SearchViewModel>(
        parameters: new { Query = query });
}
```

## Code Fix

This analyzer provides an automatic code fix that adds the `[ViewModelDefinition]` attribute to the target ViewModel.

## Related Analyzers

-   **[BLAZMVVM0002](BLAZMVVM0002.md)**: ViewModelDefinition Attribute
-   **[BLAZMVVM0011](BLAZMVVM0011.md)**: MvvmNavLink Type Safety
-   **[BLAZMVVM0010](BLAZMVVM0010.md)**: Route-ViewModel Mapping

## Additional Resources

-   [MVVM Navigation Documentation](https://github.com/gragra33/Blazing.Mvvm?tab=readme-ov-file#mvvm-navigation)
-   [Blazor Routing](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing)
