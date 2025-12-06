# BLAZMVVM0017: RelayCommand Async Pattern Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0017
- **Category**: Blazing.Mvvm
- **Severity**: Warning
- **Title**: RelayCommand method should not be async void

## Description

Detects methods marked with `[RelayCommand]` attribute that are `async void`. Async void methods cannot be awaited, errors cannot be properly caught, and they don't work correctly with AsyncRelayCommand.

## Examples

### ❌ Incorrect

```csharp
public class MyViewModel : ViewModelBase
{
    [RelayCommand]
    private async void LoadData() // ⚠️ Warning: async void
    {
        await _service.LoadAsync();
    }
}
```

### ✅ Correct

```csharp
public class MyViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task LoadDataAsync() // ? async Task
    {
        await _service.LoadAsync();
    }
}
```

## Benefits

- Proper error handling with try/catch
- Can be awaited in unit tests
- Generates AsyncRelayCommand with CanExecute support
- Better debugability

## Related

- **[BLAZMVVM0012](BLAZMVVM0012.md)**: Command Pattern Analyzer
