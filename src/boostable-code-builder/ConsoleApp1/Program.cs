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
            using (var compooser = CodeBuilder.Open<CodeComposerBase>(sb))
            {
                compooser.AppendFragment("This is a test string.")
                         .AppendTerminatedFragment("This is another line.")
                         .AppendFragment("Final line without termination.");
                using (compooser.BeginSegment<CodeComposerBase>())
                {
                    compooser.AppendFragment("This is a test string.")
                             .AppendTerminatedFragment("This is another line.")
                             .AppendFragment("Final line without termination.");
                }
            }
            Console.WriteLine($"Composed Code:\n{sb}");
        }
    }
}
