using Boostable.CodeBuilding.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Provides functionality for managing and composing code generation workflows using a stack-based approach. This
    /// class is designed to work with implementations of <see cref="ICodeComposer"/> to facilitate structured code
    /// generation.
    /// </summary>
    /// <remarks>The <see cref="CodeBuidlerWithoutDefaultComposer"/> class is a sealed utility that manages a stack of code
    /// composers, ensuring proper initialization and disposal of composers in a thread-safe manner. It enforces strict
    /// usage patterns to prevent misuse, such as attempting to use a composer that is not the last in the stack. This
    /// class is intended for advanced scenarios where structured code generation is required.  To use this class, call
    /// the <see cref="Open{TCodeComposer}(StringBuilder)"/> method to create and initialize a new code composer of the
    /// specified type. The caller is responsible for ensuring proper disposal of composers, typically using a `using`
    /// statement.</remarks>
    public class CodeBuidlerWithoutDefaultComposer
    {
        /// <summary>
        /// Gets the root string builder used for composing code.
        /// </summary>
        private StringBuilder? RootStringBuilder { get; }

        /// <summary>
        /// Gets the instance of the build scope used for managing the lifetime and resolution of dependencies.
        /// </summary>
        private IBuildScope BuildScopeInstance { get; }

        /// <summary>
        /// Gets the stack of <see cref="ICodeComposer"/> instances used for managing code composition operations.
        /// </summary>
        private ConcurrentStack<ICodeComposer> ComposersStack { get; } = new();

        /// <summary>
        /// Gets the synchronization lock object used to coordinate access to shared resources.
        /// </summary>
        private object SyncLock { get; } = new();

        /// <summary>
        /// Gets the initial maximum stacking depth for the operation.
        /// </summary>
        private int InitialMaxStackingDepth { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBuidlerWithoutDefaultComposer"/> class with the specified
        /// <see cref="StringBuilder"/> and initial maximum stacking depth.
        /// </summary>
        /// <remarks>This constructor sets up the internal state of the composer, including initializing
        /// the root string builder and configuring the build scope. The <paramref name="initialMaxStaeckingDepth"/>
        /// determines the maximum depth of nested operations that can be performed by the composer.</remarks>
        /// <param name="sb">An optional <see cref="StringBuilder"/> instance to be used as the root string builder. If null, a new <see
        /// cref="StringBuilder"/> will be created internally.</param>
        /// <param name="initialMaxStaeckingDepth">The initial maximum stacking depth for the composer. Must be a non-negative integer. The default value is 16
        /// (0x10).</param>
        protected CodeBuidlerWithoutDefaultComposer(StringBuilder? sb, int initialMaxStaeckingDepth = 0x10)
        {
            RootStringBuilder = sb;
            InitialMaxStackingDepth = initialMaxStaeckingDepth;
            BuildScopeInstance = new BuildScope(
                (newCcp, ccp, maxStackingDepth) => InitializeComposer(newCcp, ccp, maxStackingDepth),
                (target) => OnComposerDisposed(target),
                (entries) => AppendEntriesToCurrentComposerOrRoot(entries)
            );
        }

        /// <summary>
        /// Creates and initializes an instance of the specified code composer type.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to create. Must be a class that implements <see cref="ICodeComposer"/> and
        /// derives from <see cref="CodeComposerBase"/>.</typeparam>
        /// <param name="sb">An optional <see cref="StringBuilder"/> instance to be used by the code builder. If null, a new <see
        /// cref="StringBuilder"/> will be created internally.</param>
        /// <param name="maxStackingDepth">The maximum allowed depth for nested operations. Defaults to 16.</param>
        /// <returns>An instance of the specified <typeparamref name="TCodeComposer"/> type, fully initialized and ready for use.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <typeparamref name="TCodeComposer"/> type does not derive from <see
        /// cref="CodeComposerBase"/>.</exception>
        public static TCodeComposer Open<TCodeComposer>(StringBuilder? sb = null, int maxStackingDepth = 0x10)
            where TCodeComposer : class, ICodeComposer, new()
        {
            var codeBuilder = new CodeBuidlerWithoutDefaultComposer(sb, maxStackingDepth);

            var codeComposer = new TCodeComposer();
            if (codeComposer is not CodeComposerBase codeComposerInternal)
            {
                throw new InvalidOperationException(
                    $"The composer type {typeof(TCodeComposer).Name} must implement {nameof(CodeComposerBase)}."
                );
            }
            codeBuilder.InitializeComposer(codeComposerInternal, default, maxStackingDepth);

            return codeComposer;
        }

        /// <summary>
        /// Initializes a new code composer and adds it to the stack of active composers.
        /// </summary>
        /// <remarks>This method ensures that the new composer is properly initialized and added to the
        /// stack of active composers. It enforces that the provided <paramref name="ccp"/> matches the last composer in
        /// the stack to maintain the integrity of the composer stack.</remarks>
        /// <param name="newCcp">The new code composer to initialize and add to the stack.</param>
        /// <param name="ccp">The current code composer that is expected to be the last in the stack.  If this is not the last composer,
        /// an <see cref="InvalidOperationException"/> is thrown.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="ccp"/> is not the last composer in the stack, indicating that a previous composer
        /// may not have been properly disposed or closed.</exception>
        private void InitializeComposer(CodeComposerBase newCcp, ICodeComposer? ccp, int maxStackingDepth)
        {
            lock (SyncLock)
            {
                // TryPeek allowing from the stack to be empty.
                ComposersStack.TryPeek(out ICodeComposer? lastComposer);
                if (lastComposer != ccp)
                {
                    throw new InvalidOperationException(
                        "Cannot open composer: this CodeComposer is not the last in the stack. " +
                        "Did you forget to use 'using' or dispose a previous composer? " +
                        $"(Last composer type: {(lastComposer?.GetType().Name ?? "none")})"
                    );
                }

                // If maxStackingDepth is negative, we inherit the depth from the parent composer.
                if (maxStackingDepth < 0)
                {
                    maxStackingDepth = InitialMaxStackingDepth - ComposersStack.Count();
                }

                // Initialize maxStackingDepth.
                newCcp.ResetMaxDepth(maxStackingDepth);

                // Push the new composer onto the stack.
                ComposersStack.Push(newCcp);

                // We can't check "HasTerminatedLastEntry" if the previous composer is null,
                // which in practice means it was just a StringBuilder.
                // So in that case, we assume the last entry has been terminated.
                newCcp.Initialize(BuildScopeInstance, ccp?.HasTerminatedLastEntry() ?? true);
            }
        }

        /// <summary>
        /// Handles the disposal of a code composer, ensuring it is the last composer in the stack.
        /// </summary>
        /// <remarks>This method enforces a strict disposal order for composers. If the specified composer
        /// is not at the top of the stack, an exception is thrown. Additionally, if a parent composer exists in the stack,
        /// it is updated by unsetting its child link after the target composer is removed.</remarks>
        /// <param name="targetComposer">The composer instance being disposed.</param>
        /// <exception cref="InvalidOperationException">Thrown if the specified composer is not the last composer in the stack, indicating that composers must be
        /// disposed in the correct order.</exception>
        private void OnComposerDisposed(ICodeComposer targetComposer)
        {
            lock (SyncLock)
            {
                // Peek at the top of the stack
                if (!ComposersStack.TryPeek(out var top) || top != targetComposer)
                {
                    throw new InvalidOperationException(
                        "Cannot dispose: the specified composer is not the last in the stack. " +
                        "Ensure composers are disposed in the correct order." +
                        $" (Target composer type: {targetComposer.GetType().Name}, but top of stack is: {top?.GetType().Name ?? "none"})"
                    );
                }

                // Pop the composer
                ComposersStack.TryPop(out _);

                // Set parent (if any)
                var parent = ComposersStack.TryPeek(out var prev) ? prev : null;
                if (parent is CodeComposerBase baseComposer)
                {
                    baseComposer.UnsetChildComposer();
                }
            }
        }

        /// <summary>
        /// Appends the specified entries to the current composer at the top of the stack,  or to the root string
        /// builder if no composer is available.
        /// </summary>
        /// <remarks>If the stack of composers is empty, the entries are appended directly to the  root
        /// string builder. This method is thread-safe and ensures that only one thread  can modify the composers or
        /// root string builder at a time.</remarks>
        /// <param name="entries">A collection of <see cref="IComposingEntry"/> objects to append. Each entry's  string value is appended
        /// either as a line or inline, depending on whether the  entry is marked as terminated. Entries with null or
        /// empty string values are ignored.</param>
        private void AppendEntriesToCurrentComposerOrRoot(IEnumerable<IComposingEntry> entries)
        {
            // If there are no entries, we do nothing
            if (entries == null || !entries.Any()) return;

            ICodeComposer? parent = default;
            lock (SyncLock)
            {
                // Peek at the top of the stack allowing it to be empty.
                ComposersStack.TryPeek(out parent);

                // Forward entries to the parent composer or root StringBuilder
                var append = new AppendDelegates(RootStringBuilder, parent);
                foreach (var entry in entries)
                {
                    if (string.IsNullOrEmpty(entry.Str)) continue;
                    (entry.IsTerminated ? append.AppendLine : append.Append)(entry.Str);
                }
            }
        }
    }
}
