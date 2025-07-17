using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Utils;

namespace PartialClassExtGen.GenalyzerBase
{
    /// <summary>
    /// Provides a set of diagnostic descriptors used to report issues encountered during partial class extension and
    /// code generation processes.
    /// </summary>
    /// <remarks>This class defines diagnostic descriptors for common errors and exceptions that may occur
    /// when using partial class extenders. These descriptors can be used to create diagnostic messages that help
    /// developers identify and resolve issues in their code.</remarks>
    public class PCEGDiagnostics
        : PartialClassExtendeeBase, IPCEGDiagnostics
    {
        /// <summary>
        /// Gets the diagnostic descriptor for the "missing partial modifier" code generation error.
        /// </summary>
        public DiagnosticDescriptor PCEG0001_Missing_Partial_Modifier { get; }

        /// <summary>
        /// Gets the diagnostic descriptor for the PCEG0002 diagnostic, which indicates that an exception was thrown
        /// during generating code implementations in extention side.
        /// </summary>
        public DiagnosticDescriptor PCEG0002_GenerateImplementations_ThrewException { get; }

        /// <summary>
        /// /// Gets the diagnostic descriptor for the PCEG0003 diagnostic, which indicates an unexpected exception thrown in PCEG side.
        /// </summary>
        public DiagnosticDescriptor PCEG0003_UnexpectedExceptionWhileGeneratingCode { get; }

        /// <summary>
        /// Provides diagnostic descriptors for errors and warnings related to partial class extension generation.
        /// </summary>
        public PCEGDiagnostics(IPartialClassExtender partialClassExtender) : base(partialClassExtender) {

            // define diagnostic descriptors for common issues encountered during partial class extension generation.

            // {0}: classSymbol.GenericQualifiedName()
            PCEG0001_Missing_Partial_Modifier = new(
                        Extender.PrefixForDiagnosticId + "0001",
                        "Missing Partial Modifier",
                        $"The class '{0}' must be marked as 'partial' to enable {Extender.ExtensionName} generation.",
                        "Usage",
                        DiagnosticSeverity.Error,
                        true
            );

            // {0}: classSymbol.GenericQualifiedName()
            // {1}: Exception.Message
            PCEG0002_GenerateImplementations_ThrewException = new(
                    Extender.PrefixForDiagnosticId + "0002",
                    "GenerateImplementations Threw An Exception.",
                    $"The extender '{Extender.GetType().Name}', threw an exception while generating implementations for {Extender.ExtensionName}: [{0}] {1}",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
            );

            // {0}: Exception.Message
            PCEG0003_UnexpectedExceptionWhileGeneratingCode = new(
                    Extender.PrefixForDiagnosticId + "0003",
                    "Unexpected Exception While Generating Code.",
                    $"Unexpected exception during '{Extender.ExtensionName}' code generation: {0}",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
            );
        }
    }
}
