using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects manual StateHasChanged() calls that may be unnecessary with proper property notifications.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StateHasChangedOveruseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.StateHasChangedUnnecessary);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze ViewModels or Blazor components
        var isViewModel = InheritsFromViewModelBase(namedTypeSymbol, context.Compilation);
        var isComponent = InheritsFromComponentBase(namedTypeSymbol);

        if (!isViewModel && !isComponent)
        {
            return;
        }

        // Skip abstract classes
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Check if the type has any property notification mechanisms
        var hasPropertyNotification = HasPropertyNotificationMechanism(namedTypeSymbol);

        if (!hasPropertyNotification)
        {
            // No property notification - StateHasChanged might be necessary
            return;
        }

        // Analyze each method for StateHasChanged calls
        foreach (var syntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax(context.CancellationToken);
            if (syntax is ClassDeclarationSyntax classDeclaration)
            {
                // Check all methods
                foreach (var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
                {
                    AnalyzeMethodForStateHasChanged(context, method);
                }

                // Check constructors
                foreach (var constructor in classDeclaration.Members.OfType<ConstructorDeclarationSyntax>())
                {
                    AnalyzeMethodForStateHasChanged(context, constructor);
                }
            }
        }
    }

    private static void AnalyzeMethodForStateHasChanged(SymbolAnalysisContext context, SyntaxNode methodNode)
    {
        // Find all StateHasChanged invocations in this method
        var stateHasChangedCalls = methodNode.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                var methodName = invocation.Expression.ToString();
                return methodName.EndsWith("StateHasChanged", StringComparison.Ordinal);
            })
            .ToList();

        if (!stateHasChangedCalls.Any())
        {
            return;
        }

        // Get the method body text for analysis
        var methodBody = methodNode.ToString();

        // Check if method contains property notification patterns
        var hasSetPropertyCalls = methodBody.Contains("SetProperty") || 
                                   methodBody.Contains("OnPropertyChanged") ||
                                   methodBody.Contains("PropertyChanged");

        // Check for property assignments (properties typically start with uppercase)
        var hasPropertyAssignments = methodNode.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(assignment =>
            {
                var left = assignment.Left.ToString().Trim();
                // Check if left side looks like a property (starts with capital letter, not underscore field)
                return left.Length > 0 && char.IsUpper(left[0]) && !left.StartsWith("_");
            });

        // Report diagnostic for each StateHasChanged call if method uses property notifications
        if (hasSetPropertyCalls || hasPropertyAssignments)
        {
            foreach (var call in stateHasChangedCalls)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.StateHasChangedUnnecessary,
                    call.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool HasPropertyNotificationMechanism(INamedTypeSymbol typeSymbol)
    {
        // MvvmComponentBase components have automatic property notification
        if (InheritsFromMvvmComponentBase(typeSymbol))
        {
            return true;
        }

        // Check for [ObservableProperty] attributes on fields
        if (HasObservablePropertyAttribute(typeSymbol))
        {
            return true;
        }

        // Check if any property uses SetProperty in its setter
        if (HasSetPropertyInProperties(typeSymbol))
        {
            return true;
        }

        return false;
    }

    private static bool HasObservablePropertyAttribute(INamedTypeSymbol typeSymbol)
    {
        var members = typeSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => field.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name is "ObservablePropertyAttribute" or "ObservableProperty"));

        return members.Any();
    }

    private static bool HasSetPropertyInProperties(INamedTypeSymbol typeSymbol)
    {
        var syntaxReferences = typeSymbol.DeclaringSyntaxReferences;
        foreach (var syntaxRef in syntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is ClassDeclarationSyntax classDeclaration)
            {
                var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
                if (properties.Any(property => property.ToString().Contains("SetProperty")))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool InheritsFromMvvmComponentBase(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            var baseName = baseType.Name;
            if (baseName == "MvvmComponentBase" ||
                baseName == "MvvmOwningComponentBase" ||
                baseName == "MvvmLayoutComponentBase")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
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
