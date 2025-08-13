using System;

namespace Boostable.WhatAgentsTalkAbout.Abstractions
{
    /// <summary>
    /// Represents a testimony that includes an associated chapter and a prompt.
    /// </summary>
    /// <typeparam name="TPrompt">The type of the prompt associated with the testimony. Must implement <see cref="IPromptForTalking{TPrompt}"/>.</typeparam>
    public interface ITestimonyWithChapterAndPrompt<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        /// <summary>
        /// Gets the current chapter of the talk, if available.
        /// </summary>
        ITalkChapter? Chapter { get; }

        /// <summary>
        /// Gets the prompt associated with the current operation or context.
        /// </summary>
        TPrompt? Prompt { get; }

        /// <summary>
        /// Gets the exception that provides details about a specific error or issue.
        /// </summary>
        Exception Testimony { get; }
    }
}
