using System;
using System.Collections.Generic;

namespace Boostable.CodeBuilding.Abstractions
{
    /// <summary>
    /// Represents an entry in a string builder-like structure, containing a string value and a termination state.
    /// </summary>
    /// <remarks>This interface is designed to encapsulate a string value along with a flag indicating whether
    /// the entry is terminated. It can be used in scenarios where managing a sequence of string entries with
    /// termination states is required.</remarks>
    public interface ICodeBuilderEntry
    {
        /// <summary>
        /// Gets the string value associated with this instance.
        /// </summary>
        string Str { get; }

        /// <summary>
        /// Gets a value indicating whether the process or operation has been terminated.
        /// </summary>
        bool IsTerminated { get; }
    }

    /// <summary>
    /// Defines a scope for managing code composition operations, including posting builder entries and opening new code
    /// composer instances.
    /// </summary>
    /// <remarks>The <see cref="IBuildScope"/> interface provides methods for interacting with code composers
    /// and managing the lifecycle of code composition operations. It allows posting collections of builder entries to a
    /// composer and creating new instances of specific composer types.</remarks>
    public interface IBuildScope
    {
        /// <summary>
        /// Posts a collection of code builder entries to the specified code composer.
        /// </summary>
        /// <param name="from">The code composer responsible for initiating the post operation. Cannot be <see langword="null"/>.</param>
        /// <param name="entries">A collection of code builder entries to be posted. Cannot be <see langword="null"/> or empty.</param>
        void Post(ICodeComposer from, IEnumerable<ICodeBuilderEntry> entries);

        /// <summary>
        /// Opens a new instance of the specified code composer type and initializes it using the provided composer.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to open. Must be a class that implements <see cref="ICodeComposer"/> and has a
        /// parameterless constructor.</typeparam>
        /// <param name="cb">The composer instance used to initialize the new code composer. Cannot be <see langword="null"/>.</param>
        /// <returns>A new instance of <typeparamref name="TCodeComposer"/> initialized with the provided composer.</returns>
        TCodeComposer Open<TCodeComposer>(ICodeComposer cb)
            where TCodeComposer : class, ICodeComposer, new();
    }

    /// <summary>
    /// Defines a contract for composing and managing code or text content, with support for appending,  formatting, and
    /// managing termination states of compositions.
    /// </summary>
    /// <remarks>This interface provides methods for appending strings, managing termination states, and
    /// creating  new instances of code composers. It is designed to support fluent method chaining and ensures  proper
    /// resource management through the <see cref="IDisposable"/> interface.</remarks>
    public interface ICodeComposer : IDisposable
    {
        /// <summary>
        /// Opens a new instance of the specified code composer type and initializes it using the provided composer.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to open. Must be a class that implements <see cref="ICodeComposer"/> and has a
        /// parameterless constructor.</typeparam>
        /// <param name="cb">An instance of <see cref="ICodeComposer"/> used to initialize the new code composer.</param>
        /// <returns>A new instance of <typeparamref name="TCodeComposer"/> initialized with the provided composer.</returns>
        TCodeComposer Open<TCodeComposer>(ICodeComposer cb)
            where TCodeComposer : class, ICodeComposer, new();

        /// <summary>
        /// Appends the specified string to the current composition and optionally terminates the composition.
        /// </summary>
        /// <param name="str">The string to append. Cannot be null.</param>
        /// <param name="shouldTerminate">A value indicating whether the composition should be terminated after appending the string.  <see
        /// langword="true"/> to terminate; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="ICodeComposer"/> instance representing the updated composition.</returns>
        ICodeComposer Append(string str, bool shouldTerminate = false);

        /// <summary>
        /// Appends the specified string followed by a newline character to the current composition.
        /// </summary>
        /// <param name="str">The string to append. If <see langword="null"/> or omitted, only a newline character is appended.</param>
        /// <returns>The current instance of <see cref="ICodeComposer"/>, allowing for method chaining.</returns>
        ICodeComposer AppendLine(string? str = null);

        /// <summary>
        /// Determines whether the last entry in the log has been terminated.
        /// </summary>
        /// <remarks>This method checks the termination status of the most recent log entry.  Use this to
        /// verify if the last entry has been properly closed or finalized.</remarks>
        /// <returns><see langword="true"/> if the last entry in the log has been terminated; otherwise, <see langword="false"/>.</returns>
        bool HasTerminatedLastEntry();
    }

    /// <summary>
    /// Provides internal methods for initializing and managing the code composition process within a specific build
    /// scope.
    /// </summary>
    /// <remarks>This interface extends <see cref="ICodeComposer"/> with additional methods intended for
    /// internal use.  It is designed to support advanced scenarios such as incremental composition by leveraging a
    /// previous code composer instance.</remarks>
    internal interface ICodeComposerInternal : ICodeComposer
    {
        /// <summary>
        /// Initializes the build process with the specified scope and termination state.
        /// </summary>
        /// <param name="buildScope">The scope of the build process, which defines the context and boundaries for the operation.</param>
        /// <param name="prevHasTerminatedLastEntry">A value indicating whether the previous process terminated the last entry.  If <see langword="true"/>, the
        /// initialization will account for the termination state; otherwise, it will proceed as if no termination
        /// occurred.</param>
        void Initialize(IBuildScope buildScope, bool prevHasTerminatedLastEntry);
    }
}

namespace Boostable.CodeBuilding.Abstractions.Core
{
    using Boostable.CodeBuilding.Abstractions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Provides functionality for managing and composing code generation workflows using a stack-based approach. This
    /// class is designed to work with implementations of <see cref="ICodeComposer"/> to facilitate structured code
    /// generation.
    /// </summary>
    /// <remarks>The <see cref="CodeBuilder"/> class is a sealed utility that manages a stack of code
    /// composers, ensuring proper initialization and disposal of composers in a thread-safe manner. It enforces strict
    /// usage patterns to prevent misuse, such as attempting to use a composer that is not the last in the stack. This
    /// class is intended for advanced scenarios where structured code generation is required.  To use this class, call
    /// the <see cref="Open{TCodeComposer}(StringBuilder)"/> method to create and initialize a new code composer of the
    /// specified type. The caller is responsible for ensuring proper disposal of composers, typically using a `using`
    /// statement.</remarks>
    public sealed class CodeBuilder
    {
        /// <summary>
        /// Gets the root string builder used for composing code.
        /// </summary>
        private StringBuilder RootStringBuilder { get; }

        /// <summary>
        /// Gets the instance of the build scope used for managing the lifetime and resolution of dependencies.
        /// </summary>
        private IBuildScope BuildScopeInstance { get; }

        /// <summary>
        /// Gets the collection of code composers used to generate or manipulate code.
        /// </summary>
        /// <remarks>This property provides access to the internal code composers. It is read-only and
        /// cannot be modified directly.</remarks>
        private List<ICodeComposer> CodeComposers { get; } = new();

        /// <summary>
        /// Gets the synchronization lock object used to coordinate access to shared resources.
        /// </summary>
        private object SyncLock { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBuilder"/> class with the specified <see
        /// cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> instance used as the root for building code. Cannot be <see
        /// langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sb"/> is <see langword="null"/>.</exception>
        private CodeBuilder(StringBuilder sb)
        {
            RootStringBuilder = sb ?? throw new ArgumentNullException(nameof(sb));
            BuildScopeInstance = new BuildScope(
                (newCcp, ccp) => InitializeComposer(newCcp, ccp),
                (from, entries) => PostInternal(from, entries)
            );
        }

        /// <summary>
        /// Creates and initializes an instance of the specified code composer type using the provided <see
        /// cref="StringBuilder"/>.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to create. Must be a class that implements <see cref="ICodeComposer"/> and has
        /// a parameterless constructor.</typeparam>
        /// <param name="sb">The <see cref="StringBuilder"/> used to initialize the code composer.</param>
        /// <returns>An instance of the specified <typeparamref name="TCodeComposer"/> type, initialized with the provided <see
        /// cref="StringBuilder"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <typeparamref name="TCodeComposer"/> type does not implement <see
        /// cref="ICodeComposerInternal"/>.</exception>
        public static TCodeComposer Open<TCodeComposer>(StringBuilder sb)
            where TCodeComposer : class, ICodeComposer, new()
        {
            var codeBuilder = new CodeBuilder(sb);

            var codeComposer = new TCodeComposer();
            if (codeComposer is not ICodeComposerInternal codeComposerInternal)
            {
                throw new InvalidOperationException(
                    $"The composer type {typeof(TCodeComposer).Name} must implement {nameof(ICodeComposerInternal)}."
                );
            }
            codeBuilder.InitializeComposer(codeComposerInternal, default);

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
        private void InitializeComposer(ICodeComposerInternal newCcp, ICodeComposer? ccp)
        {
            lock (SyncLock)
            {
                var lastComposer = CodeComposers.LastOrDefault();
                if (lastComposer != ccp)
                {
                    throw new InvalidOperationException(
                        "Cannot open composer: this CodeComposer is not the last in the stack. " +
                        "Did you forget to use 'using' or dispose a previous composer? " +
                        $"(Last composer type: {(lastComposer?.GetType().Name ?? "none")})"
                    );
                }
                CodeComposers.Add(newCcp);
                // We can't check "HasTerminatedLastEntry" if the previous composer is null,
                // which in practice means it was just a StringBuilder.
                // So in that case, we assume the last entry has been terminated.
                newCcp.Initialize(BuildScopeInstance, ccp?.HasTerminatedLastEntry() ?? true);
            }
        }

        /// <summary>
        /// Posts a collection of code builder entries to the current code composition stack.
        /// </summary>
        /// <remarks>This method processes each entry in the provided collection and appends its content
        /// to the  output, either as a single line or without a line break, depending on the entry's termination state.
        /// Entries with an empty or <see langword="null"/> string are ignored.</remarks>
        /// <param name="from">The <see cref="ICodeComposer"/> instance that is posting the entries. This must be the last composer in the
        /// stack.</param>
        /// <param name="entries">A collection of <see cref="ICodeBuilderEntry"/> objects to be processed and appended to the code composition
        /// output.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="from"/> or <paramref name="entries"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="from"/> composer is not the last composer in the stack.  Ensure that the
        /// composer is properly disposed or used within a <c>using</c> block.</exception>
        private void PostInternal(ICodeComposer from, IEnumerable<ICodeBuilderEntry> entries)
        {
            // Validate parameters.
            _ = from ?? throw new ArgumentNullException(nameof(from));
            _ = entries ?? throw new ArgumentNullException(nameof(entries));

            lock (SyncLock)
            {
                // check if the provided composer is the last in the stack.
                var lastComposer = CodeComposers.LastOrDefault();
                if (lastComposer != from)
                {
                    throw new InvalidOperationException(
                        "Cannot post entries: this CodeComposer is not the last in the stack. " +
                        "Did you forget to use 'using' or dispose a previous composer? " +
                        $"(Last composer type: {(lastComposer?.GetType().Name ?? "none")})"
                    );
                }

                // Remove the current composer from the stack.
                CodeComposers.RemoveAt(CodeComposers.Count - 1);

                // Create AppendFuncs for the prev composer or the root string builder.
                var prevCodeComposer = CodeComposers.LastOrDefault();
                var appendActions = new AppendDelegates (RootStringBuilder, prevCodeComposer);

                // Process each entry in the provided collection.                
                foreach (var entry in entries)
                {
                    if (string.IsNullOrEmpty(entry.Str)) continue;
                    (entry.IsTerminated ? appendActions.AppendLine : appendActions.Append)(entry.Str);
                }
            }
        }
    }

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
        /// Gets the action used to initialize the internal and external code composers.
        /// </summary>
        private Action<ICodeComposerInternal, ICodeComposer> InitializeComposerAction { get; }

        /// <summary>
        /// Gets the action to be executed after code composition is completed.
        /// </summary>
        public Action<ICodeComposer, IEnumerable<ICodeBuilderEntry>> PostAction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildScope"/> class with the specified actions for
        /// initialization and post-processing.
        /// </summary>
        /// <param name="initializeComponentAction">An action that initializes the component. The first parameter is an internal composer, and the second
        /// parameter is the public composer. This action cannot be <see langword="null"/>.</param>
        /// <param name="postAction">An action that performs post-processing after the build operation. The first parameter is the public
        /// composer, and the second parameter is a collection of code builder entries. This action cannot be <see
        /// langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="initializeComponentAction"/> or <paramref name="postAction"/> is <see
        /// langword="null"/>.</exception>
        public BuildScope(
            Action<ICodeComposerInternal, ICodeComposer> initializeComponentAction,
            Action<ICodeComposer, IEnumerable<ICodeBuilderEntry>> postAction
        )
        {
            // Validate parameters and store them.
            PostAction = postAction ?? throw new ArgumentNullException(nameof(postAction));
            InitializeComposerAction = initializeComponentAction ?? throw new ArgumentNullException(nameof(initializeComponentAction));
        }

        /// <summary>
        /// Creates and initializes a new instance of the specified code composer type.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to create. Must be a class that implements <see cref="ICodeComposer"/> and has
        /// a parameterless constructor.</typeparam>
        /// <param name="cb">An existing <see cref="ICodeComposer"/> instance used to initialize the new composer.</param>
        /// <returns>A new instance of the specified <typeparamref name="TCodeComposer"/> type, initialized with the provided
        /// composer. This parameter should not be null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="cb"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified <typeparamref name="TCodeComposer"/> type does not implement <see
        /// cref="ICodeComposerInternal"/>.</exception>
        public TCodeComposer Open<TCodeComposer>(ICodeComposer cb)
            where TCodeComposer : class, ICodeComposer, new()
        {
            if (cb == null) throw new ArgumentNullException(nameof(cb));
            var newComposer = new TCodeComposer();
            if (newComposer is not ICodeComposerInternal codeComposerInternal)
            {
                throw new InvalidOperationException(
                    $"The composer type {typeof(TCodeComposer).Name} must implement {nameof(ICodeComposerInternal)}."
                );
            }
            InitializeComposerAction(codeComposerInternal, cb);
            return newComposer;
        }

        /// <summary>
        /// Posts a collection of code builder entries to the specified code composer.
        /// </summary>
        /// <remarks>This method delegates the post operation to an internal action. Ensure that the
        /// <paramref name="entries"/> collection is not null and contains valid entries to avoid runtime
        /// exceptions.</remarks>
        /// <param name="from">The code composer that initiates the post operation.</param>
        /// <param name="entries">A collection of code builder entries to be posted. Cannot be null.</param>
        public void Post(ICodeComposer from, IEnumerable<ICodeBuilderEntry> entries)
        {
            PostAction(from, entries);
        }
    }

    /// <summary>
    /// Provides functionality for appending text and lines of text to an output destination.
    /// </summary>
    /// <remarks>This class encapsulates two actions, <see cref="Append"/> and <see cref="AppendLine"/>, which
    /// can be used to append text to a specified <see cref="StringBuilder"/> or delegate the operations to a provided
    /// <see cref="ICodeComposer"/>. The behavior of these actions depends on the constructor parameters.</remarks>
    internal record AppendDelegates 
    {
        /// <summary>
        /// Gets the action used to append a string to the underlying output.
        /// </summary>
        /// <remarks>The provided action is typically used to handle string output in a custom manner,
        /// such as appending to a log,  a file, or another output stream. Ensure the action is not null before invoking
        /// it.</remarks>
        public Action<string> Append { get; }

        /// <summary>
        /// Gets the action used to append a line of text.
        /// </summary>
        public Action<string> AppendLine { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppendDelegates "/> class, configuring append and append-line
        /// operations based on the provided <see cref="StringBuilder"/> or a previous <see cref="ICodeComposer"/>.
        /// </summary>
        /// <remarks>This constructor sets up two delegates, <c>Append</c> and <c>AppendLine</c>, which
        /// allow appending text either to the provided <paramref name="defaultStringBuilder"/> or to the <paramref
        /// name="prevCodeComposer"/>, depending on whether the latter is null. If <paramref name="prevCodeComposer"/>
        /// is not null, its methods take precedence.</remarks>
        /// <param name="defaultStringBuilder">The <see cref="StringBuilder"/> instance to use for appending text if no previous <see
        /// cref="ICodeComposer"/> is provided.</param>
        /// <param name="prevCodeComposer">An optional <see cref="ICodeComposer"/> instance. If provided, its append and append-line methods will be
        /// used instead of the <paramref name="defaultStringBuilder"/>.</param>
        public AppendDelegates (StringBuilder defaultStringBuilder, ICodeComposer? prevCodeComposer)
        {
            // Validate the defaultStringBuilder parameter.
            if (defaultStringBuilder == null) throw new ArgumentNullException(nameof(defaultStringBuilder));

            // Initialize Append and AppendLine actions based on whether a previous composer is provided.
            Append = prevCodeComposer != null
                ? s => prevCodeComposer.Append(s)
                : s => defaultStringBuilder.Append(s);
            AppendLine = prevCodeComposer != null
                ? s => prevCodeComposer.AppendLine(s)
                : s => defaultStringBuilder.AppendLine(s);
        }
    }

    /// <summary>
    /// Represents an entry in a code builder, containing a string value and a termination status.
    /// </summary>
    /// <remarks>This struct is used to encapsulate a string value and a flag indicating whether the
    /// entry is marked as terminated. It is immutable and designed for use in scenarios where lightweight,
    /// read-only data structures are required.</remarks>
    public readonly struct CodeBuilderEntry : ICodeBuilderEntry
    {
        /// <summary>
        /// Gets the string value associated with this instance.
        /// </summary>
        public string Str { get; }

        /// <summary>
        /// Gets a value indicating whether the process has been terminated.
        /// </summary>
        public bool IsTerminated { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBuilderEntry"/> class with the specified string value and
        /// termination status.
        /// </summary>
        /// <param name="str">The string value associated with this entry. Cannot be <see langword="null"/>. If empty, <paramref
        /// name="isTerminated"/> must be <see langword="true"/>.</param>
        /// <param name="isTerminated">A value indicating whether the entry is considered terminated. If <see langword="false"/>, <paramref
        /// name="str"/> cannot be an empty string.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="str"/> is an empty string and <paramref name="isTerminated"/> is <see
        /// langword="false"/>.</exception>
        public CodeBuilderEntry(string str, bool isTerminated)
        {
            // Validate null
            if (str == null) throw new ArgumentNullException(nameof(str));

            // We do not allow empty strings unless isTerminated is true,
            // in order to simplify the implementation of the CodeComposerBase.HasTerminatedLastEntry method.
            if (str == string.Empty && isTerminated == false)
            {
                throw new ArgumentException("Cannot create a CodeBuilderEntry with an empty string and isTerminated set to false.", nameof(str));
            }
            Str = str;
            IsTerminated = isTerminated;
        }
    }

    /// <summary>
    /// Provides a base class for composing code using a buffer of entries and supporting thread-safe operations.
    /// </summary>
    /// <remarks>This abstract class serves as the foundation for building code composition functionality. It
    /// manages a thread-safe buffer of <see cref="ICodeBuilderEntry"/> objects and provides methods for appending
    /// strings, managing code composition scopes, and handling post-processing of buffered entries. Derived classes can
    /// extend its functionality by overriding the <see cref="OnPost"/> method to customize post-processing
    /// behavior.</remarks>
    public abstract class CodeComposerBase : ICodeComposerInternal
    {
        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        /// <remarks>A value of 0 indicates that the object has not been disposed, while any non-zero
        /// value indicates that it has. This field is used internally to track the disposal state of the
        /// object.</remarks>
        int _isDisposed = 0;

        /// <summary>
        /// Gets the buffer that stores the collection of <see cref="ICodeBuilderEntry"/> objects.
        /// We use a <see cref="ConcurrentStack{T}"/> instead of a <see cref="ConcurrentQueue{T}"/> 
        /// because <c>TryPeek</c> should return the last entry added to the buffer (LIFO behavior).
        /// </summary>
        protected ConcurrentStack<ICodeBuilderEntry> Buffer { get; } = new();

        /// <summary>
        /// Gets or sets the scope used for building code within the current operation.
        /// </summary>
        private IBuildScope? BuildScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the previous operation has terminated the last entry.
        /// </summary>
        private bool PrevHasTerminatedLastEntry { get; set; }

        /// <summary>
        /// Initializes the composer with the specified build scope and termination state.
        /// </summary>
        /// <param name="builderScope">The build scope to be used for composing code. Cannot be <see langword="null"/>.</param>
        /// <param name="prevHasTerminatedLastEntry">A value indicating whether the previous entry has terminated.  <see langword="true"/> if the last entry was
        /// terminated; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the composer has already been disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builderScope"/> is <see langword="null"/>.</exception>
        public void Initialize(IBuildScope builderScope, bool prevHasTerminatedLastEntry)
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }
            BuildScope = builderScope ?? throw new ArgumentNullException(nameof(builderScope));
            PrevHasTerminatedLastEntry = prevHasTerminatedLastEntry;
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
        /// <item><description>Transform entries (e.g., wrap in region blocks, apply indentation)</description></item>
        /// <item><description>Filter out specific entries</description></item>
        /// <item><description>Sort or reorder entries</description></item>
        /// <item><description>Inject additional entries (e.g., headers, footers, markers)</description></item>
        /// </list>
        /// 
        /// This method is called automatically during <see cref="Dispose"/>, and should not be called manually.
        /// </remarks>
        /// <returns>A sequence of <see cref="ICodeBuilderEntry"/> objects to be posted to the next composer or output.</returns>
        protected virtual IEnumerable<ICodeBuilderEntry> OnPost()
        {
            var entries = Buffer.ToList();
            return entries;
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
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }
            Buffer.Push(new CodeBuilderEntry(str, shouldTerminate));
            return this;
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
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }
            Append(str ?? string.Empty, true);
            return this;
        }

        /// <summary>
        /// Determines whether the last entry in the buffer has been terminated.
        /// </summary>
        /// <returns><see langword="true"/> if the last entry in the buffer is terminated; otherwise,  the value of the previous
        /// termination state.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        public bool HasTerminatedLastEntry()
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }
            Buffer.TryPeek(out var lastEntry);
            return lastEntry?.IsTerminated ?? PrevHasTerminatedLastEntry;
        }

        /// <summary>
        /// Opens a new instance of the specified <typeparamref name="TCodeComposer"/> type within the current build
        /// scope.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to open. Must implement <see cref="ICodeComposer"/> and have a parameterless
        /// constructor.</typeparam>
        /// <param name="currentCodeComposer">The current code composer instance to associate with the new instance.</param>
        /// <returns>A new instance of <typeparamref name="TCodeComposer"/> initialized within the current build scope.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the current code composer has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the build scope is not initialized.</exception>
        public TCodeComposer Open<TCodeComposer>(ICodeComposer currentCodeComposer)
            where TCodeComposer : class, ICodeComposer, new()
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CodeComposerBase));
            }
            return BuildScope?.Open<TCodeComposer>(currentCodeComposer)
                ?? throw new InvalidOperationException("The BuildScope in the CodeComposer is not initialized.");
        }

        /// <summary>
        /// Releases the resources used by the current instance.
        /// </summary>
        /// <remarks>This method ensures that the instance is disposed only once. After disposal, the
        /// instance  should not be used. Thread-safe disposal is implemented to prevent concurrent access
        /// issues.</remarks>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            {
                return; // Already disposed
            }
            BuildScope?.Post(this, OnPost());
        }
    }
}