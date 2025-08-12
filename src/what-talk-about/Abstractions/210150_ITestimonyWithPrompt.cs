using System;

namespace Boostable.WhatTalkAbout.Abstractions
{
    /// <summary>
    /// Represents a testimony that includes an associated prompt.
    /// </summary>
    /// <typeparam name="TPrompt">The type of the prompt associated with the testimony. Must implement <see cref="IPromptForTalking{TPrompt}"/>.</typeparam>
    public interface ITestimonyWithPrompt<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        /// <summary>
        /// Gets the prompt associated with the current operation.
        /// </summary>
        TPrompt? Prompt { get; }

        /// <summary>
        /// Gets the exception that provides details about a specific error or issue.
        /// </summary>
        Exception Testimony { get; }
    }
}
