# BLAZMVVM0004: ViewParameter Attribute Usage

## Summary

ViewParameter attributes must be used on properties in ViewModels and have matching Component parameters.

## Description

This analyzer ensures that `[ViewParameter]` attributes are only used on ViewModel properties that have corresponding `[Parameter]` properties in the associated component. This maintains proper data flow between Views and ViewModels.

## Severity

**Warning** - Incorrect usage may cause runtime binding failures.

## Why This Rule Exists

- Ensures proper parameter binding between View and ViewModel
- Prevents runtime errors from missing parameter mappings
- Maintains data flow consistency
- Validates parameter synchronization

## ✅ DO: Correct Usage

```csharp
// Component
@inherits MvvmComponentBase<ProductViewModel>

@code {
    [Parameter]
    public int ProductId { get; set; }
    
    [Parameter]
    public string Category { get; set; } = string.Empty;
}

// ViewModel
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    [ViewParameter]
    public int ProductId { get; set; }
    
    [ViewParameter]
    public string Category { get; set; } = string.Empty;
    
    protected override async Task OnParametersSetAsync()
    {
        // ProductId and Category are automatically synced from component
        await LoadProductAsync(ProductId, Category);
    }
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: ViewParameter in non-ViewModel class
public class MyService
{
    [ViewParameter]  // Error: Not in a ViewModel
    public int Value { get; set; }
}

// ❌ Wrong: ViewParameter without matching Component parameter
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    [ViewParameter]  // Error: No matching [Parameter] in component
    public int ProductId { get; set; }
}

public class ProductComponent : MvvmComponentBase<ProductViewModel>
{
    // Missing [Parameter] public int ProductId { get; set; }
}

// ❌ Wrong: Name mismatch
public class ProductComponent : MvvmComponentBase<ProductViewModel>
{
    [Parameter]
    public int Id { get; set; }  // Different name
}

[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    [ViewParameter]
    public int ProductId { get; set; }  // Won't bind - names don't match
}
```

## How to Fix

### Add Matching Component Parameter

```csharp
// Before
[ViewModelDefinition]
public class MyViewModel : ViewModelBase
{
    [ViewParameter]
    public int Value { get; set; }
}

public class MyComponent : MvvmComponentBase<MyViewModel>
{
    // Missing parameter
}

// After
public class MyComponent : MvvmComponentBase<MyViewModel>
{
    [Parameter]
    public int Value { get; set; }
}
```

### Remove ViewParameter from Non-ViewModel

```csharp
// Before
public class MyService
{
    [ViewParameter]
    public int Value { get; set; }
}

// After
public class MyService
{
    public int Value { get; set; }
}
```

## Benefits

### Automatic Synchronization
```csharp
// Component parameter changes automatically sync to ViewModel
[Parameter]
public int ProductId { get; set; }

// ViewModel receives updated value
[ViewParameter]
public int ProductId { get; set; }
```

### Type Safety
```csharp
// Both must have matching types
[Parameter]
public int ProductId { get; set; }

[ViewParameter]
public int ProductId { get; set; }  // Same type required
```

## Code Fix

This analyzer provides an automatic code fix that adds the missing `[Parameter]` property to the component.

## Related Analyzers

- **[BLAZMVVM0003](BLAZMVVM0003.md)**: MvvmComponentBase Usage
- **[BLAZMVVM0020](BLAZMVVM0020.md)**: Route Parameter Binding

## Additional Resources

- [Parameter Binding Documentation](https://github.com/gragra33/Blazing.Mvvm#parameter-binding)
- [Blazor Component Parameters](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/#component-parameters)
