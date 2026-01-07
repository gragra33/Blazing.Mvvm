using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that suggests overriding lifecycle methods when constructors contain initialization logic.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LifecycleMethodOverrideAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.LifecycleMethodSuggestion];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes that inherit from ViewModelBase
        if (!InheritsFromViewModelBase(namedTypeSymbol, context.Compilation))
        {
            return;
        }

        // Check if the class has a constructor with non-trivial logic
        var constructors = namedTypeSymbol.Constructors
            .Where(c => !c.IsImplicitlyDeclared && c.MethodKind == MethodKind.Constructor)
            .ToList();

        if (!constructors.Any())
        {
            return;
        }

        // Check if any lifecycle methods are already overridden
        var hasLifecycleOverride = HasLifecycleMethodOverride(namedTypeSymbol);

        if (hasLifecycleOverride)
        {
            return;
        }

        // Check if constructor has meaningful business logic (not just DI assignments)
        foreach (var constructor in constructors)
        {
            if (constructor.DeclaringSyntaxReferences.Length == 0)
            {
                continue;
            }

            var syntaxReference = constructor.DeclaringSyntaxReferences[0];
            var syntax = syntaxReference.GetSyntax(context.CancellationToken);

            if (syntax is not ConstructorDeclarationSyntax constructorSyntax)
            {
                continue;
            }

            // Check if constructor has business logic beyond simple field/property assignments
            if (HasBusinessLogic(constructorSyntax))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.LifecycleMethodSuggestion,
                    constructor.Locations[0],
                    AnalyzerConstants.MethodNames.OnInitializedAsync);

                context.ReportDiagnostic(diagnostic);
                break;
            }
        }
    }

    private static bool HasBusinessLogic(ConstructorDeclarationSyntax constructor)
    {
        if (constructor.Body == null)
        {
            return false;
        }

        var statements = constructor.Body.Statements;
        
        if (statements.Count == 0)
        {
            return false;
        }

        // Analyze each statement to determine if it's business logic
        foreach (var statement in statements)
        {
            // Simple assignment statements need deeper analysis
            if (statement is ExpressionStatementSyntax expressionStatement)
            {
                var expression = expressionStatement.Expression;
                
                // Check if it's an assignment
                if (expression is AssignmentExpressionSyntax assignment)
                {
                    // If right side is just a parameter identifier, it's DI
                    if (assignment.Right is IdentifierNameSyntax identifier)
                    {
                        // Check if the identifier name matches a constructor parameter pattern
                        // DI parameters are typically lowercase versions of field names
                        var rightName = identifier.Identifier.Text;
                        
                        // If it looks like a parameter (lowercase, no prefix), it's likely DI
                        if (char.IsLower(rightName[0]) && !rightName.StartsWith("_"))
                        {
                            continue; // This is DI assignment
                        }
                    }
                    
                    // Literal values (strings, numbers, bools) are initialization logic
                    if (assignment.Right is LiteralExpressionSyntax)
                    {
                        return true; // Literal initialization is business logic
                    }
                }
                
                // Any method invocation is business logic
                if (expression.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any())
                {
                    return true;
                }
                
                // Complex expressions (not simple assignments) are business logic
                if (expression is not AssignmentExpressionSyntax)
                {
                    return true;
                }
                
                // Assignment with method call on right side is business logic
                if (expression is AssignmentExpressionSyntax complexAssignment &&
                    complexAssignment.Right.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any())
                {
                    return true;
                }
            }
            
            // Control flow statements indicate business logic
            if (statement is IfStatementSyntax || 
                statement is ForStatementSyntax || 
                statement is ForEachStatementSyntax ||
                statement is WhileStatementSyntax ||
                statement is DoStatementSyntax ||
                statement is SwitchStatementSyntax ||
                statement is TryStatementSyntax)
            {
                return true;
            }
            
            // Local variable declarations with method calls are business logic
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                if (localDecl.DescendantNodes().OfType<InvocationExpressionSyntax>().Any())
                {
                    return true;
                }
            }
        }

        // If we only found simple DI assignments, don't trigger
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

    private static bool HasLifecycleMethodOverride(INamedTypeSymbol typeSymbol)
    {
        var lifecycleMethodNames = new[]
        {
            AnalyzerConstants.MethodNames.OnInitialized,
            AnalyzerConstants.MethodNames.OnInitializedAsync,
            AnalyzerConstants.MethodNames.OnParametersSet,
            AnalyzerConstants.MethodNames.OnParametersSetAsync,
            AnalyzerConstants.MethodNames.OnAfterRender,
            AnalyzerConstants.MethodNames.OnAfterRenderAsync
        };

        return typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => lifecycleMethodNames.Contains(m.Name) && m.IsOverride);
    }
}
