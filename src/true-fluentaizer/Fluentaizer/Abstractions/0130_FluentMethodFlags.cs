namespace TrueFluentaizer.Abstractions
{
    /// <summary>
    /// Specifies options for configuring fluent behaviors in a system.
    /// </summary>
    /// <remarks>This enumeration provides flags that can be combined using a bitwise OR operation to specify
    /// multiple options.</remarks>
    public enum FluentMethodFlags
    {
        None = 0,                        // No options specified
        HasOnlyThisMethod = 1 << 0,      // Indicates that the method in the axis is the only one and axis name is not typoed
        KeepAxisOpen = 1 << 1,           // Indicates that the axis should remain open after the method is clalled
        CloseAllAxes = 1 << 2,           // Indicates that all axes should be closed after the method is called
    }
}
