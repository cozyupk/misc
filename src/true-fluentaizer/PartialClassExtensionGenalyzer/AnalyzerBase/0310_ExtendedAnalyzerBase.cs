using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PartialClassExtGen.Abstractions.Analyzer;
using PartialClassExtGen.Abstractions.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PartialClassExtGen.AnalyzerBase
{
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning disable RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
    public class ExtendedAnalyzerBase<TPartialClassExtender, TDiagnostics> : DiagnosticAnalyzer
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning restore RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
        where TPartialClassExtender : class, IPartialClassExtender
        where TDiagnostics : class, IPCEGDiagnostics
    {
        /// <summary>
        /// Gets or sets the collection of syntax node analyzer rules.
        /// </summary>
        private IEnumerable<ISyntaxNodeRule>? AnalyzerRules { get; set; }

        /// <summary>
        /// Gets or sets the collection of diagnostic descriptors supported by the analyzer.
        /// </summary>
        private ImmutableArray<DiagnosticDescriptor>? SupportedDiagnosticsInternal { get; set; }

        /// <summary>
        /// Gets the collection of diagnostic descriptors supported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (SupportedDiagnosticsInternal is null)
                {
                    // If the diagnostics are not initialized, throw an exception.
                    throw new InvalidOperationException("SupportedDiagnosticsInternal is not initialized. Call InitializeExtendeeBase first.");
                }
                return SupportedDiagnosticsInternal.Value;
            }
        }

        /// <summary>
        /// Initializes the base state for an extendee by configuring the provided syntax node analyzer rules.
        /// </summary>
        /// <remarks>This method processes the provided rules to aggregate their supported diagnostics,
        /// which are then stored internally for use by the extendee. Rules with <see langword="null"/> values or
        /// unsupported diagnostics are ignored during processing.</remarks>
        /// <param name="analyzerRules">A collection of <see cref="ISyntaxNodeRule"/> instances that define the rules to be used for analysis. Each
        /// rule may contribute supported diagnostics to the extendee's configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyzerRules"/> is <see langword="null"/>.</exception>
        public void InitializeExtendeeBase(
            IEnumerable<ISyntaxNodeRule> analyzerRules
        ) {
            // Validate the parameters to ensure they are not null.
            AnalyzerRules = analyzerRules ?? throw new ArgumentNullException(nameof(analyzerRules));

            // Collect supported diagnostics from rules.
            var merged = new HashSet<DiagnosticDescriptor>();
            foreach (var rule in AnalyzerRules)
            {
                if (rule is null)
                {
                    continue;
                }
                if (rule.SupportedDiagnostics is null)
                {
                    continue;
                }
                merged.UnionWith(rule.SupportedDiagnostics);
            }

            // Store the collected diagnostics in the internal property.
            SupportedDiagnosticsInternal = merged.ToImmutableArray();
        }

        /// <summary>
        /// Initializes the analysis context for the analyzer.
        /// </summary>
        /// <remarks>This method enables concurrent execution of the analyzer and disables analysis of
        /// generated code.</remarks>
        /// <param name="context">The <see cref="AnalysisContext"/> to configure for this analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // Validate the parameters and internal state before proceeding.
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = AnalyzerRules ?? throw new InvalidOperationException("AnalyzerRules is not initialized. Call InitializeExtendeeBase first.");

            // Initialize the context for analysis.
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            foreach (var rule in AnalyzerRules)
            {
                if (rule is null)
                {
                    continue;
                }
                // Register the syntax node rule for analysis.
                context.RegisterSyntaxNodeAction(
                    rule.OnVisitSyntaxNode, rule.TargetSyntaxKind
                );
            }
        }
    }
}
