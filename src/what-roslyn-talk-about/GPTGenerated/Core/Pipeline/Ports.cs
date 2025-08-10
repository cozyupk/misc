using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Boostable.WhatTalkAbout.Core.Pipeline
{
    public readonly record struct VirtualSource(string Path, string Code);

    public interface ICompiler<TPrompt>
    {
        StepResult<CompileArtifact> Compile(
            IReadOnlyList<VirtualSource> sources,
            IReadOnlyList<MetadataReference> references,
            TPrompt prompt,
            CancellationToken ct);
    }

    public interface IEmitter
    {
        StepResult<EmitArtifact> Emit(Compilation compilation, CancellationToken ct);
    }

    public interface IIlAnalyzer
    {
        StepResult<IlAnalysisArtifact> Analyze(EmitArtifact emit, CancellationToken ct);
    }

    public interface IExecutor
    {
        StepResult<ExecutionArtifact> Execute(EmitArtifact emit, string? entryPoint, string[] args, CancellationToken ct);
    }
}
