using Boostable.WhatAgentsTalkAbout.Abstractions;
using System;
using System.Diagnostics;

namespace Boostable.WhatAgentsTalkAbout.Shell
{
    public abstract class TalkDomainFactoryBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITalkDomainFactory
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts
    {
        ITestimonyWithChapterAndPrompt<TPrompt> ITalkDomainFactory.CreateTestimony(
            ITalkChapter? chapter, TPrompt? prompt, Exception testimony)
            => CreateTestimony(chapter, prompt, testimony);

        ITestimonyWithChapter ITalkDomainFactory.CreateTestimony(ITalkChapter? chapter, Exception testimony)
            => CreateTestimony(chapter, testimony);

        ITestimonyWithPrompt<TPrompt> ITalkDomainFactory.CreateTestimony(TPrompt? prompt, Exception testimony)
            => CreateTestimony(prompt, testimony);

        TArtifacts ITalkDomainFactory.CreateArtifacts()
            => CreateArtifacts();

        protected internal virtual ITestimonyWithChapterAndPrompt<TPrompt> CreateTestimony(
            ITalkChapter? chapter, TPrompt? prompt, Exception testimony
        )
            => new TestimonyWithChapterAndPrompt { Chapter = chapter, Prompt = prompt, Testimony = testimony };

        protected internal virtual ITestimonyWithChapter CreateTestimony(
            ITalkChapter? chapter, Exception testimony
        )
            => new TestimonyWithChapter { Chapter = chapter, Testimony = testimony };

        protected internal virtual ITestimonyWithPrompt<TPrompt> CreateTestimony(
            TPrompt? prompt, Exception testimony
        )
            => new TestimonyWithPrompt { Prompt = prompt, Testimony = testimony };

        protected internal abstract TArtifacts CreateArtifacts();

        [DebuggerDisplay("{Chapter?.Name,nq} / {Prompt?.Label,nq} / {Testimony.Message,nq}")]
        protected internal sealed record class TestimonyWithChapterAndPrompt : ITestimonyWithChapterAndPrompt<TPrompt>
        {
            public ITalkChapter? Chapter { get; init; }
            public TPrompt? Prompt { get; init; }
            public required Exception Testimony { get; init; }
        }

        [DebuggerDisplay("{Chapter?.Name,nq} / {Testimony.Message,nq}")]
        protected internal sealed record class TestimonyWithChapter : ITestimonyWithChapter
        {
            public ITalkChapter? Chapter { get; init; }
            public required Exception Testimony { get; init; }
        }

        [DebuggerDisplay("{Prompt?.Label,nq} / {Testimony.Message,nq}")]
        protected internal sealed record class TestimonyWithPrompt : ITestimonyWithPrompt<TPrompt>
        {
            public TPrompt? Prompt { get; init; }
            public required Exception Testimony { get; init; }
        }
    }
}
