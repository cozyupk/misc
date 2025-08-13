using System.Threading;

namespace Boostable.WhatAgentsTalkAbout.Abstractions
{
    /// <summary>
    /// Represents a marker interface for prompts used in conversational contexts.
    /// </summary>
    /// <remarks>This interface is intended to serve as a tagging mechanism for identifying types that
    /// function as prompts in a conversational or dialog-based system. It does not define any members and is used
    /// solely for type identification or categorization purposes.</remarks>
    public interface IPromptForTalking
    {
        // Just a marker interface for the prompt.
    }

    /// <summary>
    /// Defines a contract for a prompt that supports talking functionality,  including a label, cancellation support,
    /// and cloning capabilities.
    /// </summary>
    /// <typeparam name="TSelf">The type that implements this interface. Must be a class that implements <see cref="IPromptForTalking{TSelf}"/>.</typeparam>
    public interface IPromptForTalking<out TSelf> : IPromptForTalking
        where TSelf : class, IPromptForTalking<TSelf>
    {
        /// <summary>
        /// Gets the label associated with the current object.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the <see cref="System.Threading.CancellationToken"/> used to observe cancellation requests.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Creates a new instance of the current object with the specified label.
        /// </summary>
        /// <param name="label">A string representing the label to associate with the cloned object. Cannot be null or empty.</param>
        /// <returns>A new instance of <typeparamref name="TSelf"/> that is a copy of the current object, with the specified
        /// label applied.</returns>
        TSelf Clone(string label);
    }
}
