using Microsoft.CodeAnalysis;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Utils;
using System;

namespace PartialClassExtGen.Utils
{

    public class PCEGDiagnosticDescriptors<TExtender>
        : PartialClassExtendeeBase<TExtender>
        where TExtender: IPartialClassExtender, new()
    {
        public static DiagnosticDescriptor PCEG0001_Missing_Partial_Modifier { get; }
            = new(
                        Extender.PrefixForDiagnosticId + "0001",
                        "Missing Partial Modifier",
                        "The class '{0}' must be marked as 'partial' to enable {1} generation.",
                        "Usage",
                        DiagnosticSeverity.Error,
                        true
              );

        public static DiagnosticDescriptor PCEG0002_IsTargetClass_ThrewException(INamedTypeSymbol symbol, Exception ex, IPartialClassExtender externder)
            => new(
                    Extender.PrefixForDiagnosticId + "0002",
                    "IsTargetClass Threw An Exception.",
                    $"{symbol}: ({ex.Message}) from instance of {externder.GetType().Name} at Invoking IsTargetClass() method while {Extender.ExtensionName} generation",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
               );

        public static DiagnosticDescriptor PCEG0003_GenerateImplementations_ThrewException(INamedTypeSymbol symbol, Exception ex, IPartialClassExtender extender)
            => new(
                    Extender.PrefixForDiagnosticId + "0003",
                    "GenerateImplementations Threw An Exception.",
                    $"{symbol}: ({ex.Message}) from instance of {extender.GetType().Name} at Invoking GenerateImplementations() method while {Extender.ExtensionName} generation",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
               );

        public static DiagnosticDescriptor PCEG0004_UnexpectedExceptionWhileGeneratingCode(Exception ex, IPartialClassExtender extender)
            => new(
                    Extender.PrefixForDiagnosticId + "0004",
                    "Unexpected Exception While Generating Code.",
                    $"({ex.Message}) at Generating Code (extender={extender.GetType().Name})",
                    "CodeGeneration",
                    DiagnosticSeverity.Error,
                    true
               );
    }
}
