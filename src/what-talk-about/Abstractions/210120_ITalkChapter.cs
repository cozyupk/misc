namespace Boostable.WhatTalkAbout.Abstractions
{
    /// <summary>
    /// Represents a chapter in a talk or presentation.
    /// </summary>
    /// <remarks>This interface defines the structure for a chapter, including its name. Implementations may
    /// provide additional details or functionality specific to the context of the talk.</remarks>
    public interface ITalkChapter
    {
        /// <summary>
        /// Gets the name associated with the current instance.
        /// </summary>
        public string Name { get; }
    }
}
