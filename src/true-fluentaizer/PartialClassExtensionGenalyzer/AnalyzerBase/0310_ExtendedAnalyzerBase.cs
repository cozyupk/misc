using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PartialClassExtGen.Abstractions.Analyzer;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Utils;
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
        /// Gets or sets the internal base class instance for the partial class extender.
        /// </summary>
        /// <remarks>This property is intended for internal use and provides access to the base
        /// functionality of the partial class extender. It may be null if the base instance has not been
        /// initialized.</remarks>
        private PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics>? ExtendeeBaseInternal { get; set; }

        /// <summary>
        /// Gets the base instance that provides core functionality for the partial class extender.
        /// </summary>
        private PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics> ExtendeeBase {
            get
            {
                if (ExtendeeBaseInternal is null)
                {
                    throw new InvalidOperationException("ExtendeeBaseInternal is not initialized. Call InitializeExtendeeBase first.");
                }
                return ExtendeeBaseInternal;
            }
        }

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
        /// Initializes the base class for extending partial class analysis with the specified extender, diagnostics,
        /// and analyzer rules.
        /// </summary>
        /// <remarks>This method initializes the internal state required for partial class analysis,
        /// including the extender, diagnostics, and supported syntax node rules. The supported diagnostics are derived
        /// from the provided rules.</remarks>
        /// <param name="partialClassExtender">The extender instance that provides additional functionality for analyzing partial classes. Cannot be <see langword="null"/>.</param>
        /// <param name="diagnostics">The diagnostic descriptors used to report issues during analysis. Cannot be <see langword="null"/>.</param>
        /// <param name="analyzerRules">A collection of syntax node rules that define the analysis logic. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyzerRules"/> is <see langword="null"/>.</exception>
        public void InitializeExtendeeBase(
            TPartialClassExtender partialClassExtender,
            TDiagnostics diagnostics,
            IEnumerable<ISyntaxNodeRule> analyzerRules
        ) {
            // Validate the parameters to ensure they are not null.
            _ = partialClassExtender ?? throw new ArgumentNullException(nameof(partialClassExtender));
            _ = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            AnalyzerRules = analyzerRules ?? throw new ArgumentNullException(nameof(analyzerRules));

            // Initialize the base class with the provided extender and diagnostic descriptors as a property.
            // Note: We cannot inherit from PartialClassExtendeeBase because we already inherit from DiagnosticAnalyzer.
            ExtendeeBaseInternal = new PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics>(partialClassExtender, diagnostics);

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
