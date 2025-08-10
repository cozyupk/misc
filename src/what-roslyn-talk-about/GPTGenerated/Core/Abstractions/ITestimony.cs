using System;
using System.Collections.Generic;

namespace Boostable.WhatTalkAbout.Core.Abstractions
{
    public interface ITestimonyWithChapterAndPrompt<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        ITalkChapter? Chapter { get; }
        TPrompt? Prompt { get; }
        Exception Testimony { get; }
    }

    public interface ITestimonyWithChapter
    {
        ITalkChapter? Chapter { get; }
        Exception Testimony { get; }
    }

    public interface ITestimonyWithPrompt<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        TPrompt? Prompt { get; }
        Exception Testimony { get; }
    }

    public interface IReadOnlyPrerequisite<out TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        IReadOnlyList<TPrompt> Prompts { get; }
    }

    public interface IPrerequisite<TPrompt> : IReadOnlyPrerequisite<TPrompt>
        where TPrompt : class, IPromptForTalking<TPrompt>
    {
        new IReadOnlyList<TPrompt> Prompts { get; set; }
    }

    public interface ITestimony<TPrompt, out TReadOnlyPrerequisite>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IReadOnlyPrerequisite<TPrompt>
    {
        TReadOnlyPrerequisite Prerequisite { get; }
        bool IsMeaningful { get; }
        IReadOnlyList<Exception> GeneralTestimony { get; }
        IReadOnlyList<ITestimonyWithChapterAndPrompt<TPrompt>> AllTestimony { get; }
        IReadOnlyDictionary<(ITalkChapter, TPrompt), IReadOnlyList<Exception>> TestimonyForEachChapterAndPrompt { get; }
        IReadOnlyDictionary<TPrompt, IReadOnlyList<ITestimonyWithChapter>> TestimonyForEachPrompt { get; }
        IReadOnlyDictionary<ITalkChapter, IReadOnlyList<ITestimonyWithPrompt<TPrompt>>> TestimonyForEachChapter { get; }
    }

    public interface ITalkOutline<out TTalkAbout>
    {
        TTalkAbout TalkAbout();
    }
}
