using Boostable.WhatAgentsTalkAbout.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boostable.WhatAgentsTalkAbout.Core
{
    /// <summary>
    /// Provides a base implementation for managing and organizing a structured outline of a "talk" session,  including
    /// prompts, chapters, and associated testimonies.
    /// </summary>
    /// <remarks>This class serves as a foundational abstraction for managing the lifecycle of a talk session,
    /// including  caching, thread-safe operations, and the organization of testimonies and chapters. Derived classes
    /// can  extend its functionality by overriding key methods such as <see cref="PrepareForTalk"/> and  <see
    /// cref="PrepareForTalkAsync"/> to implement custom preparation logic.</remarks>
    /// <typeparam name="TPrompt">The type of the prompt used in the talk session. Must implement <see cref="IPromptForTalking{TPrompt}"/>.</typeparam>
    /// <typeparam name="TReadOnlyArtifacts">The type of the read-only artifacts associated with the talk session. Must implement <see
    /// cref="IReadOnlyArtifacts"/>.</typeparam>
    /// <typeparam name="TArtifacts">The type of the artifacts associated with the talk session. Must implement <see cref="IArtifacts"/> and 
    /// <typeparamref name="TReadOnlyArtifacts"/>.</typeparam>
    public abstract class TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkOutlineAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITestimonySummary
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITalkOutline
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, IArtifacts, TReadOnlyArtifacts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TalkOutlineBase"/> class with the specified prompts, talk
        /// domain factory, and chapters.
        /// </summary>
        /// <param name="prompts">A read-only list of prompts to be used as prerequisites. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="talkDomainFactory">The factory responsible for creating talk domain objects. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="chapters">An optional array of talk chapters to include in the outline. If no chapters are provided, an empty
        /// collection is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="prompts"/> or <paramref name="talkDomainFactory"/> is <see langword="null"/>.</exception>
        protected internal TalkOutlineBase(
            IReadOnlyList<TPrompt> prompts,
            ITalkDomainFactory talkDomainFactory,
            params ITalkChapter[] chapters
        )
        {
            _ = prompts ?? throw new ArgumentNullException(nameof(prompts));
            Prerequisite = new ConcretePrerequisite(prompts, talkDomainFactory.CreateArtifacts());
            TalkChaptersInternal = chapters?.ToList() ?? [];
            TalkDomainFactory = talkDomainFactory ?? throw new ArgumentNullException(nameof(talkDomainFactory));
            CacheTalkChapters = new ReadOnlyCollection<ITalkChapter>(TalkChaptersInternal);
            TestimonyAdmin = CreateTestimonyAdmin();
        }

        /// <summary>
        /// Gets a value indicating whether the testimony is meaningful.
        /// </summary>
        bool ITestimonySummary.IsMeaningful => IsMeaningful;

        /// <summary>
        /// Gets the prerequisite associated with the testimony.
        /// </summary>
        IReadOnlyPrerequisite ITestimonySummary.Prerequisite => Prerequisite;

        /// <summary>
        /// Adds a testimony entry based on the provided exception and optional context.
        /// </summary>
        /// <remarks>This method records the given exception along with optional contextual information,
        /// such as a chapter or prompt, to assist in tracking or debugging issues. Ensure that <paramref name="ex"/> is
        /// not <see langword="null"/> when calling this method.</remarks>
        /// <param name="ex">The exception to be recorded as testimony. Cannot be <see langword="null"/>.</param>
        /// <param name="chapter">An optional chapter providing additional context for the testimony. Can be <see langword="null"/>.</param>
        /// <param name="prompt">An optional prompt associated with the testimony. Can be <see langword="null"/>.</param>
        protected void AddTestimony(Exception ex, ITalkChapter? chapter = null, TPrompt? prompt = null)
            => TestimonyAdmin.Add(ex, chapter, prompt);

        /// <summary>
        /// Gets a read-only list of exceptions representing the general testimony.
        /// </summary>
        IReadOnlyList<Exception> ITestimonySummary.GeneralTestimony
        {
            get
            {
                var cached = CacheGeneralTestimony;
                if (cached is not null) return cached;
                return CacheGeneralTestimony ??= TestimonyAdmin.SnapshotGeneral();
            }
        }

        /// <summary>
        /// Gets a read-only list of all testimonies, including their associated chapters and prompts.
        /// </summary>
        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> ITestimonySummary.AllTestimony
        {
            get
            {
                var cached = CacheAllTestimony;
                if (cached is not null) return cached;
                return CacheAllTestimony ??= TestimonyAdmin.SnapshotAll();
            }
        }

        /// <summary>
        /// Gets a read-only dictionary that maps each chapter and prompt pair to a read-only list of exceptions.
        /// </summary>
        /// <remarks>This property provides a snapshot of the testimony data for each chapter and prompt
        /// combination.  The returned dictionary is thread-safe and reflects the state of the testimony at the time it
        /// was accessed.</remarks>
        IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> ITestimonySummary.TestimonyForEachChapterAndPrompt
        {
            get
            {
                var cached = CacheTestimonyForEachChapterAndPrompt;
                if (cached is not null) return cached;
                return CacheTestimonyForEachChapterAndPrompt ??= TestimonyAdmin.SnapshotByChapterPrompt();
            }
        }

        /// <summary>
        /// Gets a read-only dictionary that maps each prompt to a read-only list of testimonies associated with that
        /// prompt.
        /// </summary>
        /// <remarks>The returned dictionary provides a snapshot of the current state of testimonies for
        /// each prompt.  Changes to the underlying data will not be reflected in the returned dictionary.</remarks>
        IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> ITestimonySummary.TestimonyForEachPrompt
        {
            get
            {
                var cached = CacheTestimonyForEachPrompt;
                if (cached is not null) return cached;
                return CacheTestimonyForEachPrompt ??= TestimonyAdmin.SnapshotByPrompt();
            }
        }

        /// <summary>
        /// Gets a read-only dictionary that maps each chapter to its associated list of testimonies with prompts.
        /// </summary>
        /// <remarks>This property provides a snapshot of the testimony data for each chapter. The
        /// returned dictionary and its contents are immutable, ensuring thread safety and consistency when
        /// accessed.</remarks>
        IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> ITestimonySummary.TestimonyForEachChapter
        {
            get
            {
                var cached = CacheTestimonyForEachChapter;
                if (cached is not null) return cached;
                return CacheTestimonyForEachChapter ??= TestimonyAdmin.SnapshotByChapter();
            }
        }

        /// <summary>
        /// Read-only view of chapters in execution order.
        /// </summary>
        /// <remarks>
        /// The chapter list is immutable after construction. To ensure stable enumeration, a read-only snapshot is returned.
        /// </remarks>
        IReadOnlyList<ITalkChapter> ITalkOutline.TalkChapters => CacheTalkChapters;

        /// <summary>
        /// Provides a testimony based on the current context of the implementation.
        /// </summary>
        /// <returns>An <see cref="ITestimony"/> instance representing the testimony generated by the implementation.</returns>
        ITestimonySummary ITalkOutline.TalkAbout() => TalkAbout();

        /// <summary>
        /// Asynchronously retrieves a testimony based on the current talk outline.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  <see cref="ITestimony"/>
        /// instance representing the testimony.</returns>
        Task<ITestimonySummary> ITalkOutline.TalkAboutAsync() => TalkAboutAsync();

        // ===== Prerequisite =====

        /// <summary>
        /// Represents a concrete implementation of the <see cref="IPrerequisite"/> interface,  providing internal
        /// storage and management of prompts and artifacts.
        /// </summary>
        /// <remarks>This class is intended for internal use and provides the necessary functionality  to
        /// manage prompts and artifacts as part of the prerequisite system. It enforces  constraints such as ensuring
        /// prompts are set only once and provides access to  artifacts in both mutable and immutable forms.</remarks>
        protected class ConcretePrerequisite(IReadOnlyList<TPrompt> prompts, TArtifacts artifactsInternal) : IPrerequisite
        {
            /// <summary>
            /// Gets the collection of prompts used internally by the system.
            /// </summary>
            public IReadOnlyList<TPrompt> Prompts { get; } = prompts ?? throw new ArgumentNullException(nameof(prompts));

            /// <summary>
            /// Gets the internal collection of artifacts.
            /// </summary>
            /// <remarks>This property is intended for internal use only and provides access to the
            /// underlying collection of artifacts. It is not recommended for external consumers to rely on this
            /// property.</remarks>
            public TArtifacts Artifacts { get; } = artifactsInternal ?? throw new ArgumentNullException(nameof(artifactsInternal));

            /// <summary>
            /// Gets the collection of prompts associated with the prerequisite.
            /// </summary>
            IReadOnlyList<TPrompt> IReadOnlyPrerequisite.Prompts
                => Prompts ?? throw new InvalidOperationException("The prompts have not yet been set internally.");

            /// <summary>
            /// Gets the collection of artifacts associated with the prerequisite.
            /// </summary>
            TArtifacts IPrerequisite.Artifacts => Artifacts;

            /// <summary>
            /// Gets the collection of artifacts associated with the prerequisite.
            /// </summary>
            TReadOnlyArtifacts IReadOnlyPrerequisite.Artifacts => Artifacts;
        }

        /// <summary>
        /// Gets the prerequisite required for the operation.
        /// </summary>
        protected ConcretePrerequisite Prerequisite { get; }

        /// <summary>
        /// Gets a value indicating whether the current state is considered meaningful.
        /// </summary>
        protected virtual bool IsMeaningful => TestimonyAdmin.TotalTestimonyCount > 0;

        /// <summary>
        /// Creates an instance of a testimony administration object.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to provide a custom
        /// implementation  of the <see cref="ITestimonyAdmin"/> interface. By default, it creates a new instance of 
        /// <see cref="TestimonyAdmin{TPrompt, TReadOnlyArtifacts, TArtifacts}"/> using the provided  domain factory and
        /// cache invalidation delegate.</remarks>
        /// <returns>An instance of <see cref="ITestimonyAdmin"/> for managing testimony-related operations.</returns>
        protected virtual ITestimonyAdmin CreateTestimonyAdmin()
            => new TestimonyAdmin<TPrompt, TReadOnlyArtifacts, TArtifacts>(
                TalkDomainFactory,
                InvalidateCaches
            );

        /// <summary>
        /// Gets the instance of <see cref="ITestimonyAdmin"/> used to manage testimony-related operations.
        /// </summary>
        protected ITestimonyAdmin TestimonyAdmin { get; }

        /// <summary>
        /// Gets the collection of talk chapters associated with the current instance.
        /// </summary>
        /// <remarks>This property provides access to the internal list of talk chapters. It is intended
        /// for use within the class or derived classes.</remarks>
        private List<ITalkChapter> TalkChaptersInternal { get; }

        /// <summary>
        /// Gets the factory responsible for creating instances of domain objects related to "Talk".
        /// </summary>
        private ITalkDomainFactory TalkDomainFactory { get; }

        /// <summary>
        /// Extension point to implement the session's synchronous preparation/execution logic.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>Exceptions should be recorded via <see cref="AddTestimony(Exception, ITalkChapter?, TPrompt?)"/>; continue processing as appropriate.</description></item>
        /// <item><description>For long-running or I/O-bound work, prefer <see cref="PrepareForTalkAsync"/>.</description></item>
        /// <item><description>Thread-safety: this method itself is intended to run on a single thread; internal testimony stores are protected by locks.</description></item>
        /// <item><description>Cancellation: check the <c>CancellationToken</c> carried by the prompt(s) and either throw or return early when requested.</description></item>
        /// </list>
        /// The base implementation does nothing.
        /// </remarks>
        protected virtual void PrepareForTalk() { }

        /// <summary>
        /// Prepares for and returns a testimony about the current instance.
        /// </summary>
        /// <returns>An <see cref="ITestimony"/> instance representing the testimony of the current object.</returns>
        protected ITestimonySummary TalkAbout()
        {
            PrepareForTalk();
            return this;
        }

        /// <summary>
        /// Asynchronous counterpart for preparation/execution. Use this for I/O-bound logic.
        /// </summary>
        /// <returns>A completed <see cref="Task"/>.</returns>
        /// <remarks>
        /// The base implementation completes immediately. Even if you currently implement only the synchronous path,
        /// prefer implementing this method to ease future migration.
        /// </remarks>
        protected abstract Task PrepareForTalkAsync();

        /// <summary>
        /// Prepares for and initiates a talk asynchronously, returning the current testimony instance.
        /// </summary>
        /// <remarks>This method performs any necessary preparation before initiating the talk.  It
        /// returns the current instance to allow for fluent usage or further processing.</remarks>
        /// <returns>An instance of <see cref="ITestimony"/> representing the current testimony.</returns>
        protected async Task<ITestimonySummary> TalkAboutAsync()
        {
            await PrepareForTalkAsync();
            return this;
        }


        /// <summary>
        /// Represents a chapter in a talk or presentation, identified by its name.
        /// </summary>
        /// <remarks>This class is used to encapsulate the concept of a chapter within a talk, providing a
        /// name to identify the chapter. It implements the <see cref="ITalkChapter"/> interface.</remarks>
        /// <param name="name"></param>
        protected class TalkChapter(string name) : ITalkChapter
        {
            public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
            public override string ToString() => Name;
        }


        // ===== Snapshot Cache =====

        /// <summary>
        /// Represents a cached collection of general testimony exceptions.
        /// </summary>
        /// <remarks>This property holds a read-only list of exceptions that may be used for caching
        /// purposes. The value can be null if no exceptions are currently cached.</remarks>
        private IReadOnlyList<Exception>? CacheGeneralTestimony { get; set; }

        /// <summary>
        /// Represents a cached collection of testimony objects, each associated with a chapter and a prompt.
        /// </summary>
        /// <remarks>This field is intended to store a read-only list of testimony objects for efficient
        /// reuse.  The value may be <see langword="null"/> if the cache has not been initialized or
        /// populated.</remarks>
        private IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>>? CacheAllTestimony { get; set; }

        /// <summary>
        /// Represents a cache that maps a combination of a chapter and a prompt to a list of exceptions.
        /// </summary>
        /// <remarks>This dictionary is intended to store precomputed or cached data for each chapter and
        /// prompt pair, where the key is a tuple consisting of an <see cref="ITalkChapter"/> and a <typeparamref
        /// name="TPrompt"/>, and the value is a read-only list of exceptions associated with that pair.</remarks>
        private IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>>? CacheTestimonyForEachChapterAndPrompt { get; set; }

        /// <summary>
        /// Represents a cached mapping of prompts to their associated testimonies with chapters.
        /// </summary>
        /// <remarks>This dictionary provides a read-only view of the cached data, where each key is a
        /// prompt and the corresponding value is a read-only list of testimonies associated with that prompt. The cache
        /// may be null if no data has been initialized or stored.</remarks>
        private IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>>? CacheTestimonyForEachPrompt { get; set; }

        /// <summary>
        /// Represents a cached mapping of chapters to their associated testimonies with prompts.
        /// </summary>
        /// <remarks>The dictionary keys represent chapters, while the values are read-only lists of
        /// testimonies associated with each chapter. This cache may be null if no data has been initialized or
        /// loaded.</remarks>
        private IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>>? CacheTestimonyForEachChapter { get; set; }

        /// <summary>
        /// Represents a cached collection of talk chapters.
        /// </summary>
        /// <remarks>This field is used to store a read-only list of talk chapters, which may be null if
        /// the cache has not been initialized.</remarks>
        private IReadOnlyList<ITalkChapter> CacheTalkChapters { get; set; }

        /// <summary>
        /// Invalidates all cached testimony data, resetting the caches to their initial state.
        /// </summary>
        /// <remarks>This method clears various internal caches related to testimony data.  It should be
        /// called when the cached data becomes outdated or needs to be refreshed.</remarks>
        private void InvalidateCaches()
        {
            // Keep the chapter list cache as it is assumed to be fixed.
            CacheGeneralTestimony = null;
            CacheAllTestimony = null;
            CacheTestimonyForEachChapterAndPrompt = null;
            CacheTestimonyForEachPrompt = null;
            CacheTestimonyForEachChapter = null;
        }
    }
}
