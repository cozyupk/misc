using Boostable.WhatAgentsTalkAbout.Abstractions;
using System;
using System.Collections.Generic;

namespace Boostable.WhatAgentsTalkAbout.Core
{
    /// <summary>
    /// Provides an abstract base class for managing the outline of a talk session, including prompts, artifacts, and
    /// testimony administration.
    /// </summary>
    /// <remarks>This class serves as a foundation for defining the structure and behavior of a talk session,
    /// including the management of prompts, artifacts, and testimony. It extends <see
    /// cref="TalkSessionAbstractions{TPrompt, TReadOnlyArtifacts, TArtifacts}"/> to provide additional functionality
    /// specific to talk outlines.</remarks>
    /// <typeparam name="TPrompt">The type of the prompt used in the talk session. Must implement <see cref="IPromptForTalking{TPrompt}"/>.</typeparam>
    /// <typeparam name="TReadOnlyArtifacts">The type of the read-only artifacts associated with the talk session. Must implement <see
    /// cref="IReadOnlyArtifacts"/>.</typeparam>
    /// <typeparam name="TArtifacts">The type of the artifacts associated with the talk session. Must implement <typeparamref
    /// name="TReadOnlyArtifacts"/> and <see cref="IArtifacts"/>.</typeparam>
    public abstract class TalkOutlineAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts
    {
        /// <summary>
        /// Provides administrative functionality for managing and retrieving testimony data.
        /// </summary>
        /// <remarks>This interface defines methods and properties for accessing testimony-related
        /// information, including snapshots of testimonies grouped by various criteria such as chapters and
        /// prompts.</remarks>
        public interface ITestimonyAdmin
        {
            /// <summary>
            /// Gets the total count of all testimonies.
            /// </summary>
            int TotalTestimonyCount { get; }

            /// <summary>
            /// Adds a testimony to the specified chapter with an optional prompt.
            /// </summary>
            /// <param name="testimony">The exception instance representing the testimony to be added. Cannot be <see langword="null"/>.</param>
            /// <param name="chapter">The chapter to which the testimony will be added. If <see langword="null"/>, the testimony is added to
            /// the default chapter.</param>
            /// <param name="prompt">An optional prompt associated with the testimony. If <see langword="null"/>, no prompt is associated.</param>
            void Add(
                Exception testimony,
                ITalkChapter? chapter = null,
                TPrompt? prompt = null
            );

            /// <summary>
            /// Captures a snapshot of general exceptions that have occurred.
            /// </summary>
            /// <remarks>This method provides a way to retrieve a collection of exceptions for
            /// diagnostic or logging purposes.  The returned list is immutable and reflects the state of exceptions at
            /// the time of the snapshot.</remarks>
            /// <returns>A read-only list of <see cref="Exception"/> objects representing the captured exceptions.  The list will
            /// be empty if no exceptions have been recorded.</returns>
            IReadOnlyList<Exception> SnapshotGeneral();

            /// <summary>
            /// Captures a snapshot of all testimonies, including their associated chapters and prompts.
            /// </summary>
            /// <returns>A read-only list of testimonies, where each testimony includes its chapter and associated prompt. The
            /// list will be empty if no testimonies are available.</returns>
            IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> SnapshotAll();

            /// <summary>
            /// Captures a snapshot of the current state of prompts grouped by chapter and prompt type.
            /// </summary>
            /// <remarks>This method provides a way to analyze the state of prompts and their
            /// associated exceptions for each chapter. Use this to identify and handle issues related to specific
            /// chapters or prompts.</remarks>
            /// <returns>A read-only dictionary where each key is a tuple containing a chapter and a prompt,  and the value is a
            /// read-only list of exceptions associated with that chapter and prompt. The dictionary will be empty if no
            /// data is available.</returns>
            IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> SnapshotByChapterPrompt();

            /// <summary>
            /// Creates a snapshot of testimonies grouped by their associated prompts.
            /// </summary>
            /// <remarks>The returned snapshot represents the state of the testimonies at the time of
            /// the method call.  Subsequent changes to the underlying data will not affect the snapshot.</remarks>
            /// <returns>A read-only dictionary where each key is a prompt of type <typeparamref name="TPrompt"/>,  and the
            /// corresponding value is a read-only list of testimonies with chapters of type <see
            /// cref="ITestimonyWithChapter"/>. The dictionary will be empty if no testimonies are available.</returns>
            IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> SnapshotByPrompt();

            /// <summary>
            /// Retrieves a snapshot of testimonies grouped by their associated chapters.
            /// </summary>
            /// <returns>A read-only dictionary where the keys are chapters of type <see cref="ITalkChapter"/>  and the values
            /// are read-only lists of testimonies with prompts of type  <see cref="ITestimonyWithPrompt{TPrompt}"/>.
            /// The dictionary will be empty if no testimonies are available.</returns>
            IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> SnapshotByChapter();
        }
    }
}
