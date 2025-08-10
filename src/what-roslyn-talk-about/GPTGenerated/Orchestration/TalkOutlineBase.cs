using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Boostable.WhatTalkAbout.Core.Abstractions;

namespace Boostable.WhatTalkAbout.Orchestration
{
    public class TalkOutlineBase<TPrompt, TReadOnlyPrerequisite, TPrerequisite>
        : ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>>, ITestimony<TPrompt, TReadOnlyPrerequisite>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IReadOnlyPrerequisite<TPrompt>
        where TPrerequisite : IPrerequisite<TPrompt>, TReadOnlyPrerequisite
    {
        protected TPrerequisite Prerequisite { get; }

        public TalkOutlineBase(IReadOnlyList<TPrompt> prompts, Func<TPrerequisite> prereqFactory)
        {
            if (prereqFactory is null) throw new ArgumentNullException(nameof(prereqFactory));
            Prerequisite = prereqFactory();
            Prerequisite.Prompts = prompts ?? throw new ArgumentNullException(nameof(prompts));
        }

        // Internal stores (mutable)
        private readonly List<Exception> _general = new();
        private readonly List<ITestimonyWithChapterAndPrompt<TPrompt>> _all = new();

        private readonly ConcurrentDictionary<(ITalkChapter, TPrompt), List<Exception>> _perChapterPrompt
            = new(ChapterPromptReferenceComparer<TPrompt>.Instance);
        private readonly ConcurrentDictionary<TPrompt, List<ITestimonyWithChapter>> _perPrompt = new();
        private readonly ConcurrentDictionary<ITalkChapter, List<ITestimonyWithPrompt<TPrompt>>> _perChapter = new();

        // Public views (read-only)
        bool ITestimony<TPrompt, TReadOnlyPrerequisite>.IsMeaningful => _all.Any();
        IReadOnlyList<Exception> ITestimony<TPrompt, TReadOnlyPrerequisite>.GeneralTestimony => _general;
        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> ITestimony<TPrompt, TReadOnlyPrerequisite>.AllTestimony => _all;

        IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> ITestimony<TPrompt, TReadOnlyPrerequisite>.TestimonyForEachChapterAndPrompt
            => _perChapterPrompt.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<Exception>)kv.Value);

        IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> ITestimony<TPrompt, TReadOnlyPrerequisite>.TestimonyForEachPrompt
            => _perPrompt.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<ITestimonyWithChapter>)kv.Value);

        IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> ITestimony<TPrompt, TReadOnlyPrerequisite>.TestimonyForEachChapter
            => _perChapter.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<ITestimonyWithPrompt<TPrompt>>)kv.Value);

        TReadOnlyPrerequisite ITestimony<TPrompt, TReadOnlyPrerequisite>.Prerequisite => Prerequisite;

        ITestimony<TPrompt, TReadOnlyPrerequisite> ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>>.TalkAbout() => this;

        protected void AddTestimony(ITalkChapter? chapter, TPrompt? prompt, Exception testimony)
        {
            if (testimony is null) throw new ArgumentNullException(nameof(testimony));

            _all.Add(new TestimonyWithChapterAndPrompt(chapter, prompt, testimony));

            if (chapter is null && prompt is null)
            {
                _general.Add(testimony);
                return;
            }

            if (chapter is not null && prompt is not null)
            {
                var list = _perChapterPrompt.GetOrAdd((chapter, prompt), _ => new List<Exception>());
                lock (list) list.Add(testimony);
            }

            if (prompt is not null)
            {
                var list = _perPrompt.GetOrAdd(prompt, _ => new List<ITestimonyWithChapter>());
                lock (list) list.Add(new TestimonyWithChapter(chapter, testimony));
            }

            if (chapter is not null)
            {
                var list = _perChapter.GetOrAdd(chapter, _ => new List<ITestimonyWithPrompt<TPrompt>>());
                lock (list) list.Add(new TestimonyWithPrompt(prompt, testimony));
            }
        }

        protected record TestimonyWithChapterAndPrompt(ITalkChapter? Chapter, TPrompt? Prompt, Exception Testimony)
            : ITestimonyWithChapterAndPrompt<TPrompt>;
        protected record TestimonyWithChapter(ITalkChapter? Chapter, Exception Testimony) : ITestimonyWithChapter;
        protected record TestimonyWithPrompt(TPrompt? Prompt, Exception Testimony) : ITestimonyWithPrompt<TPrompt>;
    }
}
