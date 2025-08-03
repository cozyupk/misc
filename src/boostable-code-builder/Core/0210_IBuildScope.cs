using Boostable.CodeBuilding.Abstractions;
using System.Collections.Generic;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Defines a scope for managing code composition, including segment creation, fragment processing, and disposal
    /// notifications.
    /// </summary>
    /// <remarks>This interface provides methods for creating new segments within a scope, sending code
    /// fragments to the appropriate composer or root builder, and notifying composers when the scope is disposed. It is
    /// intended for use in scenarios where structured code generation and composition are required.</remarks>
    internal interface IBuildScope
    {
        /// <summary>
        /// Begins a new segment in the current scope with the specified code composer and maximum nesting depth.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to use for the segment. Must implement <see cref="ICodeComposer"/> and have a
        /// parameterless constructor.</typeparam>
        /// <param name="cb">The current code composer instance that defines the context for the new segment. Cannot be <see
        /// langword="null"/>.</param>
        /// <param name="maxNestingDepth">The maximum allowed nesting depth for the segment. Must be a non-negative integer.</param>
        /// <returns>An instance of <typeparamref name="TCodeComposer"/> representing the new segment in the current scope.</returns>
        TCodeComposer BeginSegmentInScope<TCodeComposer>(ICodeComposer cb, int maxNestingDepth)
            where TCodeComposer : class, ICodeComposer, new();

        /// <summary>
        /// Sends the specified code fragments to the previous composer or the root string builder for processing.
        /// </summary>
        /// <remarks>If a previous composer is available, the fragments are passed to it for further
        /// processing. Otherwise, the fragments are sent to the root string builder. Ensure that the collection is not
        /// null and contains valid code fragments to avoid unexpected behavior.</remarks>
        /// <param name="fragments">A collection of code fragments to be processed. Each fragment represents a unit of code to be handled.</param>
        void PostbackToPrevComposerOrRootStringBudiler(IEnumerable<ICodeFragment> fragments);

        /// <summary>
        /// Removes the specified composer from the top of the stack.
        /// </summary>
        /// <remarks>This method enforces a strict stack-based disposal order. If the specified composer
        /// is not the top of the stack, an exception is thrown. After removal, the parent composer (if any) is updated
        /// to unset its child composer.</remarks>
        /// <param name="composer">The composer to remove from the stack. Cannot be <see langword="null"/>.</param>
        void RemoveComposerFromStack(ICodeComposer composer);
    }
}
