using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PartialClassExtGen.Abstractions.Generator
{
    /// <summary>
    /// Defines a contract for generating source code based on a given compilation and associated metadata.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for producing source code by analyzing the
    /// provided compilation and metadata. The generated source code is added to the context, and any diagnostics
    /// encountered during the process can also be reported through the context.</remarks>
    public interface ISourceOutput
    {
        /// <summary>
        /// Generates source code based on the provided compilation and metadata.
        /// </summary>
        /// <param name="spc">The context used to report diagnostics and add generated source code.</param>
        /// <param name="source">A tuple containing the compilation and an immutable array of builder class metadata. The <c>Left</c> element
        /// represents the current compilation, and the <c>Right</c> element contains metadata for builder classes,
        /// which may include null values.</param>
        void SourceOutput(
            SourceProductionContext spc,
            (Compilation Left, ImmutableArray<ITargetClassMeta?> Right) source
        );
    }
}
