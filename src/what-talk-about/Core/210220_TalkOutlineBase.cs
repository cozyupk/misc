using Boostable.WhatTalkAbout.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Boostable.WhatTalkAbout.Core
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
    /// <typeparamref name="TReadOnlyArtifacts"/>, and must have a parameterless constructor.</typeparam>
    public class TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITestimonySummary
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITalkOutline
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, IArtifacts, TReadOnlyArtifacts, new()
    {
        /// <summary>
        /// Gets a value indicating whether the testimony is meaningful.
        /// </summary>
        bool ITestimonySummary.IsMeaningful => IsMeaningful;

        /// <summary>
        /// Gets the prerequisite associated with the testimony.
        /// </summary>
        IReadOnlyPrerequisite ITestimonySummary.Prerequisite => Prerequisite;

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
        private IReadOnlyList<ITalkChapter>? CacheTalkChapters { get; set; }

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

        /// <summary>
        /// Creates a snapshot of the specified read-only list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">The source list to create a snapshot from. Must not be <see langword="null"/>.</param>
        /// <returns>A new read-only list containing the elements of the source list at the time of the call. Changes to the
        /// original list after the snapshot is created will not affect the returned list.</returns>
        private static IReadOnlyList<T> SnapshotList<T>(IReadOnlyList<T> source)
            => source is List<T> list ? new List<T>(list)
                                      : new List<T>(source);

        /// <summary>
        /// Creates a deep snapshot of a dictionary where the values are read-only lists.
        /// </summary>
        /// <remarks>This method creates a deep copy of the dictionary and its lists, ensuring that
        /// modifications to the returned dictionary or its lists do not affect the original dictionary or its
        /// lists.</remarks>
        /// <typeparam name="TKey">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
        /// <typeparam name="TVal">The type of the elements in the lists that are the values of the dictionary.</typeparam>
        /// <param name="source">The source dictionary to snapshot. Cannot be null.</param>
        /// <returns>A new dictionary containing the same keys as the source dictionary, where each value is a new list
        /// containing the elements of the corresponding list in the source dictionary.</returns>
        private static IReadOnlyDictionary<TKey, IReadOnlyList<TVal>> SnapshotDictOfList<TKey, TVal>(
            Dictionary<TKey, IReadOnlyList<TVal>> source)
            where TKey : notnull
        {
            var copy = new Dictionary<TKey, IReadOnlyList<TVal>>(source.Count, source.Comparer);
            foreach (var kv in source)
            {
                var v = kv.Value is List<TVal> l ? new List<TVal>(l) : new List<TVal>(kv.Value);
                copy[kv.Key] = v;
            }
            return copy;
        }

        /// <summary>
        /// Gets a read-only list of exceptions representing the general testimony.
        /// </summary>
        IReadOnlyList<Exception> ITestimonySummary.GeneralTestimony
        {
            get
            {
                var cached = CacheGeneralTestimony;
                if (cached is not null) return cached;
                lock (SyncLockGeneralTestimony)
                    return CacheGeneralTestimony ??= SnapshotList(GeneralTestimony);
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
                lock (SyncLockAllTestimony)
                    return CacheAllTestimony ??= SnapshotList(AllTestimony);
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
                lock (SyncLockTestimonyForEachChapterAndPrompt)
                    return CacheTestimonyForEachChapterAndPrompt ??= SnapshotDictOfList(TestimonyForEachChapterAndPrompt);
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
                lock (SyncLockTestimonyForEachPrompt)
                    return CacheTestimonyForEachPrompt ??= SnapshotDictOfList(TestimonyForEachPrompt);
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
                lock (SyncLockTestimonyForEachChapter)
                    return CacheTestimonyForEachChapter ??= SnapshotDictOfList(TestimonyForEachChapter);
            }
        }

        /// <summary>
        /// Read-only view of chapters in execution order.
        /// </summary>
        /// <remarks>
        /// The chapter list is immutable after construction. To ensure stable enumeration, a read-only snapshot is returned.
        /// </remarks>
        IReadOnlyList<ITalkChapter> ITalkOutline.TalkChapters
            => CacheTalkChapters ??= new ReadOnlyCollection<ITalkChapter>(TalkChaptersInternal);

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
        protected class ConcretePrerequisite : IPrerequisite
        {
            /// <summary>
            /// Gets or sets the internal collection of prompts.
            /// </summary>
            /// <remarks>This property is intended for internal use and may not be suitable for direct
            /// access in external code.</remarks>
            public List<TPrompt>? PromptsInternal { get; set; }

            /// <summary>
            /// Gets the internal collection of artifacts.
            /// </summary>
            /// <remarks>This property is intended for internal use only and provides access to the
            /// underlying collection of artifacts. It is not recommended for external consumers to rely on this
            /// property.</remarks>
            public TArtifacts ArtifactsInternal { get; } = new();

            /// <summary>
            /// Gets the collection of prompts associated with the prerequisite.
            /// </summary>
            IReadOnlyList<TPrompt> IReadOnlyPrerequisite.Prompts
                => PromptsInternal as IReadOnlyList<TPrompt> ?? throw new InvalidOperationException("The prompts have not yet been set internally.");

            /// <summary>
            /// Gets the collection of artifacts associated with the prerequisite.
            /// </summary>
            TArtifacts IPrerequisite.Artifacts => ArtifactsInternal;

            /// <summary>
            /// Gets the collection of artifacts associated with the prerequisite.
            /// </summary>
            TReadOnlyArtifacts IReadOnlyPrerequisite.Artifacts => ArtifactsInternal;

            /// <summary>
            /// Sets the prompt collection used by the session (may be set exactly once).
            /// </summary>
            /// <param name="prompts">Prompts to use. Must not be null.</param>
            /// <exception cref="ArgumentNullException"><paramref name="prompts"/> is null.</exception>
            /// <exception cref="InvalidOperationException">Already set once.</exception>
            /// <remarks>
            /// Ownership: the sequence may be copied into an internal <c>List&lt;TPrompt&gt;</c>.
            /// Modifying <paramref name="prompts"/> after this call does not affect this instance.
            /// </remarks>
            void IPrerequisite.SetPrompt(IReadOnlyList<TPrompt> prompts)
            {
                if (prompts is null) throw new ArgumentNullException(nameof(prompts));
                if (PromptsInternal is not null && PromptsInternal.Count > 0)
                    throw new InvalidOperationException("Prompts have already been set. Cannot set them again.");
                PromptsInternal = prompts as List<TPrompt> ?? [.. prompts];
            }
        }

        /// <summary>
        /// Gets the prerequisite required for the operation.
        /// </summary>
        protected IPrerequisite Prerequisite { get; } = new ConcretePrerequisite();

        // ===== Thread-safe IsMeaningful (atomic counter) =====

        /// <summary>
        /// Represents the total count of all testimonies processed.
        /// </summary>
        /// <remarks>This field is incremented atomically using <see cref="System.Threading.Interlocked"/>
        /// to ensure thread safety. Reads are performed using <see cref="System.Threading.Volatile.Read"/> to guarantee
        /// visibility across threads.</remarks>
        private int _allTestimonyCount; // Interlocked で増分、読み取りは Volatile.Read

        /// <summary>
        /// Gets a value indicating whether the current state is considered meaningful.
        /// </summary>
        protected virtual bool IsMeaningful => Volatile.Read(ref _allTestimonyCount) > 0;

        // Internal storage for testimonies and chapters

        /// <summary>
        /// Gets the collection of general testimonies.
        /// </summary>
        private List<Exception> GeneralTestimony { get; } = [];

        /// <summary>
        /// Gets the synchronization lock object used to ensure thread-safe access to shared resources related to
        /// general testimony operations.
        /// </summary>
        private object SyncLockGeneralTestimony { get; } = new object();

        /// <summary>
        /// Gets the collection of all testimonies, including their associated chapters and prompts.
        /// </summary>
        private List<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; } = [];

        /// <summary>
        /// Gets the synchronization lock object used to coordinate access to shared resources  related to testimony
        /// operations.
        /// </summary>
        private object SyncLockAllTestimony { get; } = new object();

        /// <summary>
        /// Gets a dictionary that maps each chapter and prompt pair to a read-only list of exceptions.
        /// </summary>
        private Dictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> TestimonyForEachChapterAndPrompt { get; } = new(ChapterPromptReferenceComparer.Instance);

        /// <summary>
        /// Gets the synchronization lock object used to ensure thread safety  when accessing or modifying testimony
        /// data for each chapter and prompt.
        /// </summary>
        private object SyncLockTestimonyForEachChapterAndPrompt { get; } = new object();

        /// <summary>
        /// Gets a dictionary that maps each prompt to a read-only list of testimonies associated with it.
        /// </summary>
        private Dictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> TestimonyForEachPrompt { get; } = [];

        /// <summary>
        /// Gets the synchronization lock object used to ensure thread safety for operations related to testimony for
        /// each prompt.
        /// </summary>
        private object SyncLockTestimonyForEachPrompt { get; } = new object();

        /// <summary>
        /// Gets a dictionary that maps each chapter to a read-only list of testimonies with prompts.
        /// </summary>
        private Dictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> TestimonyForEachChapter { get; } = [];

        /// <summary>
        /// Gets the synchronization lock object used to ensure thread-safe access to testimony data for each chapter.
        /// </summary>
        private object SyncLockTestimonyForEachChapter { get; } = new object();

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
            Prerequisite.SetPrompt(prompts);
            TalkChaptersInternal = chapters?.ToList() ?? [];
            TalkDomainFactory = talkDomainFactory ?? throw new ArgumentNullException(nameof(talkDomainFactory));
            CacheTalkChapters = new ReadOnlyCollection<ITalkChapter>(TalkChaptersInternal);
        }

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
        protected virtual Task PrepareForTalkAsync() => Task.CompletedTask;

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
        /// Records a testimony (typically an exception) at chapter/prompt granularity.
        /// </summary>
        /// <param name="testimony">Exception to record. Must not be null.</param>
        /// <param name="chapter">Associated chapter, or null for “general”.</param>
        /// <param name="prompt">Associated prompt, or null for chapter-only/general.</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>Thread-safety: internal stores are protected by locks.</description></item>
        /// <item><description>Cache: reading snapshots are invalidated after this call and will be rebuilt on next access.</description></item>
        /// <item><description>Aggregation rules: entries are grouped by (chapter,prompt), by prompt, by chapter, and as general (both null).</description></item>
        /// </list>
        /// </remarks>
        internal void AddTestimony<TKey, TValue>(
            TKey key,
            TValue value,
            Dictionary<TKey, IReadOnlyList<TValue>> dictionary,
            object syncLock)
        {
            lock (syncLock)
            {
                if (!dictionary.TryGetValue(key, out var existing))
                {
                    dictionary[key] = [value];
                }
                else if (existing is List<TValue> list)
                {
                    list.Add(value);
                }
                else
                {
                    var newList = new List<TValue>(existing) { value };
                    dictionary[key] = newList;
                }
            }
        }

        /// <summary>
        /// Adds a testimony to the appropriate collections based on the specified chapter and prompt.
        /// </summary>
        /// <remarks>This method organizes testimonies into various collections based on the provided
        /// chapter and prompt.  If both <paramref name="chapter"/> and <paramref name="prompt"/> are <see
        /// langword="null"/>, the testimony is added to the general testimony collection. If both are provided, the
        /// testimony is added to collections specific to the chapter and prompt combination. Additionally, the method
        /// ensures thread safety when modifying shared collections and invalidates caches after updates.</remarks>
        /// <param name="testimony">The exception instance representing the testimony to be added. Cannot be <see langword="null"/>.</param>
        /// <param name="chapter">The chapter associated with the testimony, or <see langword="null"/> if the testimony is general.</param>
        /// <param name="prompt">The prompt associated with the testimony, or <see langword="null"/> if the testimony is not tied to a
        /// specific prompt.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="testimony"/> is <see langword="null"/>.</exception>
        protected virtual void AddTestimony(
            Exception testimony,
            ITalkChapter? chapter = null,
            TPrompt? prompt = null
        )
        {
            if (testimony == null) throw new ArgumentNullException(nameof(testimony));

            // Add to AllTestimony (1 item = 1 count)
            lock (SyncLockAllTestimony)
            {
                AllTestimony.Add(TalkDomainFactory.Create(chapter, prompt, testimony));
                Interlocked.Increment(ref _allTestimonyCount); // Increment the count atomically
            }

            if (chapter is null && prompt is null)
            {
                lock (SyncLockGeneralTestimony)
                {
                    GeneralTestimony.Add(testimony);
                }
                InvalidateCaches();
                return;
            }

            if (chapter is not null && prompt is not null)
            {
                AddTestimony((chapter, prompt), testimony, TestimonyForEachChapterAndPrompt, SyncLockTestimonyForEachChapterAndPrompt);
            }
            if (prompt is not null)
            {
                AddTestimony(prompt, TalkDomainFactory.Create(chapter, testimony), TestimonyForEachPrompt, SyncLockTestimonyForEachPrompt);
            }
            if (chapter is not null)
            {
                AddTestimony(chapter, TalkDomainFactory.Create(prompt, testimony), TestimonyForEachChapter, SyncLockTestimonyForEachChapter);
            }

            InvalidateCaches(); // Invalidate all caches after adding testimony
        }

        /// <summary>
        /// Comparer for (chapter, prompt) based on <b>reference equality</b>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>Equality is determined by instance identity, not value equality (e.g., not record value comparison).</description></item>
        /// <item><description>When using records or other value-equality types as keys, be aware that this comparer intentionally ignores value equality.</description></item>
        /// <item><description>Uses <see cref="RuntimeHelpers.GetHashCode(object)"/> to compute a reference-based hash.</description></item>
        /// </list>
        /// </remarks>
        protected sealed class ChapterPromptReferenceComparer
            : IEqualityComparer<(ITalkChapter, TPrompt)>
        {
            public static ChapterPromptReferenceComparer Instance { get; } = new();
            private ChapterPromptReferenceComparer() { }

            public bool Equals((ITalkChapter, TPrompt) x, (ITalkChapter, TPrompt) y)
                => ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

            public int GetHashCode((ITalkChapter, TPrompt) obj)
            {
                int h1 = obj.Item1 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item1);
                int h2 = obj.Item2 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item2);
                return ((h1 << 5) | (h1 >> 27)) ^ h2;
            }
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
    }
}
