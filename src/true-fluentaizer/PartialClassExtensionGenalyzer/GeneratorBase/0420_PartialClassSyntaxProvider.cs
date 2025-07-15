using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Abstractions.Generator;
using PartialClassExtGen.Utils;
using System;

namespace PartialClassExtGen.GeneratorBase
{
    public sealed class PartialClassSyntaxProvider<TPartialClassExtender> 
        : PartialClassExtendeeBase<TPartialClassExtender>, IClassSyntaxProvider
        where TPartialClassExtender : IPartialClassExtender, new()
    {
        /// <summary>
        /// Gets the fully qualified metadata name to which this syntax provider applies.
        /// </summary>
        public Type TargetAttribute => Extender.TargetAttribute;

        private ITargetClassMeta? CachedBuilderClassMeta { get; }

        public ITargetClassMeta? GetExtensionTarget(GeneratorAttributeSyntaxContext context)
        {
            var retval = GetExtensionTargetInternal(context);
            if (retval is null)
            {
                return null;
            }
            return retval;
        }

        private ITargetClassMeta? GetExtensionTargetInternal(GeneratorAttributeSyntaxContext context)
        {
            // Ensure the node is a class declaration
            var classDecl = (ClassDeclarationSyntax)context.TargetNode;
            if (context.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol symbol)
                return null;

            // Retutn a new instance of BuilderClassMeta with the symbol
            return new BuilderClassMeta(symbol);
        }
    }
}
