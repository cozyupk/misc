using Boostable.CodeBuilding.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Provides a base class for composing code using a buffer of entries and supporting thread-safe operations.
    /// </summary>
    /// <remarks>This class serves as the foundation for building code composition functionality. It
    /// manages a thread-safe buffer of <see cref="ICodeFragment"/> objects and provides methods for appending
    /// strings, managing code composition scopes, and handling post-processing of buffered entries. Derived classes can
    /// extend its functionality by overriding the <see cref="OnEmitSegment"/> method to customize post-processing
    /// behavior.</remarks>
    public class CodeComposerBase : ICodeComposer
    {
        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        /// <remarks>A value of 0 indicates that the object has not been disposed, while any non-zero
        /// value indicates that it has. This field is used internally to track the disposal state of the
        /// object.</remarks>
        int _isDisposed = 0;

        /// <summary>
        /// Gets a thread-safe stack used to store and manage reusable code fragments.
        /// </summary>
        /// <remarks>This property provides a thread-safe mechanism for managing a pool of reusable code
        /// fragments. It is intended for internal use to optimize performance by reducing object allocations.</remarks>
        private ConcurrentStack<ICodeFragment> SegmentBuffer { get; } = new();

        /// <summary>
        /// Gets or sets the scope used for building code within the current operation.
        /// </summary>
        private IBuildScope? BuildScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the last fragment in the previous segment has terminated.
        /// </summary>
        private bool HasLastFragmentInPrevSegmentTerminated { get; set; }

        /// <summary>
        /// Gets or sets the current child composer being used.
        /// </summary>
        private CodeComposerBase? CurrentChildComposer { get; set; }

        /// <summary>
        /// Gets the synchronization lock object used to coordinate access to the current child composer.
        /// </summary>
        private object CurrentChildComposerSyncLock { get; } = new();

        /// <summary>
        /// Gets the remaining allowed nesting depth for segments in the current context.
        /// </summary>
        internal int SegmentNestingDepthLeft { get; private set; }

        /// <summary>
        /// Attaches the current instance to the specified build scope.
        /// </summary>
        /// <param name="builderScope">The build scope to which the instance will be attached. Cannot be <see langword="null"/>.</param>
        /// <param name="hasLastFragmentInPrevSegmentTerminated">A value indicating whether the last fragment in the previous segment has terminated.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the current instance has been disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builderScope"/> is <see langword="null"/>.</exception>
        internal void AttachToScope(IBuildScope builderScope, bool hasLastFragmentInPrevSegmentTerminated)
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }
            BuildScope = builderScope ?? throw new ArgumentNullException(nameof(builderScope));
            HasLastFragmentInPrevSegmentTerminated = hasLastFragmentInPrevSegmentTerminated;
        }

        /// <summary>
        /// Invoked when the object is being disposed.
        /// </summary>
        /// <remarks>This method provides an opportunity for derived classes to perform custom cleanup
        /// logic  during the disposal process. The default implementation does nothing.</remarks>
        public virtual void OnDisposing()
        {
            // Default implementation does nothing.
            // Derived classes can override this method to perform custom cleanup.
        }

        /// <summary>
        /// Emits the current segment as a collection of code fragments.
        /// </summary>
        /// <remarks>
        /// This method retrieves all entries currently stored in the buffer, and returns them for further processing
        /// by the <see cref="IBuildScope"/>. The default implementation simply returns all entries in LIFO order as-is.
        ///
        /// Override this method in derived classes to:
        /// <list type="bullet">
        /// <item><description>Wrap entries in region blocks or indentation scopes</description></item>
        /// <item><description>Filter or transform specific entries</description></item>
        /// <item><description>Reorder or selectively emit fragments</description></item>
        /// <item><description>Inject additional fragments (e.g., headers, footers, markers)</description></item>
        /// </list>
        /// 
        /// This method is called automatically during <see cref="Dispose"/>, and should not be called manually.
        /// </remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ICodeFragment"/> representing the current segment's contents.</returns>
        protected virtual IEnumerable<ICodeFragment> OnEmitSegment()
        {
            var entries = SegmentBuffer.ToList();
            return entries;
        }

        /// <summary>
        /// Sets the child composer for the current instance.
        /// </summary>
        /// <remarks>This method ensures thread safety by locking access to the child composer during the
        /// operation.</remarks>
        /// <param name="childComposer">The child composer to associate with the current instance. Must be of type <see cref="CodeComposerBase"/>.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the current instance has been disposed.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="childComposer"/> is not of type <see cref="CodeComposerBase"/>.</exception>
        internal void SetChildComposer(ICodeComposer childComposer)
        {
            lock (CurrentChildComposerSyncLock)
            {
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                if (childComposer is not CodeComposerBase cc)
                {
                    throw new ArgumentException("Child composer must be of type CodeComposerBase.", nameof(childComposer));
                }
                CurrentChildComposer = cc;
            }
        }

        /// <summary>
        /// Clears the current child composer, setting it to <see langword="null"/>.
        /// </summary>
        /// <remarks>This method ensures thread safety by locking on the synchronization object <see
        /// cref="CurrentChildComposerSyncLock"/>. If the instance has been disposed, an <see
        /// cref="ObjectDisposedException"/> is thrown.</remarks>
        /// <exception cref="ObjectDisposedException">Thrown if the instance has been disposed.</exception>
        internal void UnsetChildComposer()
        {
            lock (CurrentChildComposerSyncLock)
            {
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                CurrentChildComposer = null;
            }
        }

        /// <summary>
        /// Sets the remaining nesting depth for the current composer, ensuring that the nesting depth does not exceed
        /// the specified maximum.
        /// </summary>
        /// <remarks>If a child composer is active, the method delegates the operation to the child
        /// composer. Otherwise, it updates the nesting depth for the current composer. This method ensures that the
        /// nesting depth remains within valid bounds.</remarks>
        /// <param name="maxNestingDepth">The maximum allowable nesting depth. Must be a non-negative integer.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the composer has been disposed.</exception>
        /// <exception cref="SegmentNestingDepthExceededException">Thrown if <paramref name="maxNestingDepth"/> is less than zero, indicating an invalid nesting depth.</exception>
        public void SetRemainingNestingDepth(int maxNestingDepth)
        {
            lock (CurrentChildComposerSyncLock)
            {
                // Check if the composer has been disposed.
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                // If there is a child composer, delegate the updating to it.
                if (CurrentChildComposer != null)
                {
                    CurrentChildComposer.HasLastFragmentTerminated();
                    return;
                }

                // If the depth is negative, the composer is already in an invalid state.
                if (maxNestingDepth < 0)
                {
                    throw new SegmentNestingDepthExceededException(GetType());
                }

                // Store the new depth value.
                SegmentNestingDepthLeft = maxNestingDepth;

            }
        }

        /// <summary>
        /// Recursively collects code fragments from the current composer and its child composers.
        /// </summary>
        /// <remarks>This method retrieves the code fragments emitted by the current composer and, if a
        /// child composer exists,  recursively collects fragments from the child composer as well. The operation is
        /// thread-safe, as it locks  on the <see cref="CurrentChildComposerSyncLock"/> to ensure consistency during
        /// collection.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ICodeFragment"/> objects representing the collected code
        /// fragments.</returns>
        private IEnumerable<ICodeFragment> CollectFragmentsRecursively()
        {
            lock (CurrentChildComposerSyncLock)
            {
                // Get the entries from this composer.
                var entries = OnEmitSegment();
                // If there is a child composer, get its entries as well.
                if (CurrentChildComposer != null)
                {
                    entries = entries.Concat(CurrentChildComposer.CollectFragmentsRecursively());
                }
                return entries;
            }
        }

        /// <summary>
        /// Appends a code fragment to the current composer.
        /// </summary>
        /// <remarks>If a child composer is active, the append operation is delegated to the child
        /// composer.  Otherwise, the fragment is appended to the internal buffer of the current composer.</remarks>
        /// <param name="payload">The string content of the code fragment to append. Cannot be <see langword="null"/>.</param>
        /// <param name="isTerminated">A value indicating whether the appended fragment is terminated.  If <see langword="true"/>, the fragment is
        /// considered terminated; otherwise, it is not.</param>
        /// <returns>The current <see cref="ICodeComposer"/> instance, allowing for method chaining.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the composer has been disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="payload"/> is <see langword="null"/>.</exception>
        public ICodeComposer AppendFragment(string payload, bool isTerminated = false)
        {
            lock (CurrentChildComposerSyncLock)
            {
                // Check if the composer has been disposed.
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                // Validate the input string.
                if (payload == null)
                {
                    throw new ArgumentNullException(nameof(payload), "String to append cannot be null.");
                }
                // Check if there is a child composer.
                if (CurrentChildComposer != null)
                {
                    // If there is a child composer, delegate the append operation to it.
                    return CurrentChildComposer.AppendFragment(payload, isTerminated);
                }
                // If there is no child composer, append the string to the buffer.
                SegmentBuffer.Push(new CodeFragment(payload, isTerminated));
                return this;
            }
        }

        /// <summary>
        /// Appends a fragment to the current composition, ensuring it is terminated, and returns the current composer
        /// instance.
        /// </summary>
        /// <param name="payload">The string payload to append. If <see langword="null"/>, an empty string is appended.</param>
        /// <returns>The current <see cref="ICodeComposer"/> instance, allowing for method chaining.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the composer has been disposed.</exception>
        public ICodeComposer AppendTerminatedFragment(string? payload = null)
        {
            // Even though Append() also locks CurrentChildComposerSyncLock internally,
            // we explicitly check for disposal here to avoid entering the method if already disposed.
            lock (CurrentChildComposerSyncLock)
            {
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                AppendFragment(payload ?? string.Empty, true);
                return this;
            }
        }

        /// <summary>
        /// Determines whether the last fragment in the current or previous segment has been terminated.
        /// </summary>
        /// <remarks>This method checks if the last fragment in the current segment buffer or the previous
        /// segment has been properly terminated. If a child composer is present, the check is delegated to
        /// it.</remarks>
        /// <returns><see langword="true"/> if the last fragment has been terminated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the composer has been disposed.</exception>
        public bool HasLastFragmentTerminated()
        {
            lock (CurrentChildComposerSyncLock)
            {
                // Check if the composer has been disposed.
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                // If there is a child composer, delegate the check to it.
                if (CurrentChildComposer != null)
                {
                    return CurrentChildComposer.HasLastFragmentTerminated();
                }
                // If there is no child composer, check the last entry in the buffer.
                SegmentBuffer.TryPeek(out var lastEntry);
                return lastEntry?.IsTerminated ?? HasLastFragmentInPrevSegmentTerminated;
            }
        }

        /// <summary>
        /// Opens a new instance of the specified code composer type.
        /// </summary>
        /// <remarks>If a child composer is currently active, the open operation is delegated to the child
        /// composer. Otherwise, a new instance of the specified code composer type is created.</remarks>
        /// <typeparam name="TCodeComposer">The type of the code composer to open. Must implement <see cref="ICodeComposer"/> and have a parameterless
        /// constructor.</typeparam>
        /// <param name="maxNestingDepth">The maximum allowed nesting depth for the segment. If set to -1, the value is automatically decided.</param>
        /// <returns>An instance of the specified <typeparamref name="TCodeComposer"/> type.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the current code composer has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <c>BuildScope</c> is not initialized.</exception>
        public TCodeComposer BeginSegment<TCodeComposer>(int maxNestingDepth = -1)
            where TCodeComposer : class, ICodeComposer, new()
        {
            lock (CurrentChildComposerSyncLock)
            {
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                // If there is a child composer, delegate the open operation to it.
                if (CurrentChildComposer != null)
                {
                    return CurrentChildComposer.BeginSegment<TCodeComposer>();
                }

                // If there is no child composer, create a new instance of the specified code composer type.
                return BuildScope?.BeginSegmentInScope<TCodeComposer>(this, maxNestingDepth)
                    ?? throw new InvalidOperationException("The BuildScope in the CodeComposer is not initialized.");
            }
        }

        /// <summary>
        /// Returns a string representation of the current <see cref="CodeComposer"/> instance.
        /// </summary>
        /// <remarks>If the instance has been disposed, the returned string indicates that the object is
        /// no longer usable. Otherwise, the string is composed of the concatenated string representations of all
        /// collected entries.</remarks>
        /// <returns>A string representing the state of the <see cref="CodeComposer"/> instance.  Returns "[CodeComposer:
        /// Disposed]" if the instance has been disposed; otherwise, a concatenated string of all collected entries.</returns>
        public override string ToString()
        {
            if (_isDisposed != 0)
                return "[CodeComposer: Disposed]";

            lock (CurrentChildComposerSyncLock)
            {
                var sb = new StringBuilder();
                foreach (var f in CollectFragmentsRecursively())
                {
                    if (f.IsTerminated)
                        sb.AppendLine(f.Payload);
                    else
                        sb.Append(f.Payload);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the composer.
        /// </summary>
        /// <remarks>This method ensures that any child composers are disposed first, processes any
        /// buffered entries,  and notifies the associated build scope that the composer is no longer active.  It also
        /// invokes a virtual method to allow derived classes to perform additional cleanup.</remarks>
        public void Dispose()
        {
            // Check if already disposed
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            {
                return; // Already disposed
            }

            lock (CurrentChildComposerSyncLock)
            {
                // If there is a child composer, dispose it first.
                // ChildComposer will invoke UnsetChildComposer() via NotifyDisposed() in the BuildScope.
                // This guarantees that parent-child references are cleaned up safely and consistently.
                CurrentChildComposer?.Dispose();
            }

            // Notify that this composer is no longer active
            BuildScope?.RemoveComposerFromStack(this);

            // Perform post-processing of buffered entries
            BuildScope?.PostbackToPrevComposerOrRootStringBudiler(SegmentBuffer.ToArray().Reverse());

            // Call the virtual method for additional cleanup
            OnDisposing();
        }
    }
}
