namespace Boostable.Strings.StackedStringBuilder.Abstractions
{
    /// <summary>
    /// Represents an entry in a string builder-like structure, containing a string value and a termination state.
    /// </summary>
    /// <remarks>This interface is designed to encapsulate a string value along with a flag indicating whether
    /// the entry is terminated. It can be used in scenarios where managing a sequence of string entries with
    /// termination states is required.</remarks>
    public interface IStringBuilderEntry
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
}
