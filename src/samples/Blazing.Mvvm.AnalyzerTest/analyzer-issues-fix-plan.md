# Blazing.Mvvm Analyzer Issues - Fix Plan

**Project:** Blazing.Mvvm.AnalyzerTest  
**Date:** 2024  
**Purpose:** Detailed fix plan for triggering all 21 BLAZMVVM analyzers

---

## Overview

This document outlines specific fixes to trigger all 21 Blazing.Mvvm analyzers. Fixes are organized by priority and category.

**Current Status:**
- ? Working: 5 analyzers (BLAZMVVM0001, 0002, 0015, 0016, 0017)
- ?? Partial: 1 analyzer (BLAZMVVM0007)
- ? Not Working: 15 analyzers

**Goal:** Achieve 100% analyzer trigger rate (21/21)

---

## Priority 1: Quick Wins (3 Analyzers)

### Fix 1.1: BLAZMVVM0011 - MvvmNavLink Type Safety

**Current Issue:** RZ10012 Razor warning prevents analyzer from running  
**Root Cause:** Missing `@using Blazing.Mvvm.Components` directive  
**Severity:** High - Easy fix, should trigger immediately

**Implementation:**

1. **Update `Components/_Imports.razor`:**
   ```razor
   @using System.Net.Http
   @using System.Net.Http.Json
   @using Microsoft.AspNetCore.Components.Forms
   @using Microsoft.AspNetCore.Components.Routing
   @using Microsoft.AspNetCore.Components.Web
   @using Microsoft.AspNetCore.Components.Web.Virtualization
   @using Microsoft.JSInterop
   @using Blazing.Mvvm.AnalyzerTest
   @using Blazing.Mvvm.AnalyzerTest.Components
   @using Blazing.Mvvm.Components              <!-- ADD THIS -->
   @using Blazing.Mvvm.ComponentModel           <!-- ADD THIS -->
   ```

2. **Verify Test011.razor** still has the violation:
   ```razor
   <MvvmNavLink TViewModel="UnregisteredTargetViewModel">
       Link to Unregistered ViewModel
   </MvvmNavLink>
   ```

**Expected Result:** BLAZMVVM0011 warning should appear in build output

**Test Method:**
- Build project
- Check Error List for BLAZMVVM0011
- Verify message: "MvvmNavLink references unregistered ViewModel"

---

### Fix 1.2: BLAZMVVM0014 - StateHasChanged Overuse

**Current Issue:** StateHasChanged() is not accessible in ViewModels  
**Root Cause:** Analyzer targets components, not ViewModels  
**Severity:** High - Test is in wrong location

**Implementation:**

1. **Create new component `Components/Pages/Test014.razor`:**
   ```razor
   @page "/test014"
   @using Blazing.Mvvm.AnalyzerTest.ViewModels
   @using Blazing.Mvvm.Components
   
   @inherits MvvmComponentBase<Test014ViewModel>
   
   <PageTitle>Test014 - StateHasChanged Overuse</PageTitle>
   
   <h1>BLAZMVVM0014: StateHasChanged Overuse</h1>
   <p>First Name: @ViewModel.FirstName</p>
   <p>Last Name: @ViewModel.LastName</p>
   
   <button @onclick="UpdateName">Update Name</button>
   
   @code {
       private void UpdateName()
       {
           ViewModel.FirstName = "John";   // Already triggers PropertyChanged
           ViewModel.LastName = "Doe";     // Already triggers PropertyChanged
           StateHasChanged();   // ?? Should trigger BLAZMVVM0014 - Unnecessary!
       }
   }
   ```

2. **Keep Test014ViewModel.cs** for reference but remove StateHasChanged comment

**Expected Result:** BLAZMVVM0014 info diagnostic

**Test Method:**
- Build project
- Check Error List for BLAZMVVM0014
- Verify message about unnecessary StateHasChanged

---

### Fix 1.3: BLAZMVVM0009 - Service Injection

**Current Issue:** [Inject] in ViewModel not triggering despite clear violation  
**Root Cause:** Unknown - possible analyzer implementation issue  
**Severity:** High - Core pattern that should work

**Implementation:**

**Step 1: Verify current Test009ViewModel.cs code:**
```csharp
using Blazing.Mvvm.AnalyzerTest.Data;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test009ViewModel : ViewModelBase
{
    [Inject]  // ? Wrong! Should use constructor injection
    public ITestService TestService { get; set; } = null!;

    [Inject]  // ? Wrong! Should use constructor injection
    public HttpClient HttpClient { get; set; } = null!;
}
```

**Step 2: Check Blazing.Mvvm.Analyzers.Tests for ServiceInjectionAnalyzerTests:**
- Look for test patterns that successfully trigger BLAZMVVM0009
- Verify test input matches our violation
- Compare with our test code

**Step 3: If analyzer is working in tests, try variations:**

**Variation A - Add more context:**
```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test009ViewModel : ViewModelBase
{
    [Inject]
    public ITestService TestService { get; set; } = null!;
    
    [Inject]
    public ILogger<Test009ViewModel> Logger { get; set; } = null!;
    
    public async Task<string> LoadData()
    {
        return await TestService.GetDataAsync();
    }
}
```

**Variation B - Create component that uses it:**
```razor
@page "/test009"
@inherits MvvmComponentBase<Test009ViewModel>

<p>Service: @ViewModel.TestService</p>
```

**Step 4: If still not working, investigate analyzer:**
- Review `ServiceInjectionAnalyzer.cs` in Blazing.Mvvm.Analyzers
- Check if analyzer is registered in `AnalyzerRelease.Shipped.md`
- Verify analyzer is included in build

**Expected Result:** BLAZMVVM0009 warning

**Test Method:**
- Build project
- Check Error List for BLAZMVVM0009
- If not present, review analyzer implementation

---

## Priority 2: Component Context Analyzers (7 Analyzers)

These analyzers likely require components that actually USE the ViewModels/patterns.

### Fix 2.1: BLAZMVVM0003 - MvvmComponentBase Usage

**Current Issue:** Component inherits ComponentBase but analyzer doesn't fire  
**Root Cause:** May require ViewModel usage or specific patterns  
**Severity:** Medium

**Implementation:**

**Update Test003.razor:**
```razor
@page "/test003"
@using Microsoft.AspNetCore.Components
@inject ITestService TestService

<!-- BLAZMVVM0003: Should inherit from MvvmComponentBase -->
@inherits ComponentBase

<PageTitle>Test003 - MvvmComponentBase Usage</PageTitle>

<h1>BLAZMVVM0003</h1>
<p>Data: @_data</p>
<button @onclick="LoadData">Load</button>

@code {
    private string _data = "No data";
    
    // Complex business logic suggests this should be a ViewModel
    private async Task LoadData()
    {
        await Task.Delay(100);
        _data = await TestService.GetDataAsync();
        StateHasChanged();
    }
    
    // More logic suggesting ViewModel pattern
    private void ProcessData()
    {
        // Complex processing
    }
}
```

**Expected Result:** BLAZMVVM0003 warning suggesting to use MvvmComponentBase

---

### Fix 2.2: BLAZMVVM0004 & BLAZMVVM0020 - ViewParameter & Route Parameter Binding

**Current Issue:** Using [ObservableProperty] may not be detected  
**Root Cause:** Analyzer may need explicit property declaration  
**Severity:** Medium

**Implementation:**

**Option A: Use explicit property (Recommended):**

Update `ViewModels/Test004And020ViewModel.cs`:
```csharp
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0004 & BLAZMVVM0020: Route parameter without [ViewParameter]
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test004And020ViewModel : ViewModelBase
{
    // Explicit property WITHOUT [ViewParameter] - should trigger both analyzers
    public int Id { get; set; }
    
    [ObservableProperty]
    private string _name = string.Empty;
}
```

Ensure `Test004.razor` has:
```razor
@page "/test004/{id:int}"
@inherits MvvmComponentBase<Test004And020ViewModel>

<p>Current ID: @ViewModel.Id</p>
```

**Option B: Add alternative test with explicit property:**

```csharp
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// Alternative test with explicit property
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test004And020ViewModel : ViewModelBase
{
    // Explicitly defined Id property
    public int Id { get; set; }
    
    [ObservableProperty]
    private string _name = string.Empty;
}
```

Verify with `Test004.razor`:
```razor
@page "/test004/{id:int}"
@inherits MvvmComponentBase<Test004And020ViewModel>

<p>Current ID: @ViewModel.Id</p>
```

**Expected Result:** BLAZMVVM0004 & BLAZMVVM0020 warnings for ViewParameter & Route Parameter Binding

---

### Fix 2.3: BLAZMVVM0010 - Logical and Visual Parent

**Current Issue:** Logical and visual parent mismatch  
**Root Cause:** Test component may not represent real usage  
**Severity:** Medium

**Implementation:**

**Update `Test010ViewModel.cs`:**
```csharp
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test010ViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Initial Title";

    public void UpdateTitle()
    {
        Title = "Updated Title";
    }
}
```

**Create `Components/Pages/Test010.razor`:**
```razor
@page "/test010"
@using Blazing.Mvvm.AnalyzerTest.ViewModels
@using Blazing.Mvvm.Components

@inherits MvvmComponentBase<Test010ViewModel>

<PageTitle>Test010 - Logical and Visual Parent</PageTitle>

<h1>BLAZMVVM0010: Logical and Visual Parent</h1>
<p>Title: @ViewModel.Title</p>

<!-- Add button to trigger ViewModel method -->
<button @onclick="ViewModel.UpdateTitle">Update Title</button>

@code {
    // Intentionally left blank
}
```

**Expected Result:** BLAZMVVM0010 info suggesting to extract logic to ViewModel

---

### Fix 2.4: BLAZMVVM0012 - Command Pattern

**Current Issue:** Public methods not triggering analyzer  
**Root Cause:** May need component that calls these methods  
**Severity:** Medium

**Implementation:**

**Create component `Components/Pages/Test012.razor`:**
```razor
@page "/test012"
@using Blazing.Mvvm.AnalyzerTest.ViewModels
@using Blazing.Mvvm.Components

@inherits MvvmComponentBase<Test012ViewModel>

<PageTitle>Test012 - Command Pattern</PageTitle>

<h1>BLAZMVVM0012: Command Pattern</h1>
<p>Data: @ViewModel.Data</p>

<!-- Directly calling public methods instead of commands -->
<button @onclick="ViewModel.LoadDataAsync">Load Data</button>
<button @onclick="ViewModel.ResetData">Reset</button>

@code {
    // Direct method binding may trigger analyzer
}
```

**Expected Result:** BLAZMVVM0012 info suggesting to use [RelayCommand]

---

### Fix 2.5: BLAZMVVM0018 - NotifyPropertyChangedFor

**Current Issue:** Missing attribute on dependent properties  
**Root Cause:** May need component binding to detect  
**Severity:** Medium

**Implementation:**

**Create component `Components/Pages/Test018.razor`:**
```razor
@page "/test018"
@using Blazing.Mvvm.AnalyzerTest.ViewModels
@using Blazing.Mvvm.Components

@inherits MvvmComponentBase<Test018ViewModel>

<PageTitle>Test018 - NotifyPropertyChangedFor</PageTitle>

<h1>BLAZMVVM0018</h1>

<p>First Name: <input @bind="ViewModel.FirstName" /></p>
<p>Last Name: <input @bind="ViewModel.LastName" /></p>

<!-- Computed property that won't update when dependencies change -->
<p>Full Name: @ViewModel.FullName</p>

<button @onclick="UpdateNames">Update Names</button>

@code {
    private void UpdateNames()
    {
        ViewModel.FirstName = "John";
        ViewModel.LastName = "Doe";
        // FullName won't update automatically - should trigger BLAZMVVM0018
    }
}
```

**Expected Result:** BLAZMVVM0018 info about missing [NotifyPropertyChangedFor]

---

### Fix 2.6: BLAZMVVM0020 - Route Parameter Binding (Additional Test)

**Covered in Fix 2.2** - See BLAZMVVM0004 & BLAZMVVM0020

---

## Priority 3: Analyzer Implementation Review (5 Analyzers)

These analyzers may have implementation issues or require specific investigation.

### Fix 3.1: BLAZMVVM0005 - Navigation Type Safety

**Current Issue:** Not triggering despite navigation to unregistered ViewModel  
**Root Cause:** May be blocked by BLAZMVVM0002 on same type  
**Severity:** High

**Implementation:**

**Step 1: Review Blazing.Mvvm.Analyzers.Tests:**
```
File: Blazing.Mvvm.Analyzers.Tests/AnalyzerTests/NavigationTypeSafetyAnalyzerTests.cs
```
- Check test patterns that successfully trigger BLAZMVVM0005
- Identify expected code structure

**Step 2: Create cleaner test scenario:**

Create new ViewModel that's registered but has no route:

**ViewModels/NoRouteViewModel.cs:**
```csharp
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// ViewModel WITH [ViewModelDefinition] but no route
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class NoRouteViewModel : ViewModelBase
{
    public string Title { get; set; } = "No Route";
}
```

**Update Test005ViewModel.cs:**
```csharp
[RelayCommand]
private void NavigateToNoRoute()
{
    // Navigating to registered ViewModel but it has no route
    _navigation.NavigateTo<NoRouteViewModel>();  // Should trigger BLAZMVVM0005
}
```

**Step 3: If still not working, check analyzer source:**
- File: `Blazing.Mvvm.Analyzers/Analyzers/NavigationTypeSafetyAnalyzer.cs`
- Verify analyzer is checking NavigateTo calls
- Check registration in analyzer assembly

**Expected Result:** BLAZMVVM0005 warning about navigation to ViewModel without route

---

### Fix 3.2: BLAZMVVM0013 - MvvmOwningComponentBase Usage

**Current Issue:** Not detecting DbContext usage  
**Root Cause:** Detection logic may require specific patterns  
**Severity:** Medium

**Implementation:**

**Step 1: Enhance Test013ViewModel.cs with actual DbContext usage:**
```csharp
using Blazing.Mvvm.AnalyzerTest.Data;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test013ViewModel : ViewModelBase
{
    private readonly TestDbContext _context;

    public Test013ViewModel(TestDbContext context)
    {
        _context = context;
    }

    [ObservableProperty]
    private string _data = string.Empty;

    public override async Task OnInitializedAsync()
    {
        // Actually USE the DbContext to trigger detection
        var items = await _context.TestItems.ToListAsync();
        Data = $"Loaded {items.Count} items";
    }
}
```

**Step 2: Ensure Test013.razor still uses MvvmComponentBase:**
```razor
@page "/test013"
@inherits MvvmComponentBase<Test013ViewModel>  <!-- Should be MvvmOwningComponentBase -->
```

**Step 3: Check analyzer test:**
```
File: Blazing.Mvvm.Analyzers.Tests/AnalyzerTests/MvvmOwningComponentBaseUsageAnalyzerTests.cs
```
- Review test cases to see what triggers the analyzer
- Compare with our test code

**Expected Result:** BLAZMVVM0013 warning to use MvvmOwningComponentBase

---

### Fix 3.3: BLAZMVVM0019 - CascadingParameter vs Inject

**Current Issue:** Not detecting [CascadingParameter] on DI services  
**Root Cause:** Razor semantic analysis may have issues  
**Severity:** Medium

**Implementation:**

**Step 1: Verify Test019.razor code:**
```razor
@page "/test019"
@using Blazing.Mvvm.AnalyzerTest.Data

<PageTitle>Test019</PageTitle>

<h1>BLAZMVVM0019</h1>

@code {
    // Using CascadingParameter for DI services - should be [Inject]
    [CascadingParameter]
    public ITestService TestService { get; set; } = null!;

    [CascadingParameter]
    public HttpClient HttpClient { get; set; } = null!;
    
    protected override Task OnInitializedAsync()
    {
        // Using the services
        _ = TestService.GetDataAsync();
        return base.OnInitializedAsync();
    }
}
```

**Step 2: Check analyzer tests:**
```
File: Blazing.Mvvm.Analyzers.Tests/AnalyzerTests/CascadingParameterVsInjectAnalyzerTests.cs
```

**Step 3: Try alternative patterns:**
- Multiple properties
- Different service types
- Usage in methods

**Expected Result:** BLAZMVVM0019 info suggesting [Inject] instead

---

### Fix 3.4: BLAZMVVM0021 - EventCallback Two-Way Binding

**Current Issue:** Complex manual pattern not detected  
**Root Cause:** Pattern matching may be very specific  
**Severity:** Medium

**Implementation:**

**Step 1: Review analyzer tests:**
```
File: Blazing.Mvvm.Analyzers.Tests/AnalyzerTests/EventCallbackTwoWayBindingAnalyzerTests.cs
```
- Study exact patterns that trigger the analyzer
- Check for required method names, signatures, etc.

**Step 2: Verify Test021Component.razor pattern:**
```razor
@using System.ComponentModel
@inherits MvvmComponentBase<Test021ViewModel>

@code {
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;  // Pattern to detect
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Counter))
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

**Step 3: Ensure ViewModel has [ViewParameter]:**
```csharp
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test021ViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]  // Required for automatic binding
    private int _counter;
}
```

**Expected Result:** BLAZMVVM0021 info about obsolete manual pattern

---

## Priority 4: Feature Verification (1 Analyzer)

### Fix 4.1: BLAZMVVM0006 - ViewModelKey Consistency

**Current Issue:** Unclear if ViewModelKey attribute exists  
**Root Cause:** Feature may not be implemented  
**Severity:** Low

**Implementation:**

**Step 1: Search framework source for ViewModelKey:**
```powershell
# In Blazing.Mvvm project
Get-ChildItem -Recurse -Filter "*.cs" | Select-String "ViewModelKey"
```

**Step 2: Check analyzer tests:**
```
File: Blazing.Mvvm.Analyzers.Tests/AnalyzerTests/ViewModelKeyConsistencyAnalyzerTests.cs
```

**Step 3A: If attribute exists, use correct syntax:**
```csharp
// ViewModel
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient, Key = "MyKey")]
public class Test006ViewModel : ViewModelBase

// Component
@attribute [ViewModelKey("DifferentKey")]  // Mismatch should trigger
@inherits MvvmComponentBase<Test006ViewModel>
```

**Step 3B: If attribute doesn't exist, document as not implemented:**
- Update README.md
- Mark BLAZMVVM0006 as "Feature Not Available"
- Remove from test coverage requirements

**Expected Result:** 
- If implemented: BLAZMVVM0006 warning
- If not implemented: Documentation updated

---

## Priority 5: C# Compiler Redundancy (1 Analyzer)

### Fix 5.1: BLAZMVVM0007 - Lifecycle Method Override

**Current Status:** C# compiler warning CS0114 fires  
**Action:** Verify if BLAZMVVM0007 provides additional value  
**Severity:** Low

**Implementation:**

**Step 1: Check if both warnings should fire:**
```csharp
// Test007ViewModel.cs
protected async Task OnInitializedAsync()  // Missing 'override'
{
    await Task.CompletedTask;
}
```

**Current:** CS0114 warning  
**Expected:** BLAZMVVM0007 info (in addition or instead?)

**Step 2: Review analyzer tests:**
```
File: Blazing.Mvvm.Analyzers.Tests/AnalyzerTests/LifecycleMethodOverrideAnalyzerTests.cs
```

**Step 3: Determine if analyzer should:
**
- A) Supplement CS0114 with MVVM-specific guidance
- B) Only fire when CS0114 doesn't (different scenarios)
- C) Provide additional context or fixes

**Step 4: If analyzer should fire:**
- Check registration in analyzer assembly
- Verify severity level doesn't conflict with compiler
- Test with different scenarios

**Expected Result:** Clarification of analyzer purpose and behavior

---

## Implementation Checklist

### Phase 1: Quick Wins (Immediate)
- [ ] Fix 1.1: Add using directives to _Imports.razor (BLAZMVVM0011)
- [ ] Fix 1.2: Create Test014.razor component (BLAZMVVM0014)
- [ ] Fix 1.3: Investigate BLAZMVVM0009 in analyzer tests
- [ ] Build and verify 3 additional analyzers trigger

### Phase 2: Component Tests (Short-term)
- [ ] Fix 2.1: Enhance Test003.razor (BLAZMVVM0003)
- [ ] Fix 2.2: Update Test004And020ViewModel.cs (BLAZMVVM0004, 0020)
- [ ] Fix 2.3: Create Test008.razor (BLAZMVVM0008)
- [ ] Fix 2.4: Enhance Test010.razor (BLAZMVVM0010)
- [ ] Fix 2.5: Create Test012.razor (BLAZMVVM0012)
- [ ] Fix 2.6: Create Test018.razor (BLAZMVVM0018)
- [ ] Build and verify 6-7 additional analyzers trigger

### Phase 3: Analyzer Investigation (Medium-term)
- [ ] Fix 3.1: Create NoRouteViewModel (BLAZMVVM0005)
- [ ] Fix 3.2: Enhance Test013ViewModel with DbContext usage (BLAZMVVM0013)
- [ ] Fix 3.3: Review CascadingParameter tests (BLAZMVVM0019)
- [ ] Fix 3.4: Review EventCallback pattern tests (BLAZMVVM0021)
- [ ] Review analyzer source code for any issues
- [ ] Build and verify 3-4 additional analyzers trigger

### Phase 4: Feature Verification (Low-priority)
- [ ] Fix 4.1: Verify ViewModelKey attribute exists (BLAZMVVM0006)
- [ ] Fix 5.1: Clarify BLAZMVVM0007 behavior
- [ ] Document any unimplemented features

---

## Testing Strategy

### After Each Fix
1. Clean solution: `dotnet clean`
2. Build solution: `dotnet build`
3. Check build output for new analyzer warnings
4. Verify warning ID, message, and location
5. Document result in test matrix

### Verification Commands
```powershell
# Build and capture output
dotnet build samples\Blazing.Mvvm.AnalyzerTest\Blazing.Mvvm.AnalyzerTest.csproj > build-output.txt 2>&1

# Filter for BLAZMVVM warnings
Select-String -Path build-output.txt -Pattern "BLAZMVVM"

# Count unique analyzer warnings
(Select-String -Path build-output.txt -Pattern "BLAZMVVM\d{4}" -AllMatches).Matches.Value | Sort-Object -Unique
```

### Success Criteria
- All 21 BLAZMVVM analyzers trigger at least once
- Build output shows clear warning messages
- Each warning points to correct file and line
- No false positives (warnings on correct code)

---

## Test Matrix Template

Track progress using this matrix:

| ID | Analyzer | Status | Fixed In | Build Output | Notes |
|----|----------|--------|----------|--------------|-------|
| 0001 | ViewModelBase Inheritance | ? PASS | N/A | Line 6, Col 14-30 | Working |
| 0002 | ViewModelDefinition Attribute | ? PASS | N/A | 2 instances | Working |
| 0003 | MvvmComponentBase Usage | ? FAIL | Fix 2.1 | | Needs component |
| 0004 | ViewParameter Attribute | ? FAIL | Fix 2.2 | | Needs explicit property |
| 0005 | Navigation Type Safety | ? FAIL | Fix 3.1 | | Blocked by 0002? |
| 0006 | ViewModelKey Consistency | ? FAIL | Fix 4.1 | | Feature verification needed |
| 0007 | Lifecycle Method Override | ?? PARTIAL | Fix 5.1 | CS0114 instead | Redundant with compiler? |
| 0008 | Observable Property | ? FAIL | Fix 2.3 | | Needs component binding |
| 0009 | Service Injection | ? FAIL | Fix 1.3 | | Implementation issue? |
| 0010 | Route-ViewModel Mapping | ? FAIL | Fix 2.4 | | Needs complex logic |
| 0011 | MvvmNavLink Type Safety | ? FAIL | Fix 1.1 | RZ10012 | Missing using directive |
| 0012 | Command Pattern | ? FAIL | Fix 2.5 | | Needs component usage |
| 0013 | MvvmOwningComponentBase | ? FAIL | Fix 3.2 | | DbContext usage pattern |
| 0014 | StateHasChanged Overuse | ? FAIL | Fix 1.2 | | Wrong location (ViewModel) |
| 0015 | Dispose Pattern | ? PASS | N/A | 3 instances | Working |
| 0016 | Messenger Registration | ? PASS | N/A | Line 16, Col 12-34 | Working |
| 0017 | RelayCommand Async Pattern | ? PASS | N/A | Line 12, Col 24-32 | Working |
| 0018 | NotifyPropertyChangedFor | ? FAIL | Fix 2.6 | | Needs component binding |
| 0019 | CascadingParameter vs Inject | ? FAIL | Fix 3.3 | | Razor analysis issue? |
| 0020 | Route Parameter Binding | ? FAIL | Fix 2.2 | | Same as 0004 |
| 0021 | EventCallback Two-Way Binding | ? FAIL | Fix 3.4 | | Complex pattern |

**Current:** 5/21 (23.8%)  
**Target:** 21/21 (100%)

---

## Additional Resources

### Analyzer Test Files
Review these for working patterns:
```
Blazing.Mvvm.Analyzers.Tests/AnalyzerTests/
??? CascadingParameterVsInjectAnalyzerTests.cs
??? CommandPatternAnalyzerTests.cs
??? DisposePatternAnalyzerTests.cs
??? EventCallbackTwoWayBindingAnalyzerTests.cs
??? LifecycleMethodOverrideAnalyzerTests.cs
??? MessengerRegistrationLifetimeAnalyzerTests.cs
??? MvvmComponentBaseUsageAnalyzerTests.cs
??? MvvmNavLinkTypeSafetyAnalyzerTests.cs
??? MvvmOwningComponentBaseUsageAnalyzerTests.cs
??? NavigationTypeSafetyAnalyzerTests.cs
??? NotifyPropertyChangedForAnalyzerTests.cs
??? ObservablePropertyAnalyzerTests.cs
??? RelayCommandAsyncPatternAnalyzerTests.cs
??? RouteParameterBindingAnalyzerTests.cs
??? RouteViewModelMappingAnalyzerTests.cs
??? ServiceInjectionAnalyzerTests.cs
??? StateHasChangedOveruseAnalyzerTests.cs
??? ViewModelBaseInheritanceAnalyzerTests.cs
??? ViewModelDefinitionAttributeAnalyzerTests.cs
??? ViewModelKeyConsistencyAnalyzerTests.cs
??? ViewParameterAttributeAnalyzerTests.cs
```

### Commands Reference
```powershell
# Clean build
dotnet clean samples\Blazing.Mvvm.AnalyzerTest\Blazing.Mvvm.AnalyzerTest.csproj

# Build with verbose output
dotnet build samples\Blazing.Mvvm.AnalyzerTest\Blazing.Mvvm.AnalyzerTest.csproj -v detailed

# Run analyzer tests
dotnet test Blazing.Mvvm.Analyzers.Tests\Blazing.Mvvm.Analyzers.Tests.csproj --filter "FullyQualifiedName~AnalyzerTests"

# Check specific analyzer test
dotnet test --filter "ServiceInjectionAnalyzerTests"
```

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**Next Review:** After Phase 1 completion

