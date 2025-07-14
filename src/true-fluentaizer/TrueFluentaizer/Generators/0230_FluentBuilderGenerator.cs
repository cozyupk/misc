using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PartialClassExtensionGenerator.GeneratorBase;
using System.Linq;
using System.Text;
using TrueFluentaizer.Abstractions;

namespace TrueFluentaizer.Generators
{
    [Generator]
    public class FluentBuilderGenerator : PartialClassExtensionGenerator<FluentBuiderComposer>
    {
    }
        /*
        private static string GenericQualifiedName(INamedTypeSymbol symbol) {
            return symbol.IsGenericType
                ? $"{symbol.Name}<{string.Join(", ", symbol.TypeArguments.Select(t => t.ToDisplayString()))}>"
                : $"{symbol.Name}";
        }

        private static Diagnostic? GenerateFluentBuilderClass(
            IFluentBuidlerComposer composer, StringBuilder sb, INamedTypeSymbol symbol
        ) {
            return null; // return null when no errors are found
        }
        */
}