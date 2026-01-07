using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Blazing.Mvvm.Analyzers.CodeFixProviders;

/// <summary>
/// Code fix provider for EventCallback two-way binding analyzer.
/// Provides fixes for removing manual PropertyChanged subscriptions and adding missing EventCallbacks.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EventCallbackTwoWayBindingCodeFixProvider))]
[Shared]
public sealed class EventCallbackTwoWayBindingCodeFixProvider : CodeFixProvider
{
    private const string RemoveManualSubscriptionTitle = "Remove manual PropertyChanged subscription (use automatic two-way binding)";
    private const string AddEventCallbackTitle = "Add EventCallback<T> for automatic two-way binding";
    private const string FixTypeMismatchTitle = "Fix EventCallback type to match parameter type";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ManualTwoWayBindingObsolete.Id,
                              DiagnosticDescriptors.EventCallbackMissing.Id,
                              DiagnosticDescriptors.EventCallbackTypeMismatch.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (diagnostic.Id == DiagnosticDescriptors.ManualTwoWayBindingObsolete.Id)
        {
            await RegisterRemoveManualSubscriptionFix(context, root, diagnostic, diagnosticSpan);
        }
        else if (diagnostic.Id == DiagnosticDescriptors.EventCallbackMissing.Id)
        {
            await RegisterAddEventCallbackFix(context, root, diagnostic, diagnosticSpan);
        }
        else if (diagnostic.Id == DiagnosticDescriptors.EventCallbackTypeMismatch.Id)
        {
            await RegisterFixTypeMismatchFix(context, root, diagnostic, diagnosticSpan);
        }
    }

    private static async Task RegisterRemoveManualSubscriptionFix(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan)
    {
        var subscription = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<AssignmentExpressionSyntax>()
            .FirstOrDefault();

        if (subscription is null)
        {
            return;
        }

        // Find the containing class
        var classDeclaration = subscription.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: RemoveManualSubscriptionTitle,
                createChangedDocument: cancellationToken => RemoveManualSubscriptionAsync(
                    context.Document,
                    classDeclaration,
                    subscription,
                    cancellationToken),
                equivalenceKey: RemoveManualSubscriptionTitle),
            diagnostic);
    }

    private static async Task RegisterAddEventCallbackFix(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan)
    {
        var property = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (property is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: AddEventCallbackTitle,
                createChangedDocument: cancellationToken => AddEventCallbackAsync(
                    context.Document,
                    property,
                    cancellationToken),
                equivalenceKey: AddEventCallbackTitle),
            diagnostic);
    }

    private static async Task RegisterFixTypeMismatchFix(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan)
    {
        var property = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (property is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: FixTypeMismatchTitle,
                createChangedDocument: cancellationToken => FixTypeMismatchAsync(
                    context.Document,
                    property,
                    cancellationToken),
                equivalenceKey: FixTypeMismatchTitle),
            diagnostic);
    }

    private static async Task<Document> RemoveManualSubscriptionAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        AssignmentExpressionSyntax subscription,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        
        // Extract handler name from subscription (e.g., "OnViewModelPropertyChanged")
        var handlerName = subscription.Right.ToString();

        // Find and remove the subscription statement
        var subscriptionStatement = subscription.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
        if (subscriptionStatement != null)
        {
            editor.RemoveNode(subscriptionStatement);
        }

        // Find and remove the event handler method
        var handlerMethod = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == handlerName);

        if (handlerMethod != null)
        {
            editor.RemoveNode(handlerMethod);
        }

        // Find and remove the unsubscription in Dispose
        var disposeMethod = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == "Dispose");

        if (disposeMethod != null)
        {
            // Look for unsubscription (ViewModel.PropertyChanged -= ...)
            var unsubscriptions = disposeMethod.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignment =>
                    assignment.OperatorToken.IsKind(SyntaxKind.MinusEqualsToken) &&
                    assignment.Left.ToString().Contains("PropertyChanged") &&
                    assignment.Right.ToString() == handlerName);

            foreach (var unsubscription in unsubscriptions)
            {
                var unsubStatement = unsubscription.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (unsubStatement != null)
                {
                    editor.RemoveNode(unsubStatement);
                }
            }

            // Check if Dispose method is now empty (or only has base.Dispose call and if statement)
            var updatedRoot = editor.GetChangedRoot();
            var updatedDispose = updatedRoot.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == "Dispose");

            if (updatedDispose != null && IsDisposeMethodEffectivelyEmpty(updatedDispose))
            {
                editor.RemoveNode(updatedDispose);
            }
        }

        // Check if we need to remove the "using System.ComponentModel" directive
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var updatedRootAfterRemoval = editor.GetChangedRoot();
        
        // Only remove using if no other usage of System.ComponentModel exists
        if (!HasOtherComponentModelUsage(updatedRootAfterRemoval, semanticModel))
        {
            var usingDirective = updatedRootAfterRemoval.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .FirstOrDefault(u => u.Name?.ToString() == "System.ComponentModel");

            if (usingDirective != null)
            {
                editor.RemoveNode(usingDirective);
            }
        }

        return editor.GetChangedDocument();
    }

    private static async Task<Document> AddEventCallbackAsync(
        Document document,
        PropertyDeclarationSyntax parameterProperty,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var generator = editor.Generator;

        var propertyName = parameterProperty.Identifier.Text;
        var propertyType = parameterProperty.Type;

        // Create EventCallback<T> property
        var eventCallbackProperty = generator.PropertyDeclaration(
            $"{propertyName}Changed",
            generator.GenericName("EventCallback", propertyType),
            accessibility: Accessibility.Public,
            getAccessorStatements: null,
            setAccessorStatements: null);

        // Add [Parameter] attribute
        var parameterAttribute = generator.Attribute("Parameter");
        eventCallbackProperty = generator.AddAttributes(eventCallbackProperty, parameterAttribute);

        // Insert after the parameter property
        editor.InsertAfter(parameterProperty, eventCallbackProperty);

        return editor.GetChangedDocument();
    }

    private static async Task<Document> FixTypeMismatchAsync(
        Document document,
        PropertyDeclarationSyntax eventCallbackProperty,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel is null)
        {
            return document;
        }

        // Find the corresponding parameter property
        var propertyName = eventCallbackProperty.Identifier.Text;
        if (!propertyName.EndsWith("Changed"))
        {
            return document;
        }

        var baseName = propertyName.Substring(0, propertyName.Length - 7); // Remove "Changed"

        // Find the containing class
        var classDeclaration = eventCallbackProperty.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration is null)
        {
            return document;
        }

        // Find the parameter property
        var parameterProperty = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.Text == baseName);

        if (parameterProperty is null)
        {
            return document;
        }

        var correctType = parameterProperty.Type;

        // Create new EventCallback<T> with correct type
        var newType = SyntaxFactory.GenericName(
            SyntaxFactory.Identifier("EventCallback"),
            SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SingletonSeparatedList(correctType)));

        var newProperty = eventCallbackProperty.WithType(newType);

        editor.ReplaceNode(eventCallbackProperty, newProperty);

        return editor.GetChangedDocument();
    }

    private static bool IsDisposeMethodEffectivelyEmpty(MethodDeclarationSyntax disposeMethod)
    {
        if (disposeMethod.Body == null)
        {
            return true;
        }

        var statements = disposeMethod.Body.Statements;

        // Empty
        if (statements.Count == 0)
        {
            return true;
        }

        // Only has base.Dispose call
        if (statements.Count == 1)
        {
            var statement = statements[0];
            if (statement is ExpressionStatementSyntax exprStmt &&
                exprStmt.Expression is InvocationExpressionSyntax invocation &&
                invocation.Expression.ToString().Contains("base.Dispose"))
            {
                return true;
            }
        }

        // Only has if (disposing) { base.Dispose(disposing); }
        if (statements.Count == 1 && statements[0] is IfStatementSyntax ifStmt)
        {
            var ifBody = ifStmt.Statement;
            if (ifBody is BlockSyntax block)
            {
                if (block.Statements.Count == 0)
                {
                    return true;
                }
                
                if (block.Statements.Count == 1 &&
                    block.Statements[0] is ExpressionStatementSyntax exprStmt &&
                    exprStmt.Expression is InvocationExpressionSyntax invocation &&
                    invocation.Expression.ToString().Contains("base.Dispose"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasOtherComponentModelUsage(SyntaxNode root, SemanticModel? semanticModel)
    {
        if (semanticModel == null)
        {
            return true; // Safe default - don't remove using if we can't check
        }

        // Check for PropertyChangedEventArgs, INotifyPropertyChanged, etc.
        var identifiers = root.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(id => 
                id.Identifier.Text == "PropertyChangedEventArgs" ||
                id.Identifier.Text == "PropertyChangedEventHandler" ||
                id.Identifier.Text == "INotifyPropertyChanged");

        return identifiers.Any();
    }
}
