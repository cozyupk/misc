using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PartialClassExtGen.GenalyzerBase
{
	/// <summary>
	/// Provides a base class for generating partial class implementations using a specified attribute type.
	/// </summary>
	/// <remarks>This abstract class serves as a foundation for implementing partial class generators. It
	/// integrates with the <see cref="IIncrementalGenerator"/> and <see cref="IPartialClassExtender"/> interfaces to
	/// facilitate incremental source generation and partial class extension. Derived classes must implement the <see
	/// cref="GenerateImplementations"/> method to define the specific logic for generating code based on the provided
	/// symbols.</remarks>
	/// <typeparam name="TAttribute">The type of attribute that identifies the target classes for partial class generation. Must derive from <see
	/// cref="Attribute"/>.</typeparam>
	public abstract class PCEG<TAttribute, TPartialClassExtender, TDiagnostics>
		: VanillaPCEG<TAttribute, TPartialClassExtender, TDiagnostics>, IIncrementalGenerator, IPartialClassExtender
		where TAttribute : Attribute
		where TPartialClassExtender : class, IPartialClassExtender
		where TDiagnostics : class, IPCEGDiagnostics
	{
		/// <summary>
		/// Generates implementation code for a partial class extender, including using directives, header blocks,  namespace
		/// declarations, and footer blocks, while collecting diagnostics related to the generation process.
		/// </summary>
		/// <remarks>This method performs several steps to generate the implementation code: <list type="bullet">
		/// <item>Sorts and appends using directives to the provided <paramref name="sb"/>.</item> <item>Generates header
		/// blocks and collects any associated diagnostics.</item> <item>Opens a namespace declaration based on the provided
		/// <paramref name="symbol"/>.</item> <item>Generates implementation code within the namespace using a <see
		/// cref="StackedStringBuilder"/> to manage nested string building.</item> <item>Closes the namespace declaration and
		/// appends footer blocks.</item> <item>Checks for any undisposed <see cref="StackedStringBuilder"/> instances and
		/// reports them as warnings.</item> </list> If any diagnostics are generated during the process, they are returned to
		/// the caller. Otherwise, the method returns <see langword="null"/>.</remarks>
		/// <param name="extender">The partial class extender that provides the implementation details.</param>
		/// <param name="diagnostics">The diagnostics object used to report issues encountered during code generation.</param>
		/// <param name="symbol">The symbol representing the type for which the implementation is being generated.</param>
		/// <param name="compilation">The current compilation context used for resolving symbols and generating code.</param>
		/// <param name="sb">The <see cref="StringBuilder"/> instance used to append the generated code.</param>
		/// <returns>A collection of <see cref="Diagnostic"/> objects representing issues or warnings encountered during the generation
		/// process,  or <see langword="null"/> if no diagnostics were generated.</returns>
		public override IEnumerable<Diagnostic>? GenerateImplementations(
			TPartialClassExtender extender,
			TDiagnostics diagnostics,
			INamedTypeSymbol symbol,
			Compilation compilation,
			StringBuilder sb
		)
		{
            // Initialize the return value.
            List<Diagnostic>? resultDiagnostics = new();

            // Generate Using clauses
            var usings = SortUsingsRoslynStyle(DefineUsings(extender, symbol, compilation));
			foreach (var l in usings) {
				sb.AppendLine($"using {l};");
			}
			if (usings.Any())
			{
				sb.AppendLine();
			}

			// Generate Header Blocks
			var headerDiagnostics = DefineHeaderBlocks(extender, diagnostics, symbol, compilation, sb);
			if (headerDiagnostics != null)
			{
                resultDiagnostics.AddRange(headerDiagnostics);
            }

            // Generate Namespace opening declaration
            sb.AppendLine($"namespace {symbol.ContainingNamespace.ToDisplayString()} {{");

            // Create a registry for StackedStringBuilder instances to track undisposed instances.
            HashSet<IStackedStringBuilder> stringBuilderRegistry = new();

			// Generate the implementation code using a StackedStringBuilder.
			using (var ssb = new StackedStringBuilder(sb, stringBuilderRegistry))
			{
				var retval = DefineImplementationsInNamespace(extender, diagnostics, symbol, compilation, ssb);
				if (retval != null)
				{
					resultDiagnostics.AddRange(retval);
				}
			}

			// Generate Namespace closing declaration
			sb.AppendLine("}");

            // Generate Footer Blocks
            var footerDiagnostics = DefineFooterBlocks(extender, diagnostics, symbol, compilation, sb);
            if (footerDiagnostics != null)
            {
                resultDiagnostics.AddRange(footerDiagnostics);
            }

            // Check StringBuilderRegistry for any issues or diagnostics
            // and report them as warnings if necessary.
            if (stringBuilderRegistry.Any())
			{
				var errorSb = new StringBuilder();
				foreach (var s in stringBuilderRegistry)
				{
					errorSb.Append("[Dump of the one on undisposed StackedStringBuiler]> ");
					errorSb.Append(s.ToString());
				}
				resultDiagnostics.Add(
					Diagnostic.Create(
						diagnostics.PCEG0001W_DetectedUndisposedStackedStringBuilderInstance,
						symbol.Locations.FirstOrDefault() ?? Location.None, errorSb.ToString()
					)
				);
			}

			return resultDiagnostics.Any() ? resultDiagnostics : null;
		}

		/// <summary>
		/// Sorts a collection of using directives in a style consistent with Roslyn conventions.
		/// </summary>
		/// <param name="usings">The collection of using directives to sort.</param>
		/// <returns>A sorted collection of using directives, where directives starting with "System" appear first, followed by other
		/// directives sorted in ordinal order.</returns>
        private static IEnumerable<string> SortUsingsRoslynStyle(IEnumerable<string> usings)
        {
            return usings
                .OrderBy(u => u.StartsWith("System") ? 0 : 1)
                .ThenBy(u => u, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Defines header blocks for a partial class implementation and generates diagnostics, if applicable.
        /// </summary>
        /// <remarks>This method provides a default implementation that does not write any header blocks to the
        /// <paramref name="sb"/>  and does not generate any diagnostics. Override this method in a derived class to implement
        /// custom header block  definitions and diagnostic generation.</remarks>
        /// <param name="extender">The partial class extender that provides context for defining header blocks.</param>
        /// <param name="diagnostics">The diagnostics object used to report issues or warnings during header block definition.</param>
        /// <param name="symbol">The symbol representing the named type for which header blocks are being defined.</param>
        /// <param name="compilation">The current compilation context, providing access to semantic and syntactic information.</param>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to which header blocks are written.</param>
        /// <returns>An enumerable collection of <see cref="Diagnostic"/> objects representing any issues or warnings generated during
        /// the process. Returns <see langword="null"/> if no diagnostics are generated.</returns>
        public virtual IEnumerable<Diagnostic>? DefineHeaderBlocks(
			TPartialClassExtender extender,
			TDiagnostics diagnostics,
			INamedTypeSymbol symbol,
			Compilation compilation,
			StringBuilder sb
		)
		{
			// Default implementation write no header blocks to the StringBuilder.
			// And no diagnostics are generated.
			return null;
		}

		/// <summary>
		/// Defines the using directives for the generated code.
		/// </summary>
		/// <remarks>This method is abstract and must be implemented by a derived class. It is intended to define
		/// the using directives that will be included in the generated code.</remarks>
		/// <returns>A set of strings representing the using directives to be included in the generated code.
		/// No semicollon is needed for each string.</returns>
		public abstract HashSet<string> DefineUsings(
                        TPartialClassExtender extender,
			            INamedTypeSymbol symbol,
						Compilation compilation
		);

		/// <summary>
		/// Defines implementations for partial classes within the namespace the symbol belongs.
		/// </summary>
		/// <remarks>This method is abstract and must be implemented by a derived class. It is intended to define
		/// partial class implementations within the namespace the symbol belongs, leveraging the provided extender and compilation
		/// context.</remarks>
		/// <param name="extender">An object that provides functionality to extend partial classes.</param>
		/// <param name="diagnostics">An object used to collect and report diagnostic information during the operation.</param>
		/// <param name="symbol">The symbol representing the namespace in which implementations are to be defined.</param>
		/// <param name="compilation">The current compilation context, providing access to semantic and syntactic information.</param>
		/// <param name="ssb">A stacked string builder used for constructing and managing string outputs during the operation.</param>
		/// <returns>A collection of <see cref="Diagnostic"/> objects representing any issues encountered during the operation, or <see
		/// langword="null"/> if no diagnostics are generated.</returns>
		public abstract IEnumerable<Diagnostic>? DefineImplementationsInNamespace(
			TPartialClassExtender extender,
			TDiagnostics diagnostics,
			INamedTypeSymbol symbol,
			Compilation compilation,
			StackedStringBuilder ssb);

		/// <summary>
		/// Defines footer blocks for a partial class extender and writes them to the provided <see cref="StringBuilder"/>.
		/// </summary>
		/// <remarks>The default implementation does not define any footer blocks or generate diagnostics. Override
		/// this method in a derived class to provide custom footer block definitions.</remarks>
		/// <param name="extender">The partial class extender that provides context for defining footer blocks.</param>
		/// <param name="diagnostics">The diagnostics object used to report issues encountered during footer block definition.</param>
		/// <param name="symbol">The symbol representing the partial class being extended.</param>
		/// <param name="compilation">The current compilation context.</param>
		/// <param name="sb">The <see cref="StringBuilder"/> to which the footer blocks are written.</param>
		/// <returns>An enumerable collection of diagnostics generated during the footer block definition, or <see langword="null"/> if
		/// no diagnostics are produced.</returns>
		public virtual IEnumerable<Diagnostic>? DefineFooterBlocks(
			TPartialClassExtender extender,
			TDiagnostics diagnostics,
			INamedTypeSymbol symbol,
			Compilation compilation,
			StringBuilder sb
		)
		{
			// Default implementation write no footer blocks to the StringBuilder.
			// And no diagnostics are generated.
			return null;
		}
	}
}
