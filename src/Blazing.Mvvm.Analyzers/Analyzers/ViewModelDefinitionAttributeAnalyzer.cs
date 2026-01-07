using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures ViewModels have the [ViewModelDefinition] attribute for proper DI registration.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewModelDefinitionAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewModelDefinitionMissing);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Skip abstract classes first (before any inheritance checks)
        if (namedTypeSymbol.IsAbstract)
        {
            return;
        }

        // Skip nested types - they are typically helper classes within a ViewModel
        if (namedTypeSymbol.ContainingType != null)
        {
            return;
        }

        // Skip test infrastructure classes
        if (IsTestInfrastructure(namedTypeSymbol))
        {
            return;
        }

        // IMPORTANT: Check if this inherits from Blazing.Mvvm ViewModelBase FIRST
        // This ensures we only analyze ViewModels from Blazing.Mvvm.ComponentModel namespace
        if (!InheritsFromBlazingMvvmViewModelBase(namedTypeSymbol))
        {
            return; // Not a Blazing.Mvvm ViewModel, skip analysis
        }

        // NOW check if it's a non-Blazor UI ViewModel (WPF/WinForms/Avalonia without Blazor)
        // This must come AFTER confirming it's a Blazing.Mvvm ViewModel
        if (IsNonBlazorUIViewModel(namedTypeSymbol, context.Compilation))
        {
            return; // Desktop UI ViewModel using Blazing.Mvvm for INotifyPropertyChanged only
        }

        // Check if the ViewModelDefinition attribute is present
        var viewModelDefinitionAttribute = context.Compilation.GetTypeByMetadataName(
            AnalyzerConstants.TypeNames.ViewModelDefinitionAttribute);

        if (viewModelDefinitionAttribute == null)
        {
            return;
        }

        var hasAttribute = namedTypeSymbol.GetAttributes()
            .Any(a =>
            {
                if (a.AttributeClass == null)
                {
                    return false;
                }

                var attributeClass = a.AttributeClass;
                while (attributeClass != null)
                {
                    if (SymbolEqualityComparer.Default.Equals(attributeClass, viewModelDefinitionAttribute))
                    {
                        return true;
                    }
                    attributeClass = attributeClass.BaseType;
                }

                return false;
            });

        if (hasAttribute)
        {
            return;
        }

        // Report diagnostic
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ViewModelDefinitionMissing,
            namedTypeSymbol.Locations[0],
            namedTypeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Determines if a ViewModel is in a non-Blazor UI project (WPF, WinForms, Avalonia)
    /// where ViewModelBase is used only for INotifyPropertyChanged, not Blazor MVVM DI.
    /// </summary>
    /// <remarks>
    /// This method is only called AFTER confirming the ViewModel inherits from Blazing.Mvvm ViewModelBase.
    /// It checks if the ViewModel is in a desktop UI context without Blazor Components.
    /// </remarks>
    private static bool IsNonBlazorUIViewModel(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        // Check namespace indicators for desktop UI frameworks
        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        
        // Desktop UI ViewModels in framework-specific namespaces
        // These projects may use Blazing.Mvvm.ViewModelBase for INotifyPropertyChanged,
        // but don't use the Blazor MVVM DI pattern
        if (namespaceName.IndexOf(".Wpf", StringComparison.OrdinalIgnoreCase) >= 0 ||
            namespaceName.IndexOf(".WinForms", StringComparison.OrdinalIgnoreCase) >= 0 ||
            namespaceName.IndexOf(".Avalonia", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            // Only skip if this project doesn't reference Blazor components
            return !HasBlazorComponentsReference(compilation);
        }

        // Check assembly references to determine project type
        var referencedAssemblies = compilation.References
            .Select(r => compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol)
            .Where(a => a != null)
            .Select(a => a!.Name)
            .ToList();

        var hasWpfReference = referencedAssemblies.Any(name => 
            name.StartsWith("PresentationFramework", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("PresentationCore", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase));

        var hasWinFormsReference = referencedAssemblies.Any(name =>
            name.StartsWith("System.Windows.Forms", StringComparison.OrdinalIgnoreCase));

        var hasAvaloniaReference = referencedAssemblies.Any(name =>
            name.StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));

        var hasBlazorComponents = HasBlazorComponentsReference(compilation);

        // If it's a WPF/WinForms/Avalonia project WITHOUT Blazor Components,
        // the ViewModel uses Blazing.Mvvm.ViewModelBase for INotifyPropertyChanged only
        if ((hasWpfReference || hasWinFormsReference || hasAvaloniaReference) && !hasBlazorComponents)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the ViewModel specifically inherits from Blazing.Mvvm.ComponentModel.ViewModelBase
    /// (not just any ViewModelBase).
    /// </summary>
    /// <remarks>
    /// Handles all Blazing.Mvvm ViewModel base classes:
    /// - ViewModelBase
    /// - RecipientViewModelBase
    /// - RecipientViewModelBase&lt;TMessage&gt;
    /// - ValidatorViewModelBase
    /// </remarks>
    private static bool InheritsFromBlazingMvvmViewModelBase(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            var baseTypeNamespace = baseType.ContainingNamespace?.ToDisplayString();
            
            // Must be in Blazing.Mvvm.ComponentModel namespace
            if (baseTypeNamespace == "Blazing.Mvvm.ComponentModel")
            {
                // Check for all ViewModel base class names
                if (baseType.Name == "ViewModelBase" ||
                    baseType.Name == "RecipientViewModelBase" ||
                    baseType.Name == "ValidatorViewModelBase")
                {
                    return true;
                }
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Checks if the compilation references Blazor Components (Microsoft.AspNetCore.Components).
    /// </summary>
    private static bool HasBlazorComponentsReference(Compilation compilation)
    {
        var referencedAssemblies = compilation.References
            .Select(r => compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol)
            .Where(a => a != null)
            .Select(a => a!.Name)
            .ToList();

        return referencedAssemblies.Any(name =>
            name.StartsWith("Microsoft.AspNetCore.Components", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines if a ViewModel is part of test infrastructure and should be excluded from analysis.
    /// </summary>
    private static bool IsTestInfrastructure(INamedTypeSymbol typeSymbol)
    {
        // Check if the type is in a namespace containing "Test" or "Fakes"
        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        
        if (namespaceName.IndexOf("Tests", StringComparison.OrdinalIgnoreCase) >= 0 ||
            namespaceName.IndexOf("Fakes", StringComparison.OrdinalIgnoreCase) >= 0 ||
            namespaceName.IndexOf("Infrastructure", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            // Additional check: if it's internal sealed, it's likely a test helper
            if (typeSymbol.DeclaredAccessibility == Accessibility.Internal && typeSymbol.IsSealed)
            {
                return true;
            }
        }

        return false;
    }
}
