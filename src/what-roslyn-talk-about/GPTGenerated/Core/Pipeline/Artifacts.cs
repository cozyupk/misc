using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Boostable.WhatTalkAbout.Core.Pipeline
{
    public sealed record CompileArtifact(Compilation Compilation);
    public sealed record EmitArtifact(byte[] Pe, byte[]? Pdb, string AssemblyName);
    public sealed record IlAnalysisArtifact(IReadOnlyDictionary<string, object> Metrics);
    public sealed record ExecutionArtifact(object? ReturnValue, string? StdOut);
}
