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
/// Code fix provider for MvvmOwningComponentBase usage analyzer.
/// Replaces MvvmComponentBase with MvvmOwningComponentBase.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MvvmOwningComponentBaseUsageCodeFixProvider))]
[Shared]
public sealed class MvvmOwningComponentBaseUsageCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use MvvmOwningComponentBase<TViewModel>";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.MvvmOwningComponentBaseSuggested.Id);

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

        var declaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (declaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => ReplaceBaseTypeAsync(
                    context.Document,
                    declaration,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Replaces MvvmComponentBase with MvvmOwningComponentBase.
    /// </summary>
    private static async Task<Document> ReplaceBaseTypeAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        if (classDeclaration.BaseList is null)
        {
            return document;
        }

        // Find MvvmComponentBase<TViewModel>
        var baseType = classDeclaration.BaseList.Types
            .Select(t => t.Type)
            .OfType<GenericNameSyntax>()
            .FirstOrDefault(g => g.Identifier.Text == "MvvmComponentBase");

        if (baseType is null)
        {
            return document;
        }

        // Create MvvmOwningComponentBase<TViewModel> with same type arguments
        var owningComponentBase = baseType.WithIdentifier(
            SyntaxFactory.Identifier("MvvmOwningComponentBase"));

        var oldBaseTypeSyntax = classDeclaration.BaseList.Types
            .First(t => t.Type == baseType);

        var newBaseTypeSyntax = oldBaseTypeSyntax.WithType(owningComponentBase);

        var newBaseList = classDeclaration.BaseList.ReplaceNode(
            oldBaseTypeSyntax,
            newBaseTypeSyntax);

        var newClassDeclaration = classDeclaration.WithBaseList(newBaseList);
        editor.ReplaceNode(classDeclaration, newClassDeclaration);

        return editor.GetChangedDocument();
    }
}
