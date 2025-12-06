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
/// Code fix provider for dispose pattern analyzer.
/// Adds IDisposable implementation with proper cleanup code.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposePatternCodeFixProvider))]
[Shared]
public sealed class DisposePatternCodeFixProvider : CodeFixProvider
{
    private const string Title = "Implement IDisposable with cleanup";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.DisposePatternMissing.Id);

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

        var classDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => ImplementDisposeAsync(
                    context.Document,
                    classDeclaration,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Implements IDisposable interface with Dispose method.
    /// </summary>
    private static async Task<Document> ImplementDisposeAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        // Add IDisposable to base list
        var disposableType = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.ParseTypeName("IDisposable"));

        var newBaseList = classDeclaration.BaseList is null
            ? SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(disposableType))
            : classDeclaration.BaseList.AddTypes(disposableType);

        // Create Dispose method
        var disposeMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier("Dispose"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("// TODO: Unregister event handlers and dispose resources\n            GC.SuppressFinalize(this);")))
            .WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.Whitespace("    "));

        var newClassDeclaration = classDeclaration
            .WithBaseList(newBaseList)
            .AddMembers(disposeMethod);

        // Replace the class declaration in the tree
        var newRoot = compilationUnit.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using System if not present
        const string usingDirective = "System";
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
