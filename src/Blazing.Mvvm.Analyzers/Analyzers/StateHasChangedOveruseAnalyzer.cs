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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        
        // Check if this is a StateHasChanged call
        var methodName = invocationExpression.Expression.ToString();
        if (!methodName.EndsWith("StateHasChanged", StringComparison.Ordinal))
        {
            return;
        }

        // Get the containing method
        var containingMethod = invocationExpression.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (containingMethod == null)
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        var methodSymbol = semanticModel.GetDeclaredSymbol(containingMethod);

        if (methodSymbol == null)
        {
            return;
        }

        var containingType = methodSymbol.ContainingType;

        // Only analyze ViewModels or Blazor components
        var isViewModel = containingType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix);
        var isComponent = InheritsFromComponentBase(containingType);

        if (!isViewModel && !isComponent)
        {
            return;
        }

        // Check if the containing type uses property notification mechanisms
        var hasPropertyNotification = HasObservableProperties(containingType);

        if (!hasPropertyNotification)
        {
            // No observable properties - StateHasChanged might be necessary
            return;
        }

        // Get the method body to analyze
        var methodBody = containingMethod.Body;
        if (methodBody == null)
        {
            return;
        }

        // Check if method contains property assignments or uses SetProperty/OnPropertyChanged
        var bodyText = methodBody.ToString();
        var hasSetPropertyCalls = bodyText.Contains("SetProperty") || 
                                   bodyText.Contains("OnPropertyChanged") ||
                                   bodyText.Contains("PropertyChanged");

        // Check for property assignments (properties typically start with uppercase)
        var hasPropertyAssignments = methodBody.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(assignment =>
            {
                var left = assignment.Left.ToString().Trim();
                // Check if left side looks like a property (starts with capital letter, not underscore field)
                return left.Length > 0 && char.IsUpper(left[0]) && !left.StartsWith("_");
            });

        // Report diagnostic if method uses property notifications or has property assignments
        if (hasSetPropertyCalls || hasPropertyAssignments)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.StateHasChangedUnnecessary,
                invocationExpression.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasObservableProperties(INamedTypeSymbol typeSymbol)
    {
        // Check if any member (field or property) has [ObservableProperty] attribute
        var members = typeSymbol.GetMembers();
        
        foreach (var member in members)
        {
            var attributes = member.GetAttributes();
            foreach (var attr in attributes)
            {
                var attrName = attr.AttributeClass?.Name;
                if (attrName == "ObservablePropertyAttribute" || attrName == "ObservableProperty")
                {
                    return true;
                }
            }
        }

        // Check if any property uses SetProperty in its setter by examining syntax
        var syntaxReferences = typeSymbol.DeclaringSyntaxReferences;
        foreach (var syntaxRef in syntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is ClassDeclarationSyntax classDeclaration)
            {
                var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
                foreach (var property in properties)
                {
                    var propertyText = property.ToString();
                    if (propertyText.Contains("SetProperty"))
                    {
                        return true;
                    }
                }
            }
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
