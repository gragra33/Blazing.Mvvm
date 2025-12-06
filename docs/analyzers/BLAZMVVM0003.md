# BLAZMVVM0003: MvvmComponentBase Usage

## Summary

Blazor components with ViewModel properties should inherit from `MvvmComponentBase<TViewModel>` to ensure proper View-ViewModel binding.

## Description

This analyzer detects Blazor components that have a ViewModel property but don't inherit from `MvvmComponentBase<TViewModel>`. Using the proper base class ensures automatic ViewModel injection, lifecycle management, and proper state synchronization.

## Severity

**Warning** - This may work but doesn't follow Blazing.MVVM best practices.

## Why This Rule Exists

-   Provides automatic ViewModel injection
-   Handles ViewModel lifecycle (initialization, disposal)
-   Enables proper parameter binding between View and ViewModel
-   Synchronizes component and ViewModel state
-   Reduces boilerplate code

## ✅ DO: Correct Usage

```csharp
using Blazing.Mvvm.Components;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

// ✅ Correct: Inherits from MvvmComponentBase<TViewModel>
@inherits MvvmComponentBase<ProductViewModel>

<h3>@ViewModel.ProductName</h3>

@code {
    // ViewModel is automatically injected and available
}

// ViewModel
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ProductViewModel : ViewModelBase
{
    public string ProductName { get; set; } = "Sample Product";
}

// ✅ Correct: For components needing scoped services
@inherits MvvmOwningComponentBase<OrderViewModel>

<h3>Orders</h3>

// ViewModel with DbContext
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class OrderViewModel : ViewModelBase
{
    private readonly MyDbContext _context;

    public OrderViewModel(MyDbContext context)
    {
        _context = context;
    }
}
```

## ❌ DON'T: Incorrect Usage

```csharp
using Microsoft.AspNetCore.Components;

// ❌ Wrong: ComponentBase with manual ViewModel injection
@inherits ComponentBase

<h3>@ViewModel.ProductName</h3>

@code {
    [Inject]
    private ProductViewModel ViewModel { get; set; } = null!;

    // Missing lifecycle management
    // Missing automatic parameter binding
    // Missing proper disposal
}

// ❌ Wrong: Manual property initialization
@inherits ComponentBase

<h3>@_viewModel.ProductName</h3>

@code {
    private ProductViewModel _viewModel = new();

    // Not using DI
    // No lifecycle management
    // Breaks testability
}

// ❌ Wrong: Mixing View logic in ComponentBase
@inherits ComponentBase

<h3>@ProductName</h3>

@code {
    [Parameter]
    public string ProductName { get; set; }

    private async Task LoadDataAsync()
    {
        // Business logic in the View - bad MVVM!
        var data = await _httpClient.GetFromJsonAsync<Product>("api/products");
        ProductName = data.Name;
    }
}
```

## How to Fix

### Replace ComponentBase with MvvmComponentBase

```csharp
// Before
@inherits ComponentBase

<h3>@ViewModel.Title</h3>

@code {
    [Inject]
    private ProductViewModel ViewModel { get; set; } = null!;
}

// After
@inherits MvvmComponentBase<ProductViewModel>

<h3>@ViewModel.Title</h3>

@code {
    // ViewModel automatically injected!
    // No need for [Inject] attribute
}
```

### Move Business Logic to ViewModel

```csharp
// Before - Logic in View
@inherits ComponentBase

<button @onclick="SaveAsync">Save</button>

@code {
    [Inject]
    private HttpClient Http { get; set; }

    private async Task SaveAsync()
    {
        // Business logic in View - bad!
        await Http.PostAsJsonAsync("api/products", product);
    }
}

// After - Logic in ViewModel
@inherits MvvmComponentBase<ProductViewModel>

<button @onclick="ViewModel.SaveCommand">Save</button>

// ViewModel
[ViewModelDefinition]
public partial class ProductViewModel : ViewModelBase
{
    private readonly HttpClient _http;

    public ProductViewModel(HttpClient http)
    {
        _http = http;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _http.PostAsJsonAsync("api/products", Product);
    }

    public Product Product { get; set; } = new();
}
```

## Benefits of MvvmComponentBase

### Automatic ViewModel Injection

```csharp
@inherits MvvmComponentBase<MyViewModel>

// ViewModel property is automatically available
<h3>@ViewModel.Title</h3>
```

### Lifecycle Synchronization

```csharp
public class MyViewModel : ViewModelBase
{
    protected override async Task OnInitializedAsync()
    {
        // Called when component initializes
        await LoadDataAsync();
    }
}
```

### Parameter Binding

```csharp
// View
@inherits MvvmComponentBase<ProductViewModel>

@code {
    [Parameter]
    public int ProductId { get; set; }
    // Automatically synced to ViewModel.ProductId
}

// ViewModel
public class ProductViewModel : ViewModelBase
{
    [ViewParameter]
    public int ProductId { get; set; }
}
```

## Code Fix

This analyzer provides an automatic code fix that:

1. Replaces `ComponentBase` with `MvvmComponentBase<TViewModel>`
2. Removes redundant `[Inject]` attributes from ViewModel properties
3. Adds necessary using directives

## Related Analyzers

-   **[BLAZMVVM0013](BLAZMVVM0013.md)**: MvvmOwningComponentBase Usage - For scoped service dependencies
-   **[BLAZMVVM0004](BLAZMVVM0004.md)**: ViewParameter Attribute - For parameter binding
-   **[BLAZMVVM0001](BLAZMVVM0001.md)**: ViewModelBase Inheritance - Ensures ViewModel base class

## Additional Resources

-   [MvvmComponentBase Documentation](https://github.com/gragra33/Blazing.Mvvm?tab=readme-ov-file#create-a-viewmodel-inheriting-the-viewmodelbase-class)
-   [Blazor Component Lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)
