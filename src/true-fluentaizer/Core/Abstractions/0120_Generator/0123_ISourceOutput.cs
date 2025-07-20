using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Boostable.Syntax.Core.Abstractions.Analyzer
{
    public interface ISourceOutput<TTargetSymbolMeta>
        where TTargetSymbolMeta : ITargetSymbolMeta
    {
        void GenerateSource(
            SourceProductionContext spc,
            (Compilation Left, ImmutableArray<TTargetSymbolMeta?> Right) source
        );
    }
}
