namespace Boostable.CodeBuilding.Abstractions
{
    /// <summary>
    /// Represents an entry with a string value and a termination status.
    /// </summary>
    /// <remarks>This interface is typically used to encapsulate a string value and a flag indicating whether 
    /// a process or operation associated with the entry has been terminated.</remarks>
    public interface IComposingEntry
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
