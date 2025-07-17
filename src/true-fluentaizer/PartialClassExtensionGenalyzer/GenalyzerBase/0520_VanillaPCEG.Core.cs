using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.AnalyzerBase;
using PartialClassExtGen.Generator;
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
    public abstract partial class VanillaPCEG<TAttribute, TPartialClassExtender, TDiagnostics>
        : PartialClassAnalyzerBase<TPartialClassExtender, TDiagnostics>, IIncrementalGenerator, IPartialClassExtender
        where TAttribute : Attribute
        where TPartialClassExtender : class, IPartialClassExtender
        where TDiagnostics : class, IPCEGDiagnostics
    {
        /// <summary>
        /// Gets the type of the target attribute associated with this instance.
        /// </summary>
        public Type TargetAttribute { get; } = typeof(TAttribute);

        /// <summary>
        /// Gets the incremental generator associated with this instance.
        /// </summary>
        IIncrementalGenerator Generator { get; }

        private int _isGeneratingImplementetions = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="VanillaPCEG "/> class.
        /// </summary>
        /// <remarks>This constructor sets up the necessary components for partial class extension
        /// functionality, including diagnostics, generator components, and the base analyzer.
        /// By default, it uses itself as an <see cref="IPartialClassExtender"/> to initialize these components.</remarks>
        protected VanillaPCEG()
        {
            // Use itself as IPartialClassExtender to initialize diagnostics and other components.
            var extender = CreateExtender();
            var diagnostics = CreateDiagnostics(extender);

            // Initialize generator components
            Generator = CreateGenerator(
                extender, diagnostics
            );

            // Initialize the base class for partial class extension functionality.
            InitializeAnalyzerBase(
                extender, diagnostics
            );
        }

        /// <summary>
        /// Creates and returns an instance of an object that implements the <see cref="IPartialClassExtender"/>
        /// interface.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to provide a custom
        /// implementation of  <see cref="IPartialClassExtender"/>. By default, it returns the current
        /// instance.</remarks>
        /// <returns>An instance of an object that implements the <see cref="IPartialClassExtender"/> interface.</returns>
        protected virtual TPartialClassExtender CreateExtender()
        {
            // Create a new instance of the extender.
            if (this is not TPartialClassExtender retval)
            {
                throw new InvalidCastException(
                    $"Cannot cast {nameof(VanillaPCEG<TAttribute, TPartialClassExtender, TDiagnostics>)} to {typeof(TPartialClassExtender).FullName}. " +
                    "Please implement {typeof(TPartialClassExtender).FullName on yout derived class to satisfy correct type."
                );
            }
            return retval;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PCEGDiagnostics"/> for analyzing partial class extensions.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to provide custom implementations of
        /// <see cref="PCEGDiagnostics"/>. By default, it creates a new instance using the provided extender.</remarks>
        /// <param name="extender">The <see cref="IPartialClassExtender"/> instance used to provide extension-related data for diagnostics.</param>
        /// <returns>A new <see cref="PCEGDiagnostics"/> instance initialized with the specified extender.</returns>
        protected virtual TDiagnostics CreateDiagnostics(TPartialClassExtender extender)
        {
            // Create a new instance of the diagnostics.
            if (new PCEGDiagnostics<TPartialClassExtender>(extender) is not TDiagnostics retval)
            {
                // If the cast fails, throw an exception indicating that the type is not compatible.
                throw new InvalidCastException(
                    $"Cannot cast {nameof(PCEGDiagnostics<TPartialClassExtender>)} to {typeof(TDiagnostics).FullName}. " +
                    "Please override the CreateDiagnostics method in your derived class to return the correct type."
                );
            }
            return retval;
        }

        /// <summary>
        /// Creates and returns a new instance of an incremental generator.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to provide a custom implementation of
        /// <see cref="IIncrementalGenerator"/>. By default, it creates a new instance of <see cref="PartialClassGenerator"/>
        /// <param name="extender">An object that provides functionality to extend partial classes.</param>
        /// <param name="diagnostics">An object used to report diagnostics during the generation process.</param>
        /// <returns>An instance of <see cref="IIncrementalGenerator"/> configured with the specified extender and diagnostics.</returns>
        protected virtual IIncrementalGenerator CreateGenerator(
            TPartialClassExtender extender, TDiagnostics diagnostics
        )
        {
            // Create a new instance of the generator.
            return new PartialClassGenerator<TPartialClassExtender, TDiagnostics>(extender, diagnostics);
        }

        /// <summary>
        /// Initializes the base class for partial class extension functionality.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to provide custom initialization logic for the analyzer base.</remarks>
        /// <param name="extender"> The extender that provides additional functionality for partial classes. Cannot be <see langword="null"/>.</param>
        /// <param name="diagnostics"> The diagnostics interface used for reporting errors and warnings. Cannot be <see langword="null"/>.</param>
        protected virtual void InitializeAnalyzerBase(
            TPartialClassExtender extender, TDiagnostics diagnostics
        )
        {
            // Initizalize parent class, implementing DiagnosticAnalyzer, using the extender and diagnostics.
            InitializeExtendeeBase(
                extender, diagnostics
            );
        }

        /// <summary>
        /// Initializes the source generator with the specified context.
        /// </summary>
        /// <remarks>This method delegates the initialization process to the <see cref="Generator"/>
        /// instance.</remarks>
        /// <param name="context">The <see cref="IncrementalGeneratorInitializationContext"/> used to configure the source generator.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            Generator.Initialize(context);
        }

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
            if (Interlocked.Exchange(ref _isGeneratingImplementetions, 1) != 0)
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
                Interlocked.Exchange(ref _isGeneratingImplementetions, 0);
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
