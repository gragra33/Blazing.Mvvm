using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects messenger registrations without corresponding unregistrations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MessengerRegistrationLifetimeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MessengerRegistrationLeakPossible);

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

        // Skip abstract classes
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Check if inherits from RecipientViewModelBase (which handles registration lifecycle automatically)
        var inheritsRecipientViewModel = InheritsFromRecipientViewModelBase(namedTypeSymbol, context.Compilation);
        if (inheritsRecipientViewModel)
        {
            return; // RecipientViewModelBase handles OnActivated/OnDeactivated lifecycle
        }

        // Check if class implements IDisposable
        var implementsDisposable = namedTypeSymbol.AllInterfaces.Any(i =>
            i.Name == "IDisposable" && i.ContainingNamespace.ToString() == "System");

        // Check for any Unregister calls in the class
        bool hasAnyUnregisterCall = false;

        foreach (var syntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax(context.CancellationToken);
            if (syntax is ClassDeclarationSyntax classDeclaration)
            {
                hasAnyUnregisterCall = classDeclaration.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Any(invocation =>
                    {
                        var identifierText = invocation.Expression.ToString();
                        return identifierText.Contains("Messenger.Unregister") ||
                               identifierText.Contains("UnregisterAll");
                    });

                if (hasAnyUnregisterCall || implementsDisposable)
                {
                    break; // Has cleanup mechanism, no need to check further
                }
            }
        }

        // If class has cleanup mechanism (Dispose or Unregister calls), no diagnostic needed
        if (hasAnyUnregisterCall || implementsDisposable)
        {
            return;
        }

        // Analyze each method for messenger registrations
        foreach (var syntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax(context.CancellationToken);
            if (syntax is ClassDeclarationSyntax classDeclaration)
            {
                // Check constructors
                foreach (var constructor in classDeclaration.Members.OfType<ConstructorDeclarationSyntax>())
                {
                    if (HasMessengerRegistration(constructor))
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.MessengerRegistrationLeakPossible,
                            constructor.Identifier.GetLocation(),
                            constructor.Identifier.Text);

                        context.ReportDiagnostic(diagnostic);
                    }
                }

                // Check methods
                foreach (var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
                {
                    if (HasMessengerRegistration(method))
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.MessengerRegistrationLeakPossible,
                            method.Identifier.GetLocation(),
                            method.Identifier.Text);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

    private static bool HasMessengerRegistration(SyntaxNode node)
    {
        return node.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation =>
            {
                var identifierText = invocation.Expression.ToString();
                return identifierText.Contains("Messenger.Register") ||
                       (identifierText.Contains("WeakReferenceMessenger") &&
                        identifierText.Contains("Register"));
            });
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
