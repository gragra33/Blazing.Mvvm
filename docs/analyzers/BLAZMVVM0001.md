# BLAZMVVM0001: ViewModelBase Inheritance

## Summary

ViewModels should inherit from `ViewModelBase` to ensure proper MVVM pattern implementation and lifecycle management.

## Description

This analyzer ensures that classes named with the "ViewModel" suffix inherit from the `Blazing.Mvvm.ComponentModel.ViewModelBase` class. This is essential for proper integration with the Blazing.MVVM framework's lifecycle management, property change notification, and navigation features.

## Severity

**Error** - This is a critical issue that will prevent proper MVVM functionality.

## Why This Rule Exists

-   Ensures proper INotifyPropertyChanged implementation
-   Provides lifecycle hooks (OnInitialized, OnNavigatedTo, etc.)
-   Enables automatic property change notifications
-   Integrates with the framework's dependency injection
-   Provides access to navigation and messenger services

## ✅ DO: Correct Usage

```csharp
using Blazing.Mvvm.ComponentModel;

// ✅ Correct: Inherits from ViewModelBase
public class ProductViewModel : ViewModelBase
{
    public string Name { get; set; }
}

// ✅ Correct: Inherits from ObservableObject (CommunityToolkit.Mvvm)
using CommunityToolkit.Mvvm.ComponentModel;

public partial class UserViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username;
}

// ✅ Correct: Inherits from custom base that extends ViewModelBase
public class CustomViewModelBase : ViewModelBase
{
    // Custom shared logic
}

public class OrderViewModel : CustomViewModelBase
{
    // Specific logic
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: No base class
public class ProductViewModel
{
    public string Name { get; set; }
}

// ❌ Wrong: Implements INotifyPropertyChanged manually
using System.ComponentModel;

public class UserViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}

// ❌ Wrong: Inherits from ComponentBase (this is for Views, not ViewModels)
using Microsoft.AspNetCore.Components;

public class OrderViewModel : ComponentBase
{
    // This is mixing View and ViewModel concerns
}
```

## How to Fix

### Option 1: Inherit from ViewModelBase

```csharp
// Before
public class ProductViewModel
{
    public string Name { get; set; }
}

// After
using Blazing.Mvvm.ComponentModel;

public class ProductViewModel : ViewModelBase
{
    public string Name { get; set; }
}
```

### Option 2: Use ObservableObject from CommunityToolkit.Mvvm

```csharp
// Before
public class ProductViewModel
{
    private string _name;
    public string Name
    {
        get => _name;
        set => _name = value;
    }
}

// After
using CommunityToolkit.Mvvm.ComponentModel;

public partial class ProductViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name;
}
```

## Code Fix

This analyzer provides an automatic code fix that:

1. Adds `: ViewModelBase` to the class declaration
2. Adds the necessary `using Blazing.Mvvm.ComponentModel;` directive

## Related Analyzers

-   **[BLAZMVVM0002](BLAZMVVM0002.md)**: ViewModelDefinition Attribute - Ensures proper DI registration
-   **[BLAZMVVM0008](BLAZMVVM0008.md)**: Observable Property - Validates property notification patterns

## Additional Resources

-   [ViewModelBase Documentation](https://github.com/gragra33/Blazing.Mvvm?tab=readme-ov-file#create-a-viewmodel-inheriting-the-viewmodelbase-class)
-   [MVVM Pattern Best Practices](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm)
