using Microsoft.CodeAnalysis;

namespace Boostable.Syntax.Core.Abstractions.Common
{
    public interface IDiagnostics
    {
        DiagnosticDescriptor ERR9801_DetectedExceptionOnSyntaxProvider { get; }
        DiagnosticDescriptor ERR9802_DetectedExceptionOnSourceOutput { get; }
        DiagnosticDescriptor WARN9801_DetectedExceptionOnAnalyzer { get; }
        DiagnosticDescriptor WARN9802_DetectedUndisposedStackedStringBuilderInstance { get; }
    }
}
