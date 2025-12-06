# BLAZMVVM0014: StateHasChanged Overuse Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0014
- **Category**: Blazing.Mvvm
- **Severity**: Info
- **Title**: StateHasChanged call may be unnecessary

## Description

This analyzer detects manual `StateHasChanged()` calls in ViewModels that may be unnecessary when using proper property notification patterns with `[ObservableProperty]` or `SetProperty`. The Blazing.MVVM framework automatically handles UI updates through the `INotifyPropertyChanged` interface.

## Problem

Calling `StateHasChanged()` manually in ViewModels when using proper property notification is:

1. **Redundant**: Property changes already trigger UI updates automatically
2. **Code smell**: Indicates misunderstanding of the MVVM pattern
3. **Performance**: Can cause unnecessary re-renders
4. **Maintenance**: Extra code to maintain without benefit

## Solution

Remove unnecessary `StateHasChanged()` calls and rely on the built-in property notification system:

- Use `[ObservableProperty]` attribute on backing fields
- Use `SetProperty` method for property setters
- Let the framework handle UI updates automatically via `INotifyPropertyChanged`

## Examples

### ❌ Incorrect (Unnecessary StateHasChanged)

```csharp
public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private decimal _price;

    public void UpdateProduct(string name, decimal price)
    {
        Name = name;     // Already triggers PropertyChanged
        Price = price;   // Already triggers PropertyChanged
        StateHasChanged(); // ℹ️ Info BLAZMVVM0014 - Unnecessary!
    }
}
```

```csharp
public class ProductViewModel : ViewModelBase
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value); // Already triggers PropertyChanged
    }

    public void UpdateName(string newName)
    {
        Name = newName;
        StateHasChanged(); // ℹ️ Info BLAZMVVM0014 - Redundant!
    }
}
```

### ✅ Correct (Let property notification work)

```csharp
public partial class ProductViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private decimal _price;

    public void UpdateProduct(string name, decimal price)
    {
        Name = name;   // ✅ Automatically notifies UI
        Price = price; // ✅ Automatically notifies UI
        // No StateHasChanged() needed!
    }
}
```

```csharp
public class ProductViewModel : ViewModelBase
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value); // ✅ Automatic notification
    }

    public void UpdateName(string newName)
    {
        Name = newName; // ? UI updates automatically
    }
}
```

## When StateHasChanged IS Needed

There are legitimate cases where `StateHasChanged()` might be necessary:

### 1. After Async Operations (in View code, not ViewModel)

```csharp
// In a Blazor component (not ViewModel)
protected override async Task OnInitializedAsync()
{
    await Task.Delay(1000);
    // May need StateHasChanged() here in component code
    StateHasChanged();
}
```

### 2. Direct Field Assignment (without property notification)

```csharp
public class ViewModel : ViewModelBase
{
    private string _name; // Direct field, not property

    public void Update(string name)
    {
        _name = name; // No property changed event
        StateHasChanged(); // ✅ Needed because no notification occurred
    }
}
```

### 3. External Event Handlers

```csharp
public class ViewModel : ViewModelBase
{
    public ViewModel(IExternalService service)
    {
        service.DataChanged += (s, e) =>
        {
            // External event, may need explicit notification
            StateHasChanged();
        };
    }
}
```

## How Property Notification Works

```csharp
// With [ObservableProperty]
public partial class ViewModel : ViewModelBase
{
    [ObservableProperty] // Source generator creates property
    private string _name;
    
    // Generated code automatically calls OnPropertyChanged("Name")
    // which raises PropertyChanged event
    // Blazing.MVVM components listen to PropertyChanged
    // and automatically call StateHasChanged()
}

// With SetProperty
public class ViewModel : ViewModelBase
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
        // SetProperty calls OnPropertyChanged internally
        // which triggers UI update automatically
    }
}
```

## Performance Considerations

### Unnecessary StateHasChanged Calls

```csharp
// ❌ BAD: Calls StateHasChanged 3 times unnecessarily
public void UpdateMultiple()
{
    FirstName = "John";
    StateHasChanged(); // Redundant
    
    LastName = "Doe";
    StateHasChanged(); // Redundant
    
    Age = 30;
    StateHasChanged(); // Redundant
}

// ✅ GOOD: Property notifications handle UI updates
public void UpdateMultiple()
{
    FirstName = "John"; // Auto-updates UI
    LastName = "Doe";   // Auto-updates UI
    Age = 30;           // Auto-updates UI
}
```

## Best Practices

### DO ✅

- Use `[ObservableProperty]` on backing fields
- Use `SetProperty` in property setters
- Trust the property notification system
- Let the framework handle UI updates

### DON'T ❌

- Call `StateHasChanged()` after setting observable properties
- Mix manual `StateHasChanged()` with property notification
- Override the automatic notification system unnecessarily

## Quick Reference

| Pattern | StateHasChanged Needed? | Why |
|---------|------------------------|-----|
| `[ObservableProperty]` | ✅ No | Auto-notification via PropertyChanged |
| `SetProperty(ref field, value)` | ✅ No | Auto-notification via PropertyChanged |
| Direct field assignment | ⚠️ Maybe | No PropertyChanged event |
| After `await` in ViewModel | ⚠️ Maybe | Async context switch |
| External event handler | ⚠️ Maybe | Outside property system |

## Related Analyzers

- **[BLAZMVVM0008](BLAZMVVM0008.md)**: Observable Property Analyzer
- **[BLAZMVVM0018](BLAZMVVM0018.md)**: NotifyPropertyChangedFor Analyzer
- **[BLAZMVVM0003](BLAZMVVM0003.md)**: MvvmComponentBase Usage Analyzer

## Additional Resources

- [INotifyPropertyChanged Interface](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged)
- [Blazor Component Lifecycle](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)
- [CommunityToolkit.Mvvm ObservableProperty](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty)
