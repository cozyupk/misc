using Microsoft.CodeAnalysis;

namespace PartialClassExtGen.Abstractions.Generator
{
    public interface ITargetClassMeta
    {
        /// <summary>
        /// Gets the symbol representing the named type in the source code.
        /// </summary>
        INamedTypeSymbol Symbol { get; }
    }
}
