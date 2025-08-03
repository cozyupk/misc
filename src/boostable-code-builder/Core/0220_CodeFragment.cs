using Boostable.CodeBuilding.Abstractions;
using System;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Represents a segment of code, including its content and termination status.
    /// </summary>
    /// <remarks>A <see cref="CodeFragment"/> encapsulates a code fragment and a flag indicating whether the
    /// fragment is terminated.  This structure is immutable and ensures that invalid combinations of code fragment and
    /// termination status are not allowed.</remarks>
    internal readonly struct CodeFragment : ICodeFragment
    {
        /// <summary>
        /// Gets the code fragment associated with this instance.
        /// </summary>
        public string Payload { get; }

        /// <summary>
        /// Gets a value indicating whether the process or operation has been terminated.
        /// </summary>
        public bool IsTerminated { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFragment"/> class with the specified code fragment and
        /// termination status.
        /// </summary>
        /// <param name="codeFragment">The code fragment represented by this segment. Cannot be <see langword="null"/>.  If an empty string is
        /// provided, <paramref name="isTerminated"/> must be <see langword="true"/>.</param>
        /// <param name="isTerminated">A value indicating whether the code fragment is terminated. If <paramref name="codeFragment"/> is an empty
        /// string, this must be <see langword="true"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="codeFragment"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="codeFragment"/> is an empty string and <paramref name="isTerminated"/> is <see
        /// langword="false"/>.</exception>
        public CodeFragment(string codeFragment, bool isTerminated)
        {
            // Validate null
            if (codeFragment == null) throw new ArgumentNullException(nameof(codeFragment));

            // We do not allow empty strings unless isTerminated is true,
            // in order to simplify the implementation of the CodeComposerBase.HasTerminatedLastSegment method.
            if (codeFragment == string.Empty && isTerminated == false)
            {
                throw new ArgumentException("Cannot create a CodeSegment with an empty string and isTerminated set to false.", nameof(codeFragment));
            }
            Payload = codeFragment;
            IsTerminated = isTerminated;
        }
    }
}
