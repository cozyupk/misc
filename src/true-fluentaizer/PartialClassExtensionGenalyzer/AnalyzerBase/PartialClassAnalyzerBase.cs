using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Utils;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace PartialClassExtGen.AnalyzerBase
{
    /// <summary>
    /// Serves as a base class for diagnostic analyzers that validate partial class declarations based on specific
    /// attributes and requirements.
    /// </summary>
    /// <remarks>This class provides functionality to analyze class declarations and ensure they meet specific
    /// criteria, such as being marked as partial when a target attribute is applied. It cannot directly inherit from
    /// <see cref="PartialClassExtendeeBase"/> because it already inherits from <see
    /// cref="DiagnosticAnalyzer"/>.
    /// Note: This analyzer is not intended to be discovered by [DiagnosticAnalyzer] attribute scanning.
    /// It is used programmatically via Roslyn source generators.</remarks>
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning disable RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
    public class PartialClassAnalyzerBase<TPartialClassExtender, TDiagnostics> : DiagnosticAnalyzer
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning restore RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
        where TPartialClassExtender : class, IPartialClassExtender
        where TDiagnostics : class, IPCEGDiagnostics
    {
        /// <summary>
        /// Gets or sets the internal base instance for the partial class extender.
        /// </summary>
        /// <remarks>This property is intended for internal use and provides access to the base
        /// functionality  of the partial class extender. It may be null if the base instance has not been
        /// initialized.</remarks>
        protected PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics>? ExtendeeBaseInternal { get; set; }

        /// <summary>
        /// Gets the base instance of the partial class extender.
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
        /// Gets the collection of diagnostic descriptors that this analyzer supports.  
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(ExtendeeBase.Diagnostics.PCEG0001_Missing_Partial_Modifier);

        /// <summary>
        /// Initializes the base class for the partial class extender with the specified extender and diagnostic
        /// descriptors.
        /// </summary>
        /// <remarks>This method sets up the internal base class representation using the provided
        /// extender and diagnostics. Note that inheritance from <c>PartialClassExtendeeBase</c> is not possible due to
        /// existing inheritance from <c>DiagnosticAnalyzer</c>.</remarks>
        /// <param name="partialClassExtender">The extender instance used to provide additional functionality for the partial class.</param>
        /// <param name="diagnostics">The diagnostic descriptors associated with the partial class extender.</param>
        public void InitializeExtendeeBase(
            TPartialClassExtender partialClassExtender,
            TDiagnostics diagnostics
        ) {
            // Initialize the base class with the provided extender and diagnostic descriptors as a property.
            // Note: We cannot inherit from PartialClassExtendeeBase because we already inherit from DiagnosticAnalyzer.
            ExtendeeBaseInternal = new PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics>(partialClassExtender, diagnostics);
        }

        /// <summary>
        /// Initializes the analysis context for this analyzer.
        /// </summary>
        /// <remarks>This method enables concurrent execution of the analyzer, disables analysis of
        /// generated code,  and registers a syntax node action to analyze class declarations.</remarks>
        /// <param name="context">The <see cref="AnalysisContext"/> to configure. This provides the mechanism for registering analysis actions
        /// and configuring analysis behavior.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        /// <summary>
        /// Analyzes a class declaration to ensure it meets specific requirements, such as being marked as partial when
        /// a target attribute is applied.
        /// </summary>
        /// <remarks>This method checks if the class declaration has a specific target attribute applied,
        /// as determined by the extender's configuration. If the target attribute is present, the method verifies that
        /// the class is declared as partial. If the class is not partial, a diagnostic is reported to indicate the
        /// missing partial modifier.</remarks>
        /// <param name="context">The <see cref="SyntaxNodeAnalysisContext"/> providing the context for the analysis, including the syntax
        /// node to analyze and the semantic model for symbol resolution.</param>
        private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            // Ensure the node is a class declaration
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol) return;

            // Get the fully qualified name of the target attribute from the extender
            var extender = ExtendeeBase.Extender;
            var fullName = extender.TargetAttribute.FullName;
            if (string.IsNullOrEmpty(fullName)) return; // fail-safe fallback
            INamedTypeSymbol? targetAttrSymbol =
                context.SemanticModel.Compilation.GetTypeByMetadataName(extender.TargetAttribute.FullName!);
            if (targetAttrSymbol is null) return; // fail-safe fallback

            // Check if the class has the target attribute
            bool hasTargetAttribute = classSymbol
                .GetAttributes()
                .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, targetAttrSymbol));

            // If the class does not have the target attribute, no further action is needed
            if (!hasTargetAttribute) return; // fail-safe fallback

            // Check if the class is declared as partial
            if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                // If the class is not partial, report a diagnostic
                var name = classSymbol.GetGenericQualifiedName();
                var extensionName = extender.ExtensionName;
                var diagnostic
                    = Diagnostic.Create(
                        ExtendeeBase.Diagnostics.PCEG0001_Missing_Partial_Modifier,
                        classDecl.Identifier.GetLocation(),
                        name, extensionName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
