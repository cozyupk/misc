using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Abstractions.Generator;
using PartialClassExtGen.Utils;
using System;

namespace PartialClassExtGen.Generator
{
    /// <summary>
    /// Provides functionality for analyzing and extending partial class syntax in source generators.
    /// </summary>
    /// <remarks>This class is designed to work with partial class syntax in source generators, leveraging an
    /// implementation of <see cref="IPartialClassExtender"/> to extend functionality and <see
    /// cref="IPCEGDiagnostics"/> to report diagnostics. It identifies target metadata for extension methods
    /// and facilitates metadata retrieval for partial class generation.</remarks>
    public sealed class PartialClassSyntaxProvider<TPartialClassExtender, TDiagnostics>
        : PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics>, IClassSyntaxProvider
        where TPartialClassExtender : class, IPartialClassExtender
        where TDiagnostics : class, IPCEGDiagnostics
    {
        /// <summary>
        /// Gets the fully qualified metadata name via <see cref="Type"/> for Roslyn's metadata-based filtering.
        /// </summary>
        public Type TargetAttribute => Extender.TargetAttribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialClassSyntaxProvider{TPartialClassExtender,
        /// TPCEGDiagnostics}"/> class.
        /// </summary>
        /// <param name="extender">An instance of <typeparamref name="TPartialClassExtender"/> that provides functionality for extending
        /// partial class syntax.</param>
        /// <param name="diagnostics">An instance of <typeparamref name="TDiagnostics"/> used for handling diagnostics related to partial
        /// class generation.</param>
        public PartialClassSyntaxProvider(
            TPartialClassExtender extender, TDiagnostics diagnostics
        ) : base(extender, diagnostics)
        {
            // No additional initialization needed here.
        }

        /// <summary>
        /// Retrieves metadata for the target class associated with the specified generator context.
        /// </summary>
        /// <param name="context">The <see cref="GeneratorAttributeSyntaxContext"/> that provides information about the target node and its
        /// associated semantic model.</param>
        /// <returns>An instance of <see cref="ITargetClassMeta"/> representing the metadata of the target class, or <see
        /// langword="null"/> if the target node is not a valid class declaration or its symbol cannot be resolved.</returns>
        public ITargetClassMeta? GetExtensionTarget(GeneratorAttributeSyntaxContext context)
        {
            // Ensure the node is a class declaration
            var classDecl = (ClassDeclarationSyntax)context.TargetNode;
            if (context.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol symbol)
                return null;

            // Return a new instance of BuilderClassMeta with the symbol
            return new BuilderClassMeta(symbol);
        }
    }
}
