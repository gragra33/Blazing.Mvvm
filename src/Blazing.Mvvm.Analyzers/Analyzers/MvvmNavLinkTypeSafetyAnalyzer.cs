using System.Collections.Immutable;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that validates MvvmNavLink TViewModel parameter references valid registered ViewModels.
/// Works with both C# generic syntax and Razor component attribute syntax.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MvvmNavLinkTypeSafetyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MvvmNavLinkInvalidViewModel);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var validViewModels = new ConcurrentBag<INamedTypeSymbol>();

            // Collect all valid ViewModels (those that inherit from ViewModelBase and have [ViewModelDefinition])
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                
                if (!namedType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
                {
                    return;
                }

                if (namedType.TypeKind != TypeKind.Class || namedType.IsAbstract)
                {
                    return;
                }

                // Check if it inherits from ViewModelBase (like BLAZMVVM0001 does)
                var inheritsBase = InheritsFromViewModelBase(namedType, symbolContext.Compilation);
                
                // Check if it has ViewModelDefinition attribute (required for DI registration)
                var hasDefinition = namedType.GetAttributes().Any(attr =>
                    attr.AttributeClass?.Name == AnalyzerConstants.AttributeNames.ViewModelDefinition ||
                    attr.AttributeClass?.Name == (AnalyzerConstants.AttributeNames.ViewModelDefinition + "Attribute"));

                // ViewModel is valid if it inherits from base AND has definition attribute
                if (inheritsBase && hasDefinition)
                {
                    validViewModels.Add(namedType);
                }
            }, SymbolKind.NamedType);

            // Analyze GenericNameSyntax nodes to find MvvmNavLink<TViewModel> usages
            compilationContext.RegisterSyntaxNodeAction(syntaxContext =>
            {
                var genericName = (GenericNameSyntax)syntaxContext.Node;

                // Check if this is MvvmNavLink
                if (genericName.Identifier.ValueText != "MvvmNavLink")
                    return;

                // Get semantic info
                var typeInfo = syntaxContext.SemanticModel.GetTypeInfo(genericName, syntaxContext.CancellationToken);
                
                if (typeInfo.Type is not INamedTypeSymbol namedType)
                    return;

                // Verify it's from Blazing.Mvvm.Components.Routing namespace
                var ns = namedType.OriginalDefinition.ContainingNamespace?.ToDisplayString();
                if (ns != "Blazing.Mvvm.Components.Routing")
                    return;

                // Extract TViewModel type argument
                if (namedType.TypeArguments.Length == 0)
                    return;

                var viewModelType = namedType.TypeArguments[0] as INamedTypeSymbol;
                if (viewModelType == null)
                    return;

                // Skip interfaces - they don't need [ViewModelDefinition]
                if (viewModelType.TypeKind == TypeKind.Interface)
                    return;

                // Validate immediately
                var validViewModelSet = new HashSet<INamedTypeSymbol>(validViewModels, SymbolEqualityComparer.Default);
                if (!validViewModelSet.Contains(viewModelType, SymbolEqualityComparer.Default))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MvvmNavLinkInvalidViewModel,
                        genericName.GetLocation(),
                        viewModelType.Name);

                    syntaxContext.ReportDiagnostic(diagnostic);
                }
                
            }, SyntaxKind.GenericName);

            // Also check ObjectCreationExpressionSyntax for direct C# usage
            compilationContext.RegisterSyntaxNodeAction(syntaxContext =>
            {
                var creation = (ObjectCreationExpressionSyntax)syntaxContext.Node;
                
                // Check if the type is a GenericNameSyntax (e.g., new MvvmNavLink<TViewModel>())
                if (creation.Type is not GenericNameSyntax genericType)
                    return;

                // Check if this is MvvmNavLink
                if (genericType.Identifier.ValueText != "MvvmNavLink")
                    return;

                // Get semantic info
                var typeInfo = syntaxContext.SemanticModel.GetTypeInfo(creation, syntaxContext.CancellationToken);
                
                if (typeInfo.Type is not INamedTypeSymbol namedType)
                    return;

                // Verify it's from Blazing.Mvvm.Components.Routing namespace
                var ns = namedType.OriginalDefinition.ContainingNamespace?.ToDisplayString();
                if (ns != "Blazing.Mvvm.Components.Routing")
                    return;

                // Extract TViewModel type argument
                if (namedType.TypeArguments.Length == 0)
                    return;

                var viewModelType = namedType.TypeArguments[0] as INamedTypeSymbol;
                if (viewModelType == null)
                    return;

                // Skip interfaces - they don't need [ViewModelDefinition]
                if (viewModelType.TypeKind == TypeKind.Interface)
                    return;

                // Validate immediately
                var validViewModelSet = new HashSet<INamedTypeSymbol>(validViewModels, SymbolEqualityComparer.Default);
                if (!validViewModelSet.Contains(viewModelType, SymbolEqualityComparer.Default))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MvvmNavLinkInvalidViewModel,
                        genericType.GetLocation(),
                        viewModelType.Name);

                    syntaxContext.ReportDiagnostic(diagnostic);
                }
                
            }, SyntaxKind.ObjectCreationExpression);
        });
    }

    /// <summary>
    /// Checks if a type inherits from ViewModelBase (same logic as BLAZMVVM0001)
    /// </summary>
    private static bool InheritsFromViewModelBase(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var viewModelBaseTypes = new[]
        {
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ViewModelBase),
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.RecipientViewModelBase),
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ValidatorViewModelBase)
        };

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (viewModelBaseTypes.Any(vb => vb != null && SymbolEqualityComparer.Default.Equals(baseType, vb)))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }
}
