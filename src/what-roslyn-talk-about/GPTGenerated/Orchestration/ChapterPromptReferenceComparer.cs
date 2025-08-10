using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boostable.WhatTalkAbout.Core.Abstractions;

namespace Boostable.WhatTalkAbout.Orchestration
{
    public sealed class ChapterPromptReferenceComparer<TPrompt>
        : IEqualityComparer<(ITalkChapter, TPrompt)>
        where TPrompt : class
    {
        public static ChapterPromptReferenceComparer<TPrompt> Instance { get; } = new();

        private ChapterPromptReferenceComparer() { }

        public bool Equals((ITalkChapter, TPrompt) x, (ITalkChapter, TPrompt) y)
            => ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

        public int GetHashCode((ITalkChapter, TPrompt) obj)
        {
            int h1 = obj.Item1 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item1);
            int h2 = obj.Item2 is null ? 0 : RuntimeHelpers.GetHashCode(obj.Item2);
            return ((h1 << 5) | (h1 >> 27)) ^ h2;
        }
    }
}
