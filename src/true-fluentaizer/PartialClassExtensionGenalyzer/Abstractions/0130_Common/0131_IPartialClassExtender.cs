using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PartialClassExtGen.Abstractions.Common
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
        ////////////////////////////
        //// General Properties ////
        ////////////////////////////

        /// <summary>
        /// Gets the type of the target attribute associated with this instance.
        /// </summary>
        Type TargetAttribute { get; }

        /// <summary>
        /// Gets the name of extension that this implementation generates.
        /// ex. "Fluent Builder", "Fluent API", etc.
        /// </summary>
        string ExtensionName { get; }

        /// <summary>
        /// Gets the suffix appended to the names of generated files.
        /// </summary>
        string SuffixForGeneratedFiles { get; }

        /// <summary>
        /// Gets the prefix used for diagnostic error identifiers.
        /// </summary>
        string ErrorPrefixForDiagnosticId { get; }

        /// <summary>
        /// Gets the prefix used to identify warning diagnostic identifiers.
        /// </summary>
        string WarningPrefixForDiagnosticId { get; }

        //////////////////////////////
        //// For Source Generator ////
        //////////////////////////////

        /// <summary>
        /// Generates implementations for the specified symbol and appends them to the provided <see
        /// cref="StringBuilder"/>.
        /// </summary>
        /// <param name="symbol">The symbol representing the type for which implementations are to be generated. Cannot be null.</param>
        /// <param name="compilation">The compilation context that provides access to the compilation being processed. Cannot be null.</param>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to which the generated implementations will be appended. Cannot be
        /// null.</param>
        /// <returns>A collection of <see cref="Diagnostic"/> objects representing any issues encountered during the generation
        /// process, or <see langword="null"/> if no diagnostics were produced.</returns>
        IEnumerable<Diagnostic>? GenerateImplementationsInternal<TArgPartialClassExtender, TArgDiagnostics>(
            TArgPartialClassExtender extender,
            TArgDiagnostics diagnostics,
            INamedTypeSymbol symbol,
            Compilation compilation,
            StringBuilder sb
        )
            where TArgPartialClassExtender : class, IPartialClassExtender
            where TArgDiagnostics : class, IPCEGDiagnostics;

        //////////////////////
        //// For Analyzer ////
        //////////////////////

        /*
        /// <summary>
        /// Gets a collection of actions to analyze class declarations.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Action<SyntaxNodeAnalysisContext>> GetAnalyzeClassDeclarations();
        IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics();
        */

    }
}
