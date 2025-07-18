using Microsoft.CodeAnalysis;
using System;

namespace PartialClassExtGen.Abstractions.Generator
{
    public interface IClassSyntaxProvider<out TTargetClassMeta>
        where TTargetClassMeta : ITargetClassMeta
    {
        Type TargetAttribute { get; }

        /// <summary>
        /// Retrieves the target class metadata for the specified generator attribute context.
        /// </summary>
        /// <remarks>This method is typically used in source generators to extract metadata about a class
        /// or type associated with a specific attribute. Ensure that the context provided is correctly configured to
        /// avoid unexpected results.</remarks>
        /// <param name="context">The <see cref="GeneratorAttributeSyntaxContext"/> that provides information about the attribute and the
        /// syntax node it is applied to.</param>
        /// <returns>An instance of <typeparamref name="TTargetClassMeta"/> representing the metadata of the target class if the
        /// context is valid; otherwise, <see langword="null"/>.</returns>
        TTargetClassMeta? GetExtensionTarget(
            GeneratorAttributeSyntaxContext context
        );
    }
}
