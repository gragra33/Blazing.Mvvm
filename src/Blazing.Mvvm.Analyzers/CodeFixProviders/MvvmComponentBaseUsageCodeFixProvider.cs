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
/// Code fix provider for MvvmComponentBase usage analyzer.
/// Replaces ComponentBase with MvvmComponentBase&lt;TViewModel&gt;.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MvvmComponentBaseUsageCodeFixProvider))]
[Shared]
public sealed class MvvmComponentBaseUsageCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use MvvmComponentBase<TViewModel>";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.MvvmComponentBaseMissing.Id);

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

        // Extract ViewModel type from diagnostic properties
        if (!diagnostic.Properties.TryGetValue("ViewModelType", out var viewModelTypeName) ||
            viewModelTypeName is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => ReplaceBaseTypeAsync(
                    context.Document,
                    declaration,
                    viewModelTypeName,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Replaces ComponentBase with MvvmComponentBase&lt;TViewModel&gt;.
    /// </summary>
    private static async Task<Document> ReplaceBaseTypeAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        string viewModelTypeName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        if (classDeclaration.BaseList is null)
        {
            return document;
        }

        // Find and replace ComponentBase
        var componentBaseType = classDeclaration.BaseList.Types
            .FirstOrDefault(t => t.Type.ToString().Contains("ComponentBase"));

        if (componentBaseType is null)
        {
            return document;
        }

        // Create MvvmComponentBase<TViewModel>
        var mvvmComponentBase = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("MvvmComponentBase"))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                        SyntaxFactory.ParseTypeName(viewModelTypeName)))));

        var newBaseList = classDeclaration.BaseList.ReplaceNode(
            componentBaseType,
            mvvmComponentBase);

        var newClassDeclaration = classDeclaration.WithBaseList(newBaseList);

        // Replace the class declaration in the tree
        var newRoot = compilationUnit.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using statement if not present
        const string usingDirective = "Blazing.Mvvm.Components";
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
