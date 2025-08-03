namespace Boostable.CodeBuilding.Abstractions
{
    /// <summary>
    /// Represents a fragment of code with associated metadata.
    /// </summary>
    /// <remarks>This interface provides access to the code fragment's payload and its termination state.
    /// Implementations of this interface can be used to encapsulate and manage code snippets or other textual data in a
    /// structured manner.</remarks>
    public interface ICodeFragment
    {
        /// <summary>
        /// Gets the payload associated with the current operation.
        /// </summary>
        string Payload { get; }

        /// <summary>
        /// Gets a value indicating whether the process or operation has been terminated.
        /// </summary>
        bool IsTerminated { get; }
    }
}
