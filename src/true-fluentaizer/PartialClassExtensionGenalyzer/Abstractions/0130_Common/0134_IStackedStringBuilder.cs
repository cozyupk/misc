using System.Collections.Generic;

namespace PartialClassExtGen.Abstractions.Common
{
    /// <summary>
    /// Represents a string builder that supports hierarchical or stacked operations,  allowing for the creation of
    /// child builders and merging of entries with indentation.
    /// </summary>
    /// <remarks>This interface is designed for scenarios where strings need to be built in a structured  or
    /// hierarchical manner, such as generating nested text or code with consistent indentation.</remarks>
    public interface IStackedStringBuilder
    {
        /// <summary>
        /// Gets the registry of stacked string builders.
        /// </summary>
        HashSet<IStackedStringBuilder>? Registry { get; }

        /// <summary>
        /// Creates and returns a new child instance of the current <see cref="IStackedStringBuilder"/>.
        /// </summary>
        /// <remarks>The child instance inherits the context of the parent <see
        /// cref="IStackedStringBuilder"/> and can be used to build strings independently while maintaining a
        /// relationship with the parent.</remarks>
        /// <returns>A new <see cref="IStackedStringBuilder"/> instance that is a child of the current instance.</returns>
        IStackedStringBuilder SpawnChild();

        /// <summary>
        /// Appends the specified string to the current instance of the stacked string builder.
        /// </summary>
        /// <param name="str">The string to append. Cannot be <see langword="null"/>.</param>
        /// <param name="isTerminated">A value indicating whether the appended string is considered terminated.  If <see langword="true"/>, the
        /// string is treated as a complete segment; otherwise, it is treated as part of an ongoing segment.</param>
        /// <returns>The current instance of the stacked string builder, allowing for method chaining.</returns>
        IStackedStringBuilder Append(string str, bool isTerminated = false);

        /// <summary>
        /// Appends the specified string followed by a newline character to the current instance of the string builder.
        /// </summary>
        /// <param name="str">The string to append. Can be an empty string but cannot be <see langword="null"/>.</param>
        /// <returns>The current instance of <see cref="IStackedStringBuilder"/> with the appended string and newline.</returns>
        IStackedStringBuilder AppendLine(string str);

        /// <summary>
        /// Merges the specified string builder entries into the current instance, applying the given indentation unit.
        /// </summary>
        /// <param name="entries">A collection of <see cref="IStringBuilderEntry"/> objects to merge. Cannot be null.</param>
        /// <param name="indentUnitString">The string representing a single unit of indentation to apply. Cannot be null or empty.</param>
        void MergeFrom(IEnumerable<IStringBuilderEntry> entries, string indentUnitString);
    }
}
