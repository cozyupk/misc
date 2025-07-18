using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Genalyzer;
using System.Collections.Generic;
using TrueFluentaizer.Abstractions;

namespace TrueFluentaizer.Generators
{
    [Generator]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FluentBuilderGenerator
        : PCEG<FluentBuilderAttribute>
    {
        public override IEnumerable<Diagnostic>? DefineImplementationsInNamespace(IPartialClassExtender extender, IPCEGDiagnostics diagnostics, INamedTypeSymbol symbol, Compilation compilation, StackedStringBuilder ssb)
        {
            ssb.AppendLine("// This method should be overridden to provide specific implementation logic for generating fluent builder code.");
            using var nssb = ssb.SpawnChild();
            nssb.Append("// Placeholder for fluent builder implementation logic.");
            nssb.Append("// Placeholder for fluent builder implementation logic.");
            // Implementation logic goes here
            return null; // Placeholder for actual implementation
        }

        public override HashSet<string> DefineUsings(IPartialClassExtender extender, INamedTypeSymbol symbol, Compilation compilation)
        {
            return new();
        }
    }
}