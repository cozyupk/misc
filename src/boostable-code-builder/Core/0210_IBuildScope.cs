using Boostable.CodeBuilding.Abstractions;
using System.Collections.Generic;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Defines a scope for managing code composition operations, including posting builder entries and opening new code
    /// composer instances.
    /// </summary>
    /// <remarks>The <see cref="IBuildScope"/> interface provides methods for interacting with code composers
    /// and managing the lifecycle of code composition operations. It allows posting collections of builder entries to a
    /// composer and creating new instances of specific composer types.</remarks>
    internal interface IBuildScope

    {
        /// <summary>
        /// Posts a collection of code builder entries to the current composer or root StringBuilder if exists and no composer is available.
        /// </summary>
        /// <param name="entries">A collection of code builder entries to be posted. Cannot be <see langword="null"/> or empty.</param>
        void Post(IEnumerable<IComposingEntry> entries);

        /// <summary>
        /// Opens a new instance of the specified code composer type and initializes it using the provided composer.
        /// </summary>
        /// <typeparam name="TCodeComposer">The type of the code composer to open. Must be a class that implements <see cref="ICodeComposer"/> and has a
        /// parameterless constructor.</typeparam>
        /// <param name="cb">The composer instance used to initialize the new code composer. Cannot be <see langword="null"/>.</param>
        /// <param name="maxStackingDepth">The maximum stacking depth for the new composer. This parameter is optional and the default value is -1.</param>
        /// <returns>A new instance of <typeparamref name="TCodeComposer"/> initialized with the provided composer.</returns>
        TCodeComposer Open<TCodeComposer>(ICodeComposer cb, int maxStackingDepth)
            where TCodeComposer : class, ICodeComposer, new();

        /// <summary>
        /// Notifies the specified <see cref="ICodeComposer"/> that the current object has been disposed.
        /// </summary>
        /// <param name="composer">The <see cref="ICodeComposer"/> to notify. Cannot be <see langword="null"/>.</param>
        void NotifyDisposed(ICodeComposer composer);
    }
}
