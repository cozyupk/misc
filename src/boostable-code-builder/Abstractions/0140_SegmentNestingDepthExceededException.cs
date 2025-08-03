using System;

namespace Boostable.CodeBuilding.Abstractions
{
    /// <summary>
    /// Represents an exception that is thrown when the nesting depth of segments exceeds the allowed limit.
    /// </summary>
    /// <remarks>This exception is typically thrown during operations involving segment composition when the
    /// nesting depth surpasses the configured maximum. To resolve this issue, consider increasing the maximum allowed
    /// depth by calling <see cref="SetRemainingNestingDepth(int)"/>.</remarks>
    public class SegmentNestingDepthExceededException : InvalidOperationException
    {
        /// <summary>
        /// Gets the type of the composer associated with this instance.
        /// </summary>
        public Type ComposerType { get; }

        /// <summary>
        /// Represents an exception that is thrown when the nesting depth of segments exceeds the allowed limit.
        /// </summary>
        /// <remarks>This exception is typically thrown during operations involving segment composition
        /// when the nesting depth surpasses the configured maximum. To resolve this issue, consider increasing the
        /// maximum allowed depth by calling <see cref="SetRemainingNestingDepth(int)"/>.</remarks>
        /// <param name="composerType">The type of the composer where the nesting depth limit was exceeded. This parameter cannot be <see
        /// langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="composerType"/> is <see langword="null"/>.</exception>
        public SegmentNestingDepthExceededException(Type composerType)
            : base($"Nesting segment depth exceeded the allowed limit in {composerType.Name} ({composerType.FullName}). " +
                   "Consider calling SetRemainingNestingDepth(int) to increase the depth if needed.")
        {
            ComposerType = composerType ?? throw new ArgumentNullException(nameof(composerType), "Composer type cannot be null.");
        }
    }
}
