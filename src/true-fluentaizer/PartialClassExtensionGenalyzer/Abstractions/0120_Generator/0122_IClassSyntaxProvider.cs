using Microsoft.CodeAnalysis;
using System;

namespace PartialClassExtGen.Abstractions.Generator
{
    public interface IClassSyntaxProvider
    {
        Type TargetAttribute { get; }

        /// <summary>
        /// Retrieves the target metadata for an extension based on the provided syntax context.
        /// </summary>
        /// <param name="context">The <see cref="GeneratorSyntaxContext"/> that provides the context for analyzing the syntax node. This
        /// parameter cannot be null.</param>
        /// <returns>An <see cref="ITargetClassMeta"/> instance representing the metadata of the extension target, or <see
        /// langword="null"/> if no valid target is found.</returns>
        ITargetClassMeta? GetExtensionTarget(
            GeneratorAttributeSyntaxContext context
        );
    }
}
