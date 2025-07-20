using Boostable.Syntax.Core.Abstractions.Analyzer;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Boostable.Syntax.Core.BaseImpl.Generator
{
    /// <summary>
    /// Represents metadata for a target symbol, including its associated type symbol and any exceptions encountered
    /// during syntax processing.
    /// </summary>
    /// <remarks>This record encapsulates information about a target symbol, such as its type symbol and a
    /// collection of exceptions that occurred during syntax provider operations.</remarks>
    public record TargetSymbolMeta : ITargetSymbolMeta
    {
        /// <summary>
        /// Gets the symbol representing the named type associated with this instance.
        /// </summary>
        public INamedTypeSymbol Symbol { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetSymbolMeta"/> class with the specified symbol.
        /// </summary>
        /// <param name="symbol">The <see cref="INamedTypeSymbol"/> representing the target symbol. This parameter cannot be <see
        /// langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="symbol"/> is <see langword="null"/>.</exception>
        public TargetSymbolMeta(INamedTypeSymbol symbol)
        {
            // Store the parameters, with validation.
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        /// <summary>
        /// Gets the collection of exceptions encountered during syntax provider operations.
        /// </summary>
        /// <remarks>This property is read-only and provides access to the exceptions that were captured
        /// during syntax provider execution. Callers can inspect this collection to diagnose issues or handle errors as
        /// needed.</remarks>
        public ICollection<Exception> ExceptionsOnSyntaxProvider { get; } = new List<Exception>();
    }
}
