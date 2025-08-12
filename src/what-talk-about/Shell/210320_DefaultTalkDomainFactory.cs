using Boostable.WhatTalkAbout.Abstractions;
using System;
using System.Diagnostics;

namespace Boostable.WhatTalkAbout.Utils
{
    public sealed class DefaultTalkDomainFactory<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        , TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>.ITalkDomainFactory
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts, new()
    {
        ITestimonyWithChapterAndPrompt<TPrompt> ITalkDomainFactory.Create(
            ITalkChapter? chapter, TPrompt? prompt, Exception testimony)
            => new TestimonyWithChapterAndPrompt { Chapter = chapter, Prompt = prompt, Testimony = testimony };

        ITestimonyWithChapter ITalkDomainFactory.Create(ITalkChapter? chapter, Exception testimony)
            => new TestimonyWithChapter { Chapter = chapter, Testimony = testimony };

        ITestimonyWithPrompt<TPrompt> ITalkDomainFactory.Create(TPrompt? prompt, Exception testimony)
            => new TestimonyWithPrompt { Prompt = prompt, Testimony = testimony };

        [DebuggerDisplay("{Chapter?.Name,nq} / {Prompt?.Label,nq} / {Testimony.Message,nq}")]
        internal sealed record class TestimonyWithChapterAndPrompt : ITestimonyWithChapterAndPrompt<TPrompt>
        {
            public ITalkChapter? Chapter { get; init; }
            public TPrompt? Prompt { get; init; }
            public required Exception Testimony { get; init; }
        }

        [DebuggerDisplay("{Chapter?.Name,nq} / {Testimony.Message,nq}")]
        internal sealed record class TestimonyWithChapter : ITestimonyWithChapter
        {
            public ITalkChapter? Chapter { get; init; }
            public required Exception Testimony { get; init; }
        }

        [DebuggerDisplay("{Prompt?.Label,nq} / {Testimony.Message,nq}")]
        internal sealed record class TestimonyWithPrompt : ITestimonyWithPrompt<TPrompt>
        {
            public TPrompt? Prompt { get; init; }
            public required Exception Testimony { get; init; }
        }
    }
}
