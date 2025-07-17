using PartialClassExtGen.Abstractions.Common;
using System;

namespace PartialClassExtGen.Utils
{
    /// <summary>
    /// Serves as the base class for partial class extensions, providing core functionality and diagnostic support.
    /// </summary>
    /// <remarks>This class is designed to be extended by partial class implementations. It provides a
    /// mechanism to integrate additional functionality through an <see cref="IPartialClassExtender"/> and supports
    /// diagnostic handling via <see cref="IPCEGDiagnostics"/>. Both dependencies are required for proper
    /// operation.</remarks>
    public class PartialClassExtendeeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartialClassExtendeeBase"/> class.
        /// </summary>
        /// <param name="extender">The extender instance that provides additional functionality for the partial class.</param>
        /// <param name="pcegDiagnostics">The diagnostic descriptors used for handling diagnostics in the partial class extension.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="extender"/> is <see langword="null"/> or if <paramref
        /// name="pcegDiagnostics"/> is <see langword="null"/>.</exception>
        public PartialClassExtendeeBase(IPartialClassExtender extender, IPCEGDiagnostics? pcegDiagnostics = null)
        {
            // Validate parameters to ensure they are not null and store them in properties.
            Extender = extender ?? throw new ArgumentNullException(nameof(extender));
            PCEGDiagnosticsInternal = pcegDiagnostics;
        }

        /// <summary>
        /// Gets the extender that provides additional functionality for partial classes.
        /// </summary>
        public IPartialClassExtender Extender { get; }

        /// <summary>
        /// Gets the internal diagnostic descriptors for PCEG (Process Control Execution Graph).
        /// </summary>
        private IPCEGDiagnostics? PCEGDiagnosticsInternal { get; }

        /// <summary>
        /// Gets the diagnostic descriptors used for PCEG (Partial Class Extention Genalyzer).
        /// </summary>
        public IPCEGDiagnostics PCEGDiagnostics
        {
            get
            {
                if (PCEGDiagnosticsInternal is null)
                {
                    throw new InvalidOperationException("pcegDiagnosticsInternal is null. Ensure it is initialized properly.");
                }
                return PCEGDiagnosticsInternal;
            }
        }
    }
}