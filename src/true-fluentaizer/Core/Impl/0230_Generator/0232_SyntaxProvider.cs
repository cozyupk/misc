using Boostable.Syntax.Core.Abstractions.Analyzer;
using Boostable.Syntax.Core.Abstractions.Common;
using Boostable.Syntax.Core.BaseImpl.BaseUtils;
using Microsoft.CodeAnalysis;
using System;

namespace Boostable.Syntax.Core.BaseImpl.Generator
{
    /// <summary>
    /// Provides an abstract base class for syntax providers that analyze syntax contexts and retrieve metadata for
    /// boosting target symbols.
    /// </summary>
    /// <remarks>This class serves as a foundation for implementing syntax providers that interact with syntax
    /// contexts and diagnostics. It provides methods for retrieving target symbol metadata, accessing syntax nodes, and
    /// reporting diagnostics in a flexible and extensible manner.</remarks>
    /// <typeparam name="TBoostingStrategy">The type of the boosting strategy used to extend syntax capabilities.</typeparam>
    /// <typeparam name="TDiagnostics">The type of the diagnostics provider used for reporting syntax-related issues.</typeparam>
    /// <typeparam name="TTargetSymbolMeta">The type of the metadata associated with the boosting target symbol.</typeparam>
    /// <typeparam name="TSyntaxContext">The type of the syntax context used to provide information for analysis. This type must be non-nullable.</typeparam>
    /// <typeparam name="TStackedStringBuilder">The type of the stacked string builder used by the boosting strategy.</typeparam>
    public abstract class SyntaxProvider<TBoostingStrategy, TDiagnostics, TTargetSymbolMeta, TSyntaxContext>
        : SyntaxBoostableBase<TBoostingStrategy, TDiagnostics>, ISyntaxProvider<TTargetSymbolMeta, TSyntaxContext>
        where TBoostingStrategy : class, IBoostingStrategy<TTargetSymbolMeta, TSyntaxContext>
        where TDiagnostics : class, IDiagnostics
        where TTargetSymbolMeta : ITargetSymbolMeta
        where TSyntaxContext : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxProvider{TBoostingStrategy, TDiagnostics}"/> class.
        /// </summary>
        /// <param name="extender">The boosting strategy used to extend the syntax capabilities.</param>
        /// <param name="diagnostics">The diagnostics provider used for reporting syntax-related issues.</param>
        public SyntaxProvider(
            TBoostingStrategy strategy, TDiagnostics diagnostics
        ) : base(strategy, diagnostics)
        {
            // No additional initialization needed here.
        }

        /// <summary>
        /// Attempts to retrieve the boosting target symbol metadata based on the provided syntax context.
        /// </summary>
        /// <remarks>If an exception occurs during the analysis, it is reported as a diagnostic, and the
        /// method returns <see langword="null"/>.</remarks>
        /// <param name="context">The syntax context that provides the necessary information for determining the boosting target.</param>
        /// <returns>The boosting target symbol metadata of type <typeparamref name="TTargetSymbolMeta"/> if successfully
        /// determined;  otherwise, <see langword="null"/>.</returns>
        public TTargetSymbolMeta? GetBoostingTarget(TSyntaxContext context)
        {
            TTargetSymbolMeta? result = default;
            try
            {
                result = Strategy.GetBoostingTarget(context);
            }
            catch (Exception ex)
            {
                // If an exception occurs, report it as a diagnostic.
                var syntaxNode = TryGetNode(context);
                // If an exception occurs during analysis, report it as a diagnostic.
                TryReportDiagnostic(context, Diagnostic.Create(
                    Diagnostics.ERR9801_DetectedExceptionOnSyntaxProvider,
                    syntaxNode?.GetLocation() ?? Location.None,
                    syntaxNode,
                    ex.Message
                ));
            }

            return result;
        }

        /// <summary>
        /// Attempts to retrieve a <see cref="SyntaxNode"/> from the specified context object.
        /// </summary>
        /// <remarks>This method uses reflection to access a property named "Node" on the provided
        /// <paramref name="context"/> object. If the property does not exist or its value is not a <see
        /// cref="SyntaxNode"/>, the method returns <see langword="null"/>.</remarks>
        /// <param name="context">An object that is expected to contain a property named "Node" representing a <see cref="SyntaxNode"/>.</param>
        /// <returns>The <see cref="SyntaxNode"/> if the "Node" property exists and is of type <see cref="SyntaxNode"/>;
        /// otherwise, <see langword="null"/>.</returns>
        public static SyntaxNode? TryGetNode(object context)
        {
            return context.GetType()
                          .GetProperty("Node")
                          ?.GetValue(context) as SyntaxNode;
        }

        /// <summary>
        /// Attempts to report a diagnostic using the provided context.
        /// </summary>
        /// <remarks>If the <c>ReportDiagnostic</c> method is not found on the <paramref name="context"/>
        /// object, the diagnostic is not reported, and no exception is thrown.</remarks>
        /// <param name="context">The context object that is expected to have a method named <c>ReportDiagnostic</c> accepting a <see
        /// cref="Diagnostic"/> parameter. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="diagnostic">The diagnostic information to report. This parameter cannot be <see langword="null"/>.</param>
        public static void TryReportDiagnostic(object context, Diagnostic diagnostic)
        {
            var method = context.GetType()
                                .GetMethod("ReportDiagnostic", new[] { typeof(Diagnostic) });

            if (method is not null)
            {
                try
                {
                    method?.Invoke(context, new object[] { diagnostic });
                }
                catch
                {
                    // swallow - diagnostic can't be reported
                }
            }
            else
            {
                // Currently, Nothing we can do if the method is not found.
            }
        }
    }
}