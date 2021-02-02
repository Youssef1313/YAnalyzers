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

            if (diagnostic.Id == UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    title: YAnalyzersResources.UseImplicitType,
                    createChangedDocument: _ => UseImplicitType(context.Document, root, variableDeclaration.Type),
                    equivalenceKey: nameof(YAnalyzersResources.UseImplicitType)),
                diagnostic);
            }
            else if (diagnostic.Id == UseImplicitOrExplicitTypeAnalyzer.UseExplicitTypeDiagnosticId)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    title: YAnalyzersResources.UseExplicitType,
                    createChangedDocument: ct => UseExplicitType(context.Document, root, variableDeclaration, ct),
                    equivalenceKey: nameof(YAnalyzersResources.UseImplicitType)),
                diagnostic);
            }
            else
            {
                throw new InvalidOperationException($"Unexpected diagnostic id for '{nameof(UseImplicitOrExplicitTypeCodeFix)}'.");
            }
        }

        private Task<Document> UseImplicitType(Document document, SyntaxNode root, TypeSyntax typeSyntax)
        {
            var newNode = SyntaxFactory.IdentifierName(SyntaxFacts.GetText(SyntaxKind.VarKeyword)).WithTriviaFrom(typeSyntax);
            var newDocument = document.WithSyntaxRoot(root.ReplaceNode(typeSyntax, newNode));
            return Task.FromResult(newDocument);
        }

        private async Task<Document> UseExplicitType(Document document, SyntaxNode root, VariableDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            Debug.Assert(declarationSyntax.Type.IsVar);
            var model = (await document.GetSemanticModelAsync(cancellationToken))!;
            var typeInfo = model.GetTypeInfo(declarationSyntax.Variables.Single().Initializer!.Value, cancellationToken);
            
            var generator = SyntaxGenerator.GetGenerator(document);
            var newNode = generator.TypeExpression(typeInfo.ConvertedType).WithTriviaFrom(declarationSyntax.Type);

            return document.WithSyntaxRoot(root.ReplaceNode(declarationSyntax.Type, newNode));
        }
    }
}
