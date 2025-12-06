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
/// Code fix provider for CascadingParameter vs Inject analyzer.
/// Replaces [CascadingParameter] with [Inject] for services.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CascadingParameterVsInjectCodeFixProvider))]
[Shared]
public sealed class CascadingParameterVsInjectCodeFixProvider : CodeFixProvider
{
    private const string Title = "Replace [CascadingParameter] with [Inject]";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.InjectPreferredOverCascading.Id);

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

        var propertyDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (propertyDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => ReplaceAttributeAsync(
                    context.Document,
                    propertyDeclaration,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Replaces [CascadingParameter] with [Inject].
    /// </summary>
    private static async Task<Document> ReplaceAttributeAsync(
        Document document,
        PropertyDeclarationSyntax propertyDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        // Find CascadingParameter attribute
        var cascadingAttribute = propertyDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString().Contains("CascadingParameter"));

        if (cascadingAttribute is null)
        {
            return document;
        }

        var attributeList = propertyDeclaration.AttributeLists
            .First(al => al.Attributes.Contains(cascadingAttribute));

        // Create new Inject attribute
        var injectAttribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("Inject"));

        var newAttributeList = attributeList.ReplaceNode(
            cascadingAttribute,
            injectAttribute);

        var newPropertyDeclaration = propertyDeclaration.ReplaceNode(
            attributeList,
            newAttributeList);

        // Replace the property declaration in the tree
        var newRoot = compilationUnit.ReplaceNode(propertyDeclaration, newPropertyDeclaration);

        // Add using statement if not present
        const string usingDirective = "Microsoft.AspNetCore.Components";
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
