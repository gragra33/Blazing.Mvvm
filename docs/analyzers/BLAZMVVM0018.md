# BLAZMVVM0018: NotifyPropertyChangedFor Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0018
- **Category**: Blazing.Mvvm
- **Severity**: Info
- **Title**: Missing NotifyPropertyChangedFor attribute

## Description

Detects computed/read-only properties that depend on observable properties but lack `[NotifyPropertyChangedFor]` attribute to notify UI when dependencies change.

## Examples

### ❌ Incorrect

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _firstName; // ? Info: Should notify FullName

    [ObservableProperty]
    private string _lastName; // ? Info: Should notify FullName

    public string FullName => $"{FirstName} {LastName}";
    // FullName won't update when FirstName/LastName change!
}
```

### ✅ Correct

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string _firstName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string _lastName;

    public string FullName => $"{FirstName} {LastName}";
    // FullName updates automatically when dependencies change
}
```

## Benefits

- UI automatically reflects computed property changes
- No manual StateHasChanged() calls needed
- Declarative dependency tracking
- Better performance with targeted updates

## Related

- **[BLAZMVVM0008](BLAZMVVM0008.md)**: Observable Property Analyzer
- **[BLAZMVVM0014](BLAZMVVM0014.md)**: StateHasChanged Overuse Analyzer
