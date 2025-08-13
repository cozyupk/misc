using Boostable.WhatAgentsTalkAbout.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Boostable.WhatAgentsTalkAbout.Shell
{
    /// <summary>
    /// Serves as the base class for managing a talk session, providing core functionality for  handling prompts,
    /// artifacts, and outlines in a structured conversation flow.
    /// </summary>
    /// <remarks>This abstract class provides a foundation for implementing talk sessions, including support
    /// for  prompt variation, outline generation, and ensuring that a session is executed only once.  Derived classes
    /// can customize behavior by overriding the default prompt variation builder  and outline factory
    /// methods.</remarks>
    /// <typeparam name="TPrompt">The type of the prompt used in the session. Must implement <see cref="IPromptForTalking{TPrompt}"/>.</typeparam>
    /// <typeparam name="TReadOnlyArtifacts">The type of the read-only artifacts associated with the session. Must implement <see
    /// cref="IReadOnlyArtifacts"/>.</typeparam>
    /// <typeparam name="TArtifacts">The type of the artifacts associated with the session. Must implement <see cref="IArtifacts"/>  and extend
    /// <typeparamref name="TReadOnlyArtifacts"/>.</typeparam>
    public abstract class TalkSessionBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts
    {
        protected ITalkDomainFactory TalkDomainFactory { get; }

        protected ITalkOutline Outline { get; }

        private int _hasRun = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TalkSessionBase{TPrompt}"/> class with the specified base
        /// prompt,  domain factory, and optional customization functions for prompt variations and outline creation.
        /// </summary>
        /// <remarks>This constructor allows for customization of the prompt variation and outline
        /// creation processes by providing  optional delegates. If no customization is needed, the default
        /// implementations will be used.</remarks>
        /// <param name="basePrompt">The base prompt used to generate variations and construct the talk outline. This parameter cannot be null.</param>
        /// <param name="talkDomainFactory">The factory responsible for creating domain-specific components for the talk session. This parameter cannot
        /// be null.</param>
        /// <param name="promptVariationBuilder">An optional function to generate a list of prompt variations based on the <paramref name="basePrompt"/>.  If
        /// not provided, a default variation builder will be used.</param>
        /// <param name="outlineFactory">An optional function to create an <see cref="ITalkOutline"/> from the generated prompt variations.  If not
        /// provided, a default outline factory will be used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="talkDomainFactory"/> is null.</exception>
        public TalkSessionBase(
            TPrompt basePrompt,
            ITalkDomainFactory talkDomainFactory,
            Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null,
            Func<IReadOnlyList<TPrompt>, ITalkOutline>? outlineFactory = null
        )
        {
            promptVariationBuilder ??= basePrompt => DefaultVariationBuilder(basePrompt);
            var prompts = promptVariationBuilder(basePrompt);
            outlineFactory ??= DefaultOutlineFactory; // // Allow the factory to be overridden, e.g. for outline injection or behavior customization.
            Outline = outlineFactory(prompts);
            TalkDomainFactory = talkDomainFactory ?? throw new ArgumentNullException(nameof(talkDomainFactory));
        }

        /// <summary>
        /// Ensures that the current session has not already been executed.
        /// </summary>
        /// <remarks>This method enforces that the session can only be executed once. If the session has
        /// already been executed,  an <see cref="InvalidOperationException"/> is thrown. This is useful for scenarios
        /// where re-execution  of a session is not allowed.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session has already been executed.</exception>
        protected internal void EnsureNotYetHasRun()
        {
            if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
            {
                throw new InvalidOperationException("This session has already been run. Let it run only once per session.");
            }
        }

        /// <summary>
        /// Initiates a discussion and retrieves a testimony based on the current outline.
        /// </summary>
        /// <remarks>This method ensures that the operation has not already been executed before
        /// proceeding.</remarks>
        /// <returns>An <see cref="ITestimony"/> instance representing the testimony generated from the discussion.</returns>
        public virtual ITestimonySummary TalkAbout()
        {
            EnsureNotYetHasRun();
            return Outline.TalkAbout();
        }

        /// <summary>
        /// Initiates an asynchronous operation to retrieve a testimony.
        /// </summary>
        /// <remarks>This method ensures that the operation has not already been executed before
        /// proceeding. It delegates the retrieval of the testimony to the underlying <see cref="Outline"/>
        /// instance.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ITestimony"/>
        /// object representing the retrieved testimony.</returns>
        public async virtual Task<ITestimonySummary> TalkAboutAsync()
        {
            EnsureNotYetHasRun();
            return await Outline.TalkAboutAsync();
        }

        /// <summary>
        /// Generates a default variation of the specified base prompt.
        /// </summary>
        /// <remarks>This method provides a default implementation that returns the base prompt as the
        /// only variation. Derived classes can override this method to provide custom variation logic.</remarks>
        /// <param name="basePrompt">The base prompt to use for generating variations. Must not be <see langword="null"/>.</param>
        /// <returns>A read-only list containing a single variation, which is a clone of the base prompt with a default label.</returns>
        protected internal virtual IReadOnlyList<TPrompt> DefaultVariationBuilder(TPrompt basePrompt)
        {
            // Default implementation that returns the base prompt as the only variation.
            return [basePrompt.Clone("Default Prompt (without promptVariationBuilder)")];
        }

        /// <summary>
        /// Creates a default outline using the provided prompts.
        /// </summary>
        /// <remarks>This method provides a default implementation for generating an outline.  Derived
        /// classes can override this method to customize the outline creation process.</remarks>
        /// <param name="prompts">A read-only list of prompts to include in the outline. Cannot be null.</param>
        /// <returns>An instance of <see cref="ITalkOutline"/> representing the default outline created with the specified
        /// prompts.</returns>
        protected abstract internal ITalkOutline DefaultOutlineFactory(
            IReadOnlyList<TPrompt> prompts
        );
    }
}
