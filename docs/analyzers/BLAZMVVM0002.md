# BLAZMVVM0002: ViewModelDefinition Attribute

## Summary

ViewModels must be decorated with the `[ViewModelDefinition]` attribute to ensure proper dependency injection registration.

## Description

This analyzer ensures that ViewModel classes that inherit from `ViewModelBase` are decorated with the `[ViewModelDefinition]` attribute. This attribute is essential for automatic service registration in the dependency injection container.

## Severity

**Error** - This is a critical issue that will prevent the ViewModel from being properly registered and resolved.

## Why This Rule Exists

-   Ensures ViewModels are registered in the DI container
-   Specifies the service lifetime (Transient, Scoped, Singleton)
-   Enables automatic ViewModel resolution in Views
-   Prevents runtime errors when navigating to Views

## ✅ DO: Correct Usage

```csharp
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

// ✅ Correct: Transient lifetime (default, recommended for most ViewModels)
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ProductViewModel : ViewModelBase
{
    public string Name { get; set; }
}

// ✅ Correct: Singleton lifetime (for app-wide state)
[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
public class AppStateViewModel : ViewModelBase
{
    public bool IsAuthenticated { get; set; }
}

// ✅ Correct: Scoped lifetime (for request-scoped data)
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public class ShoppingCartViewModel : ViewModelBase
{
    private readonly List<Item> _items = new();
}

// ✅ Correct: Abstract base ViewModels don't need the attribute
public abstract class BaseViewModel : ViewModelBase
{
    // Shared logic
}

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ConcreteViewModel : BaseViewModel
{
    // Specific implementation
}
```

## ❌ DON'T: Incorrect Usage

```csharp
using Blazing.Mvvm.ComponentModel;

// ❌ Wrong: Missing ViewModelDefinition attribute
public class ProductViewModel : ViewModelBase
{
    public string Name { get; set; }
}

// ❌ Wrong: Wrong attribute used
[Serializable]
public class UserViewModel : ViewModelBase
{
    public string Username { get; set; }
}

// ❌ Wrong: Manual service registration conflicts
// In Program.cs
services.AddTransient<OrderViewModel>(); // Don't do this!

// In ViewModel file
[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
public class OrderViewModel : ViewModelBase
{
    // This creates a conflict!
}
```

## How to Fix

### Add the ViewModelDefinition Attribute

```csharp
// Before
using Blazing.Mvvm.ComponentModel;

public class ProductViewModel : ViewModelBase
{
    public string Name { get; set; }
}

// After
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ProductViewModel : ViewModelBase
{
    public string Name { get; set; }
}
```

## Choosing the Right Lifetime

### Transient (Recommended Default)

-   New instance created each time the ViewModel is resolved
-   Use for most ViewModels
-   Prevents state leakage between navigations

```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ProductDetailsViewModel : ViewModelBase { }
```

### Scoped

-   Single instance per scope (request/circuit in Blazor)
-   Use for shopping carts, user sessions
-   Shared within the same user session

```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public class ShoppingCartViewModel : ViewModelBase { }
```

### Singleton

-   Single instance for the entire application lifetime
-   Use for app-wide state, settings
-   Be careful with thread safety

```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
public class AppSettingsViewModel : ViewModelBase { }
```

## Code Fix

This analyzer provides an automatic code fix that:

1. Adds `[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]` attribute
2. Adds the necessary using directives

## Related Analyzers

-   **[BLAZMVVM0001](BLAZMVVM0001.md)**: ViewModelBase Inheritance - Ensures proper base class
-   **[BLAZMVVM0013](BLAZMVVM0013.md)**: MvvmOwningComponentBase Usage - For ViewModels with scoped dependencies

## Additional Resources

-   [Dependency Injection in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection)
-   [Service Lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
