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
using Microsoft.CodeAnalysis.Editing;

namespace Blazing.Mvvm.Analyzers.CodeFixProviders;

/// <summary>
/// Code fix provider for RelayCommand async pattern analyzer.
/// Converts async void methods to async Task for proper command patterns.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RelayCommandAsyncPatternCodeFixProvider))]
[Shared]
public sealed class RelayCommandAsyncPatternCodeFixProvider : CodeFixProvider
{
    private const string Title = "Convert to async Task";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.AsyncVoidRelayCommand.Id);

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

        var methodDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (methodDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => ConvertToAsyncTaskAsync(
                    context.Document,
                    methodDeclaration,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Converts async void method to async Task.
    /// </summary>
    private static async Task<Document> ConvertToAsyncTaskAsync(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        // Replace void return type with Task
        var taskType = SyntaxFactory.ParseTypeName("Task")
            .WithTrailingTrivia(methodDeclaration.ReturnType.GetTrailingTrivia());

        var newMethodDeclaration = methodDeclaration.WithReturnType(taskType);

        // Ensure method name ends with Async if not already
        if (!methodDeclaration.Identifier.Text.EndsWith("Async"))
        {
            var newIdentifier = SyntaxFactory.Identifier(
                methodDeclaration.Identifier.Text + "Async")
                .WithTrailingTrivia(methodDeclaration.Identifier.TrailingTrivia);

            newMethodDeclaration = newMethodDeclaration.WithIdentifier(newIdentifier);
        }

        // Replace the method declaration in the tree
        var newRoot = compilationUnit.ReplaceNode(methodDeclaration, newMethodDeclaration);

        // Add using System.Threading.Tasks if not present
        const string usingDirective = "System.Threading.Tasks";
        var hasUsing = compilationUnit.Usings.Any(u =>
            u.Name?.ToString() == usingDirective);

        if (!hasUsing)
        {
            var newUsing = SyntaxFactory.UsingDirective(
                SyntaxFactory.ParseName(usingDirective))
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            newRoot = newRoot.AddUsings(newUsing);
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
