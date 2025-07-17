using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using System;
using System.Collections.Generic;
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
	public abstract class PCEG<TAttribute> : RawStringBuilderPCEG<TAttribute>, IIncrementalGenerator, IPartialClassExtender
		where TAttribute : Attribute
	{
		/// <summary>
		/// Gets the registry of stacked string builders.
		/// </summary>
		private HashSet<IStackedStringBuilder> StringBuilderRegistry { get; } = new();

		/// <summary>
		/// Generates implementation code for the specified symbol and returns any associated diagnostics.
		/// </summary>
		/// <remarks>This method generates implementation code for the provided symbol and appends it to the specified
		/// <see cref="StringBuilder"/>. It uses a <see cref="StackedStringBuilder"/> internally to manage nested code
		/// generation. The caller is responsible for handling any diagnostics returned by the method.</remarks>
		/// <param name="symbol">The symbol for which implementations are to be generated. Cannot be <see langword="null"/>.</param>
		/// <param name="compilation">The current compilation context. Cannot be <see langword="null"/>.</param>
		/// <param name="sb">The <see cref="StringBuilder"/> used to append the generated code. Cannot be <see langword="null"/>.</param>
		/// <returns>A collection of <see cref="Diagnostic"/> objects representing any issues or warnings encountered during the
		/// generation process. Returns <see langword="null"/> if no diagnostics are produced.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="symbol"/>, <paramref name="compilation"/>, or <paramref name="sb"/> is <see
		/// langword="null"/>.</exception>
		public override IEnumerable<Diagnostic>? GenerateImplementations(INamedTypeSymbol symbol, Compilation compilation, StringBuilder sb)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol), "Symbol cannot be null.");
			}
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation), "Compilation cannot be null.");
			}
			if (sb is null)
			{
				throw new ArgumentNullException(nameof(sb), "StringBuilder cannot be null.");
			}

			IEnumerable<Diagnostic>? retval = null;
			sb.AppendLine("// FluentBuilderComposer implementation Begin");
			using (var ssb = new StackedStringBuilder(sb, StringBuilderRegistry))
			{
				retval = OnGenerateImplementations(symbol, compilation, ssb);
			}
			sb.AppendLine("// FluentBuilderComposer implementation End");

			// TODO: Check StringBuilderRegistry for any issues or diagnostics
			//       and report them as warnings if necessary.

			return retval;
		}

		/// <summary>
		/// Generates implementations for the specified type symbol and returns any associated diagnostics.
		/// </summary>
		/// <remarks>This method is abstract and must be implemented by a derived class. It is responsible for
		/// generating code implementations based on the provided type symbol and compilation context.</remarks>
		/// <param name="symbol">The type symbol for which implementations are to be generated. Cannot be <see langword="null"/>.</param>
		/// <param name="compilation">The current compilation context. Provides access to semantic and syntactic information.</param>
		/// <param name="ssb">A <see cref="StackedStringBuilder"/> used to construct the generated code. Cannot be <see langword="null"/>.</param>
		/// <returns>A collection of <see cref="Diagnostic"/> objects representing issues or warnings encountered during generation, or
		/// <see langword="null"/> if no diagnostics are produced.</returns>
		public abstract IEnumerable<Diagnostic>? OnGenerateImplementations(INamedTypeSymbol symbol, Compilation compilation, StackedStringBuilder ssb);
	}
}
