using Boostable.CodeBuilding.Abstractions;
using System.Text;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Provides functionality to build and compose code using a specified default composer type.
    /// </summary>
    /// <remarks>This class serves as a specialized implementation of <see cref="CodeBuidlerWithoutDefaultComposer"/> that uses
    /// a default composer type to facilitate code generation. It provides methods to initialize and manage the code
    /// composition process.</remarks>
    /// <typeparam name="TDefaultComposer">The default composer type used for code composition. Must implement <see cref="ICodeComposer"/> and have a
    /// parameterless constructor.</typeparam>
    public class CodeBuilder<TDefaultComposer> : CodeBuidlerWithoutDefaultComposer
        where TDefaultComposer : class, ICodeComposer, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBuilder"/> class using the specified <see
        /// cref="StringBuilder"/>.
        /// </summary>
        /// <remarks>This constructor allows the <see cref="CodeBuilder"/> to operate on an existing <see
        /// cref="StringBuilder"/> instance, enabling code generation or manipulation directly on the provided string
        /// buffer.</remarks>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to be used for building code.</param>
        /// <param name="initialMaxStaeckingDepth">The initial maximum stacking depth for nested operations. Defaults to 0x10.</param>
        public CodeBuilder(StringBuilder sb, int initialMaxStaeckingDepth = 0x10) : base(sb, initialMaxStaeckingDepth)
        {
            // No additional initialization needed here.
        }

        /// <summary>
        /// Opens a new instance of the default composer with the specified configuration.
        /// </summary>
        /// <param name="sb">An optional <see cref="StringBuilder"/> to use for composing output. If null, a new instance will be created
        /// internally.</param>
        /// <param name="maxStackingDepth">The maximum allowed depth for nested compositions. Must be a positive integer.</param>
        /// <returns>An instance of the default composer of type <typeparamref name="TDefaultComposer"/>.</returns>
        public static TDefaultComposer Open(StringBuilder? sb = null, int maxStackingDepth = 0x10)
        {
            return CodeBuidlerWithoutDefaultComposer.Open<TDefaultComposer>(sb, maxStackingDepth);
        }
    }

    /// <summary>
    /// Provides functionality for building code using a default composer type.
    /// </summary>
    /// <remarks>This class serves as an alias for <see cref="CodeBuilder{CodeComposerBase}"/> with the
    /// default composer type set to <see cref="CodeComposerBase"/>. It simplifies the creation of code builders for
    /// common use cases.</remarks>
    public class CodeBuilder : CodeBuilder<CodeComposerBase>
    {
        // just a simple alias for the default composer type.

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBuilder"/> class,  using the specified <see
        /// cref="StringBuilder"/> as the underlying buffer.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to use as the underlying buffer.  This cannot be <see
        /// langword="null"/>.</param>
        public CodeBuilder(StringBuilder sb) : base(sb)
        {
            // No additional initialization needed here.
        }
    }
}
