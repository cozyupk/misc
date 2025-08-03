using System;

namespace Boostable.CodeBuilding.Abstractions
{
    /// <summary>
    /// Defines an interface for composing structured code or text fragments, supporting scoped segments, fragment
    /// appending, and nesting depth management.
    /// </summary>
    /// <remarks>Implementations of this interface are expected to provide mechanisms for composing and
    /// managing structured code or text, including support for scoped segments, fragment termination, and nesting depth
    /// constraints. 
    /// If a child composer exists (i.e., one that was created via a nested <see cref="BeginSegment{TCodeComposer}"/> call),
    /// all operations on the composer are recursively delegated to the most deeply nested active composer.</remarks>
    public interface ICodeComposer : IDisposable
    {
        /// <summary>
        /// Begins a new segment for composing code using the specified code composer type.
        /// </summary>
        /// <remarks>Use this method to create a scoped segment for code composition. The returned
        /// composer instance is specific to the segment and should be used only within its context. Ensure that the
        /// segment is properly finalized to avoid resource leaks or inconsistent state.</remarks>
        /// <typeparam name="TCodeComposer">The type of the code composer to use for the segment. Must implement <see cref="ICodeComposer"/> and have a
        /// parameterless constructor.</typeparam>
        /// <param name="maxSegmentNestingDepth">The maximum allowed nesting depth for the segment. If set to -1, the value is automatically decided.</param>
        /// <returns>An instance of <typeparamref name="TCodeComposer"/> to be used for composing code within the segment.</returns>
        TCodeComposer BeginSegment<TCodeComposer>(int maxSegmentNestingDepth = -1)
                    where TCodeComposer : class, ICodeComposer, new();

        /// <summary>
        /// Appends a fragment of code or text to the current composition.
        /// </summary>
        /// <param name="payload">The content to append. This must be a non-null string representing the fragment to add.</param>
        /// <param name="isTerminated">A boolean value indicating whether the appended fragment should be treated as terminated.  If <see
        /// langword="true"/>, the fragment is considered complete; otherwise, it is treated as part of an ongoing
        /// composition.</param>
        /// <returns>An instance of <see cref="ICodeComposer"/> representing the updated composition, allowing for method
        /// chaining.</returns>
        ICodeComposer AppendFragment(string payload, bool isTerminated = false);

        /// <summary>
        /// Appends a fragment to the current composition, followed by a termination sequence.
        /// </summary>
        /// <remarks>This method appends the specified fragment and ensures it is terminated
        /// appropriately.  It is commonly used to build structured code or text compositions where fragments must be
        /// clearly delimited.</remarks>
        /// <param name="payload">The optional string payload to include in the fragment. If <see langword="null"/>, an empty fragment is
        /// appended.</param>
        /// <returns>The current <see cref="ICodeComposer"/> instance, allowing for method chaining.</returns>
        ICodeComposer AppendTerminatedFragment(string? payload = null);

        /// <summary>
        /// Sets the remaining nesting depth for a recursive operation.
        /// </summary>
        /// <remarks>This method is typically used to enforce a limit on recursion depth to prevent stack
        /// overflow or excessive resource consumption. Ensure that <paramref name="maxNestingDepth"/> is set to a value
        /// appropriate for the operation's requirements.</remarks>
        /// <param name="maxNestingDepth">The maximum allowable depth for the operation. Must be a non-negative integer.</param>
        void SetRemainingNestingDepth(int maxNestingDepth);

        /// <summary>
        /// Determines whether the last fragment in the segment has terminated.
        /// </summary>
        /// <remarks>This method is typically used to check the completion status of a sequence or process
        /// that involves fragments. The definition of "terminated" depends on the specific context in which this method
        /// is used.</remarks>
        /// <returns><see langword="true"/> if the last fragment has terminated; otherwise, <see langword="false"/>.</returns>
        bool HasLastFragmentTerminated();
    }
}
