using System.Text;

namespace PartialClassExtGen.Genalyzer
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Determines whether the content of the <see cref="StringBuilder"/> ends with a newline character.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to check.</param>
        /// <returns><see langword="true"/> if the content ends with a newline character (either '\n' or "\r\n"); otherwise, <see
        /// langword="false"/>.</returns>
        public static bool IsEndsWithNewLine(this StringBuilder sb)
        {
            if (sb.Length == 0) return false;

            if (sb.Length >= 2 &&
                sb[sb.Length - 2] == '\r' &&
                sb[sb.Length - 1] == '\n')
                return true;

            if (sb[sb.Length - 1] == '\n')
                return true;

            return false;
        }

        /// <summary>
        /// Ensures that the <see cref="StringBuilder"/> ends with a newline character.
        /// </summary>
        /// <remarks>If the <see cref="StringBuilder"/> does not already end with a newline character,
        /// this method appends one.</remarks>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to modify. Cannot be <see langword="null"/>.</param>
        public static void ForceBreakLine(this StringBuilder sb)
        {
            if (!sb.IsEndsWithNewLine())
            {
                sb.AppendLine();
            }
        }

        /// <summary>
        /// Ensures that the current line in the <see cref="StackedStringBuilder"> is terminated with a line break.
        /// </summary>
        /// <remarks>If the <see cref="StackedStringBuilder"> does not already end with a terminated
        /// entry, this method appends a line break. This ensures that subsequent entries are added on a new
        /// line.</remarks>
        /// <param name="ssb">The <see cref="StackedStringBuilder"> instance to operate on. Cannot be null.</param>
        public static void ForceBreakLine(this StackedStringBuilder ssb)
        {
            if (!ssb.IsEndsWithTerminatedEntry())
            {
                ssb.AppendLine();
            }
        }
    }
}
