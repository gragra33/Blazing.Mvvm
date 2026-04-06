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
/// Code fix provider for StateHasChanged overuse analyzer.
/// Removes unnecessary StateHasChanged() calls.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StateHasChangedOveruseCodeFixProvider))]
[Shared]
public sealed class StateHasChangedOveruseCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove unnecessary StateHasChanged() call";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.StateHasChangedUnnecessary.Id);

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

        var invocation = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault();

        if (invocation is null)
        {
            return;
        }

        // Find the statement containing the invocation
        var statement = invocation.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
        if (statement is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => RemoveStateHasChangedAsync(
                    context.Document,
                    statement,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Removes the StateHasChanged() call statement.
    /// </summary>
    private static async Task<Document> RemoveStateHasChangedAsync(
        Document document,
        StatementSyntax statement,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.RemoveNode(statement);
        return editor.GetChangedDocument();
    }
}
