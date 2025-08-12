using Boostable.WhatRoslynTalkAbout.ArrangeCodes;
using Boostable.WhatRoslynTalkAbout.ExecuteAssemblies;
using Boostable.WhatTalkAbout.AssemblyLoading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;
using System.Text;
using static Boostable.WhatTalkAbout.AssemblyLoading.AssemblyLoadingAbstractions;

class Program
{
    static void Main()
    {
        var prompt = new SamplePrompt("TestPrompt");
        var outline = new SampleExecuteOutline(
            () => new[] { new VirtualSource("Hello.cs", @"
                using System;
                public class Program
                {
                    public static void Main(string[] args)
                    {
                        Console.WriteLine(""Hello from generated code!"");
                    }
                }") },
            new SimpleAssemblyLoader(),
            new[] { prompt }
        );

        outline.TalkAbout();
    }
}

// ==== Prompt 実装（必要なプロパティだけ）====
class SamplePrompt : IExecuteAssembliesPromptForTalking<SamplePrompt>
{
    public string Label { get; }
    public CancellationToken CancellationToken => CancellationToken.None;
    public SamplePrompt(string label) => Label = label;
    public SamplePrompt Clone(string label) => new SamplePrompt(label);

    public CSharpParseOptions ParseOptions => CSharpParseOptions.Default;
    public Encoding EncodingForParse => Encoding.UTF8;
    public IReadOnlyList<VirtualSource>? TargetCodes => null;
    public IReadOnlyList<MetadataReference> References => new[]
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
    };
    public bool MergeAppDomainReferences => true;
    public CSharpCompilationOptions CompilationOptions =>
        new CSharpCompilationOptions(OutputKind.ConsoleApplication);
    public bool EmitPdb => false;
    public EmitOptions? EmitOptions => null;
    public bool CollectTypeCounts => true;
    public bool CollectAssemblyIdentity => true;
    public string? EntryPointName => null;
    public IReadOnlyList<string>? Args => null;
}

// ==== 最小限のアセンブリローダ ====
class SimpleAssemblyLoader : AssemblyLoadingAbstractions.IAssemblyLoader
{
    public bool IsUnloadSupported => false;

    public LoadedAssembly LoadFromStreams(Stream pe, Stream? pdb = null,
        AssemblyLoadOptions? options = null)
    {
        var asm = AssemblyLoadContext.Default.LoadFromStream(pe, pdb);
        return new LoadedAssembly(asm, null);
    }
}

// ==== Outline 実装 ====
class SampleExecuteOutline
    : ExecuteAssembliesTalkOutlineBase<
        SamplePrompt,
        IReadOnlyExecuteAssembliesArtifacts,
        SampleArtifacts>
{
    public SampleExecuteOutline(
        Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
        AssemblyLoadingAbstractions.IAssemblyLoader loader,
        IReadOnlyList<SamplePrompt> prompts)
        : base(funcToArrangeCodes, loader, prompts) { }
}

// ==== Artifacts 実装 ====
class SampleArtifacts : IExecuteAssembliesArtifacts
{
    public IReadOnlyList<VirtualSource>? TargetCodes { get; private set; }
    public IReadOnlyList<SyntaxTree>? ParsedTrees { get; private set; }
    public CSharpCompilation? Compilation { get; private set; }
    public IReadOnlyList<MetadataReference>? UsedReferences { get; private set; }
    public IReadOnlyList<byte>? PeImage { get; private set; }
    public IReadOnlyList<byte>? PdbImage { get; private set; }
    public string? AssemblyName { get; private set; }
    public IReadOnlyDictionary<string, object>? IlMetrics { get; private set; }
    public object? ReturnValue { get; private set; }

    public void SetTargetCodes(IReadOnlyList<VirtualSource> targetCodes)
        => TargetCodes = targetCodes;
    public void SetParsedTrees(IReadOnlyList<SyntaxTree> trees)
        => ParsedTrees = trees;
    public void SetCompilation(CSharpCompilation compilation, IReadOnlyList<MetadataReference> refs)
    {
        Compilation = compilation;
        UsedReferences = refs;
    }
    public void SetEmittedImages(byte[] pe, byte[]? pdb, string? asmName)
    {
        PeImage = pe;
        PdbImage = pdb;
        AssemblyName = asmName;
    }
    public void SetIlMetrics(IReadOnlyDictionary<string, object> metrics)
        => IlMetrics = metrics;
    public void SetExecutionResult(object? returnValue)
        => ReturnValue = returnValue;
}
