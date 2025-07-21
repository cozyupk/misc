using System;

namespace Boostable.CodeBuilding.Abstractions
{
    /// <summary>
    /// Defines a contract for composing and managing code or text content, with support for appending,  formatting, and
    /// managing termination states of compositions.
    /// </summary>
    /// <remarks>This interface provides methods for appending strings, managing termination states, and
    /// creating  new instances of code composers. It is designed to support fluent method chaining and ensures  proper
    /// resource management through the <see cref="IDisposable"/> interface.</remarks>
    public interface ICodeComposer : IDisposable
    {
        /// <summary>
        /// Opens a new instance of the specified <typeparamref name="TCodeComposer"/> type.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to open. Must be a class that implements <see cref="ICodeComposer"/> and has a
        /// parameterless constructor.</typeparam>
        /// <param name="maxStackingDepth">
        /// The maximum stacking depth allowed for the code composer.
        /// Specify a negative value to inherit (depth - 1) from the parent composer.
        /// </param>
        /// <returns>A new instance of <typeparamref name="TCodeComposer"/>.</returns>
        TCodeComposer Open<TCodeComposer>(int maxStackingDepth = -1)
                    where TCodeComposer : class, ICodeComposer, new();

        /// <summary>
        /// Appends the specified string to the current composition and optionally terminates the composition.
        /// </summary>
        /// <param name="str">The string to append. Cannot be null.</param>
        /// <param name="shouldTerminate">A value indicating whether the composition should be terminated after appending the string.  <see
        /// langword="true"/> to terminate; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="ICodeComposer"/> instance representing the updated composition.</returns>
        ICodeComposer Append(string str, bool shouldTerminate = false);

        /// <summary>
        /// Appends the specified string followed by a newline character to the current composition.
        /// </summary>
        /// <param name="str">The string to append. If <see langword="null"/> or omitted, only a newline character is appended.</param>
        /// <returns>The current instance of <see cref="ICodeComposer"/>, allowing for method chaining.</returns>
        ICodeComposer AppendLine(string? str = null);

        /// <summary>
        /// Resets the maximum depth to the specified value.
        /// </summary>
        /// <param name="depth">The new maximum depth value. Must be a non-negative integer.</param>
        void ResetMaxDepth(int depth);

        /// <summary>
        /// Determines whether the last entry in the log has been terminated.
        /// </summary>
        /// <remarks>This method checks the termination status of the most recent log entry.  Use this to
        /// verify if the last entry has been properly closed or finalized.</remarks>
        /// <returns><see langword="true"/> if the last entry in the log has been terminated; otherwise, <see langword="false"/>.</returns>
        bool HasTerminatedLastEntry();
    }
}
