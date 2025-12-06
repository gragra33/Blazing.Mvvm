# BLAZMVVM0013: MvvmOwningComponentBase Usage Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0013
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: Component should use MvvmOwningComponentBase for scoped services

## Description

This analyzer detects when a Blazor component uses `MvvmComponentBase<TViewModel>` but the ViewModel depends on scoped services (such as Entity Framework `DbContext`). In such cases, the component should inherit from `MvvmOwningComponentBase<TViewModel>` instead to create a service scope and ensure proper lifetime management of scoped dependencies.

## Problem

When a ViewModel injects scoped services like `DbContext`, using the standard `MvvmComponentBase` can lead to:

1. **Shared DbContext instances** across multiple components
2. **Connection leaks** and resource exhaustion
3. **Thread safety issues** with concurrent database operations
4. **Stale data** due to long-lived contexts

## Solution

Use `MvvmOwningComponentBase<TViewModel>` which creates a dedicated service scope for the component and its ViewModel, ensuring:

- Each component instance gets its own scoped service instances
- Proper disposal of scoped services when the component is disposed
- Thread-safe database operations
- Fresh data for each component render

## Examples

### ❌ Incorrect (Using MvvmComponentBase with scoped services)

```csharp
// ViewModel with DbContext dependency
public class ProductsViewModel : ViewModelBase
{
    private readonly MyDbContext _context;

    public ProductsViewModel(MyDbContext context) // Scoped service
    {
        _context = context;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await _context.Products.ToListAsync();
    }
}

// View - INCORRECT: Should use MvvmOwningComponentBase
@page "/products"
@inherits MvvmComponentBase<ProductsViewModel> // ⚠️ Warning BLAZMVVM0013

<h3>Products</h3>
```

### ✅ Correct (Using MvvmOwningComponentBase)

```csharp
// ViewModel with DbContext dependency
public class ProductsViewModel : ViewModelBase
{
    private readonly MyDbContext _context;

    public ProductsViewModel(MyDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await _context.Products.ToListAsync();
    }
}

// View - CORRECT: Uses MvvmOwningComponentBase
@page "/products"
@inherits MvvmOwningComponentBase<ProductsViewModel> // ✅ Correct

<h3>Products</h3>
```

## Common Scoped Services

The analyzer detects these common scoped service patterns:

- **Entity Framework Core**: `DbContext`, `Database`, `DbConnection`
- **Database connections**: `IDbConnection`, `SqlConnection`, `MySqlConnection`
- **Unit of Work pattern**: Interfaces/classes containing "UnitOfWork", "Repository"
- **Custom scoped services**: Services registered with `ServiceLifetime.Scoped`

## When to Use Each Base Class

| Scenario | Base Class | Reason |
|----------|-----------|--------|
| ViewModel uses only transient/singleton services | `MvvmComponentBase` | No service scope needed |
| ViewModel injects `DbContext` or database connections | `MvvmOwningComponentBase` | Creates owned scope for proper disposal |
| ViewModel uses scoped services | `MvvmOwningComponentBase` | Ensures proper service lifetime |
| No ViewModel | `ComponentBase` | Standard Blazor component |

## Service Registration Example

```csharp
// Program.cs or Startup.cs
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(connectionString)); // Registered as SCOPED by default

// ViewModels
builder.Services.AddBlazingMvvm(options =>
{
    options.HostAssembly = typeof(Program).Assembly;
});
```

## Benefits of MvvmOwningComponentBase

1. **Automatic scope management**: Creates and disposes service scope automatically
2. **Thread safety**: Each component gets its own DbContext instance
3. **Memory management**: Scoped services are disposed when component is disposed
4. **Best practice compliance**: Follows Microsoft's recommendations for scoped services in Blazor

## Additional Resources

- [Blazor Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection)
- [Entity Framework Core in Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core)
- [Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)

## Related Analyzers

- **[BLAZMVVM0003](BLAZMVVM0003.md)**: MvvmComponentBase Usage Analyzer
- **[BLAZMVVM0015](BLAZMVVM0015.md)**: Dispose Pattern Analyzer
- **[BLAZMVVM0009](BLAZMVVM0009.md)**: Service Injection Analyzer
