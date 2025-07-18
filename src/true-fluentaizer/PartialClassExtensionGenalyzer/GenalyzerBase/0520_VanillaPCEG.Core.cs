using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Analyzer;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Abstractions.Generator;
using PartialClassExtGen.AnalyzerBase;
using PartialClassExtGen.Generator;
using System;
using System.Collections.Generic;

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
    public abstract partial class VanillaPCEG<TAttribute, TPartialClassExtender, TDiagnostics, TTargetClassMeta>
        : ExtendedAnalyzerBase<TPartialClassExtender, TDiagnostics>, IIncrementalGenerator, IPartialClassExtender
        where TAttribute : Attribute
        where TPartialClassExtender : class, IPartialClassExtender
        where TDiagnostics : class, IPCEGDiagnostics
        where TTargetClassMeta : ITargetClassMeta
    {
        /// <summary>
        /// Gets the type of the target attribute associated with this instance.
        /// </summary>
        public Type TargetAttribute { get; } = typeof(TAttribute);

        /// <summary>
        /// Gets the incremental generator associated with this instance.
        /// </summary>
        IIncrementalGenerator Generator { get; }

        /// <summary>
        /// Indicates whether the process of generating implementations is currently running.
        /// </summary>
        /// <remarks>This field is used internally to track the state of the implementation generation
        /// process. A value of 0 indicates that the process is not running, while a non-zero value indicates that it is
        /// active.</remarks>
        private int _isGeneratingImplementetionsRunning = 0;

        /// <summary>
        /// Indicates whether the process of retrieving syntax node rules is currently running.
        /// </summary>
        /// <remarks>This field is used internally to track the state of the syntax node rules retrieval
        /// process. A value of 0 indicates that the process is not running, while a non-zero value indicates that it is
        /// active.</remarks>
        private int _isGetSyntaxNodeRulesRunning = 0;

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
            var analyzerRules = GetSyntaxNodeRulesInternal(
                extender, diagnostics
            );
            InitializeAnalyzerBase(
                analyzerRules
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
                    $"Cannot cast {nameof(VanillaPCEG<TAttribute, TPartialClassExtender, TDiagnostics, TTargetClassMeta>)} to {typeof(TPartialClassExtender).FullName}. " +
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
            return new PartialClassGenerator<TPartialClassExtender, TDiagnostics, TTargetClassMeta>(extender, diagnostics);
        }

        /// <summary>
        /// Initializes the analyzer with the specified syntax node rules.
        /// </summary>
        /// <param name="analyzerRules">A collection of syntax node rules to be used by the analyzer.  This parameter cannot be <see
        /// langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyzerRules"/> is <see langword="null"/>.</exception>
        protected virtual void InitializeAnalyzerBase(
            IEnumerable<ISyntaxNodeRule> analyzerRules
        )
        {
            // Validate the parameters are not null.
            _ = analyzerRules ?? throw new ArgumentNullException(nameof(analyzerRules));

            // Initizalize parent class, implementing DiagnosticAnalyzer, using the extender and diagnostics.
            InitializeExtendeeBase(
                analyzerRules
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
    }
}
