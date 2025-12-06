# BLAZMVVM0010: Route-ViewModel Mapping

## Summary

Blazor pages with `@page` directive should have corresponding ViewModels.

## Severity

**Info** - Suggestion to follow MVVM pattern consistently.

## Why This Rule Exists

-   Promotes consistent MVVM pattern usage
-   Separates UI logic from business logic
-   Improves testability of page logic
-   Makes pages more maintainable
-   Encourages proper architecture

## ✅ DO: Correct Usage

```csharp
// ✅ Correct: Page with ViewModel
@page "/products"
@inherits MvvmComponentBase<ProductListViewModel>

<h3>Products</h3>

@foreach (var product in ViewModel.Products)
{
    <div>@product.Name - @product.Price</div>
}

<button @onclick="ViewModel.LoadCommand">Refresh</button>

// ViewModel
[ViewModelDefinition]
public partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [ObservableProperty]
    private List<Product> _products = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        Products = await _productService.GetAllAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: Page without ViewModel - logic in code-behind
@page "/products"
@inherits ComponentBase

<h3>Products</h3>

@foreach (var product in _products)
{
    <div>@product.Name - @product.Price</div>
}

<button @onclick="LoadProductsAsync">Refresh</button>

@code {
    [Inject]
    private IProductService ProductService { get; set; } = null!;

    private List<Product> _products = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        // Business logic in component - hard to test!
        _products = await ProductService.GetAllAsync();
        StateHasChanged();
    }
}

// ❌ Wrong: Simple page but with complex logic
@page "/checkout"
@inject ICartService CartService
@inject IPaymentService PaymentService

<button @onclick="ProcessPaymentAsync">Pay Now</button>

@code {
    // Complex business logic in component
    private async Task ProcessPaymentAsync()
    {
        var cart = await CartService.GetCurrentCartAsync();
        var result = await PaymentService.ProcessAsync(cart);
        // More logic...
    }
}
```

## How to Fix

### Extract Logic to ViewModel

```csharp
// Before: Logic in component
@page "/products"
@inject IProductService ProductService

<button @onclick="LoadAsync">Load</button>

@code {
    private List<Product> _products = [];

    private async Task LoadAsync()
    {
        _products = await ProductService.GetAllAsync();
    }
}

// After: Logic in ViewModel
@page "/products"
@inherits MvvmComponentBase<ProductListViewModel>

<button @onclick="ViewModel.LoadCommand">Load</button>

[ViewModelDefinition]
public partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [ObservableProperty]
    private List<Product> _products = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        Products = await _productService.GetAllAsync();
    }
}
```

## Benefits

### Testability

```csharp
// Easy to unit test ViewModel
[Fact]
public async Task LoadProducts_Success()
{
    var mockService = new Mock<IProductService>();
    var viewModel = new ProductListViewModel(mockService.Object);

    await viewModel.LoadCommand.ExecuteAsync(null);

    Assert.NotEmpty(viewModel.Products);
}
```

### Separation of Concerns

```csharp
// View: Only UI markup
@page "/products"
@inherits MvvmComponentBase<ProductListViewModel>

<ProductList Items="ViewModel.Products" />

// ViewModel: Business logic
public class ProductListViewModel : ViewModelBase
{
    // All business logic here
}
```

### Reusability

```csharp
// ViewModel can be reused in different contexts
public class ProductListViewModel : ViewModelBase
{
    // Can be used in pages, modals, embedded components, etc.
}
```

## When It's OK to Skip ViewModel

```csharp
// Simple static content pages don't need ViewModels
@page "/about"

<h3>About Us</h3>
<p>Static content...</p>

// Simple layout pages
@page "/admin"
@inherits LayoutComponentBase

<nav>Menu</nav>
<div>@Body</div>
```

## Code Fix

This analyzer provides an automatic code fix that creates a basic ViewModel and updates the component to inherit from `MvvmComponentBase<TViewModel>`.

## Related Analyzers

-   **[BLAZMVVM0003](BLAZMVVM0003.md)**: MvvmComponentBase Usage
-   **[BLAZMVVM0002](BLAZMVVM0002.md)**: ViewModelDefinition Attribute
-   **[BLAZMVVM0020](BLAZMVVM0020.md)**: Route Parameter Binding

## Additional Resources

-   [MVVM Pattern in Blazor](https://github.com/gragra33/Blazing.Mvvm#mvvm-pattern)
-   [Blazor Pages](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing)
