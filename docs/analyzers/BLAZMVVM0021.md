# BLAZMVVM0021: EventCallback Two-Way Binding Analyzer

## Diagnostic Information

- **Diagnostic ID**: BLAZMVVM0021
- **Category**: Blazing.Mvvm
- **Severity**: Info (manual pattern), Info (missing EventCallback), Warning (type mismatch)
- **Title**: Automatic two-way binding for EventCallback

## Description

This analyzer detects manual `PropertyChanged` event subscriptions used for two-way binding that can be replaced with automatic two-way binding introduced in Blazing.Mvvm v3.2.0. It also suggests adding `EventCallback<T>` parameters to enable automatic two-way binding when appropriate, and detects type mismatches that would prevent automatic binding from working.

## Problem

Manual two-way binding implementation has several issues:

1. **Boilerplate**: Requires 30+ lines of repetitive code per component
2. **Error-prone**: Easy to forget unsubscription, causing memory leaks
3. **Maintenance**: Every property requires manual event handling
4. **Code smell**: Violates DRY principle with repeated patterns

## Solution

Since v3.2.0, Blazing.Mvvm automatically handles two-way binding when:

1. Component has `[Parameter] EventCallback<T> {PropertyName}Changed` 
2. ViewModel property has `[ViewParameter]` attribute
3. Component inherits from `MvvmComponentBase<TViewModel>` or `MvvmOwningComponentBase<TViewModel>`

The framework automatically:
- Subscribes to ViewModel property changes
- Invokes the EventCallback when properties change
- Unsubscribes on component disposal (prevents memory leaks)

## Examples

### Diagnostic 1: Manual Two-Way Binding Detected (Obsolete Pattern)

#### ❌ Before (Manual Pattern - 35 lines)

```csharp
@using System.ComponentModel
@inherits MvvmComponentBase<CounterComponentViewModel>

<div class="card">
    <p>Count: <strong>@ViewModel.Counter</strong></p>
    <button @onclick="ViewModel.IncrementCommand.Execute">Increment</button>
</div>

@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged; // ℹ️ BLAZMVVM0021
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Counter) && 
            ViewModel.Counter != Counter)
        {
            await CounterChanged.InvokeAsync(ViewModel.Counter);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        base.Dispose(disposing);
    }
}
```

#### ✅ After Code Fix Applied (Automatic Binding - 9 lines)

```csharp
@inherits MvvmComponentBase<CounterComponentViewModel>

<div class="card">
    <p>Count: <strong>@ViewModel.Counter</strong></p>
    <button @onclick="ViewModel.IncrementCommand.Execute">Increment</button>
</div>

@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }
    
    // ✅ Automatic two-way binding handles everything!
}
```

**Improvement**: 70% code reduction (35 lines → 9 lines)

---

### Diagnostic 2: Missing EventCallback for Automatic Two-Way Binding

#### ❌ Incomplete Setup

```csharp
@inherits MvvmComponentBase<CounterComponentViewModel>

@code {
    [Parameter]
    public int Counter { get; set; } // ℹ️ BLAZMVVM0021: Could support two-way binding
    
    // Missing: EventCallback<int> CounterChanged
}
```

**ViewModel:**
```csharp
public partial class CounterComponentViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter; // Has [ViewParameter], ready for two-way binding
}
```

#### ✅ After Code Fix Applied

```csharp
@inherits MvvmComponentBase<CounterComponentViewModel>

@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; } // ✅ Added automatically
}
```

**Usage in Parent:**
```csharp
<CounterComponent @bind-Counter="@ViewModel.Counter" />
```

---

### Diagnostic 3: EventCallback Type Mismatch

#### ❌ Type Mismatch Prevents Automatic Binding

```csharp
@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<string> CounterChanged { get; set; } // ⚠️ BLAZMVVM0021: Type mismatch!
    // Should be: EventCallback<int>
}
```

#### ✅ After Code Fix Applied

```csharp
@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; } // ✅ Corrected type
}
```

---

## How Automatic Two-Way Binding Works

### Requirements Checklist

For automatic two-way binding to work, all of these must be true:

- ✅ Component inherits from `MvvmComponentBase<TViewModel>` or `MvvmOwningComponentBase<TViewModel>`
- ✅ Component has `[Parameter] T PropertyName { get; set; }`
- ✅ Component has `[Parameter] EventCallback<T> PropertyNameChanged { get; set; }`
- ✅ ViewModel property has `[ViewParameter]` attribute
- ✅ Types match (`T` is the same in parameter and EventCallback)

### Runtime Behavior

```csharp
// Component initialization:
// 1. TwoWayBindingHelper scans for EventCallback<T> ending with "Changed"
// 2. Verifies ViewModel property has [ViewParameter]
// 3. Automatically subscribes to ViewModel.PropertyChanged

// When ViewModel property changes:
// 4. TwoWayBindingHelper receives PropertyChanged event
// 5. Checks if new value differs from component parameter
// 6. Invokes EventCallback<T> automatically

// Component disposal:
// 7. TwoWayBindingHelper unsubscribes from PropertyChanged
// 8. No memory leaks!
```

### Architecture

```
┌──────────────────────────────────────────┐
│ MvvmComponentBase<TViewModel>            │
│                                          │
│  ┌────────────────────────────────────┐  │
│  │ TwoWayBindingHelper                │  │
│  │  • Scans EventCallback<T> params   │  │
│  │  • Subscribes to PropertyChanged   │  │
│  │  • Invokes callbacks automatically │  │
│  │  • Auto-disposes subscriptions     │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
```

---

## Complete Working Example

### ViewModel

```csharp
[ViewModelDefinition]
public partial class CounterComponentViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter;

    [RelayCommand]
    private void IncrementCounter()
    {
        Counter++; // ✅ Automatically triggers two-way binding
    }

    [RelayCommand]
    private void DecrementCounter()
    {
        Counter--; // ✅ Automatically triggers two-way binding
    }
}
```

### Child Component (CounterComponent.razor)

```razor
@inherits MvvmComponentBase<CounterComponentViewModel>

<div class="card mb-3">
    <div class="card-header">
        <h4>CounterComponent (Inner Component)</h4>
    </div>
    <div class="card-body">
        <p>Current count: <strong>@ViewModel.Counter</strong></p>

        <div class="btn-group">
            <button class="btn btn-primary" 
                    @onclick="ViewModel.IncrementCounterCommand.Execute">
                Increment
            </button>
            <button class="btn btn-danger" 
                    @onclick="ViewModel.DecrementCounterCommand.Execute">
                Decrement
            </button>
        </div>
    </div>
</div>

@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }
    
    // ✅ Zero configuration - automatic two-way binding!
}
```

### Parent Component Usage

```razor
@page "/demo"
@inherits MvvmComponentBase<ParentViewModel>

<h1>Parameter Demo</h1>

<!-- ✅ Bind parent ViewModel.Counter to child component -->
<CounterComponent @bind-Counter="@ViewModel.Counter" />

<p>Parent sees counter value: <strong>@ViewModel.Counter</strong></p>

@code {
    // Parent ViewModel also has Counter property
}
```

**Result:** When child component increments/decrements, parent automatically updates!

---

## Benefits of Automatic Two-Way Binding

| Feature | Manual Pattern | Automatic (v3.2.0+) |
|---------|---------------|---------------------|
| **Lines of code** | 30+ lines | 9 lines |
| **Memory leaks** | ⚠️ Easy to forget cleanup | ✅ Auto-cleanup |
| **Type safety** | ⚠️ Manual type checking | ✅ Compile-time validation |
| **Boilerplate** | ❌ High | ✅ Minimal |
| **Maintenance** | ❌ Per-property setup | ✅ Convention-based |
| **Error-prone** | ⚠️ High | ✅ Low |

---

## Migration Guide (v3.1.0 → v3.2.0)

### Step 1: Identify Manual Subscriptions

Look for this pattern:

```csharp
protected override void OnInitialized()
{
    ViewModel.PropertyChanged += OnViewModelPropertyChanged;
}

private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    // Manual EventCallback invocation
}

protected override void Dispose(bool disposing)
{
    ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
}
```

### Step 2: Run BLAZMVVM0021 Analyzer

The analyzer will detect and suggest:

```
ℹ️ Info BLAZMVVM0021: Manual PropertyChanged subscription for 'Counter' is obsolete.
   Since v3.2.0, automatic two-way binding is available. Remove manual subscription code.
   
💡 Quick Fix: Remove manual PropertyChanged subscription (use automatic two-way binding)
```

### Step 3: Apply Code Fix

Click the lightbulb (💡) and select:  
**"Remove manual PropertyChanged subscription (use automatic two-way binding)"**

The code fix will:
1. ✅ Remove `PropertyChanged` subscription in `OnInitialized`
2. ✅ Remove event handler method
3. ✅ Remove unsubscription in `Dispose`
4. ✅ Remove `Dispose` method if now empty
5. ✅ Remove `using System.ComponentModel` if unused

### Step 4: Verify

Ensure you have:
- ✅ `[Parameter] T PropertyName { get; set; }`
- ✅ `[Parameter] EventCallback<T> PropertyNameChanged { get; set; }`
- ✅ ViewModel property with `[ViewParameter]`

Done! Automatic two-way binding now works.

---

## Advanced Scenarios

### Multiple Properties

```csharp
@code {
    // ✅ All automatically bound
    [Parameter]
    public string FirstName { get; set; }
    [Parameter]
    public EventCallback<string> FirstNameChanged { get; set; }

    [Parameter]
    public string LastName { get; set; }
    [Parameter]
    public EventCallback<string> LastNameChanged { get; set; }

    [Parameter]
    public int Age { get; set; }
    [Parameter]
    public EventCallback<int> AgeChanged { get; set; }
}
```

### Nullable Types

```csharp
@code {
    [Parameter]
    public string? OptionalText { get; set; }

    [Parameter]
    public EventCallback<string?> OptionalTextChanged { get; set; }
}
```

**ViewModel:**
```csharp
[ObservableProperty]
[property: ViewParameter]
private string? _optionalText;
```

### Complex Types

```csharp
@code {
    [Parameter]
    public PersonModel? SelectedPerson { get; set; }

    [Parameter]
    public EventCallback<PersonModel?> SelectedPersonChanged { get; set; }
}
```

**ViewModel:**
```csharp
[ObservableProperty]
[property: ViewParameter]
private PersonModel? _selectedPerson;
```

---

## Troubleshooting

### "Automatic binding not working"

**Check:**

1. ✅ ViewModel property has `[ViewParameter]` attribute
2. ✅ EventCallback name follows `{PropertyName}Changed` convention
3. ✅ Types match exactly (including nullability)
4. ✅ Component inherits from `MvvmComponentBase<TViewModel>`
5. ✅ Parent uses `@bind-PropertyName` syntax

### "Analyzer not detecting my manual pattern"

The analyzer detects subscriptions in `OnInitialized` that:
- Subscribe to `ViewModel.PropertyChanged`
- Have corresponding event handler method
- Invoke `EventCallback.InvokeAsync` in the handler

If your pattern differs, you may need manual refactoring.

### "Type mismatch warning"

Ensure:
```csharp
// ❌ Mismatch
[Parameter] public int Counter { get; set; }
[Parameter] public EventCallback<string> CounterChanged { get; set; } // Wrong!

// ✅ Match
[Parameter] public int Counter { get; set; }
[Parameter] public EventCallback<int> CounterChanged { get; set; } // Correct!
```

---

## Best Practices

### DO ✅

- Use automatic two-way binding for all `[ViewParameter]` properties
- Follow `{PropertyName}Changed` naming convention
- Match types exactly between parameter and EventCallback
- Let the framework handle subscriptions and cleanup

### DON'T ❌

- Manually subscribe to `PropertyChanged` for two-way binding
- Mix manual and automatic binding in the same component
- Forget to add `[ViewParameter]` to ViewModel properties
- Use different types for parameter and EventCallback

---

## Performance Considerations

### Memory Usage

```
Manual Pattern:        Automatic Pattern:
┌─────────────────┐   ┌─────────────────┐
│ Component       │   │ Component       │
│  + Subscription │   │  (no manual)    │
│  + Event Handler│   │                 │
│  + Dispose code │   │                 │
└─────────────────┘   └─────────────────┘
      ⚠️ Risk              ✅ Safe
  (easy to leak)     (auto-cleanup)
```

### Execution Overhead

Automatic two-way binding has **negligible** overhead:
- Subscription setup: One-time during component initialization
- Event handling: Direct EventCallback invocation (no reflection)
- Disposal: Automatic cleanup (no manual code needed)

---

## Related Analyzers

- **[BLAZMVVM0004](BLAZMVVM0004.md)**: ViewParameter Attribute Analyzer
- **[BLAZMVVM0003](BLAZMVVM0003.md)**: MvvmComponentBase Usage Analyzer
- **[BLAZMVVM0014](BLAZMVVM0014.md)**: StateHasChanged Overuse Analyzer

---

## Additional Resources

- [Blazing.Mvvm v3.2.0 Release Notes](https://github.com/gragra33/Blazing.Mvvm/releases/tag/v3.2.0)
- [Parameter Resolution Sample](https://github.com/gragra33/Blazing.Mvvm/tree/master/src/samples/ParameterResolution.Sample.Wasm)
- [Blazor Data Binding](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/data-binding)
- [EventCallback Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling)

---

## Summary

**BLAZMVVM0021** helps you:

✅ **Detect** obsolete manual two-way binding code  
✅ **Migrate** to automatic two-way binding with one click  
✅ **Reduce** boilerplate by 70% (30+ lines → 9 lines)  
✅ **Prevent** memory leaks from forgotten unsubscriptions  
✅ **Ensure** type safety at compile time  
