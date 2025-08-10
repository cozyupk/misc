
/****************
 * Abstractions *
 ****************/

namespace Boostable.WhatTalkAbout.Abstractions
{
    using System;
    using System.Threading;

    public interface IPromptForTalking
    {
        // Just a marker interface for the prompt.
    }

    public interface IPromptForTalking<out TSelf> : IPromptForTalking
        where TSelf : class, IPromptForTalking<TSelf>
    {
        string Label { get; }
        CancellationToken CancellationToken { get; }

        TSelf Clone(string label);
    }

    public interface ITalkChapter
    {
        public string Name { get; }
    }

    public interface ITestimonyWithChapterAndPrompt<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        ITalkChapter? Chapter { get; }
        TPrompt? Prompt { get; }
        Exception Testimony { get; }
    };

    public interface ITestimonyWithChapter
    {
        ITalkChapter? Chapter { get; }
        Exception Testimony { get; }
    };

    public interface ITestimonyWithPrompt<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        TPrompt? Prompt { get; }

        Exception Testimony { get; }
    };

    public interface IReadOnlyArtifacts
    {
        // For extension points.
    }

    public interface IArtifacts
    {
        // For extension points.
    }
}

/********
 * Base *
*********/

namespace Boostable.WhatTalkAbout.Base
{
    using Boostable.WhatTalkAbout.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class TalkSessionAbstractions
    {
        public interface ITestimonyBase
        {
            bool IsMeaningful { get; }
            IReadOnlyList<Exception> GeneralTestimony { get; }
        }

        public interface ITalkOutlineBase
        {
            IReadOnlyList<ITalkChapter> TalkChapters { get; }
        }
    }

    public abstract class TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts> : TalkSessionAbstractions
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts, new()
    {
        public interface IReadOnlyPrerequisite
        {
            IReadOnlyList<TPrompt> Prompts { get; }
            TReadOnlyArtifacts Artifacts { get; }
        }

        public interface IPrerequisite : IReadOnlyPrerequisite
        {
            new TArtifacts Artifacts { get; }

            void SetPrompt(IReadOnlyList<TPrompt> prompts);
        }

        public interface ITestimony : ITestimonyBase
        {
            IReadOnlyPrerequisite Prerequisite { get; }

            IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; }

            IReadOnlyDictionary<
                (ITalkChapter, TPrompt),
                IReadOnlyList<Exception>
            > TestimonyForEachChapterAndPrompt { get; }

            IReadOnlyDictionary<
                TPrompt,
                IReadOnlyList<ITestimonyWithChapter>
            > TestimonyForEachPrompt { get; }

            IReadOnlyDictionary<
                ITalkChapter,
                IReadOnlyList<ITestimonyWithPrompt<TPrompt>>
            > TestimonyForEachChapter { get; }
        }

        public interface ITalkOutline : ITalkOutlineBase
        {
            ITestimony TalkAbout();
            Task<ITestimony> TalkAboutAsync();
        }

    }

    public class TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITestimony
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITalkOutline
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, IArtifacts, TReadOnlyArtifacts, new()
    {
        bool ITestimonyBase.IsMeaningful => IsMeaningful;

        IReadOnlyPrerequisite ITestimony.Prerequisite => Prerequisite;

        IReadOnlyList<Exception> ITestimonyBase.GeneralTestimony
            => [.. GeneralTestimony];

        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> ITestimony.AllTestimony
            => [.. AllTestimony];

        IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> ITestimony.TestimonyForEachChapterAndPrompt
            => new Dictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>>(TestimonyForEachChapterAndPrompt);

        IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> ITestimony.TestimonyForEachPrompt
            => new Dictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>>(TestimonyForEachPrompt);

        IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> ITestimony.TestimonyForEachChapter
            => new Dictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>>(TestimonyForEachChapter);

        IReadOnlyList<ITalkChapter> ITalkOutlineBase.TalkChapters => TalkChaptersInternal;

        ITestimony ITalkOutline.TalkAbout()
            => TalkAbout();
        Task<ITestimony> ITalkOutline.TalkAboutAsync()
            => TalkAboutAsync();

        protected class ConcretePrerequisite : IPrerequisite
        {
            public List<TPrompt>? PromptsInternal { get; set; }
            public TArtifacts ArtifactsInternal { get; } = new();

            IReadOnlyList<TPrompt> IReadOnlyPrerequisite.Prompts
                => PromptsInternal as IReadOnlyList<TPrompt> ?? throw new InvalidOperationException("The prompt has not yet set internally.");

            TArtifacts IPrerequisite.Artifacts => ArtifactsInternal as TArtifacts;

            TReadOnlyArtifacts IReadOnlyPrerequisite.Artifacts => ArtifactsInternal as TReadOnlyArtifacts;

            void IPrerequisite.SetPrompt(IReadOnlyList<TPrompt> prompts)
            {
                if (prompts is null) throw new ArgumentNullException(nameof(prompts));
                if (PromptsInternal is not null && PromptsInternal.Count > 0)
                {
                    throw new InvalidOperationException("Prompts have already been set. Cannot set them again.");
                }
                PromptsInternal = prompts as List<TPrompt> ?? [.. prompts];
            }
        }
        protected IPrerequisite Prerequisite { get; } = new ConcretePrerequisite();
        protected virtual bool IsMeaningful => AllTestimony.Any();
        private List<Exception> GeneralTestimony { get; } = [];
        private object SyncLockGeneralTestimony { get; } = new object();
        private List<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; } = [];
        private object SyncLockAllTestimony { get; } = new object();
        private Dictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> TestimonyForEachChapterAndPrompt { get; } = new(ChapterPromptReferenceComparer.Instance);
        private object SyncLockTestimonyForEachChapterAndPrompt { get; } = new object();
        private Dictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> TestimonyForEachPrompt { get; } = [];
        private object SyncLockTestimonyForEachPrompt { get; } = new object();
        private Dictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> TestimonyForEachChapter { get; } = [];
        private object SyncLockTestimonyForEachChapter { get; } = new object();

        private List<ITalkChapter> TalkChaptersInternal { get; }

        protected internal TalkOutlineBase(
            IReadOnlyList<TPrompt> prompts,
            params ITalkChapter[] chapters
        )
        {
            _ = prompts ?? throw new ArgumentNullException(nameof(prompts));
            Prerequisite.SetPrompt(prompts);
            TalkChaptersInternal = chapters?.ToList() ?? [];
        }

        protected virtual void PrepareForTalk()
        {
            // This method can be overridden to prepare the talk session.
            // For example, it can be used to arrange codes or set up the environment.
        }

        protected ITestimony TalkAbout()
        {
            PrepareForTalk();
            return this;
        }

        protected virtual Task PrepareForTalkAsync()
        {
            // This method can be overridden to prepare the talk session asynchronously.
            // For example, it can be used to arrange codes or set up the environment.
            return Task.CompletedTask;
        }

        protected async Task<ITestimony> TalkAboutAsync()
        {
            await PrepareForTalkAsync();
            return this;
        }

        internal void AddTestimony<TKey, TValue>(TKey key, TValue value, Dictionary<TKey, IReadOnlyList<TValue>> dictionary, object syncLock)
        {
            lock (syncLock)
            {
                if (!dictionary.TryGetValue(key, out var existing))
                {
                    dictionary[key] = [value];
                    return;
                }

                if (existing is List<TValue> list)
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

        protected virtual void AddTestimony(
            Exception testimony,
            ITalkChapter? chapter = null,
            TPrompt? prompt = null
        ) {
            if (testimony == null) throw new ArgumentNullException(nameof(testimony));

            lock (SyncLockAllTestimony)
            {
                AllTestimony.Add(new TestimonyWithChapterAndPrompt(chapter, prompt, testimony));
            }

            if (chapter is null && prompt is null)
            {
                lock (SyncLockGeneralTestimony)
                {
                    GeneralTestimony.Add(testimony);
                }
                return;
            }

            if (chapter is not null && prompt is not null)
            {
                AddTestimony(
                    (chapter, prompt), testimony, 
                    TestimonyForEachChapterAndPrompt, SyncLockTestimonyForEachChapterAndPrompt
                );
            }

            if (prompt is not null)
            {
                AddTestimony(
                    prompt, new TestimonyWithChapter(chapter, testimony),
                    TestimonyForEachPrompt, SyncLockTestimonyForEachPrompt
                );
            }

            if (chapter is not null)
            {
                AddTestimony(
                    chapter, new TestimonyWithPrompt(prompt, testimony),
                    TestimonyForEachChapter, SyncLockTestimonyForEachChapter
                );
            }
        }

        protected record TestimonyWithChapterAndPrompt : ITestimonyWithChapterAndPrompt<TPrompt>
        {
            public TestimonyWithChapterAndPrompt(ITalkChapter? chapter, TPrompt? prompt, Exception testimony)
            {
                Chapter = chapter;
                Prompt = prompt;
                Testimony = testimony;
            }

            public ITalkChapter? Chapter { get; }
            public TPrompt? Prompt { get; }
            public Exception Testimony { get; }
        };


        protected record TestimonyWithChapter : ITestimonyWithChapter
        {
            public TestimonyWithChapter(ITalkChapter? chapter, Exception testimony)
            {
                Chapter = chapter;
                Testimony = testimony;
            }

            public ITalkChapter? Chapter { get; }
            public Exception Testimony { get; }
        };

        protected record TestimonyWithPrompt : ITestimonyWithPrompt<TPrompt>
        {
            public TestimonyWithPrompt(TPrompt? prompt, Exception testimony)
            {
                Prompt = prompt;
                Testimony = testimony;
            }

            public TPrompt? Prompt { get; }
            public Exception Testimony { get; }
        };

        protected sealed class ChapterPromptReferenceComparer
            : IEqualityComparer<(ITalkChapter, TPrompt)>
        {
            public static ChapterPromptReferenceComparer Instance { get; } = new();

            private ChapterPromptReferenceComparer() { }

            public bool Equals((ITalkChapter, TPrompt) x, (ITalkChapter, TPrompt) y)
            {
                return ReferenceEquals(x.Item1, y.Item1)
                    && ReferenceEquals(x.Item2, y.Item2);
            }

            public int GetHashCode((ITalkChapter, TPrompt) obj)
            {
                // Create a hash code based on the references of the chapter and prompt.
                int h1 = obj.Item1 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item1);
                int h2 = obj.Item2 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item2);
                return ((h1 << 5) | (h1 >> 27)) ^ h2;
            }
        }

        protected class TalkChapter(string name) : ITalkChapter
        {
            public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

            public override string ToString()
            {
                return Name;
            }
        }
    }

    public abstract class TalkSessionBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts, new()
    {
        protected ITalkOutline Outline { get; }

        private int _hasRun = 0;

        public TalkSessionBase(
            TPrompt basePrompt,
            Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null,
            Func<IReadOnlyList<TPrompt>, ITalkOutline>? outlineFactory = null
        )
        {
            promptVariationBuilder ??= basePrompt => DefaultVariationBuilder(basePrompt);
            var prompts = promptVariationBuilder(basePrompt);
            outlineFactory ??= DefaultOutlineFactory; // // Allow the factory to be overridden, e.g. for outline injection or behavior customization.
            Outline = outlineFactory(prompts);
        }

        protected internal void EnsureNotYetHasRun()
        {
            if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
            {
                throw new InvalidOperationException("This session has already been run. Let it run only once per session.");
            }
        }

        public virtual ITestimony TalkAbout()
        {
            return Outline.TalkAbout();
        }

        public virtual Task<ITestimony> TalkAboutAsync()
        {
            return Task.FromResult(Outline.TalkAbout());
        }

        protected internal virtual IReadOnlyList<TPrompt> DefaultVariationBuilder(TPrompt basePrompt)
        {
            // Default implementation that returns the base prompt as the only variation.
            return [basePrompt.Clone("Default Prompt (without promptVariationBuilder)")];
        }

        protected internal virtual TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITalkOutline DefaultOutlineFactory(
            IReadOnlyList<TPrompt> prompts
        ) {
            // Default implementation that creates a simple outline with the provided arrange code and prompts.
            return new TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>(prompts);
        }
    }
}

/****************
 * ArrangeCodes *
 ****************/
namespace Boostable.WhatRoslynTalkAbout.ArrangeCodes
{

    using Boostable.WhatTalkAbout.Abstractions;
    using Boostable.WhatTalkAbout.Base;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public readonly record struct VirtualSource

    {
        public string Path { get; }
        public string Code { get; }
        public VirtualSource(string path, string code)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }
    }

    public interface IArrangeCodePromptForTalking<out TSelf> : IPromptForTalking<TSelf>
        where TSelf : class, IArrangeCodePromptForTalking<TSelf>
    {
    }

    public interface IReadOnlyArrangeCodeArtifacts : IReadOnlyArtifacts
    {
        IReadOnlyList<VirtualSource>? TargetCodes { get; }
    }

    public interface IArrangeCodeArtifacts : IArtifacts, IReadOnlyArtifacts
    {
        void SetTargetCodes(IReadOnlyList<VirtualSource> targetCodes);
    }

    public class ArrangeCodeTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IArrangeCodePromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArrangeCodeArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArrangeCodeArtifacts, IArtifacts, new()
    {
        protected int _hasArranged = 0;

        private static ITalkChapter TalkChaper { get; } = new TalkChapter("Arranging Codes Chapter");
        protected Func<IEnumerable<VirtualSource>> FuncToArrangeCodes { get; }
        protected internal ArrangeCodeTalkOutlineBase(
            Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
            IReadOnlyList<TPrompt> prompts,
            params ITalkChapter[] chapters
        ) : base(prompts, [TalkChaper, .. chapters])
        {
            FuncToArrangeCodes = funcToArrangeCodes ?? throw new ArgumentNullException(nameof(funcToArrangeCodes));
        }

        protected internal virtual void ArrangeCodes()
        {
            try
            {
                Prerequisite.Artifacts.SetTargetCodes([.. FuncToArrangeCodes()]);
            }
            catch (Exception ex)
            {
                AddTestimony(
                    ex,
                    TalkChaper
                );
            }
        }

        protected override void PrepareForTalk()
        {
            if (Interlocked.CompareExchange(ref _hasArranged, 1, 0) != 0)
            {
                throw new InvalidOperationException("The codes have already been arranged. Let them arrange only once per a session.");
            }

            base.PrepareForTalk();
            ArrangeCodes();
        }

        protected override async Task PrepareForTalkAsync()
        {
            if (Interlocked.CompareExchange(ref _hasArranged, 1, 0) != 0)
            {
                throw new InvalidOperationException("The codes have already been arranged. Let them arrange only once per a session.");
            }

            await base.PrepareForTalkAsync();
            ArrangeCodes();
        }
    }
}


/*
public CSharpParseOptions ParseOptions { get; }
public CSharpCompilationOptions CompilationOptions { get; }
public Encoding EncodingForParse { get; set; }

        protected internal virtual void ParseCodes()
        {
            if (Interlocked.CompareExchange(ref _hasParsed, 1, 0) != 0)
            {
                throw new InvalidOperationException("The codes have already been parsed. Let them parse only once per a session.");
            }

            foreach (var prompt in Prerequisite.Prompts)
            {
                try
                {
                    ParseCodeInternal(Prerequisite.TargetCode, prompt);
                }
                catch (Exception ex)
                {
                    // TODO: Let the exception be into the IWhatRoslynTalkAbout instance.
                    Console.WriteLine($"An error occurred while compiling the code: {ex.Message}");
                    throw;
                }
            }
        }

        protected internal virtual Task ParseCodesAsync(CancellationToken cancellationToken)
{
    return Task.CompletedTask;
}
*/
