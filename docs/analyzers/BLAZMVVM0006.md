# BLAZMVVM0006: ViewModelKey Consistency

## Summary

ViewModelKey attribute values must be consistent with keys used in keyed navigation.

## Severity

**Warning** - Inconsistent keys cause runtime navigation failures.

## Why This Rule Exists

- Ensures navigation keys match between View and ViewModel
- Prevents runtime errors from key mismatches
- Maintains consistency in keyed service resolution
- Validates navigation configuration at compile time

## ✅ DO: Correct Usage

```csharp
// ViewModel with key
[ViewModelDefinition(Key = "ProductDetails", Lifetime = ServiceLifetime.Transient)]
public class ProductViewModel : ViewModelBase
{
    [ViewParameter]
    public int ProductId { get; set; }
}

// Component with matching key
[ViewModelKey("ProductDetails")]
public class ProductPage : MvvmComponentBase<ProductViewModel>
{
    [Parameter]
    public int ProductId { get; set; }
}

// Navigation using the same key
public class NavigationService
{
    public void NavigateToProduct(int id)
    {
        _navigation.NavigateTo("ProductDetails", new { ProductId = id });
    }
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: Key mismatch
[ViewModelDefinition(Key = "ProductDetails")]
public class ProductViewModel : ViewModelBase { }

[ViewModelKey("ProductDetail")]  // Missing 's' - won't match!
public class ProductPage : MvvmComponentBase<ProductViewModel> { }

// ❌ Wrong: Navigation uses different key
_navigation.NavigateTo("Product", parameters);  // Wrong key

// ❌ Wrong: Key defined but not used consistently
[ViewModelDefinition(Key = "MyKey")]
public class MyViewModel : ViewModelBase { }

// Component doesn't specify key - inconsistent
public class MyComponent : MvvmComponentBase<MyViewModel> { }
```

## How to Fix

### Match Keys Across All Uses

```csharp
// Before
[ViewModelDefinition(Key = "OrderList")]
public class OrderViewModel : ViewModelBase { }

[ViewModelKey("Orders")]  // Different!
public class OrderComponent : MvvmComponentBase<OrderViewModel> { }

// After - Consistent everywhere
[ViewModelDefinition(Key = "Orders")]
public class OrderViewModel : ViewModelBase { }

[ViewModelKey("Orders")]
public class OrderComponent : MvvmComponentBase<OrderViewModel> { }
```

## Benefits

### Compile-Time Validation
```csharp
// Catches key mismatches at compile time
[ViewModelKey("WrongKey")]  // Analyzer warning!
public class MyComponent : MvvmComponentBase<MyViewModel> { }
```

### Refactoring Safety
```csharp
// Change key in one place, analyzer shows all places to update
[ViewModelDefinition(Key = "NewKey")]  // Update here
public class MyViewModel : ViewModelBase { }

[ViewModelKey("OldKey")]  // Analyzer warns here!
public class MyComponent : MvvmComponentBase<MyViewModel> { }
```

## Code Fix

This analyzer does not provide an automatic code fix. Manually update the key to match.

## Related Analyzers

- **[BLAZMVVM0002](BLAZMVVM0002.md)**: ViewModelDefinition Attribute
- **[BLAZMVVM0005](BLAZMVVM0005.md)**: Navigation Type Safety

## Additional Resources

- [Keyed Services Documentation](https://github.com/gragra33/Blazing.Mvvm#keyed-services)
