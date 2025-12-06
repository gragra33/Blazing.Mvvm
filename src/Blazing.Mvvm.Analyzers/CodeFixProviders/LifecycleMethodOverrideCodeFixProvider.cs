using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Blazing.Mvvm.Analyzers.CodeFixProviders;

/// <summary>
/// Code fix provider for lifecycle method override analyzer.
/// Adds OnInitializedAsync override to ViewModel classes.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LifecycleMethodOverrideCodeFixProvider))]
[Shared]
public sealed class LifecycleMethodOverrideCodeFixProvider : CodeFixProvider
{
    private const string _title = "Add OnInitializedAsync override";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticDescriptors.LifecycleMethodSuggestion.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the constructor declaration
        var constructorDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault();

        if (constructorDeclaration is null)
        {
            return;
        }

        // Find the containing class
        var classDeclaration = constructorDeclaration.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: cancellationToken => AddLifecycleMethodAsync(
                    context.Document,
                    classDeclaration,
                    cancellationToken),
                equivalenceKey: _title),
            diagnostic);
    }

    /// <summary>
    /// Adds OnInitializedAsync override method to the class.
    /// </summary>
    private static async Task<Document> AddLifecycleMethodAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        // Create OnInitializedAsync method
        var methodDeclaration = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.ParseTypeName("Task"),
            "OnInitializedAsync")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                SyntaxFactory.Token(SyntaxKind.OverrideKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
            .WithBody(SyntaxFactory.Block(
                // await base.OnInitializedAsync();
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.BaseExpression(),
                                SyntaxFactory.IdentifierName("OnInitializedAsync")))))))
            .WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.Whitespace("    "))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        // Add method to class
        var newClassDeclaration = classDeclaration.AddMembers(methodDeclaration);
        var newRoot = compilationUnit.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using System.Threading.Tasks if not present
        var usingsToAdd = new[] { "System.Threading.Tasks" };

        foreach (var usingDirective in usingsToAdd)
        {
            var hasUsing = compilationUnit.Usings.Any(u =>
                u.Name?.ToString() == usingDirective);

            if (!hasUsing)
            {
                var newUsing = SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(usingDirective))
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

                newRoot = newRoot.AddUsings(newUsing);
            }
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
