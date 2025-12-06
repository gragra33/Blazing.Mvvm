# BLAZMVVM0020: Route Parameter Binding Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0020
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: Route parameter missing corresponding property

## Description

Detects route parameters in `@page` directives that don't have corresponding `[Parameter]` properties in the View or `[ViewParameter]` properties in the ViewModel, preventing proper parameter binding.

## Examples

### ❌ Incorrect

```csharp
@page "/product/{id}"
@inherits MvvmComponentBase<ProductViewModel>
// ⚠️ Warning: Missing binding for route parameter 'id'

[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    // Missing [ViewParameter] for 'id'
}
```

### ✅ Correct (ViewModel Parameter)

```csharp
@page "/product/{id}"
@inherits MvvmComponentBase<ProductViewModel>

[ViewModelDefinition]
public class ProductViewModel : ViewModelBase
{
    [ViewParameter]
    public int Id { get; set; } // ? Bound to route
}
```

### ✅ Correct (View Parameter)

```csharp
@page "/product/{id}"
@inherits MvvmComponentBase<ProductViewModel>

@code {
    [Parameter]
    public int Id { get; set; } // ? Bound to route
}
```

## Route Parameter Types

Supported type constraints:

- `{id:int}` - Integer
- `{id:guid}` - GUID
- `{id:bool}` - Boolean
- `{id:datetime}` - DateTime
- `{id:decimal}` - Decimal
- `{id}` - String (default)

## Benefits

- Prevents runtime binding errors
- Ensures all route parameters are captured
- Type-safe route parameter passing
- Better compile-time validation

## Related

- **[BLAZMVVM0004](BLAZMVVM0004.md)**: ViewParameter Attribute Analyzer
- **[BLAZMVVM0005](BLAZMVVM0005.md)**: Navigation Type Safety Analyzer
