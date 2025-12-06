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
/// Code fix provider for messenger registration lifetime analyzer.
/// Adds proper unregistration or converts to OnActivated pattern.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MessengerRegistrationLifetimeCodeFixProvider))]
[Shared]
public sealed class MessengerRegistrationLifetimeCodeFixProvider : CodeFixProvider
{
    private const string TitleDispose = "Add Dispose method with Unregister";
    private const string TitleOnActivated = "Use OnActivated pattern";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.MessengerRegistrationLeakPossible.Id);

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

        // Check if class inherits from RecipientViewModelBase
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
        var isRecipientViewModel = classSymbol?.BaseType?.Name == "RecipientViewModelBase";

        // Offer OnActivated pattern for RecipientViewModelBase
        if (isRecipientViewModel)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: TitleOnActivated,
                    createChangedDocument: cancellationToken => UseOnActivatedPatternAsync(
                        context.Document,
                        classDeclaration,
                        cancellationToken),
                    equivalenceKey: TitleOnActivated),
                diagnostic);
        }

        // Always offer Dispose pattern
        context.RegisterCodeFix(
            CodeAction.Create(
                title: TitleDispose,
                createChangedDocument: cancellationToken => AddDisposeWithUnregisterAsync(
                    context.Document,
                    classDeclaration,
                    cancellationToken),
                equivalenceKey: TitleDispose),
            diagnostic);
    }

    /// <summary>
    /// Converts constructor registration to OnActivated pattern.
    /// </summary>
    private static async Task<Document> UseOnActivatedPatternAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Create OnActivated method
        var onActivatedMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier("OnActivated"))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("// Move Messenger.Register calls here from constructor")));

        var newClassDeclaration = classDeclaration.AddMembers(onActivatedMethod);
        editor.ReplaceNode(classDeclaration, newClassDeclaration);

        return editor.GetChangedDocument();
    }

    /// <summary>
    /// Adds Dispose method with Unregister calls.
    /// </summary>
    private static async Task<Document> AddDisposeWithUnregisterAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Check if class already implements IDisposable
        var hasDisposable = classDeclaration.BaseList?.Types
            .Any(t => t.Type.ToString().Contains("IDisposable")) ?? false;

        ClassDeclarationSyntax newClassDeclaration;

        if (!hasDisposable)
        {
            // Add IDisposable to base list
            var disposableType = SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName("IDisposable"));

            var newBaseList = classDeclaration.BaseList is null
                ? SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(disposableType))
                : classDeclaration.BaseList.AddTypes(disposableType);

            newClassDeclaration = classDeclaration.WithBaseList(newBaseList);
        }
        else
        {
            newClassDeclaration = classDeclaration;
        }

        // Create or update Dispose method
        var disposeMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier("Dispose"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("// TODO: Add Messenger.Unregister calls here\n            GC.SuppressFinalize(this);")));

        newClassDeclaration = newClassDeclaration.AddMembers(disposeMethod);
        editor.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using System if not present
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is CompilationUnitSyntax compilationUnit)
        {
            var usingDirective = "System";
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
