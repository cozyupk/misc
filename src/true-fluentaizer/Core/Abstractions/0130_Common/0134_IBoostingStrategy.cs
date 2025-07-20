using System.Collections.Generic;
using System.Text;

namespace Boostable.Syntax.Core.Abstractions.Common
{
    public interface IBoostingStrategy<TTargetSymbolMeta, TSyntaxContext, TStackedStringBuilder>
        : IBoostingStrategy
        where TStackedStringBuilder : IStackedStringBuilder
    {
        /// <summary>
        /// Create a new instance of <see cref="IStackedStringBuilder"/> for generating code or text output.
        /// </summary>
        TStackedStringBuilder CreateStaskedStringBuilder(
            StringBuilder parentStringBuilder, HashSet<IStackedStringBuilder>? registry = null
        );

        /*
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
            where TArgPartialClassExtender : class, IBoostingStrategy
            where TArgDiagnostics : class, IPCEGDiagnostics;

        //////////////////////
        //// For Analyzer ////
        //////////////////////

        /// <summary>
        /// Retrieves a collection of syntax node rules to be applied during analysis.
        /// </summary>
        /// <typeparam name="TArgPartialClassExtender">The type of the partial class extender, which must implement <see cref="IBoostingStrategy"/>.</typeparam>
        /// <typeparam name="TArgDiagnostics">The type of the diagnostics provider, which must implement <see cref="IPCEGDiagnostics"/>.</typeparam>
        /// <param name="extender">An instance of <typeparamref name="TArgPartialClassExtender"/> used to extend partial class functionality.</param>
        /// <param name="diagnostics">An instance of <typeparamref name="TArgDiagnostics"/> used to report diagnostics during analysis.</param>
        /// <returns>An enumerable collection of <see cref="ISyntaxNodeRule"/> objects representing the rules to be applied.</returns>
        IEnumerable<ISyntaxNodeRule> GetSyntaxNodeRulesInternal<TArgPartialClassExtender, TArgDiagnostics>(
            TArgPartialClassExtender extender,
            TArgDiagnostics diagnostics
        )
            where TArgPartialClassExtender : class, IBoostingStrategy
            where TArgDiagnostics : class, IPCEGDiagnostics;
        */
    }

    public interface IBoostingStrategy<TTargetSymbolMeta, TSyntaxContext>
    : IBoostingStrategy
    {
        TTargetSymbolMeta GetBoostingTarget(TSyntaxContext context);
    }

    public interface IBoostingStrategy
    {
        /// <summary>
        /// Gets the name of the extension that this implementation generates.
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
    }
}
