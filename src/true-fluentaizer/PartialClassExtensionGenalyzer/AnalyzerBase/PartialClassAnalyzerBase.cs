using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.Utils;
using System.Collections.Immutable;
using System.Linq;

namespace PartialClassExtGen.AnalyzerBase
{
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning disable RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
    public class PartialClassAnalyzerBase<TTPartialClassExtender> : DiagnosticAnalyzer
        where TTPartialClassExtender : IPartialClassExtender, new()
#pragma warning disable IDE0079 // Suppress unnecessary suppression
#pragma warning restore RS1001 // No diagnostic analyzer attribute
#pragma warning restore IDE0079 // Suppress unnecessary suppression
    {
        /// <summary>
        /// Gets the static instance of the <see cref="PartialClassExtendeeBase{TTPartialClassExtender}"/>  associated
        /// with the current partial class extender.
        /// </summary>
        private static PartialClassExtendeeBase<TTPartialClassExtender> ExtendeeBase { get; } = new();

        /// <summary>
        /// Gets the diagnostic rule that identifies a missing partial modifier in a type.
        /// </summary>
        private static DiagnosticDescriptor Rule { get; }
            = PCEGDiagnosticDescriptors<TTPartialClassExtender>
                .PCEG0001_Missing_Partial_Modifier;

        /// <summary>
        /// Gets the collection of <see cref="DiagnosticDescriptor"/> instances supported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        /// Initializes the analysis context for this analyzer.
        /// </summary>
        /// <remarks>This method enables concurrent execution, disables analysis of generated code,  and
        /// registers a syntax node action to analyze class declarations.</remarks>
        /// <param name="context">The <see cref="AnalysisContext"/> to configure for this analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        /// <summary>
        /// Analyzes a class declaration to ensure it meets specific requirements, such as having a target attribute and
        /// being declared as partial.
        /// </summary>
        /// <remarks>This method checks if the class declaration contains a specific target attribute, as
        /// determined by the extender's fully qualified target attribute name. If the attribute is present but the
        /// class is not declared as partial, a diagnostic is reported.</remarks>
        /// <param name="context">The <see cref="SyntaxNodeAnalysisContext"/> providing the context for the analysis, including the syntax
        /// node and semantic model.</param>
        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            // Ensure the node is a class declaration
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol) return;

            // Get the fully qualified name of the target attribute from the extender
            var extender = ExtendeeBase.GetExtender();
            INamedTypeSymbol? targetAttrSymbol =
                context.SemanticModel.Compilation.GetTypeByMetadataName(extender.TargetAttribute.FullName!);

            // Check if the class has the target attribute
            bool hasTargetAttribute = classSymbol
                .GetAttributes()
                .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, targetAttrSymbol));

            // If the class does not have the target attribute, no further action is needed
            if (!hasTargetAttribute) return;

            // Check if the class is declared as partial
            if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                // If the class is not partial, report a diagnostic
                var name = classSymbol.GenericQualifiedName();
                var ExtensionName = extender.ExtensionName;
                var diagnostic
                    = Diagnostic.Create(
                        Rule,
                        classDecl.Identifier.GetLocation(),
                        name, ExtensionName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
