# Boostable.CodeBuilding

Boostable.CodeBuilding is a thread-safe, scoped code composition framework for building complex source code structures programmatically.

## Features

- 🧱 Composable syntax units with buffer-based construction
- 🧵 Thread-safe and nesting-aware composition model
- 🔁 Recursive scope chaining with disposable safety
- 🪜 Hierarchical code composers with depth tracking
- 🧪 Extensive E2E test coverage and verification

## Example

```csharp
using var builder = new CodeBuilder();
builder.Append("public class HelloWorld");
builder.AppendLine("{");
using (var method = builder.Open<MethodComposer>())
{
    method.AppendLine("public void Greet() => Console.WriteLine(\"Hello\");");
}
builder.AppendLine("}");

Console.WriteLine(builder.ToString());
```
