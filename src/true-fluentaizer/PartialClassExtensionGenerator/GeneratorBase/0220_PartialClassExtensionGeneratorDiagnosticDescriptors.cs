using Microsoft.CodeAnalysis;
using PartialClassExtensionGenerator.Abstractions;
using System;


namespace PartialClassExtensionGenerator.GeneratorBase
{
    /// <summary>
    /// Provides diagnostic descriptors for errors and issues encountered during the generation of partial class
    /// extensions.
    /// </summary>
    /// <remarks>This class defines diagnostic descriptors for various scenarios, such as missing 'partial'
    /// modifiers, exceptions thrown during code generation, and unexpected errors. These descriptors are used to report
    /// issues to the user during the source generation process.</remarks>
    /// <typeparam name="TExtender">The type of the partial class extender, which must implement <see cref="IPartialClassExtender"/> and have a
    /// parameterless constructor.</typeparam>
    internal static class PartialClassExtensionGeneratorDiagnosticDescriptors<TExtender>
        where TExtender : IPartialClassExtender, new()
    {
        /// <summary>
        /// Gets the singleton instance of the partial class extender.
        /// </summary>
        static Lazy<IPartialClassExtender> Extender { get; } = new (() => new TExtender());

        /// <summary>
        /// Creates a diagnostic descriptor indicating that a class is missing the 'partial' modifier.
        /// </summary>
        /// <param name="symbol">The symbol representing the class that is missing the 'partial' modifier.</param>
        /// <returns>A <see cref="DiagnosticDescriptor"/> that describes the diagnostic issue, including the class name and the
        /// requirement to mark it as 'partial' for proper code generation.</returns>
        public static DiagnosticDescriptor EXGEN0001_Missing_Partial_Modifier(INamedTypeSymbol symbol)
            => new(
                        "EXGEN0001",
                        "Missing Partial Modifier",
                        $"The class '{symbol}' must be marked as 'partial' to enable {Extender.Value.ExtentionName} generation.",
                        "Usage",
                        DiagnosticSeverity.Error,
                        true
               );

        /// <summary>
        /// Creates a diagnostic descriptor for an exception thrown during the invocation of the <c>IsTargetClass</c>
        /// method.
        /// </summary>
        /// <param name="symbol">The symbol being generated extension code at the exception thrown.</param>
        /// <param name="ex">The exception that was thrown during the method invocation. Cannot be <see langword="null"/>.</param>
        /// <param name="externder">The instance of the <c>IPartialClassExtender</c> that caused the exception. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A <see cref="DiagnosticDescriptor"/> representing the error, including the exception message and the type of
        /// the extender instance.</returns>
        public static DiagnosticDescriptor EXGEN0002_IsTargetClass_ThrewException(INamedTypeSymbol symbol, Exception ex, IPartialClassExtender externder)
            => new(
                    "EXGEN0002",
                    "IsTargetClass Threw An Exception.",
                    $"{symbol}: ({ex.Message}) from instance of {externder.GetType().Name} at Invoking IsTargetClass() method while {Extender.Value.ExtentionName} generation",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
               );

        /// <summary>
        /// Creates a diagnostic descriptor for the EXGEN0003 error, indicating that an exception was thrown during the
        /// invocation of the <c>GenerateImplementations</c> method.
        /// </summary>
        /// <param name="symbol">The symbol being generated extension code at the exception thrown.</param>
        /// <param name="ex">The exception that was thrown. Must not be <see langword="null"/>.</param>
        /// <param name="extender">The instance of <see cref="IPartialClassExtender"/> that caused the exception. Must not be <see
        /// langword="null"/>.</param>
        /// <returns>A <see cref="DiagnosticDescriptor"/> representing the EXGEN0003 error, including details about the exception
        /// and the type of the extender that caused it.</returns>
        public static DiagnosticDescriptor EXGEN0003_GenerateImplementations_ThrewException(INamedTypeSymbol symbol, Exception ex, IPartialClassExtender extender)
            => new(
                    "EXGEN0003",
                    "GenerateImplementations Threw An Exception.",
                    $"{symbol}: ({ex.Message}) from instance of {extender.GetType().Name} at Invoking GenerateImplementations() method while {Extender.Value.ExtentionName} generation",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
               );

        /// <summary>
        /// Creates a diagnostic descriptor for an unexpected exception that occurs during code generation.
        /// </summary>
        /// <remarks>This diagnostic descriptor is used to report errors that occur unexpectedly during
        /// the code generation process. The message includes details about the exception and the type of the extender
        /// involved.</remarks>
        /// <param name="ex">The exception that was thrown during code generation. Cannot be <see langword="null"/>.</param>
        /// <param name="extender">The partial class extender involved in the code generation process. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="DiagnosticDescriptor"/> representing the error caused by the unexpected exception.</returns>
        public static DiagnosticDescriptor EXGEN0004_UnexpectedExceptionWhileGeneratingCode(Exception ex, IPartialClassExtender extender)
            => new(
                    "EXGEN0004",
                    "Unexpected Exception While Generating Code.",
                    $"({ex.Message}) at Generating Code (extender={extender.GetType().Name})",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
               );
    }
}
