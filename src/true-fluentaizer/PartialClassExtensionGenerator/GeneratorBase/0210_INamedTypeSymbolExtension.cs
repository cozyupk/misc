using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PartialClassExtensionGenerator.GeneratorBase
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

        /*
        /// <summary>
        /// Gets the fully qualified metadata name of the symbol, including namespace and type parameters.
        /// Example: "MyNamespace.MyClass&lt;T&gt;"
        /// </summary>
        /// <param name="symbol">The named type symbol.</param>
        /// <returns>A fully qualified name with generic type parameters, if any.</returns>
        public static string GetFullyQualifiedName(this INamedTypeSymbol symbol)
        {
            var ns = symbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : symbol.ContainingNamespace.ToDisplayString();

            var name = symbol.Name;

            if (symbol.TypeArguments.Length > 0)
            {
                var args = string.Join(", ", symbol.TypeArguments.Select(arg => arg.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                name += $"<{args}>";
            }

            return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
        }

        /// <summary>
        /// Gets the flattened name of the symbol, including arity if it is generic.
        /// Example: "MyClass`2"
        /// </summary>
        /// <param name="symbol">The named type symbol.</param>
        /// <returns>The flattened name (e.g., MyClass`1 for generics).</returns>
        public static string GetArityName(this INamedTypeSymbol symbol)
        {
            return symbol.Arity > 0 ? $"{symbol.Name}`{symbol.Arity}" : symbol.Name;
        }

        /// <summary>
        /// Determines whether the symbol represents a generic type with specific number of type parameters.
        /// </summary>
        /// <param name="symbol">The named type symbol.</param>
        /// <param name="arity">The expected number of generic parameters.</param>
        /// <returns>True if the symbol is generic with the given arity; otherwise, false.</returns>
        public static bool IsGenericWithArity(this INamedTypeSymbol symbol, int arity)
        {
            return symbol.IsGenericType && symbol.Arity == arity;
        }

        /// <summary>
        /// Gets all base types and interfaces implemented by the symbol, recursively.
        /// </summary>
        /// <param name="symbol">The named type symbol.</param>
        /// <returns>An enumerable of all base types and interfaces.</returns>
        public static IEnumerable<INamedTypeSymbol> GetAllBaseTypesAndInterfaces(this INamedTypeSymbol symbol)
        {
            if (symbol.BaseType is not null)
            {
                foreach (var baseType in symbol.BaseType.GetAllBaseTypesAndInterfaces())
                    yield return baseType;

                yield return symbol.BaseType;
            }

            foreach (var iface in symbol.AllInterfaces)
            {
                yield return iface;
            }
        }

        /// <summary>
        /// Returns true if the symbol is declared as partial in any of its declarations.
        /// </summary>
        /// <param name="symbol">The named type symbol.</param>
        /// <returns>True if any declaration is marked partial; otherwise, false.</returns>
        public static bool IsDeclaredPartial(this INamedTypeSymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
                .Any(cls => cls.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)));
        }
        */
    }
}
