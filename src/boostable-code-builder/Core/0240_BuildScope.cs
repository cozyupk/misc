using Boostable.CodeBuilding.Abstractions;
using System;
using System.Collections.Generic;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Represents a scope for building and managing code composers, providing initialization and post-processing
    /// actions.
    /// </summary>
    /// <remarks>The <see cref="BuildScope"/> class is used to manage the lifecycle of code composers during a
    /// build process.  It allows for the initialization of a new composer and the execution of post-processing actions
    /// on a set of  code builder entries. This class ensures that composers are properly initialized and that required
    /// actions  are executed after the build process.</remarks>
    internal class BuildScope : IBuildScope
    {
        /// <summary>
        /// Gets the action to be executed when beginning a segment within the current scope.
        /// </summary>
        private Action<CodeComposerBase, ICodeComposer, int> BeginSegmentInScopeAction { get; }

        /// <summary>
        /// Gets the action used to remove a specified <see cref="ICodeComposer"/> from the stack.
        /// </summary>
        private Action<ICodeComposer> RemoveComposerFromStackAction { get; }

        /// <summary>
        /// Gets the action that processes a collection of <see cref="ICodeFragment"/> instances and posts the result
        /// back to the previous composer or the root string builder.
        /// </summary>
        private Action<IEnumerable<ICodeFragment>> PostbackToPrevComposerOrRootStringBudilerAction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildScope"/> class, which manages the lifecycle of a code
        /// composition scope.
        /// </summary>
        /// <param name="beginSegmentInScopeAction">An action to be executed when a new segment is started within the scope. The action receives the current
        /// <see cref="CodeComposerBase"/>, the active <see cref="ICodeComposer"/>, and an integer representing the
        /// segment index.</param>
        /// <param name="removeComposerFromStackAction">An action to be executed to remove the current composer from the stack when the scope ends. The action
        /// receives the active <see cref="ICodeComposer"/>.</param>
        /// <param name="postbackToPrevComposerOrRootStringBudiler">An action to handle postback operations to the previous composer or the root string builder. The action
        /// receives a collection of <see cref="ICodeFragment"/> objects.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters <paramref name="beginSegmentInScopeAction"/>, <paramref
        /// name="removeComposerFromStackAction"/>, or <paramref name="postbackToPrevComposerOrRootStringBudiler"/> is
        /// <see langword="null"/>.</exception>
        public BuildScope(
            Action<CodeComposerBase, ICodeComposer, int> beginSegmentInScopeAction,
            Action<ICodeComposer> removeComposerFromStackAction,
            Action<IEnumerable<ICodeFragment>> postbackToPrevComposerOrRootStringBudiler
        )
        {
            // Validate parameters and store them.
            BeginSegmentInScopeAction = beginSegmentInScopeAction ?? throw new ArgumentNullException(nameof(beginSegmentInScopeAction));
            RemoveComposerFromStackAction = removeComposerFromStackAction ?? throw new ArgumentNullException(nameof(removeComposerFromStackAction));
            PostbackToPrevComposerOrRootStringBudilerAction = postbackToPrevComposerOrRootStringBudiler ?? throw new ArgumentNullException(nameof(postbackToPrevComposerOrRootStringBudiler));
        }

        /// <summary>
        /// Creates and initializes a new instance of the specified code composer type.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to create. Must be a class that implements <see cref="ICodeComposer"/> and has
        /// a parameterless constructor.</typeparam>
        /// <param name="cb">An existing <see cref="ICodeComposer"/> instance used to initialize the new composer.</param>
        /// <param name="maxStackingDepth">The maximum stacking depth for the new composer. This parameter is optional and the default value is -1.</param>
        /// <returns>A new instance of the specified <typeparamref name="TCodeComposer"/> type, initialized with the provided
        /// composer. This parameter should not be null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="cb"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified <typeparamref name="TCodeComposer"/> type does not implement <see
        /// cref="ICodeComposerInternal"/>.</exception>
        public TCodeComposer BeginSegmentInScope<TCodeComposer>(ICodeComposer cb, int maxStackingDepth = -1)
            where TCodeComposer : class, ICodeComposer, new()
        {
            // Validate the input parameters and create a new composer instance.
            if (cb == null) throw new ArgumentNullException(nameof(cb));
            var newComposer = new TCodeComposer();
            if (newComposer is not CodeComposerBase codeComposerInternal)
            {
                throw new InvalidOperationException(
                    $"The composer type {typeof(TCodeComposer).Name} must implement {nameof(CodeComposerBase)}."
                );
            }

            // Initialize the new composer using the provided action.
            BeginSegmentInScopeAction(codeComposerInternal, cb, maxStackingDepth);

            // return the initialized composer.
            return newComposer;
        }

        /// <summary>
        /// Notifies the specified <see cref="ICodeComposer"/> instance that it has been disposed.
        /// </summary>
        /// <param name="composer">The <see cref="ICodeComposer"/> instance to notify. Cannot be <see langword="null"/>.</param>
        public void RemoveComposerFromStack(ICodeComposer composer)
        {
            // Validate the input parameter.
            if (composer is not CodeComposerBase composerInternal)
            {
                throw new ArgumentNullException(nameof(composer), $"The must implement {nameof(CodeComposerBase)}.");
            }

            // Notify the composer that it has been disposed.
            RemoveComposerFromStackAction(composerInternal);
        }

        /// <summary>
        /// Posts a collection of code builder entries to the specified code composer.
        /// </summary>
        /// <remarks>This method delegates the post operation to an internal action. Ensure that the
        /// <paramref name="entries"/> collection is not null and contains valid entries to avoid runtime
        /// exceptions.</remarks>
        /// <param name="from">The code composer that initiates the post operation.</param>
        /// <param name="entries">A collection of code builder entries to be posted. Cannot be null.</param>
        public void PostbackToPrevComposerOrRootStringBudiler(IEnumerable<ICodeFragment> entries)
        {
            // Delegate the post operation to the internal action.
            PostbackToPrevComposerOrRootStringBudilerAction(entries);
        }
    }
}
