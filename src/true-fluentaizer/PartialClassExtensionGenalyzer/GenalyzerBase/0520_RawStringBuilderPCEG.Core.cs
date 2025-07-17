using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.AnalyzerBase;
using PartialClassExtGen.Generator;
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
    public abstract partial class RawStringBuilderPCEG<TAttribute> : PartialClassAnalyzerBase, IIncrementalGenerator, IPartialClassExtender
        where TAttribute : Attribute
    {
        /// <summary>
        /// Gets the incremental generator associated with this instance.
        /// </summary>
        IIncrementalGenerator Generator { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawStringBuilderPCEG"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the necessary components for partial class extension
        /// functionality, including diagnostics, generator components, and the base analyzer.
        /// By default, it uses itself as an <see cref="IPartialClassExtender"/> to initialize these components.</remarks>
        protected RawStringBuilderPCEG()
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
        protected virtual IPartialClassExtender CreateExtender()
        {
            // Create a new instance of the extender.
            return this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PCEGDiagnostics"/> for analyzing partial class extensions.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to provide custom implementations of
        /// <see cref="PCEGDiagnostics"/>. By default, it creates a new instance using the provided extender.</remarks>
        /// <param name="extender">The <see cref="IPartialClassExtender"/> instance used to provide extension-related data for diagnostics.</param>
        /// <returns>A new <see cref="PCEGDiagnostics"/> instance initialized with the specified extender.</returns>
        protected virtual PCEGDiagnostics CreateDiagnostics(IPartialClassExtender extender)
        {
            // Create a new instance of the diagnostics.
            return new PCEGDiagnostics(extender);
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
            IPartialClassExtender extender, PCEGDiagnostics diagnostics
        ) {
            // Create a new instance of the generator.
            return new PartialClassGenerator(extender, diagnostics);
        }

        /// <summary>
        /// Initializes the base class for partial class extension functionality.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to provide custom initialization logic for the analyzer base.</remarks>
        /// <param name="extender"> The extender that provides additional functionality for partial classes. Cannot be <see langword="null"/>.</param>
        /// <param name="diagnostics"> The diagnostics interface used for reporting errors and warnings. Cannot be <see langword="null"/>.</param>
        protected virtual void InitializeAnalyzerBase(
            IPartialClassExtender extender, PCEGDiagnostics diagnostics
        ) {
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
        /// Gets the type of the target attribute associated with this instance.
        /// </summary>
        public Type TargetAttribute { get; } = typeof(TAttribute);

        /// <summary>
        /// Generates implementations for the specified type symbol and appends the generated code to the provided <see
        /// cref="StringBuilder"/>.
        /// </summary>
        /// <remarks>This method analyzes the provided type symbol and generates code based on its
        /// structure and attributes. The generated code is appended to the provided <see cref="StringBuilder"/>
        /// instance. If any issues are encountered during the generation process, they are returned as
        /// diagnostics.</remarks>
        /// <param name="symbol">The type symbol for which implementations are to be generated. Cannot be <see langword="null"/>.</param>
        /// <param name="compilation">The current compilation context used to analyze and generate code. Cannot be <see langword="null"/>.</param>
        /// <param name="sb">The <see cref="StringBuilder"/> to which the generated code will be appended. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A collection of <see cref="Diagnostic"/> objects representing any issues encountered during code generation,
        /// or <see langword="null"/> if no diagnostics are produced.</returns>
        public abstract IEnumerable<Diagnostic>? GenerateImplementations(INamedTypeSymbol symbol, Compilation compilation, StringBuilder sb);
    }
}
