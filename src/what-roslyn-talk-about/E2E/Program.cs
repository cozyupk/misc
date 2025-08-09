using Boostable.WhatRoslynTalkAbout;

namespace E2E
{
    internal class Program
    {
        static void Main(string[] _)
        {
            string sourceCode = @"
using System;

public static class Hello {
    public static void Main(string[] _) {
        Console.WriteLine(""Hello, Roslyn!"");
    }
}";
            var session = new SimpleRoslynTalkSession(
                arrangeCode: sourceCode,
                actForTheCode: code =>
                {
                    Console.WriteLine("==== Compiling ====");
                    Console.WriteLine(code);
                    Console.WriteLine("===================");
                },
                promptVariationBuilder: new RoslynTestBase()
                    .CreatePromptVariationBuilderForAllCSharpLangVersions()
                    .Inject(System.Text.Encoding.UTF8)
                    .Inject(TimeSpan.FromSeconds(5))
                    .ProjectPath(label => $"Temp_{label}.cs"),
                enableExcecution: true
            );

            var result = session.TalkAbout();

            Console.WriteLine("=== Compilation completed ===");
            Console.WriteLine($"IsMeaningful: {result.IsMeaningful}");
        }
    }
}
