using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Boostable.Syntax.Core.Abstractions.Analyzer
{

    public interface ITargetSymbolMeta
    {
        INamedTypeSymbol Symbol { get; }

        ICollection<Exception> ExceptionsOnSyntaxProvider { get; }
    }
}
