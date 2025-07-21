using Boostable.CodeBuilding.Abstractions;
using System;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Represents an entry in a code composition process, containing a string value and a termination status.
    /// </summary>
    /// <remarks>This struct is used to encapsulate a string value and a flag indicating whether the entry is
    /// considered terminated. The <see cref="Str"/> property cannot be null, and if it is an empty string, the <see
    /// cref="IsTerminated"/> property must be <see langword="true"/>.</remarks>
    internal readonly struct ComposingEntry : IComposingEntry
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
        /// Initializes a new instance of the <see cref="ComposingEntry"/> class with the specified string value and
        /// termination status.
        /// </summary>
        /// <param name="str">The string value associated with this entry. Cannot be <see langword="null"/>. If empty, <paramref
        /// name="isTerminated"/> must be <see langword="true"/>.</param>
        /// <param name="isTerminated">A value indicating whether the entry is considered terminated. If <see langword="false"/>, <paramref
        /// name="str"/> cannot be an empty string.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="str"/> is an empty string and <paramref name="isTerminated"/> is <see
        /// langword="false"/>.</exception>
        public ComposingEntry(string str, bool isTerminated)
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
}
