# Blazing.Mvvm Analyzer Issues - Finding Report

**Project:** Blazing.Mvvm.AnalyzerTest  
**Date:** 2024  
**Purpose:** Comprehensive analysis of all 21 BLAZMVVM analyzers to verify triggering status

---

## Executive Summary

**Build Status:** ? **Successful**  
**Total Analyzers:** 21  
**Triggered Successfully:** 5 (23.8%)  
**Not Triggering:** 15 (71.4%)  
**Partially Working:** 1 (4.8%)

### Critical Findings
- Most analyzers (76.2%) are NOT triggering despite intentionally violating their rules
- Only 5 analyzers are confirmed working: BLAZMVVM0001, 0002, 0015, 0016, 0017
- Many analyzers may require specific context or patterns not currently implemented
- Some analyzers may have issues in their implementation or registration

---

## Build Output Analysis

### Actual Analyzer Warnings Detected

From the Visual Studio build output (Build pane ID: 1bd8a850-02d1-11d1-bee7-00a0c913d1f8):

```
BLAZMVVM0001 - Test001ViewModel.cs (Line 6, Column 14-30)
  Warning: Type 'Test001ViewModel' should inherit from ViewModelBase, RecipientViewModelBase, or ValidatorViewModelBase

BLAZMVVM0002 - Test002ViewModel.cs (Line 6, Column 14-30)
  Warning: Type 'Test002ViewModel' must be decorated with [ViewModelDefinition] attribute for dependency injection registration

BLAZMVVM0002 - Test005ViewModel.cs -> UnregisteredTargetViewModel (Line 32, Column 14-41)
  Warning: Type 'UnregisteredTargetViewModel' must be decorated with [ViewModelDefinition] attribute for dependency injection registration

BLAZMVVM0015 - Test009ViewModel.cs (Line 10, Column 22-38)
  Warning: ViewModel 'Test009ViewModel' uses disposable resources but does not implement IDisposable pattern

BLAZMVVM0015 - Test013ViewModel.cs (Line 10, Column 22-38)
  Warning: ViewModel 'Test013ViewModel' uses disposable resources but does not implement IDisposable pattern

BLAZMVVM0015 - Test015And016ViewModel.cs (Line 12, Column 22-44)
  Warning: ViewModel 'Test015And016ViewModel' uses disposable resources but does not implement IDisposable pattern

BLAZMVVM0016 - Test015And016ViewModel.cs (Line 16, Column 12-34)
  Warning: Messenger.Register call in 'Test015And016ViewModel' is not unregistered - consider calling Unregister in Dispose or use RecipientViewModelBase

BLAZMVVM0017 - Test017ViewModel.cs (Line 12, Column 24-32)
  Warning: Async method 'LoadData' with [RelayCommand] should use AsyncRelayCommand instead of async void to properly handle exceptions
```

### Additional Warnings (Non-Analyzer)

```
CS0114 - Test007ViewModel.cs (Line 15)
  Warning: 'Test007ViewModel.OnInitializedAsync()' hides inherited member 'ViewModelBase.OnInitializedAsync()'. 
  To make the current member override that implementation, add the override keyword. Otherwise add the new keyword.
  
RZ10012 - Test011.razor (Line 13)
  Warning: Found markup element with unexpected name 'MvvmNavLink'. If this is intended to be a component, 
  add a @using directive for its namespace.
```

---

## Detailed Analyzer Status

### ? Working Analyzers (5)

| ID | Name | Severity | Status | File | Evidence |
|----|------|----------|--------|------|----------|
| **BLAZMVVM0001** | ViewModelBase Inheritance | Error | ? WORKING | Test001ViewModel.cs | Confirmed in build output |
| **BLAZMVVM0002** | ViewModelDefinition Attribute | Error | ? WORKING | Test002ViewModel.cs, UnregisteredTargetViewModel | Confirmed in build output (2 instances) |
| **BLAZMVVM0015** | Dispose Pattern | Warning | ? WORKING | Test009ViewModel.cs, Test013ViewModel.cs, Test015And016ViewModel.cs | Confirmed in build output (3 instances) |
| **BLAZMVVM0016** | Messenger Registration Lifetime | Warning | ? WORKING | Test015And016ViewModel.cs | Confirmed in build output |
| **BLAZMVVM0017** | RelayCommand Async Pattern | Warning | ? WORKING | Test017ViewModel.cs | Confirmed in build output |

---

### ?? Partially Working Analyzers (1)

| ID | Name | Severity | Status | File | Issue |
|----|------|----------|--------|------|-------|
| **BLAZMVVM0007** | Lifecycle Method Override | Info | ?? PARTIAL | Test007ViewModel.cs | C# compiler warning CS0114 fires instead of BLAZMVVM0007. The analyzer may be redundant with built-in C# compiler checks. |

**Analysis for BLAZMVVM0007:**
- **Current Behavior:** C# compiler generates CS0114 warning for missing `override` keyword
- **Expected Behavior:** BLAZMVVM0007 should provide specific MVVM-focused guidance
- **Hypothesis:** The analyzer may be designed to provide additional context or may not be running because the compiler warning pre-empts it
- **Test Code:**
  ```csharp
  // Test007ViewModel.cs (Line 15)
  protected async Task OnInitializedAsync()  // Missing 'override'
  {
      await Task.CompletedTask;
  }
  ```

---

### ? Not Triggering Analyzers (15)

#### BLAZMVVM0003: MvvmComponentBase Usage
- **Severity:** Warning
- **Expected Trigger:** Test003.razor
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```razor
  @page "/test003"
  @inherits ComponentBase  <!-- Should trigger: should use MvvmComponentBase -->
  ```
- **Hypothesis:** Analyzer may require:
  1. Component to actually use a ViewModel
  2. Specific patterns of ViewModel usage to detect
  3. Component to be in a specific namespace or structure
  4. May only check components with certain characteristics

#### BLAZMVVM0004: ViewParameter Attribute
- **Severity:** Warning
- **Expected Trigger:** Test004And020ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```csharp
  [ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
  public partial class Test004And020ViewModel : ViewModelBase
  {
      [ObservableProperty]  // Missing [ViewParameter]
      private int _id;
  }
  ```
- **Hypothesis:** Analyzer may require:
  1. Explicit property declaration (not source-generated from `[ObservableProperty]`)
  2. Component with matching route parameter to be in scope
  3. Property to be referenced in a Razor component context
  4. Specific naming conventions between route params and properties

#### BLAZMVVM0005: Navigation Type Safety
- **Severity:** Warning
- **Expected Trigger:** Test005ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```csharp
  [RelayCommand]
  private void NavigateToUnregistered()
  {
      _navigation.NavigateTo<UnregisteredTargetViewModel>();  // Should warn
  }
  ```
- **Hypothesis:** Analyzer may be blocked because:
  1. UnregisteredTargetViewModel triggers BLAZMVVM0002 (missing ViewModelDefinition)
  2. Analyzer may not run if compilation has other errors
  3. May require the navigation to be in a component context
  4. NavigateTo call may need to be in specific usage pattern

#### BLAZMVVM0006: ViewModelKey Consistency
- **Severity:** Warning
- **Expected Trigger:** Test006.razor + Test006ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```csharp
  // ViewModel
  [ViewModelDefinition(Lifetime = ServiceLifetime.Transient, Key = "WrongKey")]
  public class Test006ViewModel : ViewModelBase
  
  // Component
  @inherits MvvmComponentBase<Test006ViewModel>
  @attribute [ViewModelKey("DifferentKey")]
  ```
- **Hypothesis:**
  1. `ViewModelKey` attribute may not exist in the framework
  2. Analyzer may not be checking Razor file attributes
  3. Attribute syntax or usage may be incorrect
  4. Need to verify if this feature is implemented

#### BLAZMVVM0008: Observable Property Usage
- **Severity:** Warning
- **Expected Trigger:** Test008ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```csharp
  public class Test008ViewModel : ViewModelBase  // Missing 'partial'
  {
      public string Name { get; set; } = "Test";  // No notification
  }
  ```
- **Hypothesis:** Analyzer may require:
  1. Property to be bound in a Razor component
  2. Actual usage in a view to detect
  3. Specific pattern of property access
  4. Component compilation context

#### BLAZMVVM0009: Service Injection
- **Severity:** Warning
- **Expected Trigger:** Test009ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```csharp
  [ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
  public partial class Test009ViewModel : ViewModelBase
  {
      [Inject]  // Wrong! Should use constructor injection
      public ITestService TestService { get; set; } = null!;
      
      [Inject]
      public HttpClient HttpClient { get; set; } = null!;
  }
  ```
- **Note:** This file triggered BLAZMVVM0015 (Dispose Pattern) but NOT BLAZMVVM0009
- **Hypothesis:**
  1. Analyzer may not be registered or enabled
  2. `[Inject]` attribute detection may have issues
  3. May conflict with BLAZMVVM0015 execution
  4. Implementation may have bugs

#### BLAZMVVM0010: Route-ViewModel Mapping
- **Severity:** Info
- **Expected Trigger:** Test010.razor
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```razor
  @page "/test010"
  @inject HttpClient HttpClient
  
  <button @onclick="LoadData">Load Data</button>
  
  @code {
      private string _data = "No data";
      private async Task LoadData()  // Business logic in component
      {
          await Task.Delay(100);
          _data = "Loaded";
      }
  }
  ```
- **Hypothesis:** Analyzer may require:
  1. More complex business logic to detect
  2. Specific patterns of code-behind usage
  3. Multiple methods or significant code
  4. May only check components with certain complexity

#### BLAZMVVM0011: MvvmNavLink Type Safety
- **Severity:** Error
- **Expected Trigger:** Test011.razor
- **Status:** ? NOT TRIGGERED (Razor warning RZ10012 instead)
- **Test Code:**
  ```razor
  <MvvmNavLink TViewModel="UnregisteredTargetViewModel">
      Link to Unregistered ViewModel
  </MvvmNavLink>
  ```
- **Current Warning:** RZ10012: Found markup element with unexpected name 'MvvmNavLink'
- **Hypothesis:**
  1. Missing `@using Blazing.Mvvm.Components` directive prevents component recognition
  2. Analyzer can't run because Razor compilation fails first
  3. Component isn't being resolved, so analyzer doesn't see it
  4. **FIX REQUIRED:** Add using directive to _Imports.razor

#### BLAZMVVM0012: Command Pattern
- **Severity:** Info
- **Expected Trigger:** Test012ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```csharp
  public partial class Test012ViewModel : ViewModelBase
  {
      // Should be [RelayCommand] instead of public method
      public async Task LoadDataAsync()
      {
          Data = await _testService.GetDataAsync();
      }
      
      public void ResetData()
      {
          Data = string.Empty;
      }
  }
  ```
- **Hypothesis:** Analyzer may require:
  1. Methods to be called from a Razor component
  2. Specific usage pattern (e.g., onclick binding)
  3. Component context to detect public method invocation
  4. May need component that binds to these methods

#### BLAZMVVM0013: MvvmOwningComponentBase Usage
- **Severity:** Warning
- **Expected Trigger:** Test013.razor
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```razor
  @page "/test013"
  @inherits MvvmComponentBase<Test013ViewModel>  <!-- Should use MvvmOwningComponentBase -->
  
  <!-- ViewModel uses DbContext (scoped service) -->
  ```
  ```csharp
  public partial class Test013ViewModel : ViewModelBase
  {
      private readonly TestDbContext _context;  // Scoped service
  }
  ```
- **Note:** This file triggered BLAZMVVM0015 (Dispose) but NOT BLAZMVVM0013
- **Hypothesis:**
  1. Analyzer may not detect DbContext usage pattern
  2. May require specific DbContext method calls
  3. Detection logic may have issues
  4. May need actual database operations in ViewModel

#### BLAZMVVM0014: StateHasChanged Overuse
- **Severity:** Info
- **Expected Trigger:** Test014ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Issue:** `StateHasChanged()` is not accessible in ViewModels (it's a component method)
- **Test Code:**
  ```csharp
  public void UpdateName(string first, string last)
  {
      FirstName = first;
      LastName = last;
      // StateHasChanged(); // Can't call this from ViewModel
  }
  ```
- **Hypothesis:**
  1. **DESIGN ISSUE:** Analyzer is for components, not ViewModels
  2. Test should be in a component's @code block
  3. Need to create a component that manually calls StateHasChanged
  4. **FIX REQUIRED:** Move test to a component file

#### BLAZMVVM0018: NotifyPropertyChangedFor
- **Severity:** Info
- **Expected Trigger:** Test018ViewModel.cs
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```csharp
  public partial class Test018ViewModel : ViewModelBase
  {
      [ObservableProperty]  // Missing [NotifyPropertyChangedFor(nameof(FullName))]
      private string _firstName = string.Empty;
      
      [ObservableProperty]  // Missing [NotifyPropertyChangedFor(nameof(FullName))]
      private string _lastName = string.Empty;
      
      public string FullName => $"{FirstName} {LastName}";
  }
  ```
- **Hypothesis:** Analyzer may require:
  1. Component usage context to detect computed property dependencies
  2. Actual binding in Razor to trigger
  3. Specific patterns of computed property access
  4. May not analyze source-generated properties correctly

#### BLAZMVVM0019: CascadingParameter vs Inject
- **Severity:** Info
- **Expected Trigger:** Test019.razor
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```razor
  @page "/test019"
  
  @code {
      [CascadingParameter]  // Should use [Inject] for DI services
      public ITestService TestService { get; set; } = null!;
      
      [CascadingParameter]
      public HttpClient HttpClient { get; set; } = null!;
  }
  ```
- **Hypothesis:** Analyzer may require:
  1. Component compilation context
  2. Semantic analysis of parameter types
  3. Detection of DI-registered services
  4. May have implementation issues with Razor syntax tree

#### BLAZMVVM0020: Route Parameter Binding
- **Severity:** Warning
- **Expected Trigger:** Test004.razor
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```razor
  @page "/test004/{id:int}"
  @inherits MvvmComponentBase<Test004And020ViewModel>
  
  <!-- Route has {id} but ViewModel.Id lacks [ViewParameter] -->
  ```
  ```csharp
  public partial class Test004And020ViewModel : ViewModelBase
  {
      [ObservableProperty]  // Missing [ViewParameter]
      private int _id;
  }
  ```
- **Hypothesis:** Analyzer may require:
  1. Source-generated property names to match route parameter names exactly (case-sensitive)
  2. May not work with `[ObservableProperty]` (needs explicit property)
  3. Requires both component and ViewModel to be compiled together
  4. Route parameter detection may have issues

#### BLAZMVVM0021: EventCallback Two-Way Binding
- **Severity:** Info
- **Expected Trigger:** Test021Component.razor
- **Status:** ? NOT TRIGGERED
- **Test Code:**
  ```razor
  @inherits MvvmComponentBase<Test021ViewModel>
  
  @code {
      [Parameter]
      public int Counter { get; set; }
      
      [Parameter]
      public EventCallback<int> CounterChanged { get; set; }
      
      // Manual PropertyChanged subscription (obsolete pattern since v3.2.0)
      protected override void OnInitialized()
      {
          base.OnInitialized();
          ViewModel.PropertyChanged += OnViewModelPropertyChanged;
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
- **Hypothesis:** Analyzer may require:
  1. Very specific pattern matching for manual subscription
  2. Detection may be complex and pattern-sensitive
  3. May have edge cases in implementation
  4. Requires semantic analysis of subscription pattern

---

## Summary of Issues by Category

### Category 1: Missing Prerequisites (2 analyzers)
**Issue:** Test setup is incorrect or incomplete

- **BLAZMVVM0011:** Missing `@using Blazing.Mvvm.Components` directive
- **BLAZMVVM0014:** StateHasChanged test in ViewModel instead of component

**Severity:** High - Easy to fix
**Action:** Update test files with correct setup

### Category 2: Requires Component Context (7 analyzers)
**Issue:** Analyzers may only work when ViewModel/code is used in a component

- **BLAZMVVM0003:** MvvmComponentBase Usage - may need actual ViewModel usage
- **BLAZMVVM0004:** ViewParameter Attribute - may need component with route
- **BLAZMVVM0008:** Observable Property - may need component binding
- **BLAZMVVM0010:** Route-ViewModel Mapping - may need complex logic
- **BLAZMVVM0012:** Command Pattern - may need component method calls
- **BLAZMVVM0018:** NotifyPropertyChangedFor - may need component binding
- **BLAZMVVM0020:** Route Parameter Binding - may need complete component context

**Severity:** Medium - Requires creating additional Razor components
**Action:** Create components that use the ViewModels/patterns

### Category 3: Possible Implementation Issues (5 analyzers)
**Issue:** Analyzer may have bugs or not be properly registered

- **BLAZMVVM0005:** Navigation Type Safety - blocked by BLAZMVVM0002?
- **BLAZMVVM0009:** Service Injection - not firing despite clear violation
- **BLAZMVVM0013:** MvvmOwningComponentBase Usage - not detecting DbContext
- **BLAZMVVM0019:** CascadingParameter vs Inject - not detecting pattern
- **BLAZMVVM0021:** EventCallback Two-Way Binding - complex pattern not detected

**Severity:** High - May require analyzer code review
**Action:** Investigate analyzer implementation in Blazing.Mvvm.Analyzers project

### Category 4: Feature Verification Needed (1 analyzer)
**Issue:** Unclear if feature exists in framework

- **BLAZMVVM0006:** ViewModelKey Consistency - ViewModelKey attribute may not exist

**Severity:** Medium - Requires framework investigation
**Action:** Verify if ViewModelKey attribute is implemented

### Category 5: Redundant with Compiler (1 analyzer)
**Issue:** C# compiler already provides warning

- **BLAZMVVM0007:** Lifecycle Method Override - CS0114 already warns

**Severity:** Low - Analyzer may be intentionally complementary
**Action:** Verify if analyzer provides additional value

---

## Recommendations

### Immediate Actions (High Priority)

1. **Fix BLAZMVVM0011 (MvvmNavLink):**
   - Add `@using Blazing.Mvvm.Components` to `_Imports.razor`
   - This should immediately enable the analyzer

2. **Fix BLAZMVVM0014 (StateHasChanged):**
   - Create a new component with manual StateHasChanged calls
   - Move test from ViewModel to component @code block

3. **Investigate BLAZMVVM0009 (Service Injection):**
   - Check if analyzer is registered in Blazing.Mvvm.Analyzers project
   - Review analyzer unit tests to see expected patterns
   - This is a clear violation that should trigger

### Medium Priority Actions

4. **Create Component Usage Tests:**
   - Create Razor components that bind to Test008, Test012, Test018 ViewModels
   - Add actual UI bindings to trigger component-context analyzers

5. **Verify BLAZMVVM0006 (ViewModelKey):**
   - Check framework source for ViewModelKey attribute
   - Review documentation to confirm feature exists
   - Update test if feature doesn't exist

6. **Review Analyzer Implementations:**
   - Check Blazing.Mvvm.Analyzers.Tests for patterns
   - Compare working vs non-working analyzer implementations
   - Look for registration or configuration issues

### Low Priority Actions

7. **Enhance Test Coverage:**
   - Create more complex scenarios for BLAZMVVM0010
   - Add navigation scenarios for BLAZMVVM0005
   - Improve route parameter tests for BLAZMVVM0020

---

## Next Steps

1. **Create Fix Plan Document:** `analyzer-issues-fix-plan.md`
2. **Implement Priority 1 Fixes:** BLAZMVVM0011, 0014, 0009
3. **Create Component Tests:** For analyzers requiring component context
4. **Review Analyzer Code:** For suspected implementation issues
5. **Update Test Documentation:** Document patterns that successfully trigger each analyzer

---

## Appendix: Test File Reference

### ViewModels Created
- Test001ViewModel.cs - BLAZMVVM0001 ?
- Test002ViewModel.cs - BLAZMVVM0002 ?
- Test004And020ViewModel.cs - BLAZMVVM0004 ?, BLAZMVVM0020 ?
- Test005ViewModel.cs - BLAZMVVM0005 ?
- Test006ViewModel.cs - BLAZMVVM0006 ?
- Test007ViewModel.cs - BLAZMVVM0007 ??
- Test008ViewModel.cs - BLAZMVVM0008 ?
- Test009ViewModel.cs - BLAZMVVM0009 ?, BLAZMVVM0015 ?
- Test012ViewModel.cs - BLAZMVVM0012 ?
- Test013ViewModel.cs - BLAZMVVM0013 ?, BLAZMVVM0015 ?
- Test014ViewModel.cs - BLAZMVVM0014 ?
- Test015And016ViewModel.cs - BLAZMVVM0015 ?, BLAZMVVM0016 ?
- Test017ViewModel.cs - BLAZMVVM0017 ?
- Test018ViewModel.cs - BLAZMVVM0018 ?
- Test021ViewModel.cs - BLAZMVVM0021 ?

### Components Created
- Test003.razor - BLAZMVVM0003 ?
- Test004.razor - BLAZMVVM0020 ?
- Test005.razor - BLAZMVVM0005 ?
- Test006.razor - BLAZMVVM0006 ?
- Test010.razor - BLAZMVVM0010 ?
- Test011.razor - BLAZMVVM0011 ?
- Test013.razor - BLAZMVVM0013 ?
- Test019.razor - BLAZMVVM0019 ?
- Test021Component.razor - BLAZMVVM0021 ?
- Test021.razor - Parent component for BLAZMVVM0021

---

**Report Generated:** Build successful on .NET 9  
**Analyzer Project:** Blazing.Mvvm.Analyzers  
**Test Project:** Blazing.Mvvm.AnalyzerTest

