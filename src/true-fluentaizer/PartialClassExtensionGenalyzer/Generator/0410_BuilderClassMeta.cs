using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Generator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PartialClassExtGen.Generator
{
    /// <summary>
    /// Represents metadata for a builder class, including the symbol of the named type in the source code.
    /// </summary>
    /// <remarks>This class provides information about a builder class, encapsulating its associated symbol.
    /// It is primarily used for code generation scenarios where metadata about the builder class is
    /// required.</remarks>
    public record BuilderClassMeta : ITargetClassMeta
    {
        /// <summary>
        /// Gets the symbol representing the named type in the source code.
        /// </summary>
        public INamedTypeSymbol Symbol { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderClassMeta"/> class with the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol representing the named type. This parameter cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="symbol"/> is <see langword="null"/>.</exception>
        public BuilderClassMeta(INamedTypeSymbol symbol)
        {
            // Store the parameters, with validation.
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        public ICollection<Exception> ExceptionsInGettingExtensionTarget { get; } = new List<Exception>();

    }
}
