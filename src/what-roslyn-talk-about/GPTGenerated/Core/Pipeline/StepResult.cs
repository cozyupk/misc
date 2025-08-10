using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Boostable.WhatTalkAbout.Core.Pipeline
{
    public sealed record StepResult<T>(
        T? Artifact,
        IReadOnlyList<Diagnostic> Diagnostics,
        TimeSpan Elapsed,
        Exception? Exception = null)
    {
        public bool Succeeded => Exception is null && Diagnostics.All(d => d.Severity < DiagnosticSeverity.Error);
    }
}
