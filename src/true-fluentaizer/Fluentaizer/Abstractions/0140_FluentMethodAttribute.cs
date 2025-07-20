using System;
using System.Collections.Generic;

namespace TrueFluentaizer.Abstractions
{
    /// <summary>
    /// An attribute that marks a method as a fluent method within a fluent builder context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FluentMethodAttribute : Attribute
    {
        public string AxisNameBelongsTo { get; }
        public FluentMethodFlags Flags { get; }
        public IEnumerable<string> AxisNamesToOpen { get; }
        public FluentMethodAttribute(string axisNameBelongsTo, FluentMethodFlags flags, params string[] axisNamesToRestore)
        {
            // Validate and store parameters
            AxisNameBelongsTo = axisNameBelongsTo ?? throw new ArgumentNullException(nameof(axisNameBelongsTo));
            Flags = flags;
            AxisNamesToOpen = axisNamesToRestore ?? throw new ArgumentNullException(nameof(axisNamesToRestore));
        }
    }
}
