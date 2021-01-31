using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace YAnalyzers.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseImplicitTypeAnalyzer : UseImplicitTypeAnalyzer
    {
        protected override void InitializeWorker(AnalysisContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
