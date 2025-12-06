using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects @page directives without corresponding ViewModels.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RouteViewModelMappingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.PageMissingViewModel);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var viewModels = new HashSet<string>();

            // Collect all ViewModel names
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                if (namedType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
                {
                    viewModels.Add(namedType.Name);
                }
            }, SymbolKind.NamedType);

            // Analyze Razor components with @page directive
            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                AnalyzeRazorComponent(nodeContext, viewModels);
            }, SyntaxKind.ClassDeclaration);
        });
    }

    private static void AnalyzeRazorComponent(
        SyntaxNodeAnalysisContext context,
        HashSet<string> viewModels)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol == null)
        {
            return;
        }

        // Check if this is a Blazor component (inherits from ComponentBase)
        var inheritsComponentBase = InheritsFromComponentBase(classSymbol);
        if (!inheritsComponentBase)
        {
            return;
        }

        // Check for @page attribute (represented as RouteAttribute in C#)
        var hasRouteAttribute = classSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.Name == "RouteAttribute");

        if (!hasRouteAttribute)
        {
            return;
        }

        // Check if component has associated ViewModel
        // Look for MvvmComponentBase<TViewModel> inheritance
        var hasMvvmBase = classSymbol.BaseType?.Name == "MvvmComponentBase" ||
                          classSymbol.BaseType?.Name == "MvvmOwningComponentBase";

        if (hasMvvmBase)
        {
            return; // Already has ViewModel integration
        }

        // Check if there's a matching ViewModel by naming convention
        var componentName = classSymbol.Name;
        var expectedViewModelName = $"{componentName}ViewModel";

        if (viewModels.Contains(expectedViewModelName))
        {
            return; // ViewModel exists but not used
        }

        // Report diagnostic suggesting ViewModel creation
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.PageMissingViewModel,
            classDeclaration.Identifier.GetLocation(),
            componentName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool InheritsFromComponentBase(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "ComponentBase" || 
                baseType.Name == "MvvmComponentBase" ||
                baseType.Name == "MvvmOwningComponentBase")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
