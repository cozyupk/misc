using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PartialClassExtGen.GenalyzerBase;

namespace TrueFluentaizer.Generators
{
    [Generator]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FluentBuilderGenerator
        : PartialClassExtensionGenalyzerBase<FluentBuiderComposer>
    {
    }
}