using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace YAnalyzers.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseImplicitOrExplicitTypeAnalyzer : UseImplicitOrExplicitTypeAnalyzer
    {
        protected override void InitializeWorker(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeDeclarationExpression, SyntaxKind.DeclarationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
        }

        private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            // foreach (var x in y) {}
            // Consider as always not apparent and force usage of explicit type
            var node = (ForEachStatementSyntax)context.Node;
            if (node.Type.IsVar)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_useExplicitTypeRule, node.GetLocation()));
            }

        }

        private static void AnalyzeDeclarationExpression(SyntaxNodeAnalysisContext context)
        {
            // int.TryParse("", out var i);
            // foreach (var (a, b) in x) {}
            // Consider both cases as always not apparent and force usage of explicit.
            var node = (DeclarationExpressionSyntax)context.Node;
            if (node.Type.IsVar)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_useExplicitTypeRule, node.GetLocation()));
            }
        }

        private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (VariableDeclarationSyntax)context.Node;

            if (!ShouldAnalyze(node, out var initializer))
            {
                return;
            }

            // TODO: Create a utilities project containing NotNullWhenAttribute and use it to avoid suppression.
            TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(initializer!.Value, context.CancellationToken);

            var shouldUseVar = ShouldUseVar(node, initializer, typeInfo, context.SemanticModel);
            if (node.Type.IsVar && !shouldUseVar)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_useExplicitTypeRule, node.GetLocation()));
            }
            else if (!node.Type.IsVar && shouldUseVar)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_useImplicitTypeRule, node.GetLocation()));
            }
        }

        /// <summary>
        /// Determines whether a node can be handled by either Y0001 or Y0002.
        /// </summary>
        private static bool ShouldAnalyze(VariableDeclarationSyntax node, out EqualsValueClauseSyntax? initializer)
        {
            initializer = default;

            // Don't bother with checking nodes with compile-errors.
            if (node.DescendantNodesAndTokensAndSelf().Any(n => n.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)))
            {
                return false;
            }

            // CS0822: Implicitly-typed variables cannot be constant
            // TODO: Can it be a constant but parent is not LocalDeclarationStatementSyntax??
            if (node.Parent is LocalDeclarationStatementSyntax { IsConst: true })
            {
                return false;
            }

            // Fields can't use `var`.
            if (node.Parent.IsKind(SyntaxKind.FieldDeclaration)) // TODO: Is EventFieldDeclaration needed?
            {
                return false;
            }

            // CS0819: Implicitly-typed variables cannot have multiple declarators.
            if (node.Variables.Count != 1)
            {
                return false;
            }

            VariableDeclaratorSyntax declarator = node.Variables.Single();

            // CS0818: Implicitly-typed variables must be initialized
            if (declarator.Initializer is not EqualsValueClauseSyntax declaratorIitializer)
            {
                return false;
            }

            initializer = declaratorIitializer;
            return true;
        }

        /// <summary>
        /// Determines whether a node should use implicit type.
        /// </summary>
        private static bool ShouldUseVar(VariableDeclarationSyntax node, EqualsValueClauseSyntax initializer, TypeInfo info, SemanticModel model)
        {
            // Handle cases where using `var` can change semantics. e.g:
            // uint x = 0;
            // MyType2 t = (MyType)x;
            // MyType t = "";
            if (!SymbolEqualityComparer.Default.Equals(info.Type, info.ConvertedType))
            {
                return false;
            }

            // A type named 'var' exists in scope. Using implicit typing will change semantics and produce compile-errors unless the type is already 'var'.
            if (!model.LookupSymbols(node.Type.SpanStart, container: null, name: "var").IsEmpty &&
                info.Type?.Name != "var")
            {
                return false;
            }

            // string x = ""; // Should use var.
            // string x = $""; // Should use var.
            // int x = 0; // Should use var.
            // MyType t = (MyType)x; // Should use var.
            // MyType t = x as MyType; // Should use var.
            // int x = default(int); // Should use var.
            // TODO: Handle ArrayCreationExpression.
            return initializer.Value.IsKind(SyntaxKind.StringLiteralExpression) ||
                initializer.Value.IsKind(SyntaxKind.InterpolatedStringExpression) ||
                initializer.Value.IsKind(SyntaxKind.NumericLiteralExpression) ||
                initializer.Value.IsKind(SyntaxKind.CastExpression) ||
                initializer.Value.IsKind(SyntaxKind.AsExpression) ||
                initializer.Value.IsKind(SyntaxKind.DefaultExpression) ||
                initializer.Value.IsKind(SyntaxKind.ObjectCreationExpression);
        }
    }
}
