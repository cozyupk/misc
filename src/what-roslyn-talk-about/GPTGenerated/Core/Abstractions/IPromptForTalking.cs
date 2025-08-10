using System.Threading;

namespace Boostable.WhatTalkAbout.Core.Abstractions
{
    public interface IPromptForTalking { }

    public interface IPromptForTalking<out TSelf> : IPromptForTalking
        where TSelf : class, IPromptForTalking<TSelf>
    {
        string Label { get; }
        CancellationToken CancellationToken { get; }
        TSelf Clone(string label);
    }

    public interface ITalkChapter
    {
        string Name { get; }
    }
}
