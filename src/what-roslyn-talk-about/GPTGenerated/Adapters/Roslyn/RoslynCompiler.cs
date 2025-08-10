using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Boostable.WhatTalkAbout.Core.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Boostable.WhatTalkAbout.Adapters.Roslyn
{
    public sealed class RoslynCompiler<TPrompt> : ICompiler<TPrompt>
    {
        private readonly CSharpParseOptions _parseOptions;
        private readonly CSharpCompilationOptions _compilationOptions;

        public RoslynCompiler(CSharpParseOptions parseOptions, CSharpCompilationOptions compilationOptions)
        {
            _parseOptions = parseOptions ?? CSharpParseOptions.Default;
            _compilationOptions = compilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        }

        public StepResult<CompileArtifact> Compile(
            IReadOnlyList<VirtualSource> sources,
            IReadOnlyList<MetadataReference> references,
            TPrompt prompt,
            CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var trees = new List<SyntaxTree>(sources.Count);
                foreach (var s in sources)
                {
                    ct.ThrowIfCancellationRequested();
                    var text = SourceText.From(s.Code, System.Text.Encoding.UTF8);
                    trees.Add(CSharpSyntaxTree.ParseText(text, _parseOptions, s.Path, ct));
                }

                var compilation = CSharpCompilation.Create(
                    assemblyName: "InMemoryAssembly",
                    syntaxTrees: trees,
                    references: references,
                    options: _compilationOptions);

                return new StepResult<CompileArtifact>(
                    new CompileArtifact(compilation),
                    compilation.GetDiagnostics(ct),
                    sw.Elapsed);
            }
            catch (Exception ex)
            {
                return new StepResult<CompileArtifact>(null, Array.Empty<Diagnostic>(), sw.Elapsed, ex);
            }
        }
    }
}
