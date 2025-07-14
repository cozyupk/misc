using System;
using System.Text;

namespace TrueFluentaizer.Generators
{
    /// <summary>
    /// Provides a utility for building strings with support for managing indentation levels.
    /// </summary>
    /// <remarks>This class is useful for generating structured text, such as code or formatted documents, 
    /// where consistent indentation is required. It allows appending lines of text with the current  indentation
    /// applied and provides methods to increase or decrease the indentation level.</remarks>
    public class IndentedStringBuilder
    {
        /// <summary>
        /// Gets the <see cref="StringBuilder"/> instance used internally for string manipulation.
        /// </summary>
        /// 
        private StringBuilder Sb { get; }

        /// <summary>
        /// Gets or sets the current indentation level.
        /// </summary>
        private int IndentLevel { get; set; } = 0;

        /// <summary>
        /// Gets the string representation of a single indentation unit.
        /// </summary>
        private string IndentUnitString { get; } = "    ";

        /// <summary>
        /// Gets or sets the current indentation string used for formatting output.
        /// </summary>
        private string CurrentIndent { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedStringBuilder"/> class using the specified <see
        /// cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to be used for appending text. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sb"/> is <see langword="null"/>.</exception>
        public IndentedStringBuilder(StringBuilder sb)
        {
            Sb = sb ?? throw new ArgumentNullException(nameof(sb), "StringBuilder cannot be null.");
        }

        /// <summary>
        /// Appends the specified string to the current content, followed by a newline character.
        /// </summary>
        /// <remarks>The appended string is prefixed with the current indentation level.</remarks>
        /// <param name="value">The string to append. If <paramref name="value"/> is <see langword="null"/>, no action is taken.</param>
        public void AppendLine(string value)
        {
            Sb.Append(CurrentIndent);
            Sb.AppendLine(value);
        }

        /// <summary>
        /// Increases the current indentation level by one.
        /// </summary>
        /// <remarks>This method increments the indentation level and appends the configured indentation
        /// unit to the current indentation string. It is typically used to manage hierarchical or nested formatting in
        /// scenarios such as logging or text generation.</remarks>
        public void PushIndent()
        {
            IndentLevel++;
            CurrentIndent += IndentUnitString;
        }

        /// <summary>
        /// Decreases the current indentation level by one.
        /// </summary>
        /// <remarks>This method reduces the indentation level and updates the current indentation string
        /// accordingly.  If the indentation level is already at zero, an exception is thrown.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the current indentation level is zero or less, as the indentation level cannot be negative.</exception>
        public void PopIndent()
        {
            if (IndentLevel <= 0)
            {
                throw new InvalidOperationException("Indent level cannot be negative.");
            }
            IndentLevel--;
            CurrentIndent = CurrentIndent.Substring(0, Math.Max(0, CurrentIndent.Length - IndentUnitString.Length));
        }

        /// <summary>
        /// Returns a string representation of the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Sb.ToString();
    }
}
