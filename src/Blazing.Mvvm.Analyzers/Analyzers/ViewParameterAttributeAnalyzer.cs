using System.Collections.Immutable;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures properties marked with [ViewParameter] in ViewModels have corresponding [Parameter] properties in Views.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewParameterAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewParameterMismatch);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            // Use thread-safe collections for concurrent execution
            var viewModelParameters = new ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<IPropertySymbol>>(SymbolEqualityComparer.Default);
            var allViews = new ConcurrentBag<INamedTypeSymbol>();

            // Collect ViewModels and Views
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                
                // Collect ViewModels with [ViewParameter] properties
                if (IsViewModel(namedType))
                {
                    var viewParameterProps = GetViewParameterProperties(namedType);
                    if (viewParameterProps.Count > 0)
                    {
                        viewModelParameters[namedType] = new ConcurrentBag<IPropertySymbol>(viewParameterProps);
                        Debug.WriteLine($"[ViewParameterAnalyzer] Found ViewModel: {namedType.Name} with {viewParameterProps.Count} ViewParameter properties");
                    }
                }
                
                // Collect ALL Views (even without [Parameter] properties)
                if (IsViewComponent(namedType))
                {
                    allViews.Add(namedType);
                    Debug.WriteLine($"[ViewParameterAnalyzer] Found View: {namedType.Name}");
                }
            }, SymbolKind.NamedType);

            // At compilation end, validate ViewParameter properties
            compilationContext.RegisterCompilationEndAction(compilationEndContext =>
            {
                Debug.WriteLine($"[ViewParameterAnalyzer] CompilationEnd: Found {viewModelParameters.Count} ViewModels and {allViews.Count} Views");
                
                foreach (var kvp in viewModelParameters)
                {
                    var viewModelType = kvp.Key;
                    var viewParamProperties = kvp.Value;
                    
                    Debug.WriteLine($"[ViewParameterAnalyzer] Analyzing ViewModel: {viewModelType.Name}");
                    
                    // Find corresponding View component
                    var viewType = FindCorrespondingView(viewModelType, allViews);
                    if (viewType == null)
                    {
                        Debug.WriteLine($"[ViewParameterAnalyzer] No View found for ViewModel: {viewModelType.Name}");
                        continue;
                    }

                    Debug.WriteLine($"[ViewParameterAnalyzer] Found View: {viewType.Name} for ViewModel: {viewModelType.Name}");

                    // Get View's [Parameter] properties (from the view we found)
                    var viewParams = GetParameterPropertyNames(viewType);
                    Debug.WriteLine($"[ViewParameterAnalyzer] View {viewType.Name} has {viewParams.Count} Parameter properties");

                    // Check each [ViewParameter] property
                    foreach (var viewParamProperty in viewParamProperties)
                    {
                        Debug.WriteLine($"[ViewParameterAnalyzer] Checking ViewParameter: {viewParamProperty.Name}");
                        if (!viewParams.Contains(viewParamProperty.Name))
                        {
                            Debug.WriteLine($"[ViewParameterAnalyzer] DIAGNOSTIC: Property {viewParamProperty.Name} not found in View");
                            
                            // Report diagnostic on the ViewModel property
                            var diagnostic = Diagnostic.Create(
                                DiagnosticDescriptors.ViewParameterMismatch,
                                viewParamProperty.Locations[0],
                                viewParamProperty.Name);

                            compilationEndContext.ReportDiagnostic(diagnostic);
                        }
                        else
                        {
                            Debug.WriteLine($"[ViewParameterAnalyzer] OK: Property {viewParamProperty.Name} found in View");
                        }
                    }
                }
            });
        });
    }

    private static bool IsViewModel(INamedTypeSymbol typeSymbol)
    {
        // Check if name ends with "ViewModel"
        if (!typeSymbol.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return false;
        }

        // Must be a concrete class
        if (typeSymbol.TypeKind != TypeKind.Class || typeSymbol.IsAbstract)
        {
            return false;
        }

        return true;
    }

    private static bool IsViewComponent(INamedTypeSymbol typeSymbol)
    {
        // Check if inherits from MvvmComponentBase<TViewModel> or MvvmOwningComponentBase<TViewModel>
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            // Check both the original definition's display string and the name
            var originalDefinition = baseType.OriginalDefinition;
            var displayString = originalDefinition.ToDisplayString();
            
            // Check if it's MvvmComponentBase<TViewModel> or MvvmOwningComponentBase<TViewModel>
            if (displayString.StartsWith("Blazing.Mvvm.Components.MvvmComponentBase<") ||
                displayString.StartsWith("Blazing.Mvvm.Components.MvvmOwningComponentBase<") ||
                displayString == "Blazing.Mvvm.Components.MvvmComponentBase" ||
                displayString == "Blazing.Mvvm.Components.MvvmOwningComponentBase")
            {
                return true;
            }
            
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static List<IPropertySymbol> GetViewParameterProperties(INamedTypeSymbol typeSymbol)
    {
        var properties = new List<IPropertySymbol>();

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Check for [ViewParameter] attribute
            var hasViewParameter = property.GetAttributes().Any(attr =>
            {
                var attrName = attr.AttributeClass?.Name;
                return attrName == AnalyzerConstants.AttributeNames.ViewParameter ||
                       attrName == "ViewParameterAttribute";
            });

            if (hasViewParameter)
            {
                properties.Add(property);
            }
        }

        return properties;
    }

    private static HashSet<string> GetParameterPropertyNames(INamedTypeSymbol typeSymbol)
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Check for [Parameter] attribute from Microsoft.AspNetCore.Components
            var hasParameter = property.GetAttributes().Any(attr =>
            {
                var attrName = attr.AttributeClass?.Name;
                var attrNamespace = attr.AttributeClass?.ContainingNamespace?.ToDisplayString();
                
                return (attrName == "Parameter" || attrName == "ParameterAttribute") &&
                       (attrNamespace == "Microsoft.AspNetCore.Components" || attrNamespace == null);
            });

            if (hasParameter)
            {
                properties.Add(property.Name);
            }
        }

        return properties;
    }

    private static INamedTypeSymbol? FindCorrespondingView(
        INamedTypeSymbol viewModelType, 
        IEnumerable<INamedTypeSymbol> allViews)
    {
        // Search for View that uses this ViewModel as type parameter
        foreach (var viewType in allViews)
        {
            // Check if this View uses the ViewModel as type parameter
            if (IsViewForViewModel(viewType, viewModelType))
            {
                return viewType;
            }
        }

        return null;
    }

    private static bool IsViewForViewModel(INamedTypeSymbol viewType, INamedTypeSymbol viewModelType)
    {
        // Check if View's base type is MvvmComponentBase<TViewModel> where TViewModel matches
        var baseType = viewType.BaseType;
        while (baseType != null)
        {
            var originalDefinition = baseType.OriginalDefinition;
            var displayString = originalDefinition.ToDisplayString();
            
            // Check if this is MvvmComponentBase or MvvmOwningComponentBase
            if ((displayString.StartsWith("Blazing.Mvvm.Components.MvvmComponentBase<") ||
                 displayString.StartsWith("Blazing.Mvvm.Components.MvvmOwningComponentBase<") ||
                 displayString == "Blazing.Mvvm.Components.MvvmComponentBase" ||
                 displayString == "Blazing.Mvvm.Components.MvvmOwningComponentBase") &&
                baseType.TypeArguments.Length == 1)
            {
                var viewModelTypeArg = baseType.TypeArguments[0];
                return SymbolEqualityComparer.Default.Equals(viewModelTypeArg, viewModelType);
            }
            baseType = baseType.BaseType;
        }

        return false;
    }
}
