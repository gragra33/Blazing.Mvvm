# BLAZMVVM0011 Analyzer Fix Plan

**Analyzer:** MvvmNavLink Type Safety  
**Status:** NOT TRIGGERING  
**Date:** 2024  
**Goal:** Make BLAZMVVM0011 detect when `<MvvmNavLink TViewModel="X">` references an unregistered ViewModel in Razor files

---

## Problem Statement

The BLAZMVVM0011 analyzer is **fully implemented** but **NOT triggering** on Test011.razor despite correct test code:

```razor
<!-- Test011.razor - SHOULD trigger BLAZMVVM0011 -->
<MvvmNavLink TViewModel="UnregisteredTargetViewModel">
    Link to Unregistered ViewModel
</MvvmNavLink>
```

**Expected:** Error diagnostic on `UnregisteredTargetViewModel` (missing `[ViewModelDefinition]`)  
**Actual:** No diagnostic reported  
**Confirmed:** Other analyzers (BLAZMVVM0001, 0002, 0009, etc.) ARE working from the same project

---

## Root Cause Analysis

### Current Implementation Issues

1. **Operation-Based Approach Fails for Razor**
   - Current code uses `RegisterOperationAction(OperationKind.Invocation)` to detect `OpenComponent<MvvmNavLink<TViewModel>>()` calls
   - **Problem:** Operations may not be available for Razor-generated code at analyzer execution time
   - Razor source generators run in a different compilation phase than traditional analyzers

2. **How Razor Compiles MvvmNavLink**
   
   Razor source:
   ```razor
   <MvvmNavLink TViewModel="UnregisteredTargetViewModel">
   ```
   
   Compiles to:
   ```csharp
   __builder.OpenComponent<global::Blazing.Mvvm.Components.Routing.MvvmNavLink<
       UnregisteredTargetViewModel
   >>(6);
   ```

3. **MvvmNavLink is Generic**
   ```csharp
   public sealed class MvvmNavLink<TViewModel> : MvvmNavLinkBase
       where TViewModel : IViewModelBase
   ```
   
   - `TViewModel` in Razor is NOT a component parameter
   - It's a C# generic type parameter
   - Razor attribute `TViewModel="X"` → C# generic `MvvmNavLink<X>`

4. **Current Validator Logic**
   - ✅ Correctly requires BOTH inheritance from ViewModelBase AND `[ViewModelDefinition]` attribute
   - ✅ Uses same validation as BLAZMVVM0001
   - ❌ Detection mechanism doesn't see Razor-generated code

---

## Investigation Findings

### What We Know

1. **Analyzer IS being loaded** - Other analyzers from same project work (BLAZMVVM0001, 0002, 0009, 0015, 0016, 0017)
2. **Test ViewModel exists** - `UnregisteredTargetViewModel` in Test005ViewModel.cs (BLAZMVVM0002 detects it)
3. **Razor compilation succeeds** - No build errors
4. **Generated code exists** - `EmitCompilerGeneratedFiles=true` is enabled
5. **CompilationEnd approach is correct** - BLAZMVVM0004, 0006, 0020 use same pattern successfully

### What We Tried

| Approach | Result | Notes |
|----------|--------|-------|
| `RegisterOperationAction(OperationKind.Invocation)` | ❌ Not triggering | OpenComponent calls not seen |
| `RegisterOperationAction(OperationKind.ObjectCreation)` | ❌ Not triggering | No direct instantiation in Razor |
| `GeneratedCodeAnalysisFlags.Analyze` | ❌ Insufficient | Enabled but still not working |
| `CompilationEnd` custom tag | ✅ Added | Required but not sufficient |
| Debug logging (report ALL OpenComponent) | ❌ No diagnostics | Analyzer not seeing invocations |

---

## Required Fix: Syntax-Based Analysis

### Why Operations Don't Work

**Roslyn Compilation Pipeline:**
```
1. Parse syntax trees (Razor → C#)
2. Run source generators (Razor source generator)
3. Create semantic model
4. **Run analyzers** ← WE ARE HERE
5. Create operations from semantic model
```

**Problem:** By the time analyzers run, Razor-generated operations may not exist or be accessible via `RegisterOperationAction`.

### Solution: Analyze Syntax Directly

Instead of waiting for operations, analyze the **syntax trees** directly in `CompilationEndAction`:

```csharp
compilationContext.RegisterCompilationEndAction(compilationEndContext =>
{
    var validViewModelSet = new HashSet<INamedTypeSymbol>(validViewModels, SymbolEqualityComparer.Default);

    // Iterate through ALL syntax trees (including Razor-generated)
    foreach (var tree in compilationEndContext.Compilation.SyntaxTrees)
    {
        var semanticModel = compilationEndContext.Compilation.GetSemanticModel(tree);
        var root = tree.GetCompilationUnitRoot(compilationEndContext.CancellationToken);

        // Find GenericNameSyntax nodes like "MvvmNavLink<UnregisteredTargetViewModel>"
        var genericNames = root.DescendantNodes().OfType<GenericNameSyntax>();

        foreach (var genericName in genericNames)
        {
            // Check if this is MvvmNavLink<TViewModel>
            if (genericName.Identifier.ValueText != "MvvmNavLink")
                continue;

            // Get semantic info
            var typeInfo = semanticModel.GetTypeInfo(genericName, compilationEndContext.CancellationToken);
            
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                continue;

            // Verify it's from Blazing.Mvvm.Components.Routing namespace
            var ns = namedType.OriginalDefinition.ContainingNamespace?.ToDisplayString();
            if (ns != "Blazing.Mvvm.Components.Routing")
                continue;

            // Extract TViewModel type argument
            if (namedType.TypeArguments.Length == 0)
                continue;

            var viewModelType = namedType.TypeArguments[0] as INamedTypeSymbol;
            if (viewModelType == null)
                continue;

            // Validate against registered ViewModels
            if (!validViewModelSet.Contains(viewModelType, SymbolEqualityComparer.Default))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MvvmNavLinkInvalidViewModel,
                    genericName.GetLocation(),
                    viewModelType.Name);

                compilationEndContext.ReportDiagnostic(diagnostic);
            }
        }
    }
});
```

---

## Implementation Steps

### Step 1: Rewrite Detection Logic

**File:** `Blazing.Mvvm.Analyzers\Analyzers\MvvmNavLinkTypeSafetyAnalyzer.cs`

**Changes:**
1. ❌ Remove `RegisterOperationAction(OperationKind.Invocation)` - Not working
2. ❌ Remove `RegisterOperationAction(OperationKind.ObjectCreation)` - Not needed for Razor
3. ✅ Keep `RegisterSymbolAction` - Still needed to collect valid ViewModels
4. ✅ Rewrite `RegisterCompilationEndAction` to analyze syntax trees directly
5. ✅ Look for `GenericNameSyntax` with identifier "MvvmNavLink"
6. ✅ Use semantic model to resolve type info
7. ✅ Validate TViewModel type argument

### Step 2: Handle Edge Cases

**Scenarios to support:**

1. **Direct C# Usage** (if anyone does this):
   ```csharp
   var link = new MvvmNavLink<UnregisteredTargetViewModel>();
   ```
   - Covered by `ObjectCreationExpressionSyntax` analysis

2. **Razor Usage** (primary scenario):
   ```razor
   <MvvmNavLink TViewModel="UnregisteredTargetViewModel">
   ```
   - Covered by `GenericNameSyntax` analysis in generated code

3. **Fully Qualified Name**:
   ```razor
   <Blazing.Mvvm.Components.Routing.MvvmNavLink TViewModel="UnregisteredTargetViewModel">
   ```
   - Covered by namespace check

4. **MvvmKeyNavLink** (uses NavigationKey, not TViewModel):
   ```razor
   <MvvmKeyNavLink NavigationKey="someKey">
   ```
   - Skip if component name is "MvvmKeyNavLink"

### Step 3: Add Logging (Temporary)

To verify detection is working:

```csharp
// TODO: Remove after verification
var allMvvmNavLinks = root.DescendantNodes()
    .OfType<GenericNameSyntax>()
    .Where(g => g.Identifier.ValueText.Contains("MvvmNavLink"))
    .ToList();

if (allMvvmNavLinks.Any())
{
    // Log to Output window or create info diagnostic
    System.Diagnostics.Debug.WriteLine($"Found {allMvvmNavLinks.Count} MvvmNavLink references");
}
```

### Step 4: Update Tests

**File:** `Blazing.Mvvm.Analyzers.Tests\AnalyzerTests\MvvmNavLinkTypeSafetyAnalyzerTests.cs`

Add test cases:

```csharp
[Fact]
public async Task MvvmNavLink_UnregisteredViewModel_ReportsError()
{
    var test = new VerifyCS.Test
    {
        TestCode = @"
            using Blazing.Mvvm.Components.Routing;
            
            public class ValidViewModel : ViewModelBase { }
            
            [ViewModelDefinition]
            public class RegisteredViewModel : ViewModelBase { }
            
            public class UnregisteredViewModel : ViewModelBase { } // Missing attribute
            
            public class TestComponent
            {
                void BuildRenderTree()
                {
                    // This should trigger BLAZMVVM0011
                    var link = typeof(MvvmNavLink<{|#0:UnregisteredViewModel|}>);
                }
            }
        "
    };
    
    test.ExpectedDiagnostics.Add(
        VerifyCS.Diagnostic(DiagnosticDescriptors.MvvmNavLinkInvalidViewModel)
            .WithLocation(0)
            .WithArguments("UnregisteredViewModel"));
    
    await test.RunAsync();
}
```

---

## Verification Plan

### Test Checklist

After implementation, verify:

- [ ] Test011.razor triggers BLAZMVVM0011 for `UnregisteredTargetViewModel`
- [ ] Valid registered ViewModels do NOT trigger diagnostic
- [ ] MvvmKeyNavLink is ignored (not analyzed)
- [ ] C# direct usage (if any) is detected
- [ ] Fully qualified component names work
- [ ] Error message is clear and helpful
- [ ] Diagnostic location points to the ViewModel name
- [ ] No false positives on valid code

### Build Verification

```powershell
# 1. Clean build analyzers
dotnet clean Blazing.Mvvm.Analyzers\Blazing.Mvvm.Analyzers.csproj
dotnet build Blazing.Mvvm.Analyzers\Blazing.Mvvm.Analyzers.csproj

# 2. Clean build test project
dotnet clean samples\Blazing.Mvvm.AnalyzerTest\Blazing.Mvvm.AnalyzerTest.csproj
dotnet build samples\Blazing.Mvvm.AnalyzerTest\Blazing.Mvvm.AnalyzerTest.csproj --no-incremental

# 3. Check for BLAZMVVM0011
# Should see error on Test011.razor line 14
```

Expected output:
```
error BLAZMVVM0011: Type 'UnregisteredTargetViewModel' is not a valid registered ViewModel for MvvmNavLink
```

---

## Microsoft Documentation References

### Key Concepts

1. **Razor Compilation Process**
   - [Razor SDK](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/sdk)
   - Razor → C# via source generators (since .NET 6)
   - Generated code included in compilation

2. **Analyzer Execution Timing**
   - [Source Generators Overview](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
   - Source generators run DURING compilation
   - Analyzers can access generated code via `CompilationEndAction`

3. **Generic Type Parameters in Blazor**
   - [Generic Type Support](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/generic-type-support)
   - `@typeparam TItem` directive
   - Component parameters vs generic type arguments

4. **Analyzer Best Practices**
   - [Tutorial: Write your first analyzer](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix)
   - Use `CompilationEndAction` for cross-file analysis
   - `GeneratedCodeAnalysisFlags.Analyze` to include generated code

---

## Alternative Approaches (If Syntax Analysis Fails)

### Option A: Razor Source Generator

If syntax-based approach still doesn't work, consider creating a **Razor-specific source generator** that:
- Runs during Razor compilation
- Analyzes `.razor` files directly
- Reports diagnostics via different mechanism

**Pros:**
- Direct access to Razor syntax
- Runs at correct compilation phase

**Cons:**
- More complex implementation
- Different architecture from other analyzers

### Option B: Post-Build Analyzer

Create a post-build task that:
- Analyzes generated Razor C# files
- Reports issues as warnings

**Pros:**
- Guaranteed to see generated code

**Cons:**
- Runs after compilation
- Not integrated with IDE experience

### Option C: Hybrid Approach

Combine both:
1. Syntax analysis for most cases
2. Fallback to examining Razor files directly via `AdditionalFiles`

---

## Success Criteria

The fix is complete when:

1. ✅ BLAZMVVM0011 triggers on Test011.razor
2. ✅ Diagnostic shows on correct line (line 14, column with ViewModel name)
3. ✅ Error message is clear: `"Type 'UnregisteredTargetViewModel' is not a valid registered ViewModel for MvvmNavLink"`
4. ✅ No false positives on valid ViewModels
5. ✅ Works in both Visual Studio and command-line builds
6. ✅ Unit tests pass
7. ✅ Performance is acceptable (no noticeable build slowdown)

---

## Implementation Priority

### Critical (Must Fix)
- [ ] Rewrite detection to use syntax-based analysis
- [ ] Test with Test011.razor
- [ ] Verify no regression on other analyzers

### Important (Should Fix)
- [ ] Add unit tests for all scenarios
- [ ] Optimize performance if needed
- [ ] Update documentation

### Nice to Have (Could Fix Later)
- [ ] Add code fix to suggest adding `[ViewModelDefinition]`
- [ ] Support for namespace aliases
- [ ] Better error messages with suggestions

---

## Next Steps

1. **Immediate:** Implement syntax-based detection in `CompilationEndAction`
2. **Test:** Verify Test011.razor triggers diagnostic
3. **Debug:** If still not working, add detailed logging to understand what's happening
4. **Iterate:** Adjust approach based on findings
5. **Document:** Update progress file with results

---

## Notes for Future Maintainers

- **Key Insight:** Razor components with generic type parameters compile to C# generics, NOT component parameters
- **Timing Matters:** Analyzers must use `CompilationEndAction` to see Razor-generated code
- **Syntax Over Operations:** For Razor scenarios, syntax analysis is more reliable than operation analysis
- **Test Everything:** Always test with actual Razor files, not just C# simulations

---

**Status:** Ready for implementation  
**Assigned To:** Developer  
**Estimated Effort:** 2-4 hours  
**Dependencies:** None (all infrastructure in place)
