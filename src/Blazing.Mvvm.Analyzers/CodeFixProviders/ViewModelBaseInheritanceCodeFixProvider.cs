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
using Microsoft.CodeAnalysis.Text;

namespace Blazing.Mvvm.Analyzers.CodeFixProviders;

/// <summary>
/// Code fix provider for ViewModelBase inheritance analyzer.
/// Adds ViewModelBase as base class to ViewModels.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ViewModelBaseInheritanceCodeFixProvider))]
[Shared]
public sealed class ViewModelBaseInheritanceCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add ViewModelBase inheritance";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewModelBaseMissing.Id);

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
                createChangedDocument: cancellationToken => AddViewModelBaseAsync(
                    context.Document,
                    declaration,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Adds ViewModelBase as base class and adds necessary using statement.
    /// </summary>
    private static async Task<Document> AddViewModelBaseAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var originalText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        if (classDeclaration.BaseList?.Types.Any(type => type.Type.ToString() == "ViewModelBase") == true)
        {
            return document;
        }

        // Add base type
        var viewModelBaseType = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.ParseTypeName("ViewModelBase"));

        var newBaseList = classDeclaration.BaseList is null
            ? SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(viewModelBaseType))
            : classDeclaration.BaseList.AddTypes(viewModelBaseType);

        var newClassDeclaration = classDeclaration.WithBaseList(newBaseList);

        // Replace the class declaration in the tree
        var newRoot = compilationUnit.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using statement if not present
        const string usingDirective = "Blazing.Mvvm.ComponentModel";
        var hasUsing = compilationUnit.Usings.Any(u =>
            u.Name?.ToString() == usingDirective);

        if (!hasUsing)
        {
            var newUsing = SyntaxFactory.UsingDirective(
                SyntaxFactory.ParseName(usingDirective))
                .WithTrailingTrivia(SyntaxFactory.EndOfLine("\n"));

            newRoot = newRoot.AddUsings(newUsing);
        }

        return document.WithText(CreateNormalizedText(newRoot, originalText));
    }

    private static SourceText CreateNormalizedText(SyntaxNode root, SourceText originalText)
    {
        var original = originalText.ToString();
        var hasLeadingBlankLine = original.StartsWith("\r\n", StringComparison.Ordinal) || original.StartsWith("\n", StringComparison.Ordinal);
        var lineEnding = original.IndexOf("\r\n", StringComparison.Ordinal) >= 0 ? "\r\n" : "\n";

        var normalized = root.NormalizeWhitespace("    ", "\n").ToFullString().TrimStart('\r', '\n');
        if (hasLeadingBlankLine)
        {
            normalized = "\n" + normalized;
        }

        if (lineEnding == "\r\n")
        {
            normalized = normalized.Replace("\n", "\r\n");
        }

        return SourceText.From(normalized, originalText.Encoding);
    }
}
