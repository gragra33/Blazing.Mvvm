using System.Collections.Concurrent;
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
            var viewModelKeys = new ConcurrentDictionary<string, Location>();
            var usedKeys = new ConcurrentDictionary<string, byte>();

            // Collect all ViewModelKey attributes
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                if (!namedType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
                {
                    return;
                }

                var viewModelKeyAttr = namedType.GetAttributes().FirstOrDefault(IsViewModelKeyAttribute);

                if (viewModelKeyAttr != null && viewModelKeyAttr.ConstructorArguments.Length > 0)
                {
                    var key = viewModelKeyAttr.ConstructorArguments[0].Value?.ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        viewModelKeys[key!] = GetDiagnosticLocation(namedType, viewModelKeyAttr);
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
                    if (!usedKeys.ContainsKey(kvp.Key))
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.ViewModelKeyInconsistent,
                            kvp.Value,
                            kvp.Key);

                        endContext.ReportDiagnostic(diagnostic);
                    }
                }
            });
        });
    }

    private static void AnalyzeNavigationCall(
        SyntaxNodeAnalysisContext context,
        ConcurrentDictionary<string, byte> usedKeys)
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
                usedKeys.TryAdd(key, 0);
            }
        }
    }

    private static bool IsViewModelKeyAttribute(AttributeData attribute)
    {
        var attributeName = attribute.AttributeClass?.Name;
        var fullName = attribute.AttributeClass?.ToDisplayString();

        return attributeName == AnalyzerConstants.AttributeNames.ViewModelKey ||
               attributeName == $"{AnalyzerConstants.AttributeNames.ViewModelKey}Attribute" ||
               fullName == AnalyzerConstants.TypeNames.ViewModelKeyAttribute;
    }

    private static Location GetDiagnosticLocation(INamedTypeSymbol namedType, AttributeData attribute)
    {
        if (attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax attributeSyntax)
        {
            var argumentLocation = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault()?.GetLocation();
            if (argumentLocation is not null)
            {
                return argumentLocation;
            }

            return attributeSyntax.GetLocation();
        }

        return namedType.Locations[0];
    }
}
