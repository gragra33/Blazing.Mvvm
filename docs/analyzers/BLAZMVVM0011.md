# BLAZMVVM0011: MvvmNavLink Type Safety

## Summary

MvvmNavLink components must reference valid registered ViewModels.

## Severity

**Error** - Invalid ViewModel references cause runtime failures.

## Why This Rule Exists

-   Ensures type-safe navigation links
-   Validates ViewModel registration at compile time
-   Prevents broken navigation links
-   Catches configuration errors early
-   Improves reliability of navigation

## ✅ DO: Correct Usage

```razor
@* ✅ Correct: Reference registered ViewModel *@
<MvvmNavLink TViewModel="ProductDetailsViewModel"
             Parameters="@(new { ProductId = 123 })">
    View Product
</MvvmNavLink>

@* ✅ Correct: With CSS class *@
<MvvmNavLink TViewModel="HomeViewModel"
             Class="nav-link">
    Home
</MvvmNavLink>

@* ✅ Correct: Multiple parameters *@
<MvvmNavLink TViewModel="SearchViewModel"
             Parameters="@(new { Query = "laptop", Category = "electronics" })">
    Search Laptops
</MvvmNavLink>

@code {
    // ViewModels are properly registered
    [ViewModelDefinition]
    public class ProductDetailsViewModel : ViewModelBase
    {
        [ViewParameter]
        public int ProductId { get; set; }
    }

    [ViewModelDefinition]
    public class HomeViewModel : ViewModelBase { }

    [ViewModelDefinition]
    public class SearchViewModel : ViewModelBase
    {
        [ViewParameter]
        public string Query { get; set; } = string.Empty;

        [ViewParameter]
        public string Category { get; set; } = string.Empty;
    }
}
```

## ❌ DON'T: Incorrect Usage

```razor
@* ❌ Wrong: Unregistered ViewModel *@
<MvvmNavLink TViewModel="UnregisteredViewModel">
    Link
</MvvmNavLink>

@code {
    // Missing [ViewModelDefinition]!
    public class UnregisteredViewModel : ViewModelBase { }
}

@* ❌ Wrong: Not a ViewModel at all *@
<MvvmNavLink TViewModel="MyService">
    Link
</MvvmNavLink>

@code {
    public class MyService { }  // Not a ViewModel!
}

@* ❌ Wrong: ViewModel without route *@
<MvvmNavLink TViewModel="NoRouteViewModel">
    Link
</MvvmNavLink>

@code {
    [ViewModelDefinition]
    public class NoRouteViewModel : ViewModelBase { }

    // Missing: No component with @page directive!
}
```

## How to Fix

### Register the ViewModel

```csharp
// Before
public class MyViewModel : ViewModelBase { }

<MvvmNavLink TViewModel="MyViewModel">Link</MvvmNavLink>  // Error

// After
[ViewModelDefinition]
public class MyViewModel : ViewModelBase { }

<MvvmNavLink TViewModel="MyViewModel">Link</MvvmNavLink>  // ✅ Valid
```

### Create Associated Page

```csharp
// ViewModel
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase { }

// Component with route
@page "/products"
@inherits MvvmComponentBase<ProductViewModel>

<h3>Products</h3>

// Now link works
<MvvmNavLink TViewModel="ProductViewModel">Products</MvvmNavLink>
```

## Benefits

### Compile-Time Validation

```razor
@* Catches invalid references at compile time *@
<MvvmNavLink TViewModel="InvalidViewModel">  @* Error: Not registered *@
    Link
</MvvmNavLink>
```

### IntelliSense Support

```razor
@* Auto-completion shows only valid ViewModels *@
<MvvmNavLink TViewModel="|">  @* Lists registered ViewModels *@
```

### Refactoring Safety

```csharp
// Renaming ViewModel updates all MvvmNavLink references
[ViewModelDefinition]
public class ProductDetailsViewModel : ViewModelBase { }  // Rename here

// All uses update automatically
<MvvmNavLink TViewModel="ProductDetailsViewModel">  // Updates automatically
```

## Usage Patterns

### Simple Navigation

```razor
<MvvmNavLink TViewModel="HomeViewModel">
    Home
</MvvmNavLink>
```

### With Parameters

```razor
<MvvmNavLink TViewModel="ProductDetailsViewModel"
             Parameters="@(new { ProductId = product.Id })">
    View Details
</MvvmNavLink>
```

### With Styling

```razor
<MvvmNavLink TViewModel="ProfileViewModel"
             Class="btn btn-primary"
             ActiveClass="active">
    My Profile
</MvvmNavLink>
```

### In Navigation Menu

```razor
<nav>
    <MvvmNavLink TViewModel="HomeViewModel" Class="nav-link">
        <span class="oi oi-home"></span> Home
    </MvvmNavLink>

    <MvvmNavLink TViewModel="ProductListViewModel" Class="nav-link">
        <span class="oi oi-list"></span> Products
    </MvvmNavLink>

    <MvvmNavLink TViewModel="AboutViewModel" Class="nav-link">
        <span class="oi oi-info"></span> About
    </MvvmNavLink>
</nav>
```

## Code Fix

This analyzer provides an automatic code fix that adds the `[ViewModelDefinition]` attribute to the referenced ViewModel.

## Related Analyzers

-   **[BLAZMVVM0002](BLAZMVVM0002.md)**: ViewModelDefinition Attribute
-   **[BLAZMVVM0005](BLAZMVVM0005.md)**: Navigation Type Safety
-   **[BLAZMVVM0010](BLAZMVVM0010.md)**: Route-ViewModel Mapping

## Additional Resources

-   [MvvmNavLink Documentation](https://github.com/gragra33/Blazing.Mvvm#mvvmnavlink)
-   [Blazor NavLink](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing#navlink-component)
