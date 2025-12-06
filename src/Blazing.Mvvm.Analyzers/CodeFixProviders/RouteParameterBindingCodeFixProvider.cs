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
/// Code fix provider for route parameter binding analyzer.
/// Generates missing [Parameter] or [ViewParameter] properties.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RouteParameterBindingCodeFixProvider))]
[Shared]
public sealed class RouteParameterBindingCodeFixProvider : CodeFixProvider
{
    private const string TitleParameter = "Add [Parameter] property";
    private const string TitleViewParameter = "Add [ViewParameter] property to ViewModel";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.RouteParameterBindingMissing.Id);

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

        // Extract parameter information from diagnostic properties
        if (!diagnostic.Properties.TryGetValue("ParameterName", out var parameterName) ||
            !diagnostic.Properties.TryGetValue("ParameterType", out var parameterType) ||
            parameterName is null ||
            parameterType is null)
        {
            return;
        }

        var isViewComponent = diagnostic.Properties.TryGetValue("IsViewComponent", out var isView) &&
                             isView == "true";

        var title = isViewComponent ? TitleParameter : TitleViewParameter;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: cancellationToken => AddParameterPropertyAsync(
                    context.Document,
                    parameterName,
                    parameterType,
                    isViewComponent,
                    cancellationToken),
                equivalenceKey: title),
            diagnostic);
    }

    /// <summary>
    /// Adds missing parameter property with appropriate attribute.
    /// </summary>
    private static async Task<Document> AddParameterPropertyAsync(
        Document document,
        string parameterName,
        string parameterType,
        bool isViewComponent,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Find the class to add the property to
        var classDeclaration = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration is null)
        {
            return document;
        }

        // Create property with proper casing (capitalize first letter)
        var propertyName = char.ToUpper(parameterName[0]) + parameterName.Substring(1);
        var attributeName = isViewComponent ? "Parameter" : "ViewParameter";

        // Create the property
        var property = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(parameterType),
                SyntaxFactory.Identifier(propertyName))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName(attributeName)))))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        var newClassDeclaration = classDeclaration.AddMembers(property);
        editor.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using statements
        if (root is CompilationUnitSyntax compilationUnit)
        {
            var usings = isViewComponent
                ? new[] { "Microsoft.AspNetCore.Components" }
                : new[] { "Blazing.Mvvm.Components" };

            foreach (var usingDirective in usings)
            {
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
        }

        return editor.GetChangedDocument();
    }
}
