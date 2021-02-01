﻿using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace YAnalyzers.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseImplicitTypeAnalyzer : UseImplicitTypeAnalyzer
    {
        protected override void InitializeWorker(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
        }

        private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (VariableDeclarationSyntax)context.Node;

            // Don't bother with checking nodes with compile-errors.
            if (node.DescendantNodesAndTokensAndSelf().Any(n => n.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)))
            {
                return;
            }

            // Fields can't use `var`.
            if (node.Parent.IsKind(SyntaxKind.FieldDeclaration))
            {
                return;
            }

            // Variable is already implicitly typed. If it shouldn't, this will be handled by another analyzer.
            if (node.Type.IsVar)
            {
                return;
            }

            // CS0819: Implicitly-typed variables cannot have multiple declarators.
            if (node.Variables.Count != 1)
            {
                return;
            }

            VariableDeclaratorSyntax declarator = node.Variables.Single();

            // CS0818: Implicitly-typed variables must be initialized
            if (declarator.Initializer is not EqualsValueClauseSyntax initializer)
            {
                return;
            }

            TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(initializer.Value, context.CancellationToken);
            // Handle cases where using `var` can change semantics. e.g:
            // uint x = 0;
            // MyType2 t = (MyType)x;
            // MyType t = "";
            if (!SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeInfo.ConvertedType))
            {
                return;
            }

            // string x = ""; // Should use var.
            // string x = $""; // Should use var.
            // int x = 0; // Should use var.
            // MyType t = (MyType)x; // Should use var.
            if (initializer.Value.IsKind(SyntaxKind.StringLiteralExpression) ||
                initializer.Value.IsKind(SyntaxKind.InterpolatedStringExpression) ||
                initializer.Value.IsKind(SyntaxKind.NumericLiteralExpression) ||
                initializer.Value.IsKind(SyntaxKind.CastExpression) ||
                initializer.Value.IsKind(SyntaxKind.ObjectCreationExpression))
            {
                context.ReportDiagnostic(Diagnostic.Create(s_rule, node.GetLocation()));
            }
        }
    }
}
