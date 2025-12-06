using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures ViewModelKey attribute values are consistent with keyed navigation calls.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewModelKeyConsistencyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewModelKeyInconsistent);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var viewModelKeys = new Dictionary<string, INamedTypeSymbol>();
            var usedKeys = new HashSet<string>();

            // Collect all ViewModelKey attributes
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                if (!namedType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
                {
                    return;
                }

                var viewModelKeyAttr = namedType.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass?.Name == AnalyzerConstants.AttributeNames.ViewModelKey);

                if (viewModelKeyAttr != null && viewModelKeyAttr.ConstructorArguments.Length > 0)
                {
                    var key = viewModelKeyAttr.ConstructorArguments[0].Value?.ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        viewModelKeys[key!] = namedType;
                    }
                }
            }, SymbolKind.NamedType);

            // Collect keyed navigation calls
            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                AnalyzeNavigationCall(nodeContext, usedKeys);
            }, SyntaxKind.InvocationExpression);

            // Report unused keys at end of compilation
            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (var kvp in viewModelKeys)
                {
                    if (!usedKeys.Contains(kvp.Key))
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.ViewModelKeyInconsistent,
                            kvp.Value.Locations[0],
                            kvp.Key);

                        endContext.ReportDiagnostic(diagnostic);
                    }
                }
            });
        });
    }

    private static void AnalyzeNavigationCall(
        SyntaxNodeAnalysisContext context,
        HashSet<string> usedKeys)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check for NavigateTo(key) calls
        var methodName = invocation.Expression.ToString();
        if (!methodName.Contains("NavigateTo"))
        {
            return;
        }

        // Check if there's a string argument (the key)
        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            var firstArg = invocation.ArgumentList.Arguments[0].Expression;
            
            // Try to extract string literal key
            if (firstArg is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var key = literal.Token.ValueText;
                usedKeys.Add(key);
            }
        }
    }
}
