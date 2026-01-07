# BLAZMVVM0020: Route Parameter Binding Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0020
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: Route parameter missing corresponding property

## Description

Detects route parameters in `@page` directives that don't have corresponding `[Parameter]` properties in the View or `[ViewParameter]` properties in the ViewModel, preventing proper parameter binding.

This analyzer supports all route parameter patterns including:
- Simple required parameters: `{id}`
- Optional parameters: `{id?}`
- Catch-all parameters: `{*path}`
- Constrained parameters: `{id:int}`
- Multi-parameter routes: `{userId}/posts/{postId}`

## Examples

### ❌ Incorrect - Missing Parameter

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

### ❌ Incorrect - Multi-Parameter Route Missing One Parameter

```csharp
@page "/users/{userId}/posts/{postId}"
@inherits MvvmComponentBase<UserPostViewModel>
// ⚠️ Warning: Missing binding for route parameter 'postId'

[ViewModelDefinition]
public class UserPostViewModel : ViewModelBase
{
    [ViewParameter]
    public string? UserId { get; set; } // ✓ Present
    
    // ⚠️ Missing [ViewParameter] for 'postId'
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
    public int Id { get; set; } // ✓ Bound to route
}
```

### ✅ Correct (View Parameter)

```csharp
@page "/product/{id}"
@inherits MvvmComponentBase<ProductViewModel>

@code {
    [Parameter]
    public int Id { get; set; } // ✓ Bound to route
}
```

### ✅ Correct (Multi-Parameter Route)

```csharp
@page "/users/{userId}/posts/{postId}"
@inherits MvvmComponentBase<UserPostViewModel>

[ViewModelDefinition]
public class UserPostViewModel : ViewModelBase
{
    [ViewParameter]
    public string? UserId { get; set; } // ✓ Bound to route
    
    [ViewParameter]
    public string? PostId { get; set; } // ✓ Bound to route
}
```

**Navigation with multi-parameter routes:**
```csharp
// Navigate with slash-separated values for multiple parameters
_navigationManager.NavigateTo<UserPostViewModel>("1/101");
// Result: /users/1/posts/101

// Parameters are automatically substituted in order
_navigationManager.NavigateTo<UserPostViewModel>("42/789?filter=active");
// Result: /users/42/posts/789?filter=active
```

### ✅ Correct (Optional Parameter)

```csharp
@page "/items/{id?}"
@inherits MvvmComponentBase<ItemViewModel>

[ViewModelDefinition]
public class ItemViewModel : ViewModelBase
{
    [ViewParameter]
    public string? Id { get; set; } // ✓ Optional route parameter
}
```

### ✅ Correct (Catch-All Parameter)

```csharp
@page "/docs/{*path}"
@inherits MvvmComponentBase<DocsViewModel>

[ViewModelDefinition]
public class DocsViewModel : ViewModelBase
{
    [ViewParameter]
    public string? Path { get; set; } // ✓ Captures remaining path segments
}
```

## Route Parameter Types

### Supported Type Constraints

- `{id:int}` - Integer
- `{id:guid}` - GUID
- `{id:bool}` - Boolean
- `{id:datetime}` - DateTime
- `{id:decimal}` - Decimal
- `{id:double}` - Double
- `{id:float}` - Float
- `{id:long}` - Long
- `{id}` - String (default)

### Supported Parameter Modifiers

- **Required**: `{id}` - Parameter must be present in the URL
- **Optional**: `{id?}` - Parameter may be omitted from the URL
- **Catch-all**: `{*path}` - Captures all remaining path segments

## Parameter Resolution

The analyzer validates that route parameters are bound in either:

1. **ViewModel** - Using `[ViewParameter]` attribute (recommended for MVVM pattern)
2. **View** - Using `[Parameter]` attribute in the `@code` block

For multi-parameter routes, the `MvvmNavigationManager` automatically substitutes parameters using slash-separated values:

```csharp
// Template: /users/{userId}/posts/{postId}
NavigateTo<UserPostViewModel>("1/101");
// Resolves to: /users/1/posts/101

// Template: /api/{version}/users/{userId}
NavigateTo<ApiViewModel>("v2/42");
// Resolves to: /api/v2/users/42
```

Query strings are preserved and appended:
```csharp
NavigateTo<UserPostViewModel>("1/101?filter=recent&sort=desc");
// Resolves to: /users/1/posts/101?filter=recent&sort=desc
```

## Benefits

- ✓ Prevents runtime binding errors
- ✓ Ensures all route parameters are captured
- ✓ Type-safe route parameter passing
- ✓ Better compile-time validation
- ✓ Supports complex multi-parameter routes
- ✓ Works with optional and catch-all parameters
- ✓ Validates across View and ViewModel properties

## Common Scenarios

### Single Parameter Route
```csharp
@page "/users/{userId}"
// Requires: [ViewParameter] public string? UserId { get; set; }
```

### Multiple Parameters
```csharp
@page "/users/{userId}/posts/{postId}"
// Requires: 
// [ViewParameter] public string? UserId { get; set; }
// [ViewParameter] public string? PostId { get; set; }
```

### Constrained Parameter
```csharp
@page "/orders/{orderId:int}"
// Requires: [ViewParameter] public int OrderId { get; set; }
```

### Optional Parameter
```csharp
@page "/search/{term?}"
// Requires: [ViewParameter] public string? Term { get; set; }
```

### Catch-All Parameter
```csharp
@page "/files/{*filePath}"
// Requires: [ViewParameter] public string? FilePath { get; set; }
```

## How It Works

The analyzer:
1. Scans `.razor` files for `@page` directives
2. Extracts all route parameters using regex pattern matching
3. Identifies the component type from the `.razor` file name
4. Prioritizes components that inherit from `MvvmComponentBase` or related base classes
5. Extracts the ViewModel type from the `@inherits` directive if needed
6. Checks for matching `[Parameter]` properties in the View
7. Checks for matching `[ViewParameter]` properties in the ViewModel (including inherited properties)
8. Reports warnings for any route parameters without corresponding properties

## Related

- **[BLAZMVVM0004](BLAZMVVM0004.md)**: ViewParameter Attribute Analyzer
- **[BLAZMVVM0005](BLAZMVVM0005.md)**: Navigation Type Safety Analyzer
- **[MvvmNavigationManager](../components/MvvmNavigationManager.md)**: Route parameter substitution
