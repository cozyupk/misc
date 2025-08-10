using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Boostable.WhatTalkAbout.Core.Abstractions;

namespace Boostable.WhatTalkAbout.Orchestration
{
    public interface IOutlineFactory<TPrompt, TReadOnlyPrereq>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyPrereq : class, IReadOnlyPrerequisite<TPrompt>
    {
        ITalkOutline<ITestimony<TPrompt, TReadOnlyPrereq>> Create(IReadOnlyList<TPrompt> prompts);
    }

    public abstract class TalkSessionBase<TPrompt, TReadOnlyPrerequisite>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyPrerequisite : class, IReadOnlyPrerequisite<TPrompt>
    {
        protected ITalkOutline<ITestimony<TPrompt, TReadOnlyPrerequisite>> Outline { get; }

        private int _hasRun = 0;

        protected TalkSessionBase(
            TPrompt basePrompt,
            IOutlineFactory<TPrompt, TReadOnlyPrerequisite> outlineFactory,
            Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null)
        {
            if (outlineFactory is null) throw new ArgumentNullException(nameof(outlineFactory));
            promptVariationBuilder ??= DefaultVariationBuilder;
            var prompts = promptVariationBuilder(basePrompt);
            Outline = outlineFactory.Create(prompts);
        }

        protected internal void EnsureNotYetHasRun()
        {
            if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
            {
                throw new InvalidOperationException("This session has already spoken. Let it speak only once per session.");
            }
        }

        public virtual ITestimony<TPrompt, TReadOnlyPrerequisite> TalkAbout() => Outline.TalkAbout();

        public virtual Task<ITestimony<TPrompt, TReadOnlyPrerequisite>> TalkAboutAsync()
            => Task.FromResult(Outline.TalkAbout());

        protected internal virtual IReadOnlyList<TPrompt> DefaultVariationBuilder(TPrompt basePrompt)
            => new[] { basePrompt.Clone("Default Prompt (without promptVariationBuilder)") };
    }
}
