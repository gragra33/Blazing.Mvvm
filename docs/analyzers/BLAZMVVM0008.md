# BLAZMVVM0008: Observable Property Usage

## Summary

Properties that notify the UI should use `[ObservableProperty]` attribute or properly implement property change notification.

## Severity

**Warning** - Missing notification prevents UI updates.

## Why This Rule Exists

-   Ensures UI updates when properties change
-   Enforces proper INPC (INotifyPropertyChanged) implementation
-   Prevents stale UI data
-   Promotes use of source-generated properties
-   Reduces boilerplate code

## ✅ DO: Correct Usage

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

// ✅ Correct: Using [ObservableProperty] with partial class
[ViewModelDefinition]
public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private bool _isAvailable;
}

// ✅ Correct: Manual SetProperty
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}

// ✅ Correct: Computed property (doesn't need notification)
[ViewModelDefinition]
public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullPrice))]
    private decimal _basePrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullPrice))]
    private decimal _tax;

    public decimal FullPrice => BasePrice + Tax;  // No [ObservableProperty] needed
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: [ObservableProperty] without partial class
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase  // Missing 'partial'!
{
    [ObservableProperty]  // Won't generate - class must be partial
    private string _name = string.Empty;
}

// ❌ Wrong: Regular property without notification
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    public string Name { get; set; }  // UI won't update!
}

// ❌ Wrong: Manual setter without SetProperty
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { _name = value; }  // Missing SetProperty - no notification!
    }
}

// ❌ Wrong: [ObservableProperty] on public property
[ViewModelDefinition]
public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty]  // Only works on private fields!
    public string Name { get; set; }
}
```

## How to Fix

### Add 'partial' Keyword

```csharp
// Before
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name = string.Empty;
}

// After
[ViewModelDefinition]
public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name = string.Empty;

    // Source generator creates: public string Name { get; set; }
}
```

### Convert to ObservableProperty

```csharp
// Before
public string Name { get; set; }

// After
[ObservableProperty]
private string _name = string.Empty;
```

### Use SetProperty

```csharp
// Before
private string _name = string.Empty;
public string Name
{
    get => _name;
    set { _name = value; }
}

// After
private string _name = string.Empty;
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}
```

## Benefits

### Automatic Property Generation

```csharp
[ObservableProperty]
private string _firstName = string.Empty;

// Source generator creates:
// public string FirstName
// {
//     get => _firstName;
//     set => SetProperty(ref _firstName, value);
// }
```

### Less Boilerplate

```csharp
// Before: ~10 lines per property
private string _name = string.Empty;
public string Name
{
    get => _name;
    set
    {
        if (_name != value)
        {
            _name = value;
            OnPropertyChanged();
        }
    }
}

// After: 2 lines per property
[ObservableProperty]
private string _name = string.Empty;
```

### Automatic UI Updates

```razor
@* UI automatically updates when property changes *@
<h3>@ViewModel.Name</h3>

@code {
    private void UpdateName()
    {
        ViewModel.Name = "New Name";  // UI updates automatically
    }
}
```

## Code Fix

This analyzer provides an automatic code fix that adds the `partial` keyword to the class declaration.

## Related Analyzers

-   **[BLAZMVVM0018](BLAZMVVM0018.md)**: NotifyPropertyChangedFor - For computed properties
-   **[BLAZMVVM0014](BLAZMVVM0014.md)**: StateHasChanged Overuse - For manual UI updates

## Additional Resources

-   [ObservableProperty Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty)
-   [INPC in Blazing.Mvvm](https://github.com/gragra33/Blazing.Mvvm#observable-properties)
