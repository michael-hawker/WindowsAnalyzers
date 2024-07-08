using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DependencyPropertyNameEndsWithPropertyAnalyzerCodeFixProvider)), Shared]
public class DependencyPropertyNameEndsWithPropertyAnalyzerCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [DependencyPropertyNameEndsWithPropertyId];

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.DependencyPropertyNameEndsWithPropertyTitle,
                createChangedSolution: cancellationToken => RenameDependencyPropertyIdentifierAsync(context.Document, declaration, cancellationToken),
                equivalenceKey: nameof(CodeFixResources.DependencyPropertyNameEndsWithPropertyTitle)),
            diagnostic);
    }

    private async Task<Solution> RenameDependencyPropertyIdentifierAsync(Document document, VariableDeclaratorSyntax? identifierDecl, CancellationToken cancellationToken)
    {
        if (identifierDecl?.IsKind(SyntaxKind.VariableDeclarator) == true
            && !identifierDecl.Identifier.Text.EndsWith("Property")) // Note: Maybe this extra check isn't needed?
        {
            // Get new identifier name with "Property" suffix
            var newName = identifierDecl.Identifier.Text + "Property";

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(identifierDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, new(), newName, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }

        return document.Project.Solution;
    }
}
