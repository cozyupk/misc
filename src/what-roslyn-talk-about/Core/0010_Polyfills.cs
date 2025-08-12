#pragma warning disable IDE0130 // Namespace is not aligned with folder structure

// IsExternalInit
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved for compiler use to support the `init` accessor in C# 9.0 and later.
    /// </summary>
    /// <remarks>This type is used internally by the compiler to enable the `init` accessor, which allows
    /// properties to be set during object initialization but not modified thereafter.  It is not intended for direct
    /// use in application code.</remarks>
    internal static class IsExternalInit { }
}
#endif

#if !NET7_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
    {
        public string FeatureName { get; } = featureName;
        public bool IsOptional { get; init; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}
#endif

#pragma warning restore IDE0130 // Namespace is not aligned with folder structure
