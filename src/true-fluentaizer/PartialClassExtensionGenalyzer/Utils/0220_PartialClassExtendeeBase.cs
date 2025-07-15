using PartialClassExtGen.Abstractions.Common;
using System;

namespace PartialClassExtGen.Utils
{
    /// <summary>
    /// Serves as a base class for types that require an extensibility mechanism through a generic partial class
    /// extender.
    /// </summary>
    /// <remarks>This class provides a thread-safe, lazily initialized static instance of the specified
    /// extender type.  Derived classes can use the <see cref="Extender"/> property to access the extender instance,
    /// enabling  extensibility features without requiring direct instantiation or management of the extender.</remarks>
    /// <typeparam name="TPartialClassExtender">The type of the partial class extender, which must implement <see cref="IPartialClassExtender"/> and have a
    /// parameterless constructor.</typeparam>
    public class PartialClassExtendeeBase<TPartialClassExtender>
        where TPartialClassExtender : IPartialClassExtender, new()
    {
        /// <summary>
        /// Provides a lazily initialized instance of <see cref="IPartialClassExtender"/>.
        /// </summary>
        /// <remarks>The instance is created using a thread-safe, publication-only mode, ensuring that
        /// only one instance is created and published, even in a multithreaded environment. This is useful for
        /// scenarios where the initialization of the extender is expensive or should be deferred until first
        /// use.</remarks>
        private static readonly Lazy<IPartialClassExtender> _extender
            = new(() => new TPartialClassExtender(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets the static instance of the partial class extender.
        /// </summary>
        protected static IPartialClassExtender Extender => _extender.Value;

        public IPartialClassExtender GetExtender() => Extender;
    }
}
