using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PartialClassExtGen.Abstractions.Generator
{

    public interface ITargetClassMeta
    {
        INamedTypeSymbol Symbol { get; }

        ICollection<Exception> ExceptionsInGettingExtensionTarget { get; }
    }
}
