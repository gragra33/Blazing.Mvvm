using System.Collections.Immutable;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects ViewModelKey mismatches between components and their ViewModels.
/// Ensures that a component's [ViewModelKey("X")] attribute matches its ViewModel's ViewModelDefinition(Key="X").
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewModelKeyConsistencyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewModelKeyInconsistent);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            // Map: ViewModel fully-qualified name -> Key
            var viewModelKeys = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            var keyLocations = new ConcurrentDictionary<string, Location>(StringComparer.Ordinal);
            var navigationKeys = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

            // Collect all ViewModels and their keys from ViewModelDefinition(Key="...") / [ViewModelKey("...")]
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;

                if (TryGetDeclaredKey(namedType, out var key, out var location))
                {
                    var fqn = namedType.ToDisplayString();
                    viewModelKeys[fqn] = key;
                    if (location != null)
                    {
                        keyLocations[fqn] = location;
                    }
                }
            }, SymbolKind.NamedType);

            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                if (TryGetNavigationKey((InvocationExpressionSyntax)nodeContext.Node, nodeContext, out var navigationKey))
                {
                    navigationKeys.TryAdd(navigationKey, 0);
                }
            }, SyntaxKind.InvocationExpression);

            // Analyze Razor-generated components for ViewModelKey attribute mismatches
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                AnalyzeComponent(symbolContext, namedType, viewModelKeys);
            }, SymbolKind.NamedType);

            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                if (navigationKeys.IsEmpty)
                {
                    return;
                }

                foreach (var pair in viewModelKeys)
                {
                    if (navigationKeys.ContainsKey(pair.Value))
                    {
                        continue;
                    }

                    if (!keyLocations.TryGetValue(pair.Key, out var location))
                    {
                        continue;
                    }

                    endContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.ViewModelKeyInconsistent,
                        location,
                        pair.Value));
                }
            });
        });
    }

    private static bool TryGetDeclaredKey(INamedTypeSymbol namedType, out string key, out Location? location)
    {
        foreach (var attribute in namedType.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is AnalyzerConstants.AttributeNames.ViewModelDefinition or "ViewModelDefinitionAttribute")
            {
                foreach (var namedArg in attribute.NamedArguments)
                {
                    if (namedArg.Key == "Key" && namedArg.Value.Value is string keyValue && !string.IsNullOrEmpty(keyValue))
                    {
                        key = keyValue;
                        location = GetAttributeValueLocation(attribute) ?? attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedType.Locations.FirstOrDefault();
                        return true;
                    }
                }
            }

            if (attribute.AttributeClass?.Name is AnalyzerConstants.AttributeNames.ViewModelKey or "ViewModelKeyAttribute" &&
                attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is string attributeKey &&
                !string.IsNullOrEmpty(attributeKey))
            {
                key = attributeKey;
                location = GetAttributeValueLocation(attribute) ?? attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedType.Locations.FirstOrDefault();
                return true;
            }
        }

        key = string.Empty;
        location = null;
        return false;
    }

    private static bool TryGetNavigationKey(
        InvocationExpressionSyntax invocationExpression,
        SyntaxNodeAnalysisContext context,
        out string navigationKey)
    {
        navigationKey = string.Empty;

        if (invocationExpression.Expression.ToString().IndexOf("NavigateTo", StringComparison.Ordinal) < 0 ||
            invocationExpression.ArgumentList.Arguments.Count == 0)
        {
            return false;
        }

        var firstArgument = invocationExpression.ArgumentList.Arguments[0].Expression;
        var constantValue = context.SemanticModel.GetConstantValue(firstArgument, context.CancellationToken);
        if (!constantValue.HasValue || constantValue.Value is not string key || string.IsNullOrEmpty(key))
        {
            return false;
        }

        navigationKey = key;
        return true;
    }

    private static Location? GetAttributeValueLocation(AttributeData attribute)
    {
        if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
        {
            return null;
        }

        var expression = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
        if (expression is LiteralExpressionSyntax literalExpression && literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
        {
            var span = literalExpression.Token.Span;
            if (span.Length >= 2)
            {
                return Location.Create(literalExpression.SyntaxTree, TextSpan.FromBounds(span.Start + 1, span.End - 1));
            }
        }

        return expression?.GetLocation();
    }

    private static void AnalyzeComponent(
        SymbolAnalysisContext symbolContext,
        INamedTypeSymbol namedType,
        ConcurrentDictionary<string, string> viewModelKeys)
    {
        // Skip non-components
        if (!InheritsFromComponentBase(namedType))
        {
            return;
        }

        // Look for ViewModelKey attribute on component
        var vmKeyAttr = namedType.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.Name is AnalyzerConstants.AttributeNames.ViewModelKey or "ViewModelKeyAttribute");

        if (vmKeyAttr == null || vmKeyAttr.ConstructorArguments.Length == 0)
        {
            return;
        }

        // Extract component's ViewModelKey value
        if (vmKeyAttr.ConstructorArguments[0].Value is not string componentKey || string.IsNullOrEmpty(componentKey))
        {
            return;
        }

        CheckViewModelKeyMatch(symbolContext, namedType, vmKeyAttr, componentKey, viewModelKeys);
    }

    private static void CheckViewModelKeyMatch(
        SymbolAnalysisContext symbolContext,
        INamedTypeSymbol namedType,
        AttributeData vmKeyAttr,
        string componentKey,
        ConcurrentDictionary<string, string> viewModelKeys)
    {
        // Find the ViewModel type from MvvmComponentBase<TViewModel> inheritance
        var viewModelType = ExtractViewModelTypeFromComponent(namedType);
        if (viewModelType == null)
        {
            return;
        }

        // Look up the ViewModel's key
        var viewModelFqn = viewModelType.ToDisplayString();
        if (!viewModelKeys.TryGetValue(viewModelFqn, out var viewModelKey))
        {
            // ViewModel doesn't have a key defined - that's OK
            return;
        }

        // Check if keys match
        if (componentKey != viewModelKey)
        {
            // Keys don't match - report diagnostic
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ViewModelKeyInconsistent,
                vmKeyAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedType.Locations[0],
                componentKey);

            symbolContext.ReportDiagnostic(diagnostic);
        }
    }

    private static INamedTypeSymbol? ExtractViewModelTypeFromComponent(INamedTypeSymbol componentType)
    {
        // Walk the inheritance chain looking for MvvmComponentBase<TViewModel>
        var baseType = componentType.BaseType;
        while (baseType != null)
        {
            var originalDef = baseType.OriginalDefinition.ToDisplayString();
            
            // Check if this is a generic MvvmComponentBase
            if ((originalDef.StartsWith("Blazing.Mvvm.Components.MvvmComponentBase<", StringComparison.Ordinal) ||
                 originalDef.StartsWith("Blazing.Mvvm.Components.MvvmOwningComponentBase<", StringComparison.Ordinal) ||
                 originalDef.StartsWith("Blazing.Mvvm.Components.MvvmLayoutComponentBase<", StringComparison.Ordinal)) &&
                baseType.TypeArguments.Length > 0)
            {
                return baseType.TypeArguments[0] as INamedTypeSymbol;
            }

            baseType = baseType.BaseType;
        }

        return null;
    }

    private static bool InheritsFromComponentBase(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "ComponentBase" || 
                baseType.Name == "MvvmComponentBase" ||
                baseType.Name == "MvvmOwningComponentBase" ||
                baseType.Name == "MvvmLayoutComponentBase")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
