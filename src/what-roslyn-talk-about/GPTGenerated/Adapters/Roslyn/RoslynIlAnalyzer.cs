using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Boostable.WhatTalkAbout.Core.Pipeline;
using Microsoft.CodeAnalysis;

namespace Boostable.WhatTalkAbout.Adapters.Roslyn
{
    public sealed class RoslynIlAnalyzer : IIlAnalyzer
    {
        public StepResult<IlAnalysisArtifact> Analyze(EmitArtifact emit, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var metrics = new Dictionary<string, object>
                {
                    ["PeSize"] = emit.Pe?.Length ?? 0,
                    ["PdbSize"] = emit.Pdb?.Length ?? 0,
                };

                return new StepResult<IlAnalysisArtifact>(
                    new IlAnalysisArtifact(metrics),
                    Array.Empty<Diagnostic>(),
                    sw.Elapsed);
            }
            catch (Exception ex)
            {
                return new StepResult<IlAnalysisArtifact>(null, Array.Empty<Diagnostic>(), sw.Elapsed, ex);
            }
        }
    }
}
