using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
	partial class VanillaPCEG<TAttribute, TPartialClassExtender, TDiagnostics>
	{
		/// <summary>
		/// Generates implementations for the specified partial class extender and diagnostics provider.
		/// </summary>
		/// <remarks>This method validates the provided extender and diagnostics instances to ensure they
		/// match the expected types. It then delegates the implementation generation to the abstract
		/// <c>GenerateImplementations</c> method.</remarks>
		/// <typeparam name="TArgPartialClassExtender">The type of the partial class extender, which must implement <see cref="IPartialClassExtender"/>.</typeparam>
		/// <typeparam name="TArgDiagnostics">The type of the diagnostics provider, which must implement <see cref="IPCEGDiagnostics"/>.</typeparam>
		/// <param name="extender">The partial class extender used to generate implementations. Must be of type <typeparamref
		/// name="TArgPartialClassExtender"/>.</param>
		/// <param name="diagnostics">The diagnostics provider used to report issues during generation. Must be of type <typeparamref
		/// name="TArgDiagnostics"/>.</param>
		/// <param name="symbol">The named type symbol representing the class or interface for which implementations are being generated.</param>
		/// <param name="compilation">The current compilation context, providing access to semantic and syntactic information.</param>
		/// <param name="sb">A <see cref="StringBuilder"/> instance used to construct the generated code.</param>
		/// <returns>A collection of <see cref="Diagnostic"/> objects representing any issues encountered during the generation
		/// process. Returns <see langword="null"/> if no diagnostics are produced.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="extender"/> is not of type <typeparamref name="TArgPartialClassExtender"/> or if
		/// <paramref name="diagnostics"/> is not of type <typeparamref name="TArgDiagnostics"/>.</exception>
		public virtual IEnumerable<Diagnostic>? GenerateImplementationsInternal<TArgPartialClassExtender, TArgDiagnostics>(
			TArgPartialClassExtender extender,
			TArgDiagnostics diagnostics,
			INamedTypeSymbol symbol,
			Compilation compilation,
			StringBuilder sb
		)
			where TArgPartialClassExtender : class, IPartialClassExtender
			where TArgDiagnostics : class, IPCEGDiagnostics
		{
			if (Interlocked.Exchange(ref _isGeneratingImplementetionsRunning, 1) != 0)
			{
				throw new InvalidOperationException("GenerateImplementationsInternal is already in progress. This method is not re-entrant.");
			}

			try
			{
				// Validate extender and diagnostics parameters for safe-guard.
				if (extender is not TPartialClassExtender validExtender)
				{
					throw new ArgumentException($"Extender must be of type {typeof(TPartialClassExtender).FullName}.", nameof(extender));
				}
				if (diagnostics is not TDiagnostics validDiagnostics)
				{
					throw new ArgumentException($"Diagnostics must be of type {typeof(TDiagnostics).FullName}.", nameof(diagnostics));
				}

				// Invoke the abstract method to generate implementations.
				return GenerateImplementations(
					validExtender, validDiagnostics, symbol, compilation, sb
				);
			}
			finally
			{
				// Reset the flag to allow future calls.
				Interlocked.Exchange(ref _isGeneratingImplementetionsRunning, 0);
			}
		}

		/// <summary>
		/// Generates implementations for the specified partial class extender.
		/// </summary>
		/// <remarks>This method is abstract and must be implemented by a derived class. It is responsible
		/// for generating code implementations based on the provided extender, diagnostics, type symbol, and
		/// compilation context.</remarks>
		/// <param name="extender">The partial class extender that provides context and functionality for generating implementations.</param>
		/// <param name="diagnostics">The diagnostics object used to report issues or warnings during the generation process.</param>
		/// <param name="symbol">The named type symbol representing the class or type for which implementations are being generated.</param>
		/// <param name="compilation">The current compilation context, providing access to semantic and syntactic information.</param>
		/// <param name="sb">A <see cref="StringBuilder"/> instance used to append generated code.</param>
		/// <returns>A collection of <see cref="Diagnostic"/> objects representing any issues or warnings encountered during the
		/// generation process, or <see langword="null"/> if no diagnostics are produced.</returns>
		public abstract IEnumerable<Diagnostic>? GenerateImplementations(
			TPartialClassExtender extender,
			TDiagnostics diagnostics,
			INamedTypeSymbol symbol,
			Compilation compilation,
			StringBuilder sb
		);
	}
}
