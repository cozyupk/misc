using Boostable.WhatTalkAbout.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/****************
 * Abstractions *
 ****************/

namespace Boostable.WhatTalkAbout.Abstractions
{
    public interface ITalkOutlinePrompt<out TSelf>
    {
        string Label { get; }
        CancellationToken CancellationToken { get; set; }

        TSelf Clone(string label);
    }

    public interface ITalkChapter
    {
        public string Name { get; }

    }

    public interface ITestimonyWithChapterAndPrompt<out TPrompt>
        where TPrompt : ITalkOutlinePrompt<TPrompt>
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
    where TPrompt : ITalkOutlinePrompt<TPrompt>
    {
        TPrompt? Prompt { get; }

        Exception Testimony { get; }
    };


    public interface ITestimony<TPrompt>
        where TPrompt : ITalkOutlinePrompt<TPrompt>
    {
        public interface IPrerequisite
        {
            IReadOnlyList<TPrompt> Prompts { get; }
        }

        bool IsMeaningful { get; }
        IPrerequisite Prerequisite { get; }

        IReadOnlyList<Exception> GeneralTestimony { get; }

        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; }

        IReadOnlyDictionary<
            (ITalkChapter, TPrompt),
            IReadOnlyList<Exception>
        > TestimonyForEachChapterAndPrompt
        { get; }

        IReadOnlyDictionary<
            TPrompt,
            IReadOnlyList<ITestimonyWithChapter>
        > TestimonyForEachPrompt
        { get; }

        IReadOnlyDictionary<
            ITalkChapter,
            IReadOnlyList<ITestimonyWithPrompt<TPrompt>>
        > TestimonyForEachChapter
        { get; }
    }

    public interface ITalkOutline<out TTalkAbout>
    {
        TTalkAbout TalkedAbout();
    }
}

/********
 * Base *
*********/

namespace Boostable.WhatTalkAbout.Base {
    public class TalkOutlineBase<TPrompt> : ITalkOutline<ITestimony<TPrompt>>, ITestimony<TPrompt>
        where TPrompt : ITalkOutlinePrompt<TPrompt>
    {

        bool ITestimony<TPrompt>.IsMeaningful => throw new NotImplementedException();

        ITestimony<TPrompt>.IPrerequisite ITestimony<TPrompt>.Prerequisite => throw new NotImplementedException();

        IReadOnlyList<Exception> ITestimony<TPrompt>.GeneralTestimony => GeneralTestimony;

        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> ITestimony<TPrompt>.AllTestimony => AllTestimony;

        IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> ITestimony<TPrompt>.TestimonyForEachChapterAndPrompt => TestimonyForEachChapterAndPrompt;

        IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> ITestimony<TPrompt>.TestimonyForEachPrompt => TestimonyForEachPrompt;

        IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> ITestimony<TPrompt>.TestimonyForEachChapter => TestimonyForEachChapter;

        ITestimony<TPrompt> ITalkOutline<ITestimony<TPrompt>>.TalkedAbout()
            => this;

        protected virtual IReadOnlyList<TPrompt> Prompts { get; }

        private List<Exception> GeneralTestimony { get; } = [];
        private List<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; } = [];

        private Dictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> TestimonyForEachChapterAndPrompt { get; } = [];
        private Dictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> TestimonyForEachPrompt { get; } = [];
        private Dictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> TestimonyForEachChapter { get; } = [];

        protected internal TalkOutlineBase(
            IReadOnlyList<TPrompt> prompts
        )
        {
            Prompts = prompts ?? throw new ArgumentNullException(nameof(prompts));
        }

        protected virtual void AddTestimony(
            ITalkChapter? chapter,
            TPrompt? prompt,
            Exception testimony
        )
        {
            if (testimony == null) throw new ArgumentNullException(nameof(testimony));

            AllTestimony.Add(new TestimonyWithChapterAndPrompt(chapter, prompt, testimony));

            if (chapter is null && prompt is null)
            {
                GeneralTestimony.Add(testimony);
                return;
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
    }

    public abstract class TalkSessionBase<TPrompt>
        where TPrompt : ITalkOutlinePrompt<TPrompt>
    {
        protected ITalkOutline<ITestimony<TPrompt>> Outline { get; }

        private int _hasRun = 0;

        public TalkSessionBase(
            TPrompt basePrompt,
            Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null,
            Func<IReadOnlyList<TPrompt>, ITalkOutline<ITestimony<TPrompt>>>? outlineFactory = null
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
                throw new InvalidOperationException(" has already spoken. Let it speak only once per session.");
            }
        }

        public virtual ITestimony<TPrompt> TalkAbout()
        {
            return Outline.TalkedAbout();
        }

        public virtual Task<ITestimony<TPrompt>> TalkAboutAsync()
        {
            return Task.FromResult(Outline.TalkedAbout());
        }

        protected internal virtual IReadOnlyList<TPrompt> DefaultVariationBuilder(TPrompt basePrompt)
        {
            // Default implementation that returns the base prompt as the only variation.
            return [basePrompt.Clone("Default Prompt (without promptVariationBuilder)")];
        }

        protected internal virtual ITalkOutline<ITestimony<TPrompt>> DefaultOutlineFactory(
            IReadOnlyList<TPrompt> prompts
        ) {
            // Default implementation that creates a simple outline with the provided arrange code and prompts.
            return new TalkOutlineBase<TPrompt>(prompts);
        }
    }
}

namespace Boostable.WhatRoslynTalkAbout { 
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
}
