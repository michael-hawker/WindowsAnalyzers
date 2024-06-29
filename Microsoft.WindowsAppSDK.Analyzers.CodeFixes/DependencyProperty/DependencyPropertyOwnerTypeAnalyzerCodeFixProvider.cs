using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DependencyPropertyOwnerTypeAnalyzerCodeFixProvider)), Shared]
public class DependencyPropertyOwnerTypeAnalyzerCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [DependencyPropertyOwnerTypeId];

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var declaration = root.FindToken(diagnosticSpan.Start);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.DependencyPropertyOwnerTypeFixTitle,
                createChangedDocument: cancellationToken => SwitchOwnerTypeAsync(context.Document, declaration, cancellationToken),
                equivalenceKey: nameof(CodeFixResources.DependencyPropertyOwnerTypeFixTitle)),
            diagnostic);
    }

    private async Task<Document> SwitchOwnerTypeAsync(Document document, SyntaxToken identifierDecl, CancellationToken cancellationToken)
    {
        if (identifierDecl.IsKind(SyntaxKind.IdentifierToken) 
            && identifierDecl.Parent.Ancestors(true).Where(syntax => syntax.IsKind(SyntaxKind.ClassDeclaration)).FirstOrDefault() is ClassDeclarationSyntax classDeclarationSyntax)
        {
            var newIdentifierName = SyntaxFactory.IdentifierName(classDeclarationSyntax.Identifier.Value.ToString());

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(identifierDecl.Parent, newIdentifierName);

            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
}
