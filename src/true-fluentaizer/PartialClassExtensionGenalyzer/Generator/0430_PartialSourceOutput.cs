using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Abstractions.Generator;
using PartialClassExtGen.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PartialClassExtGen.Generator
{
    /// <summary>
    /// Represents a sealed implementation of a source output generator for partial classes,  providing functionality to
    /// generate source code and report diagnostics during the process.
    /// </summary>
    /// <remarks>This class extends <see cref="PartialClassExtendeeBase"/> and implements <see
    /// cref="ISourceOutput"/>  to facilitate the generation of source code for partial classes. It uses an <see
    /// cref="IPartialClassExtender"/>  to generate the implementations and an <see cref="IPCEGDiagnostics"/>
    /// instance to handle  diagnostic reporting. The class ensures that any exceptions or errors encountered during the
    /// generation  process are appropriately reported as diagnostics.</remarks>
    public sealed class PartialSourceOutput<TPartialClassExtender, TDiagnostics, TTargetClassMeta>
        : PartialClassExtendeeBase<TPartialClassExtender, TDiagnostics>, ISourceOutput<TTargetClassMeta>
        where TPartialClassExtender : class, IPartialClassExtender
        where TDiagnostics : class, IPCEGDiagnostics
        where TTargetClassMeta : ITargetClassMeta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartialSourceOutput{TPartialClassExtender, TPCEGDiagnostics}"/>
        /// class.  
        /// </summary>
        /// <param name="extender">The partial class extender used to provide additional functionality or modifications to the source output.</param>
        /// <param name="diagnostics">The diagnostics object used for reporting issues or providing feedback during the generation process.</param>
        public PartialSourceOutput(
            TPartialClassExtender extender, TDiagnostics diagnostics
        ) : base(extender, diagnostics)
        {
            // No additional initialization needed here.
        }

        /// <summary>
        /// Generates source code for the specified target classes and reports any diagnostics encountered during the
        /// process.
        /// </summary>
        /// <remarks>This method iterates through the provided target classes, generating source code for
        /// each valid class.  If an exception occurs during the generation process for a specific class, a diagnostic
        /// is reported using the provided <see cref="SourceProductionContext"/>.</remarks>
        /// <param name="spc">The <see cref="SourceProductionContext"/> used to add generated source code and report diagnostics.</param>
        /// <param name="source">A tuple containing the compilation context and an immutable array of target class metadata.  The first item
        /// represents the <see cref="Compilation"/> object, and the second item is an array of metadata for the target
        /// classes.</param>
        public void SourceOutput(SourceProductionContext spc, (Compilation Left, ImmutableArray<TTargetClassMeta?> Right) source)
        {
            // Get the compilation and the list of target classes
            var (compilation, targetClasses) = source;

            // Output the source code for each target class with try-catch for exception handling
            // For each target class, generate the extension codes
            foreach (var targetClass in targetClasses)
            {
                if (targetClass == null || targetClass.Symbol is not INamedTypeSymbol symbol)
                    continue;

                try
                {
                    SourceOutputInternal(
                        spc, compilation, symbol,
                        Extender
                    );
                }
                catch (Exception ex)
                {
                    // If an exception occurs during the source output, report it
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics
                            .PCEG0003_UnexpectedExceptionWhileGeneratingCode,
                        Location.None,
                        ex.Message
                    ));
                }
            }
        }

        /// <summary>
        /// Generates source code for a partial class using the specified extender and reports any diagnostics or errors
        /// encountered during the process.
        /// </summary>
        /// <remarks>This method uses the provided <paramref name="extender"/> to generate source code for
        /// the specified <paramref name="symbol"/>. If the extender generates diagnostics, they are reported through
        /// the <paramref name="spc"/> context. If any diagnostic has a severity of <see
        /// cref="DiagnosticSeverity.Error"/>, the method stops processing the current class.  If an exception occurs
        /// during the generation process, a diagnostic is reported for each location of the class, and the method skips
        /// further processing for the current class.</remarks>
        /// <param name="spc">The context used to add generated source code and report diagnostics.</param>
        /// <param name="compilation"> The compilation context that provides access to the compilation being processed.</param>
        /// <param name="symbol">The symbol representing the partial class to be extended.</param>
        /// <param name="extender">The extender responsible for generating the implementations for the partial class.</param>
        private void SourceOutputInternal(
                SourceProductionContext spc, Compilation compilation,
                INamedTypeSymbol symbol, TPartialClassExtender extender
        )
        {
            // Generate the source code for the class
            var sb = new StringBuilder();
            sb.AppendLine($"// This file is auto-generated by {extender.GetType().FullName}");

            // Generate the extended class
            IEnumerable<Diagnostic>? diagnostics;
            try
            {
                // Generate the implementations using the extender
                diagnostics = extender.GenerateImplementationsInternal(extender, Diagnostics, symbol, compilation, sb);
            }
            catch (Exception ex)
            {
                // If an exception occurs during code generation, report it for each location of the class
                foreach (var loc in symbol.Locations)
                {
                    // Report the diagnostic for the exception
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics
                            .PCEG0002_GenerateImplementations_ThrewException,
                        loc,
                        symbol.GetGenericQualifiedName(), ex.Message

                    ));
                }
                return; // Skip to the next class if an exception was thrown
            }

            if (diagnostics is not null)
            {
                bool isError = false;
                foreach (var d in diagnostics)
                {
                    // Report each diagnostic generated by the extender
                    spc.ReportDiagnostic(d);
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        isError = true; // If any diagnostic is an error, set the flag
                    }
                }
                if (isError)
                    return; // Skip to the next class if any diagnostic is an error
            }

            // Output the generated source code
            spc.AddSource(
                $"{symbol.ToSafeFileName()}-{Extender.SuffixForGeneratedFiles}.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8)
            );
        }
    }
}
