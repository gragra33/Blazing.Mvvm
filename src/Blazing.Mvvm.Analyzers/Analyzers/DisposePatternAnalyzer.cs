using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures ViewModels properly implement IDisposable when using event subscriptions or unmanaged resources.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DisposePatternAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.DisposePatternMissing);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze ViewModels
        if (!namedTypeSymbol.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return;
        }

        // Skip abstract classes and interfaces
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Check if already implements IDisposable
        var implementsDisposable = namedTypeSymbol.AllInterfaces.Any(i => 
            i.Name == "IDisposable" && i.ContainingNamespace.ToString() == "System");

        if (implementsDisposable)
        {
            return;
        }

        // Check if inherits from RecipientViewModelBase (which handles cleanup automatically)
        var inheritsRecipientViewModel = InheritsFromRecipientViewModelBase(namedTypeSymbol, context.Compilation);
        if (inheritsRecipientViewModel)
        {
            return;
        }

        // Check for disposable patterns
        bool needsDispose = false;

        // Check for disposable fields
        foreach (var member in namedTypeSymbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                if (ImplementsIDisposable(field.Type))
                {
                    needsDispose = true;
                    break;
                }
            }
            else if (member is IPropertySymbol property)
            {
                if (ImplementsIDisposable(property.Type))
                {
                    needsDispose = true;
                    break;
                }
            }
        }

        // Check for event subscriptions and Messenger.Register calls
        if (!needsDispose)
        {
            foreach (var syntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is ClassDeclarationSyntax classDeclaration)
                {
                    // Check for event subscriptions (+=)
                    var hasEventSubscription = classDeclaration.DescendantNodes()
                        .OfType<AssignmentExpressionSyntax>()
                        .Any(assignment => assignment.IsKind(SyntaxKind.AddAssignmentExpression));

                    if (hasEventSubscription)
                    {
                        needsDispose = true;
                        break;
                    }

                    // Check for Messenger.Register calls
                    var hasMessengerRegistration = classDeclaration.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(invocation =>
                        {
                            var identifierText = invocation.Expression.ToString();
                            return identifierText.Contains("Messenger.Register") ||
                                   (identifierText.Contains("WeakReferenceMessenger") &&
                                    identifierText.Contains("Register"));
                        });

                    if (hasMessengerRegistration)
                    {
                        needsDispose = true;
                        break;
                    }
                }
            }
        }

        if (needsDispose)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.DisposePatternMissing,
                namedTypeSymbol.Locations[0],
                namedTypeSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ImplementsIDisposable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType)
        {
            return namedType.AllInterfaces.Any(i =>
                i.Name == "IDisposable" && i.ContainingNamespace.ToString() == "System");
        }
        return false;
    }

    private static bool InheritsFromRecipientViewModelBase(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var recipientViewModelBase = compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.RecipientViewModelBase);
        
        if (recipientViewModelBase == null)
        {
            return false;
        }

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, recipientViewModelBase))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }
}
