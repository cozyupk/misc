using Microsoft.CodeAnalysis;

namespace PartialClassExtGen.Abstractions.Common
{
    /// <summary>
    /// Defines diagnostic descriptors used to report issues encountered during partial class extension and code generation processes.
    /// </summary>
    public interface IPCEGDiagnostics
    {
        /// <summary>
        /// Gets the diagnostic descriptor for the "missing partial modifier" code generation issue.
        /// Expected string format parameters: classSymbol.GenericQualifiedName()
        /// </summary>
        DiagnosticDescriptor PCEG0001_Missing_Partial_Modifier { get; }

        /// <summary>
        /// Gets the diagnostic descriptor for the PCEG0002 diagnostic, which indicates that an exception was thrown 
        /// during the generation of implementations.
        /// Expected string format parameters: classSymbol.GenericQualifiedName(), Exception.Message
        /// </summary>
        DiagnosticDescriptor PCEG0002_GenerateImplementations_ThrewException { get; }

        /// <summary>
        /// Gets the diagnostic descriptor for the "PCEG0003" diagnostic, which indicates an unexpected exception
        /// occurred during code generation.
        /// Expected string format parameters: Exception.Message
        /// </summary>
        DiagnosticDescriptor PCEG0003_UnexpectedExceptionWhileGeneratingCode { get; }

        /// <summary>
        /// Gets the diagnostic descriptor for the warning indicating that a stacked <see cref="StringBuilder"/>
        /// instance was detected without being properly disposed.
        /// </summary>
        /// <remarks>
        /// This diagnostic indicates that the generator's internal state may be corrupted due to a 
        /// <see cref="StringBuilder"/> instance not being disposed properly. 
        /// Such a condition suggests a malfunction in the partial class generation process 
        /// and may lead to incomplete or invalid code output.
        /// </remarks>
        DiagnosticDescriptor PCEG0001W_DetectedUndisposedStackedStringBuilderInstance { get; }
    }
}
