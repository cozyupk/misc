using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Boostable.WhatTalkAbout.Core.Pipeline;
using Microsoft.CodeAnalysis;

namespace Boostable.WhatTalkAbout.Adapters.Roslyn
{
    public sealed class RoslynEmitter : IEmitter
    {
        public StepResult<EmitArtifact> Emit(Compilation compilation, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var pe = new MemoryStream();
                using var pdb = new MemoryStream();
                var result = compilation.Emit(pe, pdb, cancellationToken: ct);
                var diags = result.Diagnostics;
                if (!result.Success)
                {
                    return new StepResult<EmitArtifact>(null, diags, sw.Elapsed);
                }
                return new StepResult<EmitArtifact>(
                    new EmitArtifact(pe.ToArray(), pdb.ToArray(), compilation.AssemblyName ?? "InMemoryAssembly"),
                    diags,
                    sw.Elapsed);
            }
            catch (Exception ex)
            {
                return new StepResult<EmitArtifact>(null, Array.Empty<Diagnostic>(), sw.Elapsed, ex);
            }
        }
    }
}
