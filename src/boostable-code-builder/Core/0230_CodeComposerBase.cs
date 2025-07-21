using Boostable.CodeBuilding.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Provides a base class for composing code using a buffer of entries and supporting thread-safe operations.
    /// </summary>
    /// <remarks>This class serves as the foundation for building code composition functionality. It
    /// manages a thread-safe buffer of <see cref="IComposingEntry"/> objects and provides methods for appending
    /// strings, managing code composition scopes, and handling post-processing of buffered entries. Derived classes can
    /// extend its functionality by overriding the <see cref="OnPost"/> method to customize post-processing
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
        /// Gets the buffer that stores the collection of <see cref="IComposingEntry"/> objects.
        /// We use a <see cref="ConcurrentStack{T}"/> instead of a <see cref="ConcurrentQueue{T}"/> 
        /// because <c>TryPeek</c> should return the last entry added to the buffer (LIFO behavior).
        /// </summary>
        private ConcurrentStack<IComposingEntry> Buffer { get; } = new();

        /// <summary>
        /// Gets or sets the scope used for building code within the current operation.
        /// </summary>
        private IBuildScope? BuildScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the previous operation has terminated the last entry.
        /// </summary>
        private bool PrevHasTerminatedLastEntry { get; set; }

        /// <summary>
        /// Gets or sets the current child composer being used.
        /// </summary>
        private CodeComposerBase? CurrentChildComposer { get; set; }

        /// <summary>
        /// Gets the synchronization lock object used to coordinate access to the current child composer.
        /// </summary>
        private object CurrentChildComposerSyncLock { get; } = new();

        /// <summary>
        /// Gets the remaining depth available for a recursive operation or process.
        /// </summary>
        internal int DepthLeft { get; private set; } = 1;

        /// <summary>
        /// Initializes the composer with the specified build scope and termination state.
        /// </summary>
        /// <param name="builderScope">The build scope to be used for composing code. Cannot be <see langword="null"/>.</param>
        /// <param name="prevHasTerminatedLastEntry">A value indicating whether the previous entry has terminated.  <see langword="true"/> if the last entry was
        /// terminated; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the composer has already been disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builderScope"/> is <see langword="null"/>.</exception>
        internal void Initialize(IBuildScope builderScope, bool prevHasTerminatedLastEntry)
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }
            BuildScope = builderScope ?? throw new ArgumentNullException(nameof(builderScope));
            PrevHasTerminatedLastEntry = prevHasTerminatedLastEntry;
        }

        /// <summary>
        /// Performs cleanup operations when the object is being disposed.
        /// </summary>
        /// <remarks>Override this method in a derived class to release unmanaged resources  or perform
        /// other custom cleanup logic during the disposal process. Ensure that base class implementations are called if
        /// overridden.</remarks>
        public virtual void OnDispose()
        {
            // Default implementation does nothing.
            // Derived classes can override this method to perform custom cleanup.
        }

        /// <summary>
        /// Handles post-processing of buffered entries before they are flushed to the parent composer or output.
        /// </summary>
        /// <remarks>
        /// This method retrieves all entries currently stored in the buffer, and returns them for further processing
        /// by the <see cref="IBuildScope"/>. The default implementation simply returns all entries in LIFO order as-is.
        ///
        /// Override this method in derived classes to:
        /// <list type="bullet">
        /// <item><description>Wrap entries in region blocks or indentation scopes</description></item>
        /// <item><description>Filter out specific entries</description></item>
        /// <item><description>Reorder or sort entries</description></item>
        /// <item><description>Inject additional entries (e.g., headers, footers, markers)</description></item>
        /// </list>
        /// 
        /// This method is called automatically during <see cref="Dispose"/>, and should not be called manually.
        /// </remarks>
        /// <returns>A sequence of <see cref="IComposingEntry"/> objects to be posted to the next composer or output.</returns>
        protected virtual IEnumerable<IComposingEntry> OnPost()
        {
            var entries = Buffer.ToList();
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
        /// Resets the remaining depth for the current operation to the specified value.
        /// </summary>
        /// <remarks>
        /// Use this method to reset the depth counter when additional nested composers are needed.
        /// The <paramref name="depth"/> must be a positive value.
        ///
        /// This method is also invoked internally by <c>CodeBuilder</c> when the composer is registered,
        /// passing in <c>parentComposer.DepthLeft - 1</c> as the new depth.
        /// </remarks>
        /// <param name="depth">The new remaining depth. Must must be positive value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="depth"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the depth limit has already been exceeded.
        /// </exception>
        public void ResetMaxDepth(int depth)
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }

            if (depth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be positive value.");
            }

            // If the depth is zero, the composer is already in an invalid state.
            if (depth == 0)
            {
                throw new CodeComposerDepthExceededException(GetType());
            }

            DepthLeft = depth;
        }

        /// <summary>
        /// Recursively collects composing entries from the current composer and its child composers.
        /// </summary>
        /// <remarks>This method retrieves entries from the current composer and, if a child composer
        /// exists, recursively collects entries from the child composer as well. The method ensures thread safety by
        /// locking on the synchronization object.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IComposingEntry"/> objects representing the collected entries
        /// from the current composer and its child composers.</returns>
        private IEnumerable<IComposingEntry> CollectEntriesRecursively()
        {
            lock (CurrentChildComposerSyncLock)
            {
                // Get the entries from this composer.
                var entries = OnPost();
                // If there is a child composer, get its entries as well.
                if (CurrentChildComposer != null)
                {
                    entries = entries.Concat(CurrentChildComposer.CollectEntriesRecursively());
                }
                return entries;
            }
        }

        /// <summary>
        /// Appends the specified string to the internal buffer, optionally marking it as terminated.
        /// </summary>
        /// <param name="str">The string to append to the buffer. Cannot be <see langword="null"/>.</param>
        /// <param name="shouldTerminate">A value indicating whether the appended string should be marked as terminated.  If <see langword="true"/>,
        /// the string is considered terminated; otherwise, it is not.</param>
        /// <returns>The current instance of <see cref="ICodeComposer"/>, allowing for method chaining.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the method is called on a disposed instance of <see cref="CodeComposerBase"/>.</exception>
        public ICodeComposer Append(string str, bool shouldTerminate = false)
        {
            lock (CurrentChildComposerSyncLock)
            {
                // Check if the composer has been disposed.
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                // Validate the input string.
                if (str == null)
                {
                    throw new ArgumentNullException(nameof(str), "String to append cannot be null.");
                }
                // Check if there is a child composer.
                if (CurrentChildComposer != null)
                {
                    // If there is a child composer, delegate the append operation to it.
                    return CurrentChildComposer.Append(str, shouldTerminate);
                }
                // If there is no child composer, append the string to the buffer.
                Buffer.Push(new ComposingEntry(str, shouldTerminate));
                return this;
            }
        }

        /// <summary>
        /// Appends the specified string followed by a newline to the current composition.
        /// </summary>
        /// <remarks>We specify <see langword="null"/> as the default value for <paramref name="str"/>, so that
        /// users can call <c>AppendLine()</c> without any parameters to append just a newline.</remarks>
        /// <param name="str">The string to append. If <see langword="null"/>, an empty string is appended.</param>
        /// <returns>The current instance of <see cref="ICodeComposer"/>, allowing for method chaining.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the current instance has been disposed.</exception>
        public ICodeComposer AppendLine(string? str = null)
        {
            // Even though Append() also locks CurrentChildComposerSyncLock internally,
            // we explicitly check for disposal here to avoid entering the method if already disposed.
            lock (CurrentChildComposerSyncLock)
            {
                if (_isDisposed != 0)
                {
                    throw new ObjectDisposedException(nameof(CodeComposerBase));
                }
                Append(str ?? string.Empty, true);
                return this;
            }
        }

        /// <summary>
        /// Determines whether the last entry in the buffer has been terminated.
        /// </summary>
        /// <returns><see langword="true"/> if the last entry in the buffer is terminated; otherwise,  the value of the previous
        /// termination state.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        public bool HasTerminatedLastEntry()
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
                    return CurrentChildComposer.HasTerminatedLastEntry();
                }
                // If there is no child composer, check the last entry in the buffer.
                Buffer.TryPeek(out var lastEntry);
                return lastEntry?.IsTerminated ?? PrevHasTerminatedLastEntry;
            }
        }

        /// <summary>
        /// Opens a new instance of the specified code composer type.
        /// </summary>
        /// <remarks>If a child composer is currently active, the open operation is delegated to the child
        /// composer. Otherwise, a new instance of the specified code composer type is created.</remarks>
        /// <typeparam name="TCodeComposer">The type of the code composer to open. Must implement <see cref="ICodeComposer"/> and have a parameterless
        /// constructor.</typeparam>
        /// <returns>An instance of the specified <typeparamref name="TCodeComposer"/> type.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the current code composer has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <c>BuildScope</c> is not initialized.</exception>
        public TCodeComposer Open<TCodeComposer>(int maxStackingDepth = -1)
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
                    return CurrentChildComposer.Open<TCodeComposer>();
                }

                // If there is no child composer, create a new instance of the specified code composer type.
                return BuildScope?.Open<TCodeComposer>(this, maxStackingDepth)
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

            return string.Concat(CollectEntriesRecursively().Select(e => e.Str));
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
            BuildScope?.NotifyDisposed(this);

            // Perform post-processing of buffered entries
            BuildScope?.Post(Buffer.ToArray().Reverse());

            // Call the virtual method for additional cleanup
            OnDispose();
        }
    }
}
