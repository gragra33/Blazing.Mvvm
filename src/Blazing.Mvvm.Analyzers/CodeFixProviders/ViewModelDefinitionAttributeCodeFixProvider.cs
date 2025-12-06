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
/// Code fix provider for ViewModelDefinition attribute analyzer.
/// Adds [ViewModelDefinition] attribute to ViewModel classes.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ViewModelDefinitionAttributeCodeFixProvider))]
[Shared]
public sealed class ViewModelDefinitionAttributeCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add [ViewModelDefinition] attribute";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewModelDefinitionMissing.Id);

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
                createChangedDocument: cancellationToken => AddAttributeAsync(
                    context.Document,
                    declaration,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Adds [ViewModelDefinition] attribute and necessary using statement.
    /// </summary>
    private static async Task<Document> AddAttributeAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        // Create attribute with default Transient lifetime
        var attribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("ViewModelDefinition"),
            SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals("Lifetime"),
                        null,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ParseName("ServiceLifetime"),
                            SyntaxFactory.IdentifierName("Transient"))))));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attribute))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        var newClassDeclaration = classDeclaration.AddAttributeLists(attributeList);

        // Replace the class declaration in the tree
        var newRoot = compilationUnit.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using statements if not present
        var usingsToAdd = new[]
        {
            "Blazing.Mvvm.ComponentModel",
            "Microsoft.Extensions.DependencyInjection"
        };

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
