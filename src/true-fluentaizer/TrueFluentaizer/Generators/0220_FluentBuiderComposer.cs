using Microsoft.CodeAnalysis;
using PartialClassExtensionGenerator.Abstractions;
using System.Collections.Generic;
using System.Text;
using TrueFluentaizer.Abstractions;

namespace TrueFluentaizer.Generators
{
    /*
    // Scan the class for methods
    foreach (var member in symbol.GetMembers())
    {
        // Skip if the member is not a method
        if (member.Kind != SymbolKind.Method)
        {
            continue; // Only process methods
        }

        var methodSymbol = (IMethodSymbol)member;
        sb.AppendLine($"// Method: {methodSymbol.Name}");
    }

    var isb = new IndentedStringBuilder(sb);
    isb.PushIndent();
    isb.AppendLine($"public partial class {GenericQualifiedName(symbol)}");
    isb.AppendLine("}");
    */


    public class FluentBuiderComposer : IPartialClassExtender
    {
        public string ExtentionName => "Fluent Buidler";
        public string GeneratedFileSuffix => "FlntBldr";

        public IEnumerable<Diagnostic>? GenerateImplementations(INamedTypeSymbol symbol, StringBuilder sb)
        {
            sb.AppendLine($"// Fluent Builder for {symbol.Name}");
            return null; // No diagnostics produced
        }

        public (bool, Diagnostic?) IsTargetClass(INamedTypeSymbol symbol)
        {
            string targetAttributeName = $"TrueFluentaizer.Abstractions.{nameof(FluentBuilderAttribute)}";
            foreach (var attributeData in symbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.ToDisplayString() == targetAttributeName)
                {
                    return (true, null);
                    // return new BuilderClassInfo(symbol);
                }
            }
            return (false, null);
        }

        public void ParseBuilderClass(INamedTypeSymbol symbol)
        {
            ParserBuidlerClassAttributes(symbol);
        }

        internal void ParserBuidlerClassAttributes(INamedTypeSymbol _)
        {
        }
    }
}
