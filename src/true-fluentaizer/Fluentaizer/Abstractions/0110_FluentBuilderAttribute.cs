using System;

namespace TrueFluentaizer.Abstractions
{
    /// <summary>
    /// Indicates that a class is a fluent builder, typically used to construct instances of another type in a fluent,
    /// chainable manner.
    /// </summary>
    /// <remarks>This attribute is applied to classes that implement a fluent builder pattern, allowing
    /// developers to identify and work with such classes programmatically. It is intended for use on class declarations
    /// and cannot be applied multiple times to the same class.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FluentBuilderAttribute : Attribute
    {
    }
}
