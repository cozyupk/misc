using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PartialClassExtGen.Abstractions.Analyzer;
using PartialClassExtGen.Abstractions.Common;
using PartialClassExtGen.AnalyzerBase;
using PartialClassExtGen.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PartialClassExtGen.GenalyzerBase
{
	/// <summary>
	/// Provides a base class for generating partial class implementations using a specified attribute type.
	/// </summary>
	/// <remarks>This abstract class serves as a foundation for implementing partial class generators. It
	/// integrates with the <see cref="IIncrementalGenerator"/> and <see cref="IExtensionStrategy"/> interfaces to
	/// facilitate incremental source generation and partial class extension. Derived classes must implement the <see
	/// cref="GenerateImplementations"/> method to define the specific logic for generating code based on the provided
	/// symbols.</remarks>
	/// <typeparam name="TAttribute">The type of attribute that identifies the target classes for partial class generation. Must derive from <see
	/// cref="Attribute"/>.</typeparam>
	partial class VanillaPCEG<TAttribute, TPartialClassExtender, TDiagnostics, TTargetClassMeta>
	{
		/// <summary>
		/// Retrieves a collection of syntax node rules based on the provided extender and diagnostics objects.
		/// </summary>
		/// <remarks>This method validates the provided parameters to ensure they are of the expected types and
		/// invokes an abstract method to generate the syntax node rules. The method is thread-safe and ensures that only one
		/// invocation can occur at a time.</remarks>
		/// <typeparam name="TArgPartialClassExtender">The type of the partial class extender. Must implement <see cref="IExtensionStrategy"/>.</typeparam>
		/// <typeparam name="TArgDiagnostics">The type of the diagnostics object. Must implement <see cref="IPCEGDiagnostics"/>.</typeparam>
		/// <param name="extender">An instance of the partial class extender. Must be of type <typeparamref name="TArgPartialClassExtender"/>.</param>
		/// <param name="diagnostics">An instance of the diagnostics object. Must be of type <typeparamref name="TArgDiagnostics"/>.</param>
		/// <returns>A collection of <see cref="ISyntaxNodeRule"/> objects representing the syntax node rules.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the method is called while a previous invocation is still in progress. This method is not re-entrant.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="extender"/> is not of the expected type <typeparamref name="TArgPartialClassExtender"/>
		/// or if <paramref name="diagnostics"/> is not of the expected type <typeparamref name="TArgDiagnostics"/>.</exception>
		public IEnumerable<ISyntaxNodeRule> GetSyntaxNodeRulesInternal<TArgPartialClassExtender, TArgDiagnostics>(
												TArgPartialClassExtender extender, TArgDiagnostics diagnostics
		)
			where TArgPartialClassExtender : class, IExtensionStrategy
			where TArgDiagnostics : class, IPCEGDiagnostics
		{
			if (Interlocked.Exchange(ref _isGetSyntaxNodeRulesRunning, 1) != 0)
			{
				throw new InvalidOperationException("GetSyntaxNodeRulesInternal is already in progress. This method is not re-entrant.");
			}

			try
			{
				// Validate extender and diagnostics parameters for safe-guard.
				if (extender is not TPartialClassExtender validExtender)
				{
					throw new ArgumentException($"Extender must be of type {typeof(TPartialClassExtender).FullName}.", nameof(extender));
				}
				if (diagnostics is not TDiagnostics validDiagnostics)
				{
					throw new ArgumentException($"Diagnostics must be of type {typeof(TDiagnostics).FullName}.", nameof(diagnostics));
				}

				// Invoke the abstract method to generate implementations.
				return GetActionForSyntaxNodes(
					validExtender, validDiagnostics
				);
			}
			finally
			{
				// Reset the flag to allow future calls.
				Interlocked.Exchange(ref _isGetSyntaxNodeRulesRunning, 0);
			}
		}

		/// <summary>
		/// Retrieves a collection of syntax node rules to be applied for analyzing syntax nodes in the context of a partial
		/// class extender.
		/// </summary>
		/// <remarks>This method generates syntax node rules that analyze class declarations to ensure they meet
		/// specific criteria, such as being marked with a target attribute and declared as partial. If a class declaration
		/// does not meet these criteria, a diagnostic is reported.</remarks>
		/// <param name="extender">An instance of the partial class extender that provides the target attribute and other metadata required for the
		/// analysis.</param>
		/// <param name="diagnostics">An instance containing diagnostic descriptors used to report issues, such as missing partial modifiers on class
		/// declarations.</param>
		/// <returns>A collection of <see cref="ISyntaxNodeRule"/> objects that define the rules to be applied for analyzing syntax
		/// nodes.</returns>
		public IEnumerable<ISyntaxNodeRule> GetActionForSyntaxNodes(
			TPartialClassExtender extender, TDiagnostics diagnostics
		) {
			return new SyntaxNodeRule[] {
				new(
					SyntaxKind.ClassDeclaration,
					(context) =>
					{
                        // Ensure the node is a class declaration
                        var classDecl = (ClassDeclarationSyntax)context.Node;
						var semanticModel = context.SemanticModel;
						if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol) return;

                        // Get the fully qualified name of the target attribute from the extender
                        var fullName = extender.TargetAttribute.FullName;
						if (string.IsNullOrEmpty(fullName)) return; // fail-safe fallback
                        INamedTypeSymbol? targetAttrSymbol =
								context.SemanticModel.Compilation.GetTypeByMetadataName(extender.TargetAttribute.FullName!);
						if (targetAttrSymbol is null) return; // fail-safe fallback

                        // Check if the class has the target attribute
                        bool hasTargetAttribute = classSymbol
													.GetAttributes()
													.Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, targetAttrSymbol));

                        // If the class does not have the target attribute, no further action is needed
                        if (!hasTargetAttribute) return; // fail-safe fallback

                        // Check if the class is declared as partial
                        if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
						{
                            // If the class is not partial, report a diagnostic
                            var name = classSymbol.GetGenericQualifiedName();
							var extensionName = extender.ExtensionName;
							var diagnostic
									= Diagnostic.Create(
										diagnostics.PCEG0001_Missing_Partial_Modifier,
										classDecl.Identifier.GetLocation(),
										name, extensionName
									);
							context.ReportDiagnostic(diagnostic);
						}
					},
					new HashSet<DiagnosticDescriptor>() {
						diagnostics.PCEG0001_Missing_Partial_Modifier
					}
				)
			};
		}
	}
}
