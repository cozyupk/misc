using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

namespace Boostable.Syntax.Core.Abstractions.Analyzer
{
    /// <summary>
    /// Represents a rule that operates on a specific syntax node kind during code analysis.
    /// </summary>
    /// <remarks>This interface defines the contract for implementing syntax node rules in analyzers. A rule
    /// specifies the target syntax kind it applies to, the diagnostics it supports,  and the action to perform when
    /// visiting a syntax node of the specified kind.</remarks>
    public interface ISyntaxNodeRule
    {
        /// <summary>
        /// Gets the target <see cref="SyntaxKind"/> associated with this instance.
        /// </summary>
        SyntaxKind TargetSyntaxKind { get; }

        /// <summary>
        /// Gets the diagnostic descriptors that may be reported by this rule.
        /// </summary>
        IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; }

        /// <summary>
        /// Gets the action to be invoked for analyzing a syntax node during code analysis.
        /// </summary>
        Action<SyntaxNodeAnalysisContext> OnVisitSyntaxNode { get; }
    }
}
