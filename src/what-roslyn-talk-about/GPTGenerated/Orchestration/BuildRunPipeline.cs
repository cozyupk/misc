using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Boostable.WhatTalkAbout.Core.Pipeline;
using Microsoft.CodeAnalysis;

namespace Boostable.WhatTalkAbout.Orchestration
{
    public sealed class BuildRunPipeline<TPrompt>
    {
        public Task<(StepResult<CompileArtifact> compile,
                     StepResult<EmitArtifact> emit,
                     StepResult<IlAnalysisArtifact>? il,
                     StepResult<ExecutionArtifact>? exec)>
        RunAsync(ICompiler<TPrompt> compiler,
                 IEmitter emitter,
                 IIlAnalyzer? analyzer,
                 IExecutor? executor,
                 IReadOnlyList<VirtualSource> sources,
                 IReadOnlyList<MetadataReference> refs,
                 TPrompt prompt,
                 CancellationToken ct)
        {
            var c = compiler.Compile(sources, refs, prompt, ct);
            if (!c.Succeeded) return Task.FromResult((c, default!, null, null));

            var e = emitter.Emit(c.Artifact!.Compilation, ct);
            if (!e.Succeeded) return Task.FromResult((c, e, null, null));

            StepResult<IlAnalysisArtifact>? ia = analyzer is null ? null : analyzer.Analyze(e.Artifact!, ct);
            StepResult<ExecutionArtifact>? ex = executor is null ? null :
                executor.Execute(e.Artifact!, entryPoint: null, args: Array.Empty<string>(), ct);

            return Task.FromResult((c, e, ia, ex));
        }
    }
}
