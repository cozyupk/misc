using Boostable.Syntax.Core.Abstractions.Common;
using System;

namespace Boostable.Syntax.Core.BaseImpl.BaseUtils
{
    /// <summary>
    /// Represents a base class for syntax-boostable components, providing a strategy for boosting and diagnostics for
    /// tracking issues or performance metrics.
    /// </summary>
    /// <remarks>This class is designed to be used as a base for components that require a boosting strategy
    /// and diagnostics functionality. It ensures that the boosting strategy is always initialized, while diagnostics
    /// are optional and can be null.</remarks>
    /// <typeparam name="TBoostingStrategy">The type of the boosting strategy, which must implement <see cref="IBoostingStrategy"/>.</typeparam>
    /// <typeparam name="TDiagnostics">The type of the diagnostics, which must implement <see cref="IDiagnostics"/>.</typeparam>
    public class SyntaxBoostableBase<TBoostingStrategy, TDiagnostics>
        where TBoostingStrategy : class, IBoostingStrategy
        where TDiagnostics : class, IDiagnostics
    {
        /// <summary>
        /// Gets the boosting strategy used for the current operation.
        /// </summary>
        public TBoostingStrategy Strategy { get; }

        /// <summary>
        /// Gets the diagnostics information associated with the current instance.
        /// </summary>
        public TDiagnostics Diagnostics
        {
            get
            {
                if (DiagnosticsInternal is null)
                {
                    throw new InvalidOperationException("DiagnosticsInternal is null. Ensure it is initialized properly.");
                }
                return DiagnosticsInternal;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxBoostableBase{TBoostingStrategy, TDiagnostics}"/> class
        /// with the specified boosting strategy and optional diagnostics.
        /// </summary>
        /// <param name="extender">The boosting strategy to be used. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="diagnostics">Optional diagnostics information. If not provided, the default value is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="extender"/> is <see langword="null"/>.</exception>
        public SyntaxBoostableBase(TBoostingStrategy extender, TDiagnostics? diagnostics = default)
        {
            // Validate parameters to ensure they are not null and store them in properties.
            Strategy = extender ?? throw new ArgumentNullException(nameof(extender));
            DiagnosticsInternal = diagnostics;
        }

        /// <summary>
        /// Gets the internal diagnostics information for the current instance.
        /// </summary>
        /// <remarks>This property is intended for internal use and may provide diagnostic data  specific
        /// to the implementation. It is not exposed publicly.</remarks>
        private TDiagnostics? DiagnosticsInternal { get; }
    }
}