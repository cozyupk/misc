using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Boostable.WhatTalkAbout.Core.Pipeline;
using Microsoft.CodeAnalysis;

namespace Boostable.WhatTalkAbout.Adapters.Runtime
{
    public sealed class AlcExecutor : IExecutor
    {
        public StepResult<ExecutionArtifact> Execute(EmitArtifact emit, string? entryPoint, string[] args, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var alc = new AssemblyLoadContext("Transient", isCollectible: true);
                using var pe = new MemoryStream(emit.Pe);
                var assembly = alc.LoadFromStream(pe, emit.Pdb is null ? null : new MemoryStream(emit.Pdb));
                object? returnValue = null;
                string? stdOut = null;

                if (!string.IsNullOrWhiteSpace(entryPoint))
                {
                    var ep = assembly.EntryPoint;
                    if (ep is not null)
                    {
                        var parameters = ep.GetParameters().Length == 0 ? Array.Empty<object?>() : new object?[] { args };
                        returnValue = ep.Invoke(null, parameters);
                    }
                }

                alc.Unload();
                return new StepResult<ExecutionArtifact>(
                    new ExecutionArtifact(returnValue, stdOut),
                    Array.Empty<Diagnostic>(),
                    sw.Elapsed);
            }
            catch (Exception ex)
            {
                return new StepResult<ExecutionArtifact>(null, Array.Empty<Diagnostic>(), sw.Elapsed, ex);
            }
        }
    }
}
