using PartialClassExtGen.Abstractions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PartialClassExtGen.Genalyzer
{
    /// <summary>
    /// Provides a hierarchical, thread-safe string-building utility that supports nested string builders and
    /// customizable indentation. This class is designed for scenarios where strings are constructed incrementally with
    /// optional termination and indentation logic.
    /// </summary>
    /// <remarks>The <see cref="StackedStringBuilder"/> class allows for efficient and structured string
    /// construction by maintaining a buffer of string entries, each of which can be marked as terminated. It supports
    /// hierarchical string building through parent-child relationships, enabling nested or scoped string operations.
    /// This class is thread-safe and ensures atomic updates to the internal buffer.</remarks>
    public class StackedStringBuilder : IStackedStringBuilder
    {
        /// <summary>
        /// Gets the registry of stacked string builders.
        /// </summary>
        public HashSet<IStackedStringBuilder>? Registry { get; }

        /// <summary>
        /// Gets the string representation of a single indentation unit.
        /// </summary>
        /// <remarks>This property can be overridden in derived classes to customize the indentation
        /// unit.</remarks>
        protected internal virtual string IndentUnitString { get; } = "    ";

        /// <summary>
        /// Creates and returns a new child <see cref="StackedStringBuilder"/> instance.
        /// </summary>
        /// <remarks>The child instance inherits the state of the current <see
        /// cref="StackedStringBuilder"/>  and can be used to build upon the existing content without modifying the
        /// parent instance.</remarks>
        /// <returns>A new <see cref="StackedStringBuilder"/> instance that is initialized with the current instance as its
        /// parent.</returns>
        public virtual IStackedStringBuilder SpawnChild()
        {
            return new StackedStringBuilder(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackedStringBuilder"/> class,
        /// associating it with a parent <see cref="StringBuilder"/> and optionally registering it in a provided registry.
        /// </summary>
        /// <param name="parentStringBuilder">
        /// The parent <see cref="StringBuilder"/> to which this instance is linked. Must not be <see langword="null"/>.
        /// </param>
        /// <param name="registry">
        /// An optional registry of <see cref="IStackedStringBuilder"/> instances. If provided,
        /// this instance will be added upon creation and automatically removed when disposed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="parentStringBuilder"/> is <see langword="null"/>.
        /// </exception>
        public StackedStringBuilder(StringBuilder parentStringBuilder, HashSet<IStackedStringBuilder>? registry = null)
        {
            ParentStringBuilder = parentStringBuilder ?? throw new ArgumentNullException(nameof(parentStringBuilder), "Parent StringBuilder cannot be null.");
            Registry = registry;
            Registry?.Add(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackedStringBuilder"/> class with a specified parent and an
        /// optional registry for tracking instances.
        /// </summary>
        /// <param name="parentStackedStringBuilder">The parent <see cref="StackedStringBuilder"/> instance. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="registry">An optional <see cref="HashSet{T}"/> used to register this instance for tracking. If provided, this instance
        /// will be added to the registry and automatically removed when disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parentStackedStringBuilder"/> is <see langword="null"/>.</exception>
        public StackedStringBuilder(StackedStringBuilder parentStackedStringBuilder)
        {
            ParentStackedStringBuilder = parentStackedStringBuilder ?? throw new ArgumentNullException(nameof(parentStackedStringBuilder), "Parent StackedStringBuilder cannot be null.");
            Registry = parentStackedStringBuilder.Registry;
            Registry?.Add(this);
        }

        /// <summary>
        /// Appends the specified string to the current buffer, optionally marking it as terminated.
        /// </summary>
        /// <remarks>If the buffer is empty or the last string in the buffer is marked as terminated,  a
        /// new entry is created in the buffer before appending the string. Otherwise, the string  is appended to the
        /// last entry in the buffer.</remarks>
        /// <param name="str">The string to append to the buffer. Cannot be <see langword="null"/>.</param>
        /// <param name="isTerminated">A value indicating whether the appended string should be marked as terminated. If <see langword="true"/>,
        /// the string is considered complete and no further appends will modify it.</param>
        /// <returns>The current instance of <see cref="IStackedStringBuilder"/> to allow method chaining.</returns>
        public IStackedStringBuilder Append(string str, bool isTerminated = false)
        {
            lock (BufferLock)
            {
                if (Buffer.Count == 0 || Buffer.Last().IsTerminated)
                {
                    Buffer.Add(new("", false));
                }
                var target = Buffer.Last();
                Buffer.RemoveAt(Buffer.Count - 1);
                Buffer.Add(new(target.Str + str, isTerminated));
            }
            return this;
        }

        /// <summary>
        /// Appends the specified string followed by a newline character to the current instance.
        /// </summary>
        /// <param name="str">The string to append. If <see langword="null"/>, an empty string is appended.</param>
        /// <returns>The current <see cref="IStackedStringBuilder"/> instance, allowing for method chaining.</returns>
        public IStackedStringBuilder AppendLine(string? str = null)
        {
            Append(str ?? "", true);
            return this;
        }

        /// <summary>
        /// Merges a collection of string builder entries into the current buffer, applying indentation to the first
        /// entry and ensuring proper termination of the last entry.
        /// </summary>
        /// <remarks>This method processes each entry in the provided collection, appending it to the
        /// buffer. If an entry is not  terminated, the next entry will continue on the same line. The first entry in
        /// the collection is prefixed with  the specified <paramref name="indentUnitString"/>. If the last entry in the
        /// collection is not terminated,  a newline is appended to ensure proper termination.</remarks>
        /// <param name="entries">The collection of <see cref="IStringBuilderEntry"/> objects to merge into the buffer.</param>
        /// <param name="indentUnitString">The string used to indent the first entry in the collection.</param>
        public void MergeFrom(IEnumerable<IStringBuilderEntry> entries, string indentUnitString)
        {
            lock (BufferLock)
            {
                var needIndent = true;
                foreach (var entry in entries)
                {
                    var str = entry.Str ?? string.Empty;
                    if (needIndent)
                    {
                        str = indentUnitString + str;
                        needIndent = false;
                    }
                    Buffer.Add(new(str, entry.IsTerminated));
                    needIndent = entry.IsTerminated;
                }
                if (!needIndent)
                {
                    this.AppendLine(); // Ensure the last entry ends with a newline if not terminated
                }
            }
        }

        /// <summary>
        /// Determines whether the last entry in the buffer is terminated.
        /// </summary>
        /// <remarks>This method checks if the buffer is empty or if the last entry in the buffer is
        /// marked as terminated. If the buffer is empty, the method returns <see langword="true"/>. If the buffer
        /// contains entries, the method returns <see langword="true"/> if the last entry is terminated; otherwise, it
        /// returns <see langword="false"/>.</remarks>
        /// <returns><see langword="true"/> if the buffer is empty or the last entry is terminated; otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsEndsWithTerminatedEntry()
        {
            lock (BufferLock)
            {
                if (Buffer.Count == 0)
                {
                    return true; // No entries to check
                }
                if (Buffer.Last().IsTerminated)
                {
                    return true; // Last entry is terminated
                }
                return false; // Last entry is not terminated
            }
        }

        /// <summary>
        /// Releases the resources used by the current instance of the object.
        /// </summary>
        /// <remarks>This method ensures that the resources associated with the instance are properly
        /// released.  It is safe to call this method multiple times; subsequent calls will have no effect.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if neither <c>ParentStringBuilder</c> nor <c>ParentStackedStringBuilder</c> is set.</exception>
        public void Dispose()
        {
            // Check if already disposed
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
            {
                return; // Already disposed
            }

            // Remove from registry if it exists
            Registry?.Remove(this);

            // Write the buffer to the parent StringBuilder or StackedStringBuilder
            if (ParentStringBuilder != null)
            {
                lock (BufferLock)
                {
                    var needIndent = true;
                    foreach (var entry in Buffer)
                    {
                        if (needIndent)
                        {
                            ParentStringBuilder.Append(IndentUnitString);
                            needIndent = false;
                        }
                        if (entry.IsTerminated)
                        {
                            ParentStringBuilder.AppendLine(entry.Str);
                            needIndent = true;
                            continue;
                        }
                        ParentStringBuilder.Append(entry.Str);
                    }
                    if (!needIndent)
                    {
                        ParentStringBuilder.AppendLine(); // Ensure the last entry ends with a newline if not terminated
                    }
                }
            }
            else if (ParentStackedStringBuilder != null)
            {
                ParentStackedStringBuilder.MergeFrom(Buffer, IndentUnitString);
            }
            else
            {
                throw new InvalidOperationException("Neither ParentStringBuilder nor ParentStackedStringBuilder is set.");
            }
        }

        /// <summary>
        /// Returns a string representation of the current object.
        /// </summary>
        /// <remarks>The returned string is constructed by merging the contents of the <see cref="Buffer">
        /// into a formatted representation. This method is useful for debugging or logging purposes.</remarks>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            using (var dumper = new StackedStringBuilder(sb))
            {
                dumper.MergeFrom(Buffer, "");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Represents an entry in a string-building operation, containing a string value and a termination flag.
        /// </summary>
        /// <remarks>This type is used to encapsulate a string and its associated termination state, 
        /// which can be useful in scenarios where strings are processed or concatenated  with specific termination
        /// conditions.</remarks>
        private record StringBuilderEntry : IStringBuilderEntry
        {
            public string Str { get; }
            public bool IsTerminated { get; }
            public StringBuilderEntry(string str, bool isTerminated)
            {
                Str = str;
                IsTerminated = isTerminated;
            }
        }

        /// <summary>
        /// Records whether the instance has been disposed.
        /// </summary>
        private int _isDisposed = 0;

        /// <summary>
        /// Gets the internal buffer that stores entries for the string builder.
        /// </summary>
        /// <remarks>This property provides access to the collection of entries managed by the string
        /// builder.  It is intended for internal use and should not be modified directly by external callers.</remarks>
        private List<StringBuilderEntry> Buffer { get; } = new List<StringBuilderEntry>();

        /// <summary>
        /// Gets an object used to synchronize access to the buffer.
        /// </summary>
        private object BufferLock { get; } = new object();

        /// <summary>
        /// Gets the parent <see cref="StringBuilder"/> or <see cref="StackedStringBuilder"/> that this instance is associated with.
        /// </summary>
        private StringBuilder? ParentStringBuilder { get; }

        /// <summary>
        /// Gets the parent <see cref="StackedStringBuilder"/> instance, if one exists.
        /// </summary>
        private StackedStringBuilder? ParentStackedStringBuilder { get; }
    }
}
