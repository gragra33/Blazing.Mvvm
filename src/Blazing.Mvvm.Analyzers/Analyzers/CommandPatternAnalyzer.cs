using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects methods that should be exposed as RelayCommand instead of direct method calls.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommandPatternAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MethodShouldBeCommand);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        var containingType = methodSymbol.ContainingType;

        // Only analyze methods in ViewModels
        if (!containingType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return;
        }

        // Skip special methods (constructors, finalizers, operators, etc.)
        if (methodSymbol.MethodKind != MethodKind.Ordinary)
        {
            return;
        }

        // Skip override methods (they're implementing base class behavior)
        if (methodSymbol.IsOverride)
        {
            return;
        }

        // Skip property accessors
        if (methodSymbol.AssociatedSymbol != null)
        {
            return;
        }

        // Only analyze public methods (UI-bindable)
        if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        // Skip if already has RelayCommand attribute
        var hasRelayCommand = methodSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.Name == "RelayCommandAttribute" ||
            attr.AttributeClass?.Name == "RelayCommand");

        if (hasRelayCommand)
        {
            return;
        }

        // Check if method returns void or Task (command-like signature)
        var returnsVoidOrTask = methodSymbol.ReturnsVoid ||
                                methodSymbol.ReturnType.Name == "Task" ||
                                methodSymbol.ReturnType.Name == "ValueTask";

        if (!returnsVoidOrTask)
        {
            return;
        }

        // Suggest using RelayCommand for public methods with command-like signatures
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.MethodShouldBeCommand,
            methodSymbol.Locations[0],
            methodSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }
}
