using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YAnalyzers.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseImplicitTypeCodeFix)), Shared]
    public class UseImplicitTypeCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseImplicitTypeAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            if (node is not VariableDeclarationSyntax variableDeclaration)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: YAnalyzersResources.UseImplicitType,
                    createChangedDocument: _ => UseImplicitType(context.Document, root, variableDeclaration.Type),
                    equivalenceKey: nameof(YAnalyzersResources.UseImplicitType)),
                diagnostic);
        }

        private Task<Document> UseImplicitType(Document document, SyntaxNode root, TypeSyntax typeSyntax)
        {
            var newNode = SyntaxFactory.IdentifierName(SyntaxFacts.GetText(SyntaxKind.VarKeyword)).WithTriviaFrom(typeSyntax);
            var newDocument = document.WithSyntaxRoot(root.ReplaceNode(typeSyntax, newNode));
            return Task.FromResult(newDocument);
        }
    }
}
