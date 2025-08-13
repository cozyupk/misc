using Boostable.WhatAgentsTalkAbout.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Boostable.WhatAgentsTalkAbout.Core
{
    public abstract class TalkOutlineAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArtifacts
    {
        public interface ITestimonyAdmin
        {
        }
    }
}
