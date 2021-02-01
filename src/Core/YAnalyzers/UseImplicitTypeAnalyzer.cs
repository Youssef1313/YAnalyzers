using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace YAnalyzers
{
    /// <summary>
    /// Y0001: Use implicit type when it's apparent.
    /// </summary>
    public abstract class UseImplicitTypeAnalyzer : DiagnosticAnalyzer // C#-only analyzer.
    {
        public const string DiagnosticId = "Y0001";

        private static readonly LocalizableString s_title = new LocalizableResourceString(nameof(YAnalyzersResources.UseImplicitTypeTitle), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));
        private static readonly LocalizableString s_message = new LocalizableResourceString(nameof(YAnalyzersResources.UseImplicitTypeMessage), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));
        private static readonly LocalizableString s_description = new LocalizableResourceString(nameof(YAnalyzersResources.UseImplicitTypeDescription), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));
        private const string Category = "Style";

        protected static readonly DiagnosticDescriptor s_rule = new(DiagnosticId, s_title, s_message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, s_description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            InitializeWorker(context);
        }

        protected abstract void InitializeWorker(AnalysisContext context);
    }
}
