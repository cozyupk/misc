using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace PartialClassExtensionGenerator.Abstractions
{
    /// <summary>
    /// Defines methods for extending partial class functionality by identifying target classes and generating
    /// implementation code.
    /// </summary>
    /// <remarks>This interface is typically used in scenarios where code generation is required for partial
    /// classes.  Implementations of this interface should provide logic to determine whether a given symbol represents
    /// a target class  and to generate the corresponding implementation code for that class.</remarks>
    public interface IPartialClassExtender
    {
        /// <summary>
        /// Gets the name of extension that this implementation generates.
        /// ex. "Fluent Builder", "Fluent API", etc.
        /// </summary>
        string ExtentionName { get; }

        /// <summary>
        /// Gets the suffix appended to the names of generated files.
        /// </summary>
        string GeneratedFileSuffix { get; }

        /// <summary>
        /// Determines whether the specified symbol represents the target class and provides an optional diagnostic
        /// message.
        /// </summary>
        /// <param name="symbol">The symbol to evaluate, representing a named type.</param>
        /// <returns>A tuple containing a boolean and an optional diagnostic: <list type="bullet"> <item> <description><see
        /// langword="true"/> if the symbol represents the target class; otherwise, <see
        /// langword="false"/>.</description> </item> <item> <description>A <see cref="Diagnostic"/> object providing
        /// additional information if applicable, or <see langword="null"/> if no diagnostic is available.</description>
        /// </item> </list></returns>
        (bool, Diagnostic?) IsTargetClass(INamedTypeSymbol symbol);

        /// <summary>
        /// Generates implementations for the specified symbol and appends them to the provided <see
        /// cref="StringBuilder"/>.
        /// </summary>
        /// <param name="symbol">The symbol representing the type for which implementations are to be generated. Cannot be null.</param>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to which the generated implementations will be appended. Cannot be
        /// null.</param>
        /// <returns>A collection of <see cref="Diagnostic"/> objects representing any issues encountered during the generation
        /// process, or <see langword="null"/> if no diagnostics were produced.</returns>
        IEnumerable<Diagnostic>? GenerateImplementations(
            INamedTypeSymbol symbol,
            StringBuilder sb
        );
    }
}
