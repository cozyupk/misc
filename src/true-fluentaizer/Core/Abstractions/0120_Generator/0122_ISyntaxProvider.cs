using Microsoft.CodeAnalysis;
using System;

namespace Boostable.Syntax.Core.Abstractions.Analyzer
{
    public interface ISyntaxProvider<out TTargetSymbolMeta, in TSyntaxContext>
        where TTargetSymbolMeta : ITargetSymbolMeta
    {
        /*
        Type TargetAttribute { get; }
        */

        TTargetSymbolMeta? GetBoostingTarget (
            TSyntaxContext context
        );
    }
}
