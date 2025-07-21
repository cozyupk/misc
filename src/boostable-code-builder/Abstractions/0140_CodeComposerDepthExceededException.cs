using System;

namespace Boostable.CodeBuilding.Abstractions
{
    /// <summary>
    /// Represents an exception that is thrown when the depth limit for a code composer is exceeded.
    /// </summary>
    /// <remarks>This exception is typically thrown when a code composer exceeds its allowed depth limit
    /// during recursive or nested operations. To resolve this issue, consider calling the <c>ResetDepth(int)</c> method
    /// to increase the depth limit if necessary.</remarks>
    public class CodeComposerDepthExceededException : InvalidOperationException
    {
        /// <summary>
        /// Gets the type of the composer associated with this instance.
        /// </summary>
        public Type ComposerType { get; }

        /// <summary>
        /// Represents an exception that is thrown when the depth limit for a code composer is exceeded.
        /// </summary>
        /// <remarks>This exception is typically thrown when the nesting or recursion depth of a code
        /// composer exceeds the allowed limit. To resolve this issue, consider calling <see cref="ResetDepth(int)"/> to
        /// increase the depth limit if appropriate.</remarks>
        /// <param name="composerType">The type of the code composer that exceeded the depth limit.</param>
        public CodeComposerDepthExceededException(Type composerType)
            : base($"CodeComposer depth exceeded the allowed limit at {composerType.Name} ({composerType.FullName}). " +
                   $"Consider calling ResetDepth(int) to increase the depth if needed.")
        {
            ComposerType = composerType ?? throw new ArgumentNullException(nameof(composerType), "Composer type cannot be null.");
        }
    }
}
