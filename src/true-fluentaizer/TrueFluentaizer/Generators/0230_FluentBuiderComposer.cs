using Microsoft.CodeAnalysis;
using PartialClassExtGen.GenalyzerBase;
using System;
using System.Collections.Generic;
using System.Text;
using TrueFluentaizer.Abstractions;

namespace TrueFluentaizer.Generators
{
    public class FluentBuiderComposer : PCEG<FluentBuilderAttribute>
    {
        public override IEnumerable<Diagnostic>? GenerateImplementations(INamedTypeSymbol symbol, Compilation compilation, StringBuilder sb)
        {
            throw new NotImplementedException();
        }
    }
}
