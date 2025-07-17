using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PartialClassExtGen.GenalyzerBase;
using System;
using System.Collections.Generic;
using System.Text;
using TrueFluentaizer.Abstractions;

namespace TrueFluentaizer.Generators
{
    [Generator]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FluentBuilderGenerator
        : RawStringBuilderPCEG<FluentBuilderAttribute>
    {


        public IEnumerable<Diagnostic>? OnGenerateImplementations(INamedTypeSymbol _1, Compilation _2, StackedStringBuilder ssb)
        {
            ssb.AppendLine("// This method should be overridden to provide specific implementation logic for generating fluent builder code.");
            // Implementation logic goes here
            return null; // Placeholder for actual implementation
        }
    }
}