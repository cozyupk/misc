using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Boostable.WhatAgentsTalkAbout.Abstractions
{
    /// <summary>
    /// Provides a set of abstractions for defining and managing a structured "talk session" workflow,  including
    /// prompts, artifacts, prerequisites, testimonies, and outlines.
    /// </summary>
    /// <remarks>This abstract class defines a framework for managing a "talk session" that involves
    /// structured interactions  between prompts, artifacts, and testimonies. It includes nested interfaces for defining
    /// prerequisites,  domain factories, testimonies, and outlines, enabling extensibility and customization of the
    /// workflow.</remarks>
    /// <typeparam name="TPrompt">The type of the prompt used in the talk session. Must implement <see cref="IPromptForTalking{TPrompt}"/>.</typeparam>
    /// <typeparam name="TReadOnlyArtifacts">The type of the read-only artifacts associated with the talk session. Must implement <see
    /// cref="IReadOnlyArtifacts"/>.</typeparam>
    /// <typeparam name="TArtifacts">The type of the mutable artifacts associated with the talk session. Must implement <typeparamref
    /// name="TReadOnlyArtifacts"/>,  <see cref="IArtifacts"/>, and have a parameterless constructor.</typeparam>
    public abstract class TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts
    {
        /// <summary>
        /// Represents a read-only prerequisite that provides access to associated prompts and artifacts.
        /// </summary>
        /// <remarks>This interface is designed to expose the prompts and artifacts required for a
        /// specific operation or context in a read-only manner. Implementations of this interface should ensure that
        /// the data is immutable or treated as such by consumers.</remarks>
        public interface IReadOnlyPrerequisite
        {
            /// <summary>
            /// Gets the collection of prompts associated with the current instance.
            /// </summary>
            IReadOnlyList<TPrompt> Prompts { get; }

            /// <summary>
            /// Gets the collection of artifacts associated with the current instance.
            /// </summary>
            TReadOnlyArtifacts Artifacts { get; }
        }

        /// <summary>
        /// Provides methods for creating testimony objects with optional chapter and prompt information.
        /// </summary>
        /// <remarks>This factory interface is designed to create various types of testimony objects, 
        /// allowing the caller to specify optional chapter and prompt details, as well as an associated
        /// exception.</remarks>
        public interface ITalkDomainFactory
        {
            /// <summary>
            /// Creates a new testimony instance with the specified chapter, prompt, and exception.
            /// </summary>
            /// <param name="chapter">The chapter of the talk associated with the testimony. Can be <see langword="null"/> if no chapter is
            /// specified.</param>
            /// <param name="prompt">The prompt associated with the testimony. Can be <see langword="null"/> if no prompt is specified.</param>
            /// <param name="testimony">The exception representing the testimony. This parameter cannot be <see langword="null"/>.</param>
            /// <returns>An instance of <see cref="ITestimonyWithChapterAndPrompt{TPrompt}"/> containing the specified chapter,
            /// prompt, and testimony.</returns>
            ITestimonyWithChapterAndPrompt<TPrompt> CreateTestimony(ITalkChapter? chapter, TPrompt? prompt, Exception testimony);

            /// <summary>
            /// Creates a new testimony with the specified chapter and exception details.
            /// </summary>
            /// <param name="chapter">The chapter associated with the testimony. Can be <see langword="null"/> if no chapter is specified.</param>
            /// <param name="testimony">The exception representing the testimony details. Must not be <see langword="null"/>.</param>
            /// <returns>An object implementing <see cref="ITestimonyWithChapter"/> that represents the created testimony.</returns>
            ITestimonyWithChapter CreateTestimony(ITalkChapter? chapter, Exception testimony);

            /// <summary>
            /// Creates a new instance of an object that associates a prompt with a testimony exception.
            /// </summary>
            /// <param name="prompt">The prompt associated with the testimony. Can be null if no prompt is provided.</param>
            /// <param name="testimony">The exception representing the testimony. This parameter cannot be null.</param>
            /// <returns>An object implementing <see cref="ITestimonyWithPrompt{TPrompt}"/> that encapsulates the provided prompt
            /// and testimony.</returns>
            ITestimonyWithPrompt<TPrompt> CreateTestimony(TPrompt? prompt, Exception testimony);

            /// <summary>
            /// Creates and returns a new instance of the artifacts.
            /// </summary>
            /// <returns>An instance of <typeparamref name="TArtifacts"/> representing the created artifacts.</returns>
            TArtifacts CreateArtifacts();
        }

        /// <summary>
        /// Represents a prerequisite that can be configured with prompts and provides access to its artifacts.
        /// </summary>
        /// <remarks>This interface extends <see cref="IReadOnlyPrerequisite"/> by allowing modification
        /// of the prerequisite through the <see cref="SetPrompt"/> method and exposing the <see cref="Artifacts"/>
        /// property.</remarks>
        public interface IPrerequisite : IReadOnlyPrerequisite
        {
            /// <summary>
            /// Gets the collection of artifacts associated with the current instance.
            /// </summary>
            new TArtifacts Artifacts { get; }
        }

        /// <summary>
        /// Represents a summary of testimonies collected during an operation, including exceptions and other evidence.
        /// </summary>
        /// <remarks>This interface provides access to various collections of testimonies, grouped and
        /// categorized by chapters, prompts,  or both. It is primarily used to analyze and retrieve information about
        /// exceptions or abnormal records encountered  during an operation.</remarks>
        public interface ITestimonySummary
        {
            /// <summary>
            /// True if at least one testimony has been recorded.
            /// </summary>
            /// <remarks>
            /// In this framework, “testimony” chiefly refers to exceptions or abnormal records.
            /// Therefore, <c>IsMeaningful == true</c> indicates that some issue may have occurred.
            /// </remarks>
            bool IsMeaningful { get; }

            /// <summary>
            /// Gets a read-only list of exceptions that represent general testimony or evidence collected during an
            /// operation.
            /// </summary>
            IReadOnlyList<Exception> GeneralTestimony { get; }

            /// <summary>
            /// Gets the prerequisite required for the operation.
            /// </summary>
            IReadOnlyPrerequisite Prerequisite { get; }

            /// <summary>
            /// Gets a read-only list of all testimonies, each associated with a chapter and a prompt.
            /// </summary>
            IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; }

            /// <summary>
            /// Gets a read-only dictionary that maps each chapter and prompt pair to a list of associated exceptions.
            /// </summary>
            /// <remarks>This property provides a way to retrieve exceptions associated with specific
            /// chapter and prompt combinations. The dictionary keys are tuples of <see cref="ITalkChapter"/> and
            /// <c>TPrompt</c>, and the values are lists of exceptions.</remarks>
            IReadOnlyDictionary<
                (ITalkChapter, TPrompt),
                IReadOnlyList<Exception>
            > TestimonyForEachChapterAndPrompt { get; }

            /// <summary>
            /// Gets a read-only dictionary that maps each prompt to a collection of testimonies associated with it.
            /// </summary>
            /// <remarks>This property provides a way to retrieve all testimonies grouped by their
            /// corresponding prompts.  The dictionary is read-only, ensuring that the mapping cannot be
            /// modified.</remarks>
            IReadOnlyDictionary<
                TPrompt,
                IReadOnlyList<ITestimonyWithChapter>
            > TestimonyForEachPrompt { get; }

            /// <summary>
            /// Gets a read-only dictionary that maps each chapter to a collection of testimonies with prompts.
            /// </summary>
            /// <remarks>This property provides a way to access testimonies grouped by their
            /// corresponding chapters.  The dictionary is read-only, ensuring that the mapping cannot be
            /// modified.</remarks>
            IReadOnlyDictionary<
                ITalkChapter,
                IReadOnlyList<ITestimonyWithPrompt<TPrompt>>
            > TestimonyForEachChapter { get; }
        }

        /// <summary>
        /// Represents an outline for a talk, providing access to its chapters and methods to generate testimony based
        /// on the talk.
        /// </summary>
        public interface ITalkOutline
        {
            /// <summary>
            /// Gets the collection of talk chapters associated with the current instance.
            /// </summary>
            IReadOnlyList<ITalkChapter> TalkChapters { get; }

            /// <summary>
            /// Initiates a testimony about a specific subject or context.
            /// </summary>
            /// <returns>An object implementing the <see cref="ITestimonySummary"/> interface, representing the testimony provided.</returns>
            ITestimonySummary TalkAbout();

            /// <summary>
            /// Initiates an asynchronous operation to retrieve a testimony.
            /// </summary>
            /// <remarks>The returned <see cref="ITestimonySummary"/> object provides access to the details
            /// of the testimony.  Ensure that the task is awaited or handled appropriately to avoid unobserved
            /// exceptions.</remarks>
            /// <returns>A task that represents the asynchronous operation. The task result contains an object  implementing the
            /// <see cref="ITestimonySummary"/> interface, which represents the retrieved testimony.</returns>
            Task<ITestimonySummary> TalkAboutAsync();
        }

    }

}
