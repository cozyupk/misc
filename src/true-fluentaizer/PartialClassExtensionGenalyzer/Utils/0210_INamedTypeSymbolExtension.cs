using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PartialClassExtGen.Utils
{
    /// <summary>
    /// Provides extension methods for working with INamedTypeSymbol instances,
    /// including formatted names, full names, and generic type handling.
    /// </summary>
    public static class INamedTypeSymbolExtension
    {
        /// <summary>
        /// Returns a formatted name for the given INamedTypeSymbol, including generic type arguments if applicable.
        /// </summary>
        /// <param name="symbol">The INamedTypeSymbol to format.</param>
        /// <returns>A string representing the formatted name of the symbol.</returns>
        public static string GenericQualifiedName(this INamedTypeSymbol symbol)
        {
            return symbol.IsGenericType
                ? $"{symbol.Name}<{string.Join(", ", symbol.TypeArguments.Select(t => t.ToDisplayString()))}>"
                : $"{symbol.Name}";
        }

        /// <summary>
        /// Converts an INamedTypeSymbol to a file-system-safe string for use in file names.
        /// </summary>
        /// <param name="symbol">The named type symbol to convert.</param>
        /// <returns>A string safe to use as part of a generated file name.</returns>
        public static string ToSafeFileName(this INamedTypeSymbol symbol)
        {
            var sb = new StringBuilder();

            // Include namespace
            if (!symbol.ContainingNamespace.IsGlobalNamespace)
            {
                sb.Append(symbol.ContainingNamespace.ToDisplayString().Replace('.', '-'));
                sb.Append('-');
            }

            // Add the base type name
            sb.Append(symbol.Name);

            // Add generic arguments if any
            if (symbol.TypeArguments.Length > 0)
            {
                sb.Append("-");
                sb.Append(string.Join("-", symbol.TypeArguments.Select(FormatTypeArgument)));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats the specified type symbol into a string representation suitable for use in file names.
        /// </summary>
        /// <param name="typeSymbol">The type symbol to format. This can represent named types, arrays, pointers, or other type symbols.</param>
        /// <returns>A string representation of the type symbol, formatted to be safe for use in file names.  For example, array
        /// types are suffixed with "Array", pointer types with "Ptr", and generic type arguments have angle brackets
        /// replaced with underscores.</returns>
        private static string FormatTypeArgument(ITypeSymbol typeSymbol)
        {
            return typeSymbol switch
            {
                INamedTypeSymbol named => named.ToSafeFileName(),
                IArrayTypeSymbol array => $"{FormatTypeArgument(array.ElementType)}Array",
                IPointerTypeSymbol pointer => $"{FormatTypeArgument(pointer.PointedAtType)}Ptr",
                _ => typeSymbol.Name.Replace('<', '_').Replace('>', '_')
            };
        }
    }
}
