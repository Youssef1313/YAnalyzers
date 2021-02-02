using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace YAnalyzers
{
    /// <summary>
    /// Y0001: Use implicit type when it's apparent.
    /// Y0002: Use explicit type when type is not apparent.
    /// </summary>
    public abstract class UseImplicitOrExplicitTypeAnalyzer : DiagnosticAnalyzer // C#-only analyzer.
    {
        private const string Category = "Style";
        public const string UseImplicitTypeDiagnosticId = "Y0001";
        public const string UseExplicitTypeDiagnosticId = "Y0002";

        private static readonly LocalizableString s_useImplicitTypeTitle = new LocalizableResourceString(nameof(YAnalyzersResources.UseImplicitTypeTitle), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));
        private static readonly LocalizableString s_useImplicitTypeMessage = new LocalizableResourceString(nameof(YAnalyzersResources.UseImplicitTypeMessage), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));
        private static readonly LocalizableString s_useImplicitTypeDescription = new LocalizableResourceString(nameof(YAnalyzersResources.UseImplicitTypeDescription), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));

        private static readonly LocalizableString s_useExplicitTypeTitle = new LocalizableResourceString(nameof(YAnalyzersResources.UseExplicitTypeTitle), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));
        private static readonly LocalizableString s_useExplicitTypeMessage = new LocalizableResourceString(nameof(YAnalyzersResources.UseExplicitTypeMessage), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));
        private static readonly LocalizableString s_useExplicitTypeDescription = new LocalizableResourceString(nameof(YAnalyzersResources.UseExplicitTypeDescription), YAnalyzersResources.ResourceManager, typeof(YAnalyzersResources));

        protected static readonly DiagnosticDescriptor s_useImplicitTypeRule = new(
            UseImplicitTypeDiagnosticId, s_useImplicitTypeTitle, s_useImplicitTypeMessage, Category, DiagnosticSeverity.Warning,
            isEnabledByDefault: true, s_useImplicitTypeDescription);

        protected static readonly DiagnosticDescriptor s_useExplicitTypeRule = new(
    UseExplicitTypeDiagnosticId, s_useExplicitTypeTitle, s_useExplicitTypeMessage, Category, DiagnosticSeverity.Warning,
    isEnabledByDefault: true, s_useExplicitTypeDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_useImplicitTypeRule, s_useExplicitTypeRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            InitializeWorker(context);
        }

        protected abstract void InitializeWorker(AnalysisContext context);
    }
}
