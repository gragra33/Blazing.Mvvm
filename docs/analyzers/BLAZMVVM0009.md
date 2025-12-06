# BLAZMVVM0009: Service Injection

## Summary

Services should be injected via constructor in ViewModels, not using `[Inject]` attribute.

## Severity

**Warning** - Using `[Inject]` in ViewModels is not the recommended pattern.

## Why This Rule Exists

-   ViewModels should use constructor injection, not property injection
-   Makes dependencies explicit and testable
-   Follows dependency injection best practices
-   `[Inject]` is for Blazor components, not ViewModels
-   Enables proper unit testing

## ✅ DO: Correct Usage

```csharp
// ✅ Correct: Constructor injection in ViewModel
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductViewModel> _logger;

    public ProductViewModel(
        IProductService productService,
        ILogger<ProductViewModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        Products = await _productService.GetAllAsync();
    }

    public List<Product> Products { get; set; } = [];
}

// ✅ Correct: [Inject] in Blazor components is fine
public class ProductPage : MvvmComponentBase<ProductViewModel>
{
    [Inject]
    public required NavigationManager Navigation { get; set; }
}
```

## ❌ DON'T: Incorrect Usage

```csharp
// ❌ Wrong: [Inject] in ViewModel
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    [Inject]  // Wrong! Use constructor injection
    public IProductService ProductService { get; set; } = null!;

    [Inject]  // Wrong! Use constructor injection
    public ILogger<ProductViewModel> Logger { get; set; } = null!;
}

// ❌ Wrong: Service locator pattern
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    private IProductService? _productService;

    protected override Task OnInitializedAsync()
    {
        // Anti-pattern: service locator
        _productService = ServiceProvider.GetService<IProductService>();
        return Task.CompletedTask;
    }
}

// ❌ Wrong: New-ing up services
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    private readonly ProductService _service = new ProductService();  // Wrong!
}
```

## How to Fix

### Convert to Constructor Injection

```csharp
// Before
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    [Inject]
    public IProductService ProductService { get; set; } = null!;

    [RelayCommand]
    private async Task LoadAsync()
    {
        await ProductService.LoadDataAsync();
    }
}

// After
[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    public ProductViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await _productService.LoadDataAsync();
    }
}
```

## Benefits

### Testability

```csharp
// Easy to test with constructor injection
[Fact]
public void Test_LoadProducts()
{
    var mockService = new Mock<IProductService>();
    var viewModel = new ProductViewModel(mockService.Object);

    // Test the ViewModel
}
```

### Explicit Dependencies

```csharp
// Constructor shows all dependencies at a glance
public ProductViewModel(
    IProductService productService,
    ILogger<ProductViewModel> logger,
    IMapper mapper)
{
    // Dependencies are clear
}
```

### Null Safety

```csharp
// Constructor injection ensures non-null
private readonly IProductService _productService;

public ProductViewModel(IProductService productService)
{
    _productService = productService;  // Never null
}

// vs property injection with nullability issues
[Inject]
public IProductService ProductService { get; set; } = null!;  // Suppression needed
```

## Service Registration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddViewModels();  // Registers ViewModels with DI
```

## Testing Example

```csharp
public class ProductViewModelTests
{
    [Fact]
    public async Task LoadProducts_Success()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Product> { new() { Id = 1 } });

        var viewModel = new ProductViewModel(mockService.Object);

        // Act
        await viewModel.LoadProductsCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(viewModel.Products);
    }
}
```

## Code Fix

This analyzer provides an automatic code fix that converts property injection to constructor injection.

## Related Analyzers

-   **[BLAZMVVM0001](BLAZMVVM0001.md)**: ViewModelBase Inheritance
-   **[BLAZMVVM0002](BLAZMVVM0002.md)**: ViewModelDefinition Attribute

## Additional Resources

-   [Dependency Injection in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection)
-   [Constructor Injection Best Practices](https://github.com/gragra33/Blazing.Mvvm#dependency-injection)
