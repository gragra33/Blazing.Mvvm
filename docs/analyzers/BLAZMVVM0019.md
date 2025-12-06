# BLAZMVVM0019: CascadingParameter vs Inject Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0019
- **Category**: Blazing.Mvvm
- **Severity**: Info
- **Title**: Use Inject instead of CascadingParameter for services

## Description

Detects `[CascadingParameter]` attributes used for dependency injection services when `[Inject]` would be more appropriate and semantically clearer.

## Examples

### ❌ Less Clear

```csharp
public class MyComponent : ComponentBase
{
    [CascadingParameter]
    public IMyService MyService { get; set; } // ℹ️ Info: Use Inject
}
```

### ✅ Clear Intent

```csharp
public class MyComponent : ComponentBase
{
    [Inject]
    public IMyService MyService { get; set; } // ✅ Clear DI intent
}
```

## When to Use Each

| Attribute | Use For |
|-----------|---------|
| `[Inject]` | Services from DI container (ILogger, HttpClient, etc.) |
| `[CascadingParameter]` | Component state cascaded from parent (Theme, AppState) |

## Benefits

- Clear semantic meaning
- Better code readability
- Consistent with Blazor best practices
- Easier to understand data flow

## Related

- **[BLAZMVVM0009](BLAZMVVM0009.md)**: Service Injection Analyzer
