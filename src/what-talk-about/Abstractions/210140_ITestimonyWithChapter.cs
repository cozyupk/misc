using System;

namespace Boostable.WhatTalkAbout.Abstractions
{
    /// <summary>
    /// Represents a testimony that is associated with a specific chapter.
    /// </summary>
    public interface ITestimonyWithChapter
    {
        /// <summary>
        /// Gets the current chapter of the talk, if available.
        /// </summary>
        ITalkChapter? Chapter { get; }

        /// <summary>
        /// Gets the exception that provides details about a specific error or issue.
        /// </summary>
        Exception Testimony { get; }
    }
}
