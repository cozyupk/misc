namespace Boostable.WhatAgentsTalkAbout.Abstractions
{
    /// <summary>
    /// Represents a read-only collection of artifacts, providing a base interface for extension points.
    /// </summary>
    /// <remarks>This interface is intended to serve as a foundation for defining read-only access to artifact
    /// collections. It can be extended to include specific members for accessing or querying artifacts without
    /// modifying them.</remarks>
    public interface IReadOnlyArtifacts
    {
        // For extension points.
    }
}
