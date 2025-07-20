using PartialClassExtGen.Abstractions.Common;
using System;

namespace PartialClassExtGen.Utils
{
    /// <summary>
    /// Represents the base class for partial class extension functionality, providing access to an extender and
    /// diagnostic descriptors.
    /// </summary>
    /// <remarks>This class serves as a foundational type for managing partial class extensions, encapsulating
    /// an extender and optional diagnostic descriptors. The <see cref="Extender"/> property provides access to the
    /// extender functionality, while the <see cref="EmbeddedDiagnostics"/> property exposes diagnostic
    /// descriptors.</remarks>
    /// <typeparam name="TPartialClassExtender">The type of the extender that implements <see cref="IExtensionStrategy"/>.</typeparam>
    /// <typeparam name="TDiagnostics">The type of the diagnostic descriptors used for Partial Class Extension Genalyzer (PCEG), implementing <see
    /// cref="IPCEGDiagnostics"/>.</typeparam>
    public class PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics>
            where TPartialClassExtender : class, IExtensionStrategy
            where TDiagnostics : class, IPCEGDiagnostics
    {
        /// <summary>
        /// Gets the extender object associated with the partial class.
        /// </summary>
        public TPartialClassExtender Extender { get; }

        /// <summary>
        /// Gets the diagnostic descriptors used for PCEG (Partial Class Extention Genalyzer).
        /// </summary>
        public TDiagnostics Diagnostics
        {
            get
            {
                if (EmbeddedDiagnosticsInternal is null)
                {
                    throw new InvalidOperationException("DiagnosticsInternal is null. Ensure it is initialized properly.");
                }
                return EmbeddedDiagnosticsInternal;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialClassExtendeeBase{TPartialClassExtender,
        /// TPCEGDiagnostics}"/> class.
        /// </summary>
        /// <param name="extender">The extender instance used to provide additional functionality. This parameter cannot be <see
        /// langword="null"/>.</param>
        /// <param name="diagnostics">Optional diagnostics information for the partial class extender. If not provided, the default value is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="extender"/> is <see langword="null"/>.</exception>
        public PartialClassExtendeeBase(TPartialClassExtender extender, TDiagnostics? diagnostics = default)
        {
            // Validate parameters to ensure they are not null and store them in properties.
            Extender = extender ?? throw new ArgumentNullException(nameof(extender));
            EmbeddedDiagnosticsInternal = diagnostics;
        }

        /// <summary>
        /// Gets the internal diagnostics information for the PCEG system.
        /// </summary>
        private TDiagnostics? EmbeddedDiagnosticsInternal { get; }
    }
}