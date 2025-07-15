using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Abstractions.Generator;
using PartialClassExtGen.AnalyzerBase;
using PartialClassExtGen.GeneratorBase;

namespace PartialClassExtGen.GenalyzerBase
{
    public abstract class PartialClassExtensionGenalyzerBase<TPartialClassExtender>
        : PartialClassExtensionGenalyzerBase<
            TPartialClassExtender,
            PartialClassSyntaxProvider<TPartialClassExtender>,
            PartialSourceOutput<TPartialClassExtender>
          >
        where TPartialClassExtender : IPartialClassExtender, new()
    {
    }

    public abstract class PartialClassExtensionGenalyzerBase<
        TPartialClassExtender,
        TClassSyntaxProvider,
        TPartialSourceOutput
    > : PartialClassAnalyzerBase<TPartialClassExtender>, IIncrementalGenerator
        where TPartialClassExtender : IPartialClassExtender, new()
        where TClassSyntaxProvider : IClassSyntaxProvider, new()
        where TPartialSourceOutput : ISourceOutput, new()
    {
        /// <summary>
        /// Gets the static instance of the <see cref="TClassSyntaxProvider"/> used for providing class syntax
        /// functionality.
        /// </summary>
        private static TClassSyntaxProvider ClassSyntaxProvider { get; } = new TClassSyntaxProvider();

        /// <summary>
        /// Gets the partial source output instance used for processing or configuration.
        /// </summary>
        private static TPartialSourceOutput PartialSourceOutput { get; } = new TPartialSourceOutput();

        /// <summary>
        /// Initializes the incremental generator by configuring the syntax and compilation providers to identify target
        /// classes and generate source output.
        /// </summary>
        /// <remarks>This method sets up the generator to process class declarations with attributes,
        /// combining the identified classes with the compilation context, and registering the source output for further
        /// processing.</remarks>
        /// <param name="context">The <see cref="IncrementalGeneratorInitializationContext"/> used to configure the generator's syntax and
        /// compilation providers.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Identify all target classes
            var goingToBeExtendedClasses
                = context.SyntaxProvider.ForAttributeWithMetadataName(
                    ClassSyntaxProvider.TargetAttribute.FullName,
                    static (s, _) => true, // s is ClassDeclarationSyntax cds,
                    static (ctx, _) => ClassSyntaxProvider.GetExtensionTarget(ctx)
                  );

            // Combine with compilation
            var compilationAndClasses = context.CompilationProvider.Combine(goingToBeExtendedClasses.Collect());

            // Register the source output for the identified classes
            context.RegisterSourceOutput(
                compilationAndClasses, (spc, source)
                    => PartialSourceOutput.SourceOutput(
                        spc,
                        source
                    )
            );
        }
    }
}