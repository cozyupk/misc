using Boostable.WhatTalkAbout.Abstractions;
using Boostable.WhatTalkAbout.Base;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/****************
 * Abstractions *
 ****************/

namespace Boostable.WhatTalkAbout.Abstractions
{
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

    public interface IReadOnlyPrerequisite<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        IReadOnlyList<TPrompt> Prompts { get; }
    }

    public interface IPrerequisite<TPrompt> : IReadOnlyPrerequisite<TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        new IReadOnlyList<TPrompt> Prompts { get; set; } // 変更はコレ経由
    }

    public interface ITestimony<TPrompt, out TReadOnlyPrerequisite>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IReadOnlyPrerequisite<TPrompt>
    {
        TReadOnlyPrerequisite Prerequisite { get; }

        bool IsMeaningful { get; }

        IReadOnlyList<Exception> GeneralTestimony { get; }

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

    public interface ITalkOutline<out TTalkAbout>
    {
        TTalkAbout TalkAbout();
    }
}

/********
 * Base *
*********/

namespace Boostable.WhatTalkAbout.Base {
    public class TalkOutlineBase<TPrompt, TReadOnlyPrerequisite, TPrerequisite>
        : ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>>, ITestimony<TPrompt, TReadOnlyPrerequisite>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IReadOnlyPrerequisite<TPrompt>
        where TPrerequisite : IPrerequisite<TPrompt>, TReadOnlyPrerequisite, new()
    {

        bool ITestimony<TPrompt, TReadOnlyPrerequisite>.IsMeaningful
            => IsMeaningful;

        IReadOnlyList<Exception> ITestimony<TPrompt, TReadOnlyPrerequisite>.GeneralTestimony
            => GeneralTestimony;

        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> ITestimony<TPrompt, TReadOnlyPrerequisite>.AllTestimony
            => AllTestimony;

        IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> ITestimony<TPrompt, TReadOnlyPrerequisite>.TestimonyForEachChapterAndPrompt
            => TestimonyForEachChapterAndPrompt;

        IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> ITestimony<TPrompt, TReadOnlyPrerequisite>.TestimonyForEachPrompt
            => TestimonyForEachPrompt;

        IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> ITestimony<TPrompt, TReadOnlyPrerequisite>.TestimonyForEachChapter
            => TestimonyForEachChapter;

        ITestimony<TPrompt, TReadOnlyPrerequisite> ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>>.TalkAbout()
            => this;

        TReadOnlyPrerequisite ITestimony<TPrompt, TReadOnlyPrerequisite>.Prerequisite => Prerequisite;

        protected TPrerequisite Prerequisite { get; } = new TPrerequisite();

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


        protected internal TalkOutlineBase(
            IReadOnlyList<TPrompt> prompts
        )
        {
            Prerequisite.Prompts = prompts ?? throw new ArgumentNullException(nameof(prompts));
        }

        protected void AddTestimony<TKey, TValue>(TKey key, TValue value, Dictionary<TKey, IReadOnlyList<TValue>> dictionary, object syncLock)
        {
            lock (syncLock)
            {
                if (!dictionary.TryGetValue(key, out var existing))
                {
                    dictionary[key] = new List<TValue> { value };
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
            ITalkChapter? chapter,
            TPrompt? prompt,
            Exception testimony
        )
        {
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

        public sealed class ChapterPromptReferenceComparer
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
    }

    public abstract class TalkSessionBase<TPrompt, TReadOnlyPrerequisite, TPrerequisite>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IReadOnlyPrerequisite<TPrompt>
        where TPrerequisite : class, IPrerequisite<TPrompt>, TReadOnlyPrerequisite, new()
    {
        protected ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>> Outline { get; }

        private int _hasRun = 0;

        public TalkSessionBase(
            TPrompt basePrompt,
            Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null,
            Func<IReadOnlyList<TPrompt>, ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>>>? outlineFactory = null
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

        public virtual ITestimony<TPrompt, TReadOnlyPrerequisite> TalkAbout()
        {
            return Outline.TalkAbout();
        }

        public virtual Task<ITestimony<TPrompt, TReadOnlyPrerequisite>> TalkAboutAsync()
        {
            return Task.FromResult(Outline.TalkAbout());
        }

        protected internal virtual IReadOnlyList<TPrompt> DefaultVariationBuilder(TPrompt basePrompt)
        {
            // Default implementation that returns the base prompt as the only variation.
            return [basePrompt.Clone("Default Prompt (without promptVariationBuilder)")];
        }

        protected internal virtual ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>> DefaultOutlineFactory(
            IReadOnlyList<TPrompt> prompts
        ) {
            // Default implementation that creates a simple outline with the provided arrange code and prompts.
            return new TalkOutlineBase<TPrompt, TReadOnlyPrerequisite, TPrerequisite>(prompts);
        }
    }
}

namespace Boostable.WhatRoslynTalkAbout
{
    /************
     * Parsable *
     ************/
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

    public interface IRoslynParsingPromptForTalking<out TSelf> : IPromptForTalking<TSelf>
        where TSelf : class, IRoslynParsingPromptForTalking<TSelf>
    {
        Func<VirtualSource> ArrangeCode { get; }
        public CSharpParseOptions ParseOptions { get; }
        public CSharpCompilationOptions CompilationOptions { get; }
        public Encoding EncodingForParse { get; set; }
    }

    public interface IRoslynParsingReadOnlyPrerequisite<out TPrompt> : IReadOnlyPrerequisite<TPrompt>
        where TPrompt : class, IRoslynParsingPromptForTalking<TPrompt>
    {
        VirtualSource TargetCode { get; }
    }

    public interface IRoslynParsingPrerequisite<TPrompt> : IRoslynParsingReadOnlyPrerequisite<TPrompt>, IPrerequisite<TPrompt>
        where TPrompt : class, IRoslynParsingPromptForTalking<TPrompt>
    {
        new VirtualSource TargetCode { get; set; }
    }

    public interface IRoslynParsingTestimony<TPrompt, out TReadOnlyPrerequisite> : ITestimony<TPrompt, TReadOnlyPrerequisite>
        where TPrompt : class, IRoslynParsingPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IRoslynParsingReadOnlyPrerequisite<TPrompt>
    {
        TReadOnlyPrerequisite RoslynParsingPrerequisite { get; }
    }

    public interface IRoslynParsingTalkOutline<out TTalkAbout> : ITalkOutline<TTalkAbout>
    {
        void ParseCodes();
        Task ParseCodesAsync(CancellationToken cancellationToken);
    }

    public class RoslynParsingTalkOutlineBase<TPrompt, TReadOnlyPrerequisite, TPrerequisite>
        : TalkOutlineBase<TPrompt, TReadOnlyPrerequisite, TPrerequisite>
        , IRoslynParsingTalkOutline<IRoslynParsingTestimony<TPrompt, TReadOnlyPrerequisite>>, IRoslynParsingTestimony<TPrompt, TReadOnlyPrerequisite>
        where TPrompt : class, IRoslynParsingPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IRoslynParsingReadOnlyPrerequisite<TPrompt>
        where TPrerequisite : IRoslynParsingPrerequisite<TPrompt>, TReadOnlyPrerequisite, new()
    {
        TReadOnlyPrerequisite IRoslynParsingTestimony<TPrompt, TReadOnlyPrerequisite>.RoslynParsingPrerequisite => Prerequisite;

        protected int _hasParsed = 0;

        protected internal RoslynParsingTalkOutlineBase(
            IReadOnlyList<TPrompt> prompts
        ) : base(prompts)
        {
        }

        void IRoslynParsingTalkOutline<IRoslynParsingTestimony<TPrompt, TReadOnlyPrerequisite>>.ParseCodes()
            => ParseCodes();

        Task IRoslynParsingTalkOutline<IRoslynParsingTestimony<TPrompt, TReadOnlyPrerequisite>>.ParseCodesAsync(CancellationToken cancellationToken)
            => ParseCodesAsync(cancellationToken);

        protected internal virtual void ParseCodes()
        {
            /*
            if (Interlocked.CompareExchange(ref _hasParsed, 1, 0) != 0)
            {
                throw new InvalidOperationException("The code has already been Parsed. Let it Parse only once per session.");
            }

            try
            {
                Prerequisite.TargetCode = Prerequisite.ArrangeCode();
            }
            catch (Exception)
            {
                // TODO: Let the exception be into the IWhatRoslynTalkAbout instance.
                throw;
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
            */
        }

        protected internal virtual Task ParseCodesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        IRoslynParsingTestimony<TPrompt, TReadOnlyPrerequisite> ITalkOutline<IRoslynParsingTestimony<TPrompt, TReadOnlyPrerequisite>>.TalkAbout()
            => this;
    }
}