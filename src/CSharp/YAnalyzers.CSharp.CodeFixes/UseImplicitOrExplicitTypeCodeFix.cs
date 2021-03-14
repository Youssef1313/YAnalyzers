using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace YAnalyzers.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseImplicitOrExplicitTypeCodeFix)), Shared]
    public class UseImplicitOrExplicitTypeCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId,
            UseImplicitOrExplicitTypeAnalyzer.UseExplicitTypeDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            TypeSyntax? type = null;
            if (node is VariableDeclarationSyntax variableDeclaration)
            {
                type = variableDeclaration.Type;
            }
            else if (node is ForEachStatementSyntax forEach)
            {
                type = forEach.Type;
            }
            else if (node is DeclarationExpressionSyntax declarationExpression)
            {
                type = declarationExpression.Type;
            }
            else
            {
                return;
            }

            if (diagnostic.Id == UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    title: YAnalyzersResources.UseImplicitType,
                    createChangedDocument: _ => UseImplicitType(context.Document, root, type),
                    equivalenceKey: nameof(YAnalyzersResources.UseImplicitType)),
                diagnostic);
            }
            else if (diagnostic.Id == UseImplicitOrExplicitTypeAnalyzer.UseExplicitTypeDiagnosticId)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    title: YAnalyzersResources.UseExplicitType,
                    createChangedDocument: ct => UseExplicitTypeAsync(context.Document, root, type, ct),
                    equivalenceKey: nameof(YAnalyzersResources.UseImplicitType)),
                diagnostic);
            }
            else
            {
                throw new InvalidOperationException($"Unexpected diagnostic id for '{nameof(UseImplicitOrExplicitTypeCodeFix)}'.");
            }
        }

        private static Task<Document> UseImplicitType(Document document, SyntaxNode root, TypeSyntax typeSyntax)
        {
            var newNode = SyntaxFactory.IdentifierName(SyntaxFacts.GetText(SyntaxKind.VarKeyword)).WithTriviaFrom(typeSyntax);
            var newDocument = document.WithSyntaxRoot(root.ReplaceNode(typeSyntax, newNode));
            return Task.FromResult(newDocument);
        }

        private static async Task<Document> UseExplicitTypeAsync(Document document, SyntaxNode root, TypeSyntax typeSyntax, CancellationToken cancellationToken)
        {
            Debug.Assert(typeSyntax.IsVar);
            var model = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;
            var typeInfo = model.GetTypeInfo(typeSyntax, cancellationToken);
            
            var generator = SyntaxGenerator.GetGenerator(document);
            var newNode = generator.TypeExpression(typeInfo.ConvertedType).WithTriviaFrom(typeSyntax);

            return document.WithSyntaxRoot(root.ReplaceNode(typeSyntax, newNode));
        }
    }
}
