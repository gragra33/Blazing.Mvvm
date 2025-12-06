using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects properties that should use [ObservableProperty] or SetProperty for proper change notification.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ObservablePropertyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ObservablePropertyMissing);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;
        var containingType = propertySymbol.ContainingType;

        // Only analyze properties in ViewModels
        if (!containingType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return;
        }

        // Skip properties that already have ObservableProperty attribute
        var hasObservableProperty = propertySymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.Name == "ObservablePropertyAttribute" ||
            attr.AttributeClass?.Name == "ObservableProperty");

        if (hasObservableProperty)
        {
            return;
        }

        // Skip read-only properties (computed properties)
        if (propertySymbol.IsReadOnly || propertySymbol.SetMethod == null)
        {
            return;
        }

        // Skip auto-properties (they're just data holders - typically ViewParameters)
        // We only care about properties with explicit backing fields and custom setters
        if (propertySymbol.SetMethod.DeclaringSyntaxReferences.Length == 0)
        {
            return;
        }

        // Get the property declaration syntax
        var propertySyntax = propertySymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);
        if (propertySyntax is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        // Check if the property has an accessor list with a setter
        var accessorList = propertyDeclaration.AccessorList;
        if (accessorList == null)
        {
            // Expression-bodied setter (e.g., set => _field = value;)
            if (propertyDeclaration.ExpressionBody != null)
            {
                return; // Skip expression-bodied properties
            }
            return;
        }

        // Find the set accessor
        var setAccessor = accessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
        if (setAccessor == null)
        {
            return;
        }

        // Check if setter has a body or is expression-bodied
        string setterContent = string.Empty;
        
        if (setAccessor.Body != null)
        {
            setterContent = setAccessor.Body.ToString();
        }
        else if (setAccessor.ExpressionBody != null)
        {
            setterContent = setAccessor.ExpressionBody.ToString();
        }
        else
        {
            // Auto-property setter (no body)
            return;
        }

        // Check if the setter calls SetProperty, OnPropertyChanged, or raises PropertyChanged
        if (setterContent.Contains("SetProperty") || 
            setterContent.Contains("OnPropertyChanged") ||
            setterContent.Contains("PropertyChanged?.Invoke"))
        {
            return;
        }

        // Report diagnostic - property has custom setter without change notification
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ObservablePropertyMissing,
            propertySymbol.Locations[0],
            propertySymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }
}
