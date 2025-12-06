using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects async void methods marked with [RelayCommand] attribute.
/// Suggests using async Task instead for proper error handling and AsyncRelayCommand.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RelayCommandAsyncPatternAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.AsyncVoidRelayCommand);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        // Check if method is async void
        if (!IsAsyncVoid(methodDeclaration))
            return;

        // Check if method has [RelayCommand] attribute
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
        if (methodSymbol == null)
            return;

        if (!HasRelayCommandAttribute(methodSymbol))
            return;

        // Report diagnostic
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.AsyncVoidRelayCommand,
            methodDeclaration.Identifier.GetLocation(),
            methodSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsAsyncVoid(MethodDeclarationSyntax methodDeclaration)
    {
        // Check if method has async modifier
        var hasAsync = methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword);
        if (!hasAsync)
            return false;

        // Check if return type is void
        var returnType = methodDeclaration.ReturnType.ToString();
        return returnType == "void";
    }

    private static bool HasRelayCommandAttribute(IMethodSymbol methodSymbol)
    {
        foreach (var attribute in methodSymbol.GetAttributes())
        {
            var attributeName = attribute.AttributeClass?.Name;
            if (attributeName == "RelayCommand" || attributeName == "RelayCommandAttribute")
            {
                // Verify it's from CommunityToolkit.Mvvm
                var ns = attribute.AttributeClass?.ContainingNamespace.ToString();
                if (ns != null && ns.StartsWith("CommunityToolkit.Mvvm"))
                    return true;
            }
        }

        return false;
    }
}
