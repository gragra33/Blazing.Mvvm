using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that validates MvvmNavLink TViewModel parameter references valid registered ViewModels.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MvvmNavLinkTypeSafetyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MvvmNavLinkInvalidViewModel);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var validViewModels = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            // Collect all valid ViewModels
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

                // Check if it has ViewModelDefinition attribute or inherits from ViewModelBase
                var hasDefinition = namedType.GetAttributes().Any(attr =>
                    attr.AttributeClass?.Name == AnalyzerConstants.AttributeNames.ViewModelDefinition);

                var inheritsBase = InheritsFromViewModelBase(namedType, symbolContext.Compilation);

                if (hasDefinition || inheritsBase)
                {
                    validViewModels.Add(namedType);
                }
            }, SymbolKind.NamedType);

            // Analyze MvvmNavLink usage in Razor components
            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                AnalyzeMvvmNavLinkUsage(nodeContext, validViewModels);
            }, SyntaxKind.GenericName);
        });
    }

    private static void AnalyzeMvvmNavLinkUsage(
        SyntaxNodeAnalysisContext context,
        HashSet<INamedTypeSymbol> validViewModels)
    {
        var genericName = (GenericNameSyntax)context.Node;

        // Check if this is MvvmNavLink
        if (!genericName.Identifier.ValueText.Equals("MvvmNavLink", StringComparison.Ordinal))
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        var typeInfo = semanticModel.GetTypeInfo(genericName, context.CancellationToken);

        if (typeInfo.Type is not INamedTypeSymbol namedType)
        {
            return;
        }

        // Get the TViewModel type argument
        if (namedType.TypeArguments.Length == 0)
        {
            return;
        }

        var viewModelType = namedType.TypeArguments[0];
        
        if (viewModelType is not INamedTypeSymbol namedViewModelType)
        {
            return;
        }

        // Check if the ViewModel is valid
        if (!validViewModels.Contains(namedViewModelType, SymbolEqualityComparer.Default))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MvvmNavLinkInvalidViewModel,
                genericName.GetLocation(),
                namedViewModelType.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

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
