# Blazing.Mvvm.Analyzers

Roslyn analyzers for [Blazing.Mvvm](https://github.com/gragra33/Blazing.Mvvm) to help developers follow MVVM best practices and catch common mistakes at compile time.

## Installation

The analyzers are available as a **separate, optional NuGet package**. Install it alongside Blazing.Mvvm:

```bash
dotnet add package Blazing.Mvvm
dotnet add package Blazing.Mvvm.Analyzers
```

Or via Package Manager Console:

```powershell
Install-Package Blazing.Mvvm
Install-Package Blazing.Mvvm.Analyzers
```

> **Note**: The analyzers package is **optional**. You can use Blazing.Mvvm without the analyzers if you prefer.

## Analyzers

This package includes **20 analyzers** to help you write better Blazing.Mvvm code:

### Phase 1: Core MVVM Pattern (High Priority)

- **[BLAZMVVM0001](../../docs/analyzers/BLAZMVVM0001.md)**: ViewModelBase Inheritance - Ensures ViewModels inherit from the correct base class
- **[BLAZMVVM0002](../../docs/analyzers/BLAZMVVM0002.md)**: ViewModelDefinition Attribute - Ensures proper ViewModel registration for DI
- **[BLAZMVVM0003](../../docs/analyzers/BLAZMVVM0003.md)**: MvvmComponentBase Usage - Ensures proper View-ViewModel binding in components
- **[BLAZMVVM0005](../../docs/analyzers/BLAZMVVM0005.md)**: Navigation Type Safety - Validates NavigateTo<TViewModel> calls reference valid routes
- **[BLAZMVVM0013](../../docs/analyzers/BLAZMVVM0013.md)**: MvvmOwningComponentBase Usage - Detects when scoped services require owned scope
- **[BLAZMVVM0017](../../docs/analyzers/BLAZMVVM0017.md)**: RelayCommand Async Pattern - Prevents async void methods with [RelayCommand]

### Phase 2: Best Practices (Medium Priority)

- **[BLAZMVVM0004](../../docs/analyzers/BLAZMVVM0004.md)**: ViewParameter Attribute - Validates ViewParameter and Parameter property matching
- **[BLAZMVVM0008](../../docs/analyzers/BLAZMVVM0008.md)**: Observable Property - Ensures proper usage of [ObservableProperty] and SetProperty
- **[BLAZMVVM0015](../../docs/analyzers/BLAZMVVM0015.md)**: Dispose Pattern - Detects ViewModels requiring IDisposable implementation
- **[BLAZMVVM0016](../../docs/analyzers/BLAZMVVM0016.md)**: Messenger Registration Lifetime - Detects messenger registrations without cleanup
- **[BLAZMVVM0018](../../docs/analyzers/BLAZMVVM0018.md)**: NotifyPropertyChangedFor - Suggests notifications for computed properties
- **[BLAZMVVM0020](../../docs/analyzers/BLAZMVVM0020.md)**: Route Parameter Binding - Validates route parameters have corresponding properties

### Phase 3: Code Quality (Info Level)

- **[BLAZMVVM0007](../../docs/analyzers/BLAZMVVM0007.md)**: Lifecycle Method Override - Suggests proper lifecycle method usage
- **[BLAZMVVM0010](../../docs/analyzers/BLAZMVVM0010.md)**: Route-ViewModel Mapping - Ensures Pages have corresponding ViewModels
- **[BLAZMVVM0012](../../docs/analyzers/BLAZMVVM0012.md)**: Command Pattern - Encourages proper [RelayCommand] usage over public methods
- **[BLAZMVVM0014](../../docs/analyzers/BLAZMVVM0014.md)**: StateHasChanged Overuse - Detects unnecessary StateHasChanged() calls
- **[BLAZMVVM0019](../../docs/analyzers/BLAZMVVM0019.md)**: CascadingParameter vs Inject - Suggests [Inject] for DI services

### Phase 4: Advanced (Specialized)

- **[BLAZMVVM0006](../../docs/analyzers/BLAZMVVM0006.md)**: ViewModelKey Consistency - Ensures ViewModelKey values match navigation keys
- **[BLAZMVVM0009](../../docs/analyzers/BLAZMVVM0009.md)**: Service Injection - Validates constructor parameters are registered services
- **[BLAZMVVM0011](../../docs/analyzers/BLAZMVVM0011.md)**: MvvmNavLink Type Safety - Validates MvvmNavLink TViewModel parameter

## Code Fix Providers

The package includes **13 code fix providers** for automatic corrections:

### Core MVVM Pattern Fixes
1. **ViewModelBaseInheritanceCodeFixProvider** - Adds ViewModelBase inheritance
2. **ViewModelDefinitionAttributeCodeFixProvider** - Adds [ViewModelDefinition] attribute
3. **MvvmComponentBaseUsageCodeFixProvider** - Replaces ComponentBase with MvvmComponentBase<TViewModel>
4. **MvvmOwningComponentBaseUsageCodeFixProvider** - Replaces MvvmComponentBase with MvvmOwningComponentBase
5. **RelayCommandAsyncPatternCodeFixProvider** - Converts async void to async Task

### Best Practices Fixes
6. **RouteParameterBindingCodeFixProvider** - Generates missing [Parameter] or [ViewParameter] properties
7. **DisposePatternCodeFixProvider** - Adds IDisposable implementation with cleanup
8. **MessengerRegistrationLifetimeCodeFixProvider** - Adds Dispose with Unregister or OnActivated pattern
9. **NotifyPropertyChangedForCodeFixProvider** - Adds [NotifyPropertyChangedFor] attribute

### Code Quality Fixes
10. **LifecycleMethodOverrideCodeFixProvider** - Adds OnInitializedAsync override method
11. **CommandPatternCodeFixProvider** - Adds [RelayCommand] attribute and makes method private
12. **StateHasChangedOveruseCodeFixProvider** - Removes unnecessary StateHasChanged() calls
13. **CascadingParameterVsInjectCodeFixProvider** - Replaces [CascadingParameter] with [Inject]

## Severity Levels

- **Error**: Must be fixed (BLAZMVVM0003, 0005, 0011)
- **Warning**: Should be addressed (BLAZMVVM0001, 0002, 0004, 0013, 0015, 0016, 0017, 0020)
- **Info**: Consider improvements (BLAZMVVM0006, 0007, 0008, 0009, 0010, 0012, 0014, 0018, 0019)

## Quick Start

After installing the package, analyzers will automatically run during compilation. Look for diagnostic messages starting with "BLAZMVVM" in your IDE:

```csharp
// ⚠ Warning BLAZMVVM0001
public class MyViewModel // Missing base class
{
}

// ✓ Correct
public class MyViewModel : ViewModelBase
{
}
```

Many diagnostics include quick fixes available via the lightbulb icon (💡) or `Ctrl+.` shortcut.

## Usage Examples

### In Sample Projects

To use the analyzers in sample projects, add a package reference:

```xml
<ItemGroup>
  <PackageReference Include="Blazing.Mvvm.Analyzers" Version="*" />
</ItemGroup>
```

Or use a project reference during development:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\Blazing.Mvvm.Analyzers\Blazing.Mvvm.Analyzers.csproj" 
                    PrivateAssets="all" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### Disabling Specific Analyzers

If you want to disable specific analyzers, add them to your `.editorconfig`:

```ini
# Disable Command Pattern analyzer
dotnet_diagnostic.BLAZMVVM0012.severity = none
```

## Documentation

For detailed information about each analyzer, click the links above or visit the [analyzers documentation folder](../../docs/analyzers/).

For more information about Blazing.Mvvm, see the [main documentation](https://github.com/gragra33/Blazing.Mvvm).

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

MIT License - see the LICENSE file for details.
