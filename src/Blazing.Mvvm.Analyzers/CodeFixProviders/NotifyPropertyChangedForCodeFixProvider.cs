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
/// Code fix provider for NotifyPropertyChangedFor analyzer.
/// Adds [NotifyPropertyChangedFor(nameof(...))] attribute to dependent properties.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyPropertyChangedForCodeFixProvider))]
[Shared]
public sealed class NotifyPropertyChangedForCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add [NotifyPropertyChangedFor]";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.NotifyPropertyChangedForMissing.Id);

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

        // Find the property or field
        var node = root.FindToken(diagnosticSpan.Start).Parent;
        var propertyDeclaration = node?.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
        var fieldDeclaration = node?.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().FirstOrDefault();

        if (propertyDeclaration is null && fieldDeclaration is null)
        {
            return;
        }

        // Extract dependent property name from diagnostic properties
        if (!diagnostic.Properties.TryGetValue("DependentProperty", out var dependentProperty) ||
            dependentProperty is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => AddNotifyAttributeAsync(
                    context.Document,
                    propertyDeclaration,
                    fieldDeclaration,
                    dependentProperty,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Adds [NotifyPropertyChangedFor] attribute to the property or field.
    /// </summary>
    private static async Task<Document> AddNotifyAttributeAsync(
        Document document,
        PropertyDeclarationSyntax? propertyDeclaration,
        FieldDeclarationSyntax? fieldDeclaration,
        string dependentProperty,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Create attribute: [NotifyPropertyChangedFor(nameof(DependentProperty))]
        var attribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("NotifyPropertyChangedFor"),
            SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName("nameof"),
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.IdentifierName(dependentProperty)))))))));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attribute));

        if (propertyDeclaration is not null)
        {
            var newPropertyDeclaration = propertyDeclaration.AddAttributeLists(attributeList);
            editor.ReplaceNode(propertyDeclaration, newPropertyDeclaration);
        }
        else if (fieldDeclaration is not null)
        {
            var newFieldDeclaration = fieldDeclaration.AddAttributeLists(attributeList);
            editor.ReplaceNode(fieldDeclaration, newFieldDeclaration);
        }

        // Add using statement if not present
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is CompilationUnitSyntax compilationUnit)
        {
            var usingDirective = "CommunityToolkit.Mvvm.ComponentModel";
            var hasUsing = compilationUnit.Usings.Any(u =>
                u.Name?.ToString() == usingDirective);

            if (!hasUsing)
            {
                var newUsing = SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(usingDirective));
                editor.InsertBefore(compilationUnit.Members.FirstOrDefault() ?? (SyntaxNode)compilationUnit,
                    new[] { newUsing });
            }
        }

        return editor.GetChangedDocument();
    }
}
