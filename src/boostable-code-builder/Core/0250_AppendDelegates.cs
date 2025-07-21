using Boostable.CodeBuilding.Abstractions;
using System;
using System.Text;

namespace Boostable.CodeBuilding.Core
{
    /// <summary>
    /// Provides functionality for appending text and lines of text to an output destination.
    /// </summary>
    /// <remarks>This class encapsulates two actions, <see cref="Append"/> and <see cref="AppendLine"/>, which
    /// can be used to append text to a specified <see cref="StringBuilder"/> or delegate the operations to a provided
    /// <see cref="ICodeComposer"/>. The behavior of these actions depends on the constructor parameters.</remarks>
    internal record AppendDelegates
    {
        /// <summary>
        /// Gets the action used to append a string to the underlying output.
        /// </summary>
        /// <remarks>The provided action is typically used to handle string output in a custom manner,
        /// such as appending to a log,  a file, or another output stream. Ensure the action is not null before invoking
        /// it.</remarks>
        public Action<string> Append { get; }

        /// <summary>
        /// Gets the action used to append a line of text.
        /// </summary>
        public Action<string> AppendLine { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppendDelegates "/> class, configuring append and append-line
        /// operations based on the provided <see cref="StringBuilder"/> or a previous <see cref="ICodeComposer"/>.
        /// </summary>
        /// <remarks>This constructor sets up two delegates, <c>Append</c> and <c>AppendLine</c>, which
        /// allow appending text either to the provided <paramref name="defaultStringBuilder"/> or to the <paramref
        /// name="prevCodeComposer"/>, depending on whether the latter is null. If <paramref name="prevCodeComposer"/>
        /// is not null, its methods take precedence.</remarks>
        /// <param name="defaultStringBuilder">The <see cref="StringBuilder"/> instance to use for appending text if no previous <see
        /// cref="ICodeComposer"/> is provided.</param>
        /// <param name="prevCodeComposer">An optional <see cref="ICodeComposer"/> instance. If provided, its append and append-line methods will be
        /// used instead of the <paramref name="defaultStringBuilder"/>.</param>
        public AppendDelegates(StringBuilder? defaultStringBuilder, ICodeComposer? prevCodeComposer)
        {
            // Validate the defaultStringBuilder parameter.
            if (defaultStringBuilder == null) throw new ArgumentNullException(nameof(defaultStringBuilder));

            // Initialize Append and AppendLine actions based on whether a previous composer is provided.
            Append = prevCodeComposer != null
                ? s => prevCodeComposer.Append(s)
                : s => defaultStringBuilder?.Append(s);
            AppendLine = prevCodeComposer != null
                ? s => prevCodeComposer.AppendLine(s)
                : s => defaultStringBuilder?.AppendLine(s);
        }
    }
}
