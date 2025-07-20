using Boostable.Syntax.Core.Abstractions.Analyzer;
using Boostable.Syntax.Core.Abstractions.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Boostable.Syntax.Core.BaseImpl.AnalyzerBase
{
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning disable RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
    public class ExtendedAnalyzerBase : DiagnosticAnalyzer
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning restore RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
    {
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
        /// Initializes the base state for an extendee by configuring diagnostics and analyzer rules.
        /// </summary>
        /// <remarks>This method validates the provided parameters and aggregates the supported
        /// diagnostics from the specified analyzer rules. The collected diagnostics are stored internally for use
        /// during analysis.</remarks>
        /// <param name="diagnostics">The diagnostics instance used to report issues during analysis. Cannot be <see langword="null"/>.</param>
        /// <param name="analyzerRules">A collection of syntax node rules to be used for analysis. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="diagnostics"/> is <see langword="null"/> or if <paramref name="analyzerRules"/> is
        /// <see langword="null"/>.</exception>
        public void InitializeExtendeeBase(
            IDiagnostics diagnostics,
            IEnumerable<ISyntaxNodeRule> analyzerRules
        ) {
            // Validate the parameters to ensure they are not null.
            AnalyzerRules = analyzerRules ?? throw new ArgumentNullException(nameof(analyzerRules));
            DiagnosticForOnExceptionInternal = diagnostics.WARN9801_DetectedExceptionOnAnalyzer
                ?? throw new ArgumentNullException(nameof(diagnostics), "Diagnostics cannot be null.");

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
            merged.Add(diagnostics.WARN9801_DetectedExceptionOnAnalyzer);
            // Store the collected diagnostics in the internal property.
            SupportedDiagnosticsInternal = merged.ToImmutableArray();
        }

        /// <summary>
        /// Initializes the analysis context for the analyzer, enabling concurrent execution and configuring the
        /// handling of generated code. Registers syntax node actions for all defined analyzer rules.
        /// </summary>
        /// <remarks>This method sets up the analyzer to process syntax nodes based on the rules defined
        /// in <c>AnalyzerRules</c>.  It enables concurrent execution for improved performance and disables analysis of
        /// generated code.  If an exception occurs during the execution of a syntax node action, it is reported as a
        /// diagnostic.</remarks>
        /// <param name="context">The <see cref="AnalysisContext"/> to configure for the analyzer. This parameter cannot be <see
        /// langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the analyzer's rules are not initialized. Ensure that <c>InitializeExtendeeBase</c> is called
        /// before invoking this method.</exception>
        public override void Initialize(AnalysisContext context)
        {
            // Validate the parameters and internal state before proceeding.
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = AnalyzerRules ?? throw new InvalidOperationException("AnalyzerRules is not initialized. Call InitializeExtendeeBase first.");

            // Initialize the context for analysis.
            // TODO: Consider adding a configuration option to enable or disable concurrent execution and generated code analysis.
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
                    (context) =>
                    {
                        try
                        {
                            rule.OnVisitSyntaxNode(context);
                        }
                        catch (Exception ex)
                        {
                            var syntaxNode = context.Node;
                            // If an exception occurs during analysis, report it as a diagnostic.
                            context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticForOnException,
                            syntaxNode.GetLocation() ?? Location.None,
                            syntaxNode,
                            ex.Message
                        ));
                        }
                    },
                    rule.TargetSyntaxKind
                );
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
        /// Gets or sets the diagnostic descriptor used internally for handling exceptions.
        /// </summary>
        private DiagnosticDescriptor? DiagnosticForOnExceptionInternal { get; set; }

        /// <summary>
        /// Gets the diagnostic descriptor used to represent exceptions encountered during execution.
        /// </summary>
        private DiagnosticDescriptor DiagnosticForOnException
        {
            get
            {
                if (DiagnosticForOnExceptionInternal is null)
                {
                    throw new InvalidOperationException("DiagnosticForOnExceptionInternal is not initialized. Call InitializeExtendeeBase first.");
                }
                return DiagnosticForOnExceptionInternal;
            }
        }
    }
}
