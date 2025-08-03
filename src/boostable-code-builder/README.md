# Boostable.CodeBuilding

Boostable.CodeBuilding is a thread-safe, scoped code composition framework for building complex source code structures programmatically.

## Features

- 🧱 Composable syntax units with buffer-based construction  
- 🧵 Thread-safe and nesting-aware composition model  
- 🔁 Recursive scope chaining with disposable safety  
- 🪜 Hierarchical code composers with depth tracking  
- 🧪 Extensive E2E test coverage and verification  

## Basic Example

Please refer to the following example to see how to use the `CodeBuilder` and `CodeComposerBase` classes for composing code. The example demonstrates how to open a code composer, append lines of code, and manage nested composers.

This approach allows you to systematically generate scoped program code by overriding `CodeComposerBase` and using it as the type parameter in the `Open` method.

```csharp
// Prepare base string builder
var sb = new StringBuilder();

// Open a code composer with the string builder
using (var composer = CodeBuilder.Open<CodeComposerBase>(sb))
{
    // Append code lines and blocks
    composer.Append("This is a test string.")
            .AppendLine("This is another line.")
            .Append("Final line without termination.");

    // Open a nested code composer
    using (composer.Open<CodeComposerBase>())
    {
        // Append more code in the nested composer
        composer.Append("This is a test string.")
                .AppendLine("This is another line.")
                .Append("Final line without termination.");
    }
}

// Output the generated code
Console.WriteLine("Composed Code:");
Console.WriteLine(sb.ToString());
````

Internal Implementation
-----------------------

*   Prefix numbers are added to the project files (`.csproj`).
*   Prefix numbers are also assigned to solution folders and files within each project.
*   These prefixes represent a topological sort based on project dependencies and help clarify the structure of the codebase.
*   However, this is an **ad-hoc convention** introduced because most IDEs, including Visual Studio, do not currently support **topological sorting based on dependencies**. This practice is expected to become obsolete as IDEs evolve.
