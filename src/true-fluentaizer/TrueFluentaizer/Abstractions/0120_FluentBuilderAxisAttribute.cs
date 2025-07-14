using System;

namespace TrueFluentaizer.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FluentBuilderAxisAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the axis to which this attribute applies.
        /// </summary>
        public string AxisName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the default state is closed.
        /// </summary>
        public bool IsDefaultClosed { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum number of applications that can be processed.
        /// </summary>
        public int MaxNumberOfApplyed { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the associated item is mandatory.
        /// </summary>
        public bool IsMandatory { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentBuilderAxisAttribute"/> class with the specified axis name.
        /// </summary>
        public FluentBuilderAxisAttribute(string axisName)
        {
            // Validate and store the axis name
            AxisName = axisName ?? throw new ArgumentNullException(nameof(axisName), "Axis name cannot be null.");
        }
    }
}
