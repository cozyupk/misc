using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Abstractions.Generator;

namespace PartialClassExtGen.Generator
{
    /// <summary>
    /// Provides functionality to generate partial class implementations for classes annotated with a specific
    /// attribute.
    /// </summary>
    /// <remarks>This class implements the <see cref="IIncrementalGenerator"/> interface to identify target
    /// classes based on a specified attribute and generate source code for extending those classes. It is designed to
    /// be used in source generators and integrates with the incremental generator infrastructure.</remarks>
    public class PartialClassGenerator<TPartialClassExtender, TDiagnostics>
        : IIncrementalGenerator
        where TPartialClassExtender : class, IPartialClassExtender
        where TDiagnostics : class, IPCEGDiagnostics
    {
        /// <summary>
        /// Gets the provider responsible for identifying target classes from syntax.
        /// </summary>
        private IClassSyntaxProvider ClassSyntaxProvider { get; }

        /// <summary>
        /// Gets the source output processor responsible for generating implementation code.
        /// </summary>
        private ISourceOutput SourceOutput { get; }

        /// <summary>
        /// Gets the fully qualified name of the target attribute.
        /// </summary>
        private string TargetAttributeFullName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialClassGenerator "/> class.
        /// </summary>
        /// <remarks>This constructor initializes the class syntax provider and source output using the
        /// provided extender and diagnostics.</remarks>
        /// <param name="extender">The extender used to provide partial class extension functionality. Cannot be <see langword="null"/>.</param>
        /// <param name="diagnostics">The diagnostics interface used for reporting errors and warnings. Cannot be <see langword="null"/>.</param>
        public PartialClassGenerator(
            TPartialClassExtender extender,
            TDiagnostics diagnostics
        ) {
            // Initialize the class syntax provider and source output with the extender and diagnostics
            ClassSyntaxProvider = new PartialClassSyntaxProvider<TPartialClassExtender, TDiagnostics>(extender, diagnostics);
            SourceOutput = new PartialSourceOutput<TPartialClassExtender, TDiagnostics>(extender, diagnostics);

            // Get the fully qualified name for the target attribute
            TargetAttributeFullName = extender.TargetAttribute.FullName;
        }

        /// <summary>
        /// Initializes the incremental generator by configuring the syntax and compilation providers to identify target
        /// classes and generate source output.
        /// </summary>
        /// <remarks>This method sets up the generator to process classes annotated with a specific
        /// attribute, combines the identified classes with the compilation, and registers the source output for further
        /// processing. It is intended to be called during the generator's initialization phase.</remarks>
        /// <param name="context">The <see cref="IncrementalGeneratorInitializationContext"/> used to configure the generator's syntax and
        /// compilation providers, and to register source output.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Identify all target classes
            var goingToBeExtendedClasses
                = context.SyntaxProvider.ForAttributeWithMetadataName(
                    TargetAttributeFullName,
                    static (s, _) => true, // s is ClassDeclarationSyntax cds,
                    (ctx, _) => ClassSyntaxProvider.GetExtensionTarget(ctx)
                  );

            // Combine with compilation
            var compilationAndClasses = context.CompilationProvider.Combine(goingToBeExtendedClasses.Collect());

            // Register the source output for the identified classes
            context.RegisterSourceOutput(
                compilationAndClasses, (spc, source)
                    => SourceOutput.SourceOutput(
                        spc,
                        source
                    )
            );
        }
    }
}