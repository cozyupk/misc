using PartialClassExtGen.Abstractions.Common;
using System;

namespace PartialClassExtGen.Genalyzer
{
    /// <summary>
    /// Represents a partial class extender generator that operates on attributes of type <typeparamref name="TAttribute"/>.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute that the generator processes. Must derive from <see cref="Attribute"/>.</typeparam>
    public abstract partial class PCEG<TAttribute>
        : PCEG<TAttribute, IPartialClassExtender, IPCEGDiagnostics>
        where TAttribute : Attribute
    {
        // Just a simple alias for the base class
    }
}
