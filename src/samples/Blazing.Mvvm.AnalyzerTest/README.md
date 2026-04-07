# Blazing.Mvvm Analyzer Test Project - Summary

## Project Created
The `Blazing.Mvvm.AnalyzerTest` Blazor Server application has been created to systematically test all 21 Blazing.Mvvm analyzers.

## Project Structure

### Configuration Files
- **Blazing.Mvvm.AnalyzerTest.csproj** - Project file with analyzer references
- **Program.cs** - Application setup with services for analyzer testing
- **Components/App.razor** - Application root
- **Components/Routes.razor** - Routing configuration

### ViewModels Created (Triggering Analyzers 0001-0021)

| ViewModel | Analyzers Triggered | Description |
|-----------|---------------------|-------------|
| **Test001ViewModel.cs** | BLAZMVVM0001 | Missing ViewModelBase inheritance - class with "ViewModel" suffix but no base class |
| **Test002ViewModel.cs** | BLAZMVVM0002 | Missing [ViewModelDefinition] attribute on ViewModelBase descendant |
| **Test004And020ViewModel.cs** | BLAZMVVM0004, BLAZMVVM0020 | Route parameter without [ViewParameter] attribute |
| **Test005ViewModel.cs** | BLAZMVVM0005 | Navigation to unregistered ViewModel (UnregisteredTargetViewModel) |
| **Test006ViewModel.cs** | BLAZMVVM0006 | ViewModelKey mismatch between ViewModel and Component |
| **Test007ViewModel.cs** | BLAZMVVM0007 | Lifecycle method missing 'override' keyword |
| **Test008ViewModel.cs** | BLAZMVVM0008 | Missing 'partial' keyword + regular properties without notification |
| **Test009ViewModel.cs** | BLAZMVVM0009 | Using [Inject] in ViewModel instead of constructor injection |
| **Test012ViewModel.cs** | BLAZMVVM0012 | Public methods called from UI instead of RelayCommands |
| **Test013ViewModel.cs** | BLAZMVVM0013 | DbContext injection (requires MvvmOwningComponentBase) |
| **Test014ViewModel.cs** | BLAZMVVM0014 | Unnecessary StateHasChanged() calls |
| **Test015And016ViewModel.cs** | BLAZMVVM0015, BLAZMVVM0016 | Missing IDisposable + Messenger registration without cleanup |
| **Test017ViewModel.cs** | BLAZMVVM0017 | RelayCommand with async void instead of async Task |
| **Test018ViewModel.cs** | BLAZMVVM0018 | Missing [NotifyPropertyChangedFor] for computed properties |
| **Test021ViewModel.cs** | BLAZMVVM0021 | Manual PropertyChanged subscription (obsolete pattern) |
| **IndexViewModel.cs** | None | Properly configured ViewModel for home page |

### Razor Components Created

| Component | Analyzers Triggered | Description |
|-----------|---------------------|-------------|
| **Components/Pages/Home.razor** (also Index.razor) | None | Home page listing all tests |
| **Components/Pages/Test003.razor** | BLAZMVVM0003 | Inherits ComponentBase instead of MvvmComponentBase |
| **Components/Pages/Test004.razor** | BLAZMVVM0020 | Route `{id}` parameter but ViewModel.Id lacks [ViewParameter] |
| **Components/Pages/Test005.razor** | None (ViewModel triggers 0005) | Demonstrates navigation to unregistered ViewModel |
| **Components/Pages/Test006.razor** | BLAZMVVM0006 | ViewModelKey mismatch with ViewModel |
| **Components/Pages/Test010.razor** | BLAZMVVM0010 | Page with @page but no ViewModel (logic in code-behind) |
| **Components/Pages/Test011.razor** | BLAZMVVM0011 | MvvmNavLink referencing unregistered ViewModel |
| **Components/Pages/Test013.razor** | BLAZMVVM0013 | Uses MvvmComponentBase with DbContext (should use MvvmOwningComponentBase) |
| **Components/Pages/Test019.razor** | BLAZMVVM0019 | Using [CascadingParameter] for DI services instead of [Inject] |
| **Components/Pages/Test021.razor** | None (child component triggers 0021) | Parent for EventCallback testing |
| **Components/Test021Component.razor** | BLAZMVVM0021 | Manual PropertyChanged subscription (obsolete pattern) |

## Analyzer Coverage Summary

### ? All 21 Analyzers Covered

| ID | Title | Severity | Triggered By | Status |
|----|-------|----------|--------------|---------|
| **BLAZMVVM0001** | ViewModelBase Inheritance | Error | Test001ViewModel.cs | ? Covered |
| **BLAZMVVM0002** | ViewModelDefinition Attribute | Error | Test002ViewModel.cs | ? Covered |
| **BLAZMVVM0003** | MvvmComponentBase Usage | Warning | Test003.razor | ? Covered |
| **BLAZMVVM0004** | ViewParameter Attribute | Warning | Test004And020ViewModel.cs | ? Covered |
| **BLAZMVVM0005** | Navigation Type Safety | Warning | Test005ViewModel.cs | ? Covered |
| **BLAZMVVM0006** | ViewModelKey Consistency | Warning | Test006.razor + Test006ViewModel.cs | ? Covered |
| **BLAZMVVM0007** | Lifecycle Method Override | Info | Test007ViewModel.cs | ? Covered |
| **BLAZMVVM0008** | Observable Property Usage | Warning | Test008ViewModel.cs | ? Covered |
| **BLAZMVVM0009** | Service Injection | Warning | Test009ViewModel.cs | ? Covered |
| **BLAZMVVM0010** | Route-ViewModel Mapping | Info | Test010.razor | ? Covered |
| **BLAZMVVM0011** | MvvmNavLink Type Safety | Error | Test011.razor | ? Covered |
| **BLAZMVVM0012** | Command Pattern | Info | Test012ViewModel.cs | ? Covered |
| **BLAZMVVM0013** | MvvmOwningComponentBase Usage | Warning | Test013.razor | ? Covered |
| **BLAZMVVM0014** | StateHasChanged Overuse | Info | Test014ViewModel.cs | ? Covered |
| **BLAZMVVM0015** | Dispose Pattern | Warning | Test015And016ViewModel.cs | ? Covered |
| **BLAZMVVM0016** | Messenger Registration Lifetime | Warning | Test015And016ViewModel.cs | ? Covered |
| **BLAZMVVM0017** | RelayCommand Async Pattern | Warning | Test017ViewModel.cs | ? Covered |
| **BLAZMVVM0018** | NotifyPropertyChangedFor | Info | Test018ViewModel.cs | ? Covered |
| **BLAZMVVM0019** | CascadingParameter vs Inject | Info | Test019.razor | ? Covered |
| **BLAZMVVM0020** | Route Parameter Binding | Warning | Test004.razor | ? Covered |
| **BLAZMVVM0021** | EventCallback Two-Way Binding | Info | Test021Component.razor | ? Covered |

## Known Build Issues

### Issue: Central Package Management Version Conflict
**Status**: Needs Resolution

The project uses Central Package Management (`Directory.Packages.props`). Current build errors are due to:

1. **CommunityToolkit.Mvvm version resolution** - The project references this transitively through Blazing.Mvvm
2. **Source generator dependencies** - ObservableProperty and RelayCommand attributes require source generators to run

### Resolution Steps

To fix the build issues:

1. **Ensure proper package references**: The project should only need:
   - Blazing.Mvvm (project reference)
   - Blazing.Mvvm.Base (project reference)  
   - Blazing.Mvvm.Analyzers (analyzer reference)
   - Microsoft.EntityFrameworkCore.InMemory (for testing BLAZMVVM0013)

2. **CommunityToolkit.Mvvm** is provided transitively through Blazing.Mvvm.csproj

3. **Alternative for testing without source generators**:
   If source generators are problematic, ViewModels can be simplified to use manual property implementation instead of `[ObservableProperty]` attribute:
   
   ```csharp
   // Instead of:
   [ObservableProperty]
   private string _name;
   
   // Use:
   private string _name = string.Empty;
   public string Name
   {
       get => _name;
       set => SetProperty(ref _name, value);
   }
   ```

## Verification Steps

Once the build issues are resolved:

1. **Build the project**: `dotnet build samples\Blazing.Mvvm.AnalyzerTest\Blazing.Mvvm.AnalyzerTest.csproj`

2. **Check Error List in Visual Studio** for analyzer warnings/errors:
   - View ? Error List
   - Group by: File or Description
   - Look for BLAZMVVM00XX diagnostics

3. **Expected Results**:
   - **3 Errors** (BLAZMVVM0001, BLAZMVVM0002, BLAZMVVM0011)
   - **10 Warnings** (BLAZMVVM0003, 0004, 0005, 0006, 0008, 0009, 0013, 0015, 0016, 0017, 0020)
   - **6 Info** (BLAZMVVM0007, 0010, 0012, 0014, 0018, 0019, 0021)

## Testing the Analyzers

### In Visual Studio
1. Open the solution
2. Navigate to each ViewModel/Component file
3. Look for light bulbs (??) indicating code fixes are available
4. Review the **Error List** pane for all diagnostics

### In Visual Studio Code
1. Install C# Dev Kit extension
2. Open the workspace
3. Check **Problems** pane
4. Hover over squiggly lines for analyzer messages

### Command Line
```bash
# Build and see analyzer diagnostics
dotnet build samples\Blazing.Mvvm.AnalyzerTest\Blazing.Mvvm.AnalyzerTest.csproj /p:TreatWarningsAsErrors=false

# Generate diagnostic report
dotnet build /v:detailed > build-log.txt 2>&1
grep "BLAZMVVM" build-log.txt
```

## File Mapping Reference

### Analyzer ? File Mapping

- **BLAZMVVM0001**: `ViewModels/Test001ViewModel.cs`
- **BLAZMVVM0002**: `ViewModels/Test002ViewModel.cs`
- **BLAZMVVM0003**: `Components/Pages/Test003.razor`
- **BLAZMVVM0004**: `ViewModels/Test004And020ViewModel.cs`
- **BLAZMVVM0005**: `ViewModels/Test005ViewModel.cs`
- **BLAZMVVM0006**: `ViewModels/Test006ViewModel.cs` + `Components/Pages/Test006.razor`
- **BLAZMVVM0007**: `ViewModels/Test007ViewModel.cs`
- **BLAZMVVM0008**: `ViewModels/Test008ViewModel.cs`
- **BLAZMVVM0009**: `ViewModels/Test009ViewModel.cs`
- **BLAZMVVM0010**: `Components/Pages/Test010.razor`
- **BLAZMVVM0011**: `Components/Pages/Test011.razor`
- **BLAZMVVM0012**: `ViewModels/Test012ViewModel.cs`
- **BLAZMVVM0013**: `Components/Pages/Test013.razor`
- **BLAZMVVM0014**: `ViewModels/Test014ViewModel.cs`
- **BLAZMVVM0015**: `ViewModels/Test015And016ViewModel.cs`
- **BLAZMVVM0016**: `ViewModels/Test015And016ViewModel.cs`
- **BLAZMVVM0017**: `ViewModels/Test017ViewModel.cs`
- **BLAZMVVM0018**: `ViewModels/Test018ViewModel.cs`
- **BLAZMVVM0019**: `Components/Pages/Test019.razor`
- **BLAZMVVM0020**: `Components/Pages/Test004.razor`
- **BLAZMVVM0021**: `Components/Test021Component.razor`

## Recommendations

### 1. Fix Build Errors First
The build needs to complete successfully before analyzers will run properly. Priority fixes:
- Resolve CommunityToolkit.Mvvm package reference
- Ensure source generators execute successfully
- Verify all project references are correct

### 2. Simplify if Needed
For immediate analyzer testing, consider creating simpler ViewModels without source generator dependencies:
- Use manual property implementation instead of `[ObservableProperty]`
- Use manual command implementation instead of `[RelayCommand]`
- This will still trigger the analyzers while avoiding source generator issues

### 3. Run in Visual Studio
The analyzers work best in Visual Studio IDE where:
- Real-time diagnostics appear as you type
- Code fixes are available via light bulbs
- Error List provides comprehensive view of all issues

## Conclusion

This test project provides comprehensive coverage of all 21 Blazing.Mvvm analyzers. Each analyzer has at least one code example specifically designed to trigger it. Once the build issues are resolved, this project will serve as an excellent demonstration and validation suite for the analyzer functionality.

The project structure makes it easy to:
- Navigate to specific analyzer tests
- Understand what each analyzer detects
- See code fixes in action
- Validate analyzer behavior during development

All files follow clear naming conventions (Test### pattern) making it simple to find examples for any specific analyzer.
