using Boostable.WhatAgentsTalkAbout.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Boostable.WhatAgentsTalkAbout.Core
{
    /// <summary>
    /// Provides administrative functionality for managing testimonies within a talk session.
    /// </summary>
    /// <remarks>This class is responsible for organizing, storing, and managing testimonies (e.g.,
    /// exceptions) associated with specific chapters and prompts in a talk session.  It ensures thread-safe access to
    /// internal collections and provides mechanisms for invalidating cached data when updates occur.
    /// Locking policy:
    /// - This implementation deliberately avoids holding more than one lock at a time.
    /// - This removes the risk of deadlocks without requiring a strict lock acquisition order.
    /// - In exchange, strong global consistency across multiple collections is NOT guaranteed.
    ///   Consumers may observe transient partial updates when reading from multiple collections.
    /// </remarks>
    /// <typeparam name="TPrompt">The type representing a prompt for talking, which must implement <see cref="IPromptForTalking{TPrompt}"/>.</typeparam>
    /// <typeparam name="TReadOnlyArtifacts">The type representing read-only artifacts, which must implement <see cref="IReadOnlyArtifacts"/>.</typeparam>
    /// <typeparam name="TArtifacts">The type representing artifacts, which must implement both <see cref="IArtifacts"/> and <typeparamref
    /// name="TReadOnlyArtifacts"/>.</typeparam>
    /// <param name="talkDomainFactory"></param>
    /// <param name="actionToInvalidateCache"></param>
    public class TestimonyAdmin<TPrompt, TReadOnlyArtifacts, TArtifacts>(TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITalkDomainFactory talkDomainFactory, Action actionToInvalidateCache)
        : TalkOutlineAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>,
        TalkOutlineAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITestimonyAdmin
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, IArtifacts, TReadOnlyArtifacts
    {
        /// <summary>
        /// Gets the factory responsible for creating instances of talk domain objects.
        /// </summary>
        private ITalkDomainFactory TalkDomainFactory { get; } = talkDomainFactory ?? throw new ArgumentNullException(nameof(talkDomainFactory));

        /// <summary>
        /// Gets the action that invalidates the cache.
        /// </summary>
        private Action ActionToInvalidateCache { get; } = actionToInvalidateCache ?? throw new ArgumentNullException(nameof(actionToInvalidateCache));

        /// <summary>
        /// Represents the total count of all testimonies.
        /// </summary>
        /// <remarks>This field is intended for internal use only and should be accessed using thread-safe
        /// operations. Use <see cref="System.Threading.Interlocked.Increment(ref int)"/> to increment the value and 
        /// <see cref="System.Threading.Volatile.Read(ref int)"/> to read it safely in a multithreaded
        /// environment.</remarks>
        private int _allTestimonyCount; // use Interlocked.Increment to increment, volatile.Read to read

        /// <summary>
        /// Gets the collection of general exceptions encountered during the operation.
        /// </summary>
        private List<Exception> GeneralTestimony { get; } = [];

        /// <summary>
        /// Gets the synchronization lock object used to ensure thread-safe access to shared resources.
        /// </summary>
        private object SyncLockGeneralTestimony { get; } = new object();

        /// <summary>
        /// Gets the collection of all testimonies, including their associated chapters and prompts.
        /// </summary>
        private List<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; } = [];

        /// <summary>
        /// Gets the synchronization lock object used to coordinate access to shared resources related to testimony
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
        /// Adds a testimony to the specified chapter and associates it with an optional prompt.
        /// </summary>
        /// <param name="testimony">The exception instance representing the testimony to be added. Cannot be <see langword="null"/>.</param>
        /// <param name="chapter">The chapter to which the testimony will be added. Can be <see langword="null"/> if no specific chapter is
        /// associated.</param>
        /// <param name="prompt">An optional prompt associated with the testimony. Can be <see langword="null"/>.</param>
        void ITestimonyAdmin.Add(Exception testimony, ITalkChapter? chapter, TPrompt? prompt)
            => Add(testimony, chapter, prompt);

        /// <summary>
        /// Gets the total count of all testimonies.
        /// </summary>
        int ITestimonyAdmin.TotalTestimonyCount => Volatile.Read(ref _allTestimonyCount);

        /// <summary>
        /// Retrieves a read-only snapshot of the current collection of general testimony exceptions.
        /// </summary>
        /// <remarks>This method is thread-safe and ensures that the returned snapshot reflects the state
        /// of the collection at the time of invocation. Subsequent modifications to the underlying collection will not
        /// affect the returned snapshot.</remarks>
        /// <returns>A read-only list of <see cref="Exception"/> objects representing the current state of general testimony. The
        /// list will be empty if no exceptions are present.</returns>
        IReadOnlyList<Exception> ITestimonyAdmin.SnapshotGeneral()
        {
            lock (SyncLockGeneralTestimony)
            {
                return GeneralTestimony.AsReadOnly();
            }
        }

        /// <summary>
        /// Creates a snapshot of all testimonies, including their chapters and prompts, at the current state.
        /// </summary>
        /// <remarks>The returned list represents a snapshot of the current state of all testimonies. 
        /// Changes made to the testimonies after this method is called will not be reflected in the snapshot.</remarks>
        /// <returns>A read-only list of testimonies, each containing its associated chapters and prompts.</returns>
        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> ITestimonyAdmin.SnapshotAll()
        {
            lock (SyncLockAllTestimony)
            {
                return AllTestimony.AsReadOnly();
            }
        }

        /// <summary>
        /// Creates an immutable snapshot of a dictionary of lists, ensuring thread safety.
        /// </summary>
        /// <remarks>This method locks on the provided <paramref name="gate"/> to ensure thread-safe
        /// access  to the source dictionary. Each list in the source dictionary is converted to an immutable  list. If
        /// a list is already an <see cref="ImmutableArray{T}"/>, it is reused directly.</remarks>
        /// <typeparam name="TKey">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
        /// <typeparam name="TItem">The type of the items in the lists.</typeparam>
        /// <param name="source">The source dictionary containing lists to snapshot. Cannot be null.</param>
        /// <param name="gate">An object used to synchronize access to the source dictionary. Cannot be null.</param>
        /// <returns>An immutable dictionary where each key maps to an immutable list of items.  The returned dictionary
        /// preserves the key comparer of the source dictionary.</returns>
        private static IReadOnlyDictionary<TKey, IReadOnlyList<TItem>> SnapshotDictOfList<TKey, TItem>(
            Dictionary<TKey, IReadOnlyList<TItem>> source,
            object gate)
            where TKey : notnull
        {
            lock (gate)
            {
                var builder = ImmutableDictionary.CreateBuilder<TKey, IReadOnlyList<TItem>>(source.Comparer);

                foreach (var kv in source)
                {
                    // If the value is already an ImmutableArray, use it directly; otherwise, create a new ImmutableArray.
                    var ro = kv.Value is ImmutableArray<TItem> ia
                        ? (IReadOnlyList<TItem>)ia
                        : [.. kv.Value];

                    builder.Add(kv.Key, ro);
                }

                return builder.ToImmutable();
            }
        }

        /// <summary>
        /// Creates a snapshot of testimony data grouped by chapter and prompt.
        /// </summary>
        /// <remarks>This method returns a dictionary where each key is a tuple consisting of a chapter
        /// and a prompt,  and the corresponding value is a read-only list of exceptions associated with that chapter
        /// and prompt.</remarks>
        /// <returns>A read-only dictionary mapping each chapter and prompt pair to a read-only list of exceptions.</returns>
        IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>>
            ITestimonyAdmin.SnapshotByChapterPrompt()
            => SnapshotDictOfList(TestimonyForEachChapterAndPrompt, SyncLockTestimonyForEachChapterAndPrompt);

        /// <summary>
        /// Retrieves a snapshot of testimonies grouped by their associated prompts.
        /// </summary>
        /// <remarks>The returned snapshot represents the current state of testimonies grouped by prompts 
        /// at the time of the method call. The dictionary and its contents are immutable, ensuring  thread-safe access
        /// to the data.</remarks>
        /// <returns>A read-only dictionary where each key is a prompt of type <typeparamref name="TPrompt"/>,  and the
        /// corresponding value is a read-only list of testimonies with chapters  of type <see
        /// cref="ITestimonyWithChapter"/>.</returns>
        IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>>
            ITestimonyAdmin.SnapshotByPrompt()
            => SnapshotDictOfList(TestimonyForEachPrompt, SyncLockTestimonyForEachPrompt);

        /// <summary>
        /// Retrieves a snapshot of testimonies grouped by chapter.
        /// </summary>
        /// <remarks>The returned snapshot reflects the current state of testimonies at the time of the
        /// call.  Changes to the underlying data after the method is called will not be reflected in the
        /// snapshot.</remarks>
        /// <returns>A read-only dictionary where the keys represent chapters and the values are read-only lists of testimonies
        /// associated with each chapter.</returns>
        IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>>
            ITestimonyAdmin.SnapshotByChapter()
            => SnapshotDictOfList(TestimonyForEachChapter, SyncLockTestimonyForEachChapter);

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
        internal void Add<TKey, TValue>(
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
        protected virtual void Add(
            Exception testimony,
            ITalkChapter? chapter = null,
            TPrompt? prompt = null
        )
        {
            if (testimony == null) throw new ArgumentNullException(nameof(testimony));

            // Add to AllTestimony (1 item = 1 count)
            lock (SyncLockAllTestimony)
            {
                AllTestimony.Add(TalkDomainFactory.CreateTestimony(chapter, prompt, testimony));
                Interlocked.Increment(ref _allTestimonyCount); // Increment the count atomically
            }

            if (chapter is null && prompt is null)
            {
                lock (SyncLockGeneralTestimony)
                {
                    GeneralTestimony.Add(testimony);
                }
                ActionToInvalidateCache();
                return;
            }

            if (chapter is not null && prompt is not null)
            {
                Add((chapter, prompt), testimony, TestimonyForEachChapterAndPrompt, SyncLockTestimonyForEachChapterAndPrompt);
            }
            if (prompt is not null)
            {
                Add(prompt, TalkDomainFactory.CreateTestimony(chapter, testimony), TestimonyForEachPrompt, SyncLockTestimonyForEachPrompt);
            }
            if (chapter is not null)
            {
                Add(chapter, TalkDomainFactory.CreateTestimony(prompt, testimony), TestimonyForEachChapter, SyncLockTestimonyForEachChapter);
            }

            ActionToInvalidateCache(); // Invalidate all caches after adding testimony
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
            /// <summary>
            /// Gets the singleton instance of the <see cref="ChapterPromptReferenceComparer"/> class.
            /// </summary>
            public static ChapterPromptReferenceComparer Instance { get; } = new();

            /// <summary>
            /// Provides a comparer for chapter prompt references. This constructor is private to prevent instantiation
            /// of the class.
            /// </summary>
            /// <remarks>This class is likely intended to be used as a singleton or through static
            /// members, as it cannot be instantiated directly.</remarks>
            private ChapterPromptReferenceComparer() { }

            /// <summary>
            /// Determines whether two tuples, each containing an <see cref="ITalkChapter"/> and a <typeparamref
            /// name="TPrompt"/>, are equal.
            /// </summary>
            /// <param name="x">The first tuple to compare, containing an <see cref="ITalkChapter"/> and a <typeparamref
            /// name="TPrompt"/>.</param>
            /// <param name="y">The second tuple to compare, containing an <see cref="ITalkChapter"/> and a <typeparamref
            /// name="TPrompt"/>.</param>
            /// <returns><see langword="true"/> if both tuples have the same <see cref="ITalkChapter"/> and <typeparamref
            /// name="TPrompt"/> instances; otherwise, <see langword="false"/>.</returns>
            public bool Equals((ITalkChapter, TPrompt) x, (ITalkChapter, TPrompt) y)
                => ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

            /// <summary>
            /// Generates a hash code for the specified tuple containing an <see cref="ITalkChapter"/> and a
            /// <typeparamref name="TPrompt"/>.
            /// </summary>
            /// <remarks>The hash code is computed based on the individual hash codes of the tuple's
            /// items. If either item is <see langword="null"/>, its hash code is treated as 0.</remarks>
            /// <param name="obj">The tuple for which to generate the hash code. The first item represents an <see cref="ITalkChapter"/>,
            /// and the second item represents a <typeparamref name="TPrompt"/>.</param>
            /// <returns>An integer hash code that uniquely represents the specified tuple.</returns>
            public int GetHashCode((ITalkChapter, TPrompt) obj)
            {
                int h1 = obj.Item1 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item1);
                int h2 = obj.Item2 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item2);
                return (h1 << 5 | h1 >> 27) ^ h2;
            }
        }

    }
}
