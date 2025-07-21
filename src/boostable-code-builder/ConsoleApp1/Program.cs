using Boostable.CodeBuilding.Core;
using System.Text;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // Example usage of the Boostable.CodeBuilding.Abstractions
            var sb = new StringBuilder();
            using (var compooser = CodeBuilder.Open(sb))
            {
                compooser.Append("This is a test string.")
                         .AppendLine("This is another line.")
                         .Append("Final line without termination.");
                using (compooser.Open<CodeComposerBase>())
                {
                    compooser.Append("This is a test string.")
                             .AppendLine("This is another line.")
                             .Append("Final line without termination.");
                }
            }
            Console.WriteLine($"Composed Code:\n{sb.ToString()}");
        }
    }
}
