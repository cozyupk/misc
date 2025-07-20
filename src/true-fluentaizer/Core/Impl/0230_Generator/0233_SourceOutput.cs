using Boostable.Syntax.Core.Abstractions.Analyzer;
using Boostable.Syntax.Core.Abstractions.Common;
using Boostable.Syntax.Core.BaseImpl.BaseUtils;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Boostable.Syntax.Core.BaseImpl.Generator
{
    /// <summary>
    /// Represents an abstract base class for generating source code using a specified boosting strategy, diagnostics,
    /// and target class metadata.
    /// </summary>
    /// <remarks>This class provides a foundation for source generation by combining a boosting strategy,
    /// diagnostics, and metadata for target classes. Derived classes must implement the <see
    /// cref="GenerateSource(SourceProductionContext, (Compilation, ImmutableArray{TTargetClassMeta?}))"/> method to
    /// define the logic for generating source code.</remarks>
    /// <typeparam name="TBoostingStrategy">The type of the boosting strategy used for processing. Must implement <see cref="IBoostingStrategy"/>.</typeparam>
    /// <typeparam name="TDiagnostics">The type of the diagnostics used for logging or tracking issues. Must implement <see cref="IDiagnostics"/>.</typeparam>
    /// <typeparam name="TTargetClassMeta">The type of metadata associated with the target classes. Must implement <see cref="ITargetSymbolMeta"/>.</typeparam>
    public abstract class SourceOutput<TBoostingStrategy, TDiagnostics, TTargetClassMeta>
        : SyntaxBoostableBase<TBoostingStrategy, TDiagnostics>, ISourceOutput<TTargetClassMeta>
        where TBoostingStrategy : class, IBoostingStrategy
        where TDiagnostics : class, IDiagnostics
        where TTargetClassMeta : ITargetSymbolMeta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOutput{TBoostingStrategy, TDiagnostics}"/> class with the
        /// specified boosting strategy and diagnostics.
        /// </summary>
        /// <remarks>This constructor sets up the base functionality using the provided strategy and
        /// diagnostics. Ensure that both parameters are properly initialized before calling this constructor.</remarks>
        /// <param name="strategy">The boosting strategy to be used for processing. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="diagnostics">The diagnostics instance for logging or tracking issues. This parameter cannot be <see langword="null"/>.</param>
        public SourceOutput(
            TBoostingStrategy strategy, TDiagnostics diagnostics
        ) : base(strategy, diagnostics)
        {
            // No additional initialization needed here.
        }

        /// <summary>
        /// Generates source code based on the provided compilation and metadata.
        /// </summary>
        /// <remarks>This method is abstract and must be implemented by a derived class to define the
        /// logic for  generating source code. The implementation should handle null values in the metadata array 
        /// appropriately and ensure that any diagnostics are reported through the <paramref name="spc"/>
        /// parameter.</remarks>
        /// <param name="spc">The context for source production, used to report diagnostics and add generated source files.</param>
        /// <param name="source">A tuple containing the compilation and an immutable array of target class metadata.  The <c>Left</c> element
        /// represents the current compilation, and the <c>Right</c> element  contains metadata for the target classes
        /// to be processed. The array may include null values.</param>
        public abstract void GenerateSource(SourceProductionContext spc, (Compilation Left, ImmutableArray<TTargetClassMeta?> Right) source);
    }
}
