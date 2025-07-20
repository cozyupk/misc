using Boostable.Syntax.Core.Abstractions.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

namespace Boostable.Syntax.Core.BaseImpl.AnalyzerBase
{
    /// <summary>
    /// Represents a rule for analyzing syntax nodes of a specific kind during code analysis.
    /// </summary>
    /// <remarks>This type is used to define a syntax node analysis rule by specifying the target <see
    /// cref="SyntaxKind"/>, the analysis logic to be executed for each matching syntax node, and the diagnostics that
    /// the rule can report. Instances of this class are typically used in custom analyzers to implement syntax-based
    /// code analysis.</remarks>
    public sealed record SyntaxNodeRule : ISyntaxNodeRule
    {
        /// <summary>
        /// Gets the target <see cref="SyntaxKind"/> associated with this instance.
        /// </summary>
        public SyntaxKind TargetSyntaxKind { get; }

        /// <summary>
        /// Gets the action to be invoked for analyzing a syntax node during code analysis.
        /// </summary>
        /// <remarks>Use this property to specify custom logic for handling syntax nodes during analysis.
        /// The provided action should define the behavior for analyzing individual syntax nodes.</remarks>
        public Action<SyntaxNodeAnalysisContext> OnVisitSyntaxNode { get; }

        /// <summary>
        /// Gets the collection of diagnostic descriptors supported by this analyzer.
        /// </summary>
        public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNodeRule"/> class, which defines a rule for analyzing
        /// syntax nodes of a specific kind and reporting diagnostics.
        /// </summary>
        /// <remarks>This constructor is used to define a syntax node analysis rule by specifying the
        /// target syntax kind, the analysis logic, and the diagnostics that the rule can report. The <paramref
        /// name="onVisitSyntaxNode"/> delegate is invoked for each syntax node of the specified kind encountered during
        /// analysis.</remarks>
        /// <param name="targetSyntaxKind">The <see cref="SyntaxKind"/> of the syntax nodes to be analyzed. Must be a valid <see cref="SyntaxKind"/>
        /// value.</param>
        /// <param name="onVisitSyntaxNode">The action to perform when a syntax node of the specified kind is visited during analysis. This action
        /// typically contains the logic for analyzing the node and reporting diagnostics.</param>
        /// <param name="supportedDiagnostics">A collection of <see cref="DiagnosticDescriptor"/> objects that describe the diagnostics supported by this
        /// rule. Cannot be null.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="targetSyntaxKind"/> is not a valid <see cref="SyntaxKind"/> value.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onVisitSyntaxNode"/> or <paramref name="supportedDiagnostics"/> is null.</exception>
        public SyntaxNodeRule(
            SyntaxKind targetSyntaxKind, Action<SyntaxNodeAnalysisContext> onVisitSyntaxNode, IEnumerable<DiagnosticDescriptor> supportedDiagnostics
        )
        {
            // Validate parameters
            if (!Enum.IsDefined(typeof(SyntaxKind), targetSyntaxKind))
            {
                throw new ArgumentException($"Invalid value for {nameof(targetSyntaxKind)}", nameof(targetSyntaxKind));
            }
            _ = onVisitSyntaxNode ?? throw new ArgumentNullException(nameof(onVisitSyntaxNode));
            _ = supportedDiagnostics ?? throw new ArgumentNullException(nameof(supportedDiagnostics));

            // Store the parameters
            TargetSyntaxKind = targetSyntaxKind;
            OnVisitSyntaxNode = onVisitSyntaxNode ?? throw new ArgumentNullException(nameof(onVisitSyntaxNode));
            SupportedDiagnostics = new HashSet<DiagnosticDescriptor>(supportedDiagnostics);
        }
    }
}
