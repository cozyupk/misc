
/****************
 * Abstractions *
 ****************/

using System.IO;
using System.Reflection;


/********
 * Base *
*********/

namespace Boostable.WhatTalkAbout.Base
{
    using global::Boostable.WhatTalkAbout.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;



    }

    namespace Boostable.WhatTalkAbout.AssemblyLoading
    {
        /// <summary>
        /// Provides abstractions for loading assemblies, including support for managing assembly lifetimes and optional
        /// unloading capabilities.
        /// </summary>
        /// <remarks>This class defines a set of types and interfaces for working with assembly loading in a
        /// flexible and extensible manner. It includes support for managing loaded assemblies, specifying load options, and
        /// implementing custom assembly loaders. The abstractions are designed to be used in scenarios where advanced
        /// control over assembly loading and unloading is required, such as plugin systems or dynamic code execution
        /// environments.</remarks>
        public abstract class AssemblyLoadingAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts> : TalkSessionAbstractions<TPrompt, TReadOnlyArtifacts, TArtifacts>
            where TPrompt : class, IPromptForTalking<TPrompt>
            where TReadOnlyArtifacts : class, IReadOnlyArtifacts
            where TArtifacts : class, TReadOnlyArtifacts, IArtifacts, new ()
        {

        }

        public abstract class AssemblyLoadingOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
            : TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
            where TPrompt : class, IPromptForTalking<TPrompt>
            where TReadOnlyArtifacts : class, IReadOnlyArtifacts
            where TArtifacts : class, TReadOnlyArtifacts, IArtifacts, new()
        {
            protected internal AssemblyLoadingOutlineBase(
                IReadOnlyList<TPrompt> prompts,
                params ITalkChapter[] chapters
            ) : base(prompts, [.. chapters])
            {
            }

            /// <summary>
            /// Represents a loaded assembly and an optional lease for managing its lifetime.
            /// </summary>
            /// <remarks>If the <see cref="Lease"/> is <see langword="null"/>, the assembly cannot be
            /// unloaded. Use the <see cref="CanUnload"/> property to determine whether the assembly supports
            /// unloading.</remarks>
            /// <param name="assembly"></param>
            /// <param name="lease"></param>
            public readonly struct LoadedAssembly(Assembly assembly, IDisposable? lease = null)
            {
                /// <summary>
                /// Gets the assembly associated with the current context.
                /// </summary>
                public Assembly Assembly { get; } = assembly ?? throw new ArgumentNullException(nameof(assembly));

                /// <summary>
                /// Gets the lease object that manages the resource's lifetime.
                /// </summary>
                /// <remarks>The lease is used to control the lifetime of the associated resource.  Call
                /// <see cref="IDisposable.Dispose"/> on the lease to release the resource when it is no longer
                /// needed.</remarks>
                public IDisposable? Lease { get; } = lease;

                /// <summary>
                /// Gets a value indicating whether the current instance can be unloaded.
                /// </summary>
                public bool CanUnload => Lease != null;
            }

            public sealed record class AssemblyLoadOptions
            {
                public string? LogicalName { get; init; }
                public bool LeaveStreamsOpen { get; init; }
                public CancellationToken CancellationToken { get; init; } = CancellationToken.None;

                public static AssemblyLoadOptions Default { get; } = new();
            }

            // .NET Standard 側が依存する抽象。ホスト側が ALC 等で実装。
            /// <summary>
            /// Defines a mechanism for loading assemblies from streams, with optional support for unloading.
            /// </summary>
            /// <remarks>This interface is typically implemented by a host to provide custom assembly loading
            /// functionality, such as using an AssemblyLoadContext (ALC). It allows loading assemblies from PE and PDB
            /// streams and optionally supports unloading loaded assemblies.</remarks>
            public interface IAssemblyLoader
            {
                LoadedAssembly LoadFromStreams(Stream pe, Stream? pdb = null, AssemblyLoadOptions? options = null);
                bool IsUnloadSupported { get; }
            }
        }
    }
}

/****************
 * ArrangeCodes *
 ****************/
namespace Boostable.WhatRoslynTalkAbout.ArrangeCodes
{
    using Boostable.WhatTalkAbout.Abstractions;
    using Boostable.WhatTalkAbout.Base.Boostable.WhatTalkAbout.AssemblyLoading;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public readonly record struct VirtualSource
    {
        public required string Path { get; init; }
        public required string Code { get; init; }
    }

    public interface IArrangeCodesPromptForTalking<out TSelf> : IPromptForTalking<TSelf>
        where TSelf : class, IArrangeCodesPromptForTalking<TSelf>
    {
    }

    public interface IReadOnlyArrangeCodesArtifacts : IReadOnlyArtifacts
    {
        IReadOnlyList<VirtualSource>? TargetCodes { get; }
    }

    public interface IArrangeCodesArtifacts : IArtifacts, IReadOnlyArtifacts
    {
        void SetTargetCodes(IReadOnlyList<VirtualSource> targetCodes);
    }

    public class ArrangeCodesTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : AssemblyLoadingOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IArrangeCodesPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArrangeCodesArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IArrangeCodesArtifacts, IArtifacts, new()
    {
        protected int _hasArranged = 0;

        private static ITalkChapter CurrentTalkChapter { get; } = new TalkChapter("Arranging Codes Chapter");
        protected Func<IEnumerable<VirtualSource>> FuncToArrangeCodes { get; }
        protected internal ArrangeCodesTalkOutlineBase(
            Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
            IReadOnlyList<TPrompt> prompts,
            params ITalkChapter[] chapters
        ) : base(prompts, [CurrentTalkChapter, .. chapters])
        {
            FuncToArrangeCodes = funcToArrangeCodes ?? throw new ArgumentNullException(nameof(funcToArrangeCodes));
        }

        protected internal virtual void ArrangeCodes()
        {
            try
            {
                Prerequisite.Artifacts.SetTargetCodes([.. FuncToArrangeCodes()]);
            }
            catch (Exception ex)
            {
                AddTestimony(
                    ex,
                    CurrentTalkChapter
                );
            }
        }

        protected override void PrepareForTalk()
        {
            if (Interlocked.CompareExchange(ref _hasArranged, 1, 0) != 0)
            {
                throw new InvalidOperationException("The codes have already been arranged. Let them arrange only once per session.");
            }

            base.PrepareForTalk();
            ArrangeCodes();
        }

        protected override async Task PrepareForTalkAsync()
        {
            if (Interlocked.CompareExchange(ref _hasArranged, 1, 0) != 0)
            {
                throw new InvalidOperationException("The codes have already been arranged. Let them arrange only once per session.");
            }

            await base.PrepareForTalkAsync();
            ArrangeCodes();
        }
    }
}

/************
 * ParseCodes
 ************/
namespace Boostable.WhatRoslynTalkAbout.ParseCodes
    {
        using Boostable.WhatRoslynTalkAbout.ArrangeCodes;
    using Boostable.WhatTalkAbout.Abstractions;
    using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp;
        using Microsoft.CodeAnalysis.Text;
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;

        // Prompt: Arrange を継承して、Parseに必要なオプションだけ足す
        public interface IParseCodesPromptForTalking<out TSelf> : IArrangeCodesPromptForTalking<TSelf>
        where TSelf : class, IParseCodesPromptForTalking<TSelf>
    {
        CSharpParseOptions ParseOptions { get; }
        Encoding EncodingForParse { get; }
    }

    // Artifacts: 解析結果（SyntaxTree の束）を保持
    public interface IReadOnlyParseCodesArtifacts : IReadOnlyArrangeCodesArtifacts
    {
        IReadOnlyList<SyntaxTree>? ParsedTrees { get; }
    }

    public interface IParseCodesArtifacts : IArrangeCodesArtifacts, IReadOnlyParseCodesArtifacts
    {
        void SetParsedTrees(IReadOnlyList<SyntaxTree> trees);
    }

    // TalkOutline: Arrange → Parse を重ねる
    public class ParseCodesTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : ArrangeCodesTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IParseCodesPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyParseCodesArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IParseCodesArtifacts, IArtifacts, new()
    {
        private static ITalkChapter CurrentTalkChapter { get; } = new TalkChapter("Parsing Codes Chapter");
        private int _hasParsed;

        protected internal ParseCodesTalkOutlineBase(
            Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
            IReadOnlyList<TPrompt> prompts,
            params ITalkChapter[] chapters
        ) : base(funcToArrangeCodes, prompts, [CurrentTalkChapter, .. chapters]) { }

        protected internal virtual void ParseCodes(TPrompt prompt)
        {
            try
            {
                var codes = Prerequisite.Artifacts.TargetCodes;
                if (codes is null)
                {
                    // ArrangeCodes failed to set target codes.
                    // Silently skip parsing because the reason of failure must has been recoded in Testimony.
                    return;
                }

                var trees = new List<SyntaxTree>(codes.Count);
                foreach (var src in codes)
                {
                    // Respect prompt-level CancellationToken & options
                    prompt.CancellationToken.ThrowIfCancellationRequested();
                    var text = SourceText.From(src.Code, prompt.EncodingForParse);
                    var tree = CSharpSyntaxTree.ParseText(text, prompt.ParseOptions, path: src.Path, cancellationToken: prompt.CancellationToken);
                    trees.Add(tree);
                }

                // 直近の結果で上書き（スナップショット渡し）
                Prerequisite.Artifacts.SetParsedTrees(trees);
            }
            catch (OperationCanceledException oce)
            {
                AddTestimony(oce, CurrentTalkChapter, prompt);
                throw;
            }
            catch (Exception ex)
            {
                AddTestimony(ex, CurrentTalkChapter, prompt);
            }
        }

        protected override void PrepareForTalk()
        {
            // First, we check if the codes have already been parsed before calling the base method,
            // because the outline does not have to be reusable and should only be prepared once per session.
            if (Interlocked.CompareExchange(ref _hasParsed, 1, 0) != 0)
                throw new InvalidOperationException("The codes have already been parsed. Let them parse only once per session.");

            base.PrepareForTalk();

            foreach (var p in Prerequisite.Prompts)
                ParseCodes(p);
        }

        protected override async Task PrepareForTalkAsync()
        {
            // First, we check if the codes have already been parsed before calling the base method,
            // because the outline does not have to be reusable and should only be prepared once per session.
            if (System.Threading.Interlocked.CompareExchange(ref _hasParsed, 1, 0) != 0)
                throw new InvalidOperationException("The codes have already been parsed. Let them parse only once per session.");

            await base.PrepareForTalkAsync().ConfigureAwait(false);

            var tasks = Prerequisite.Prompts.Select(p => Task.Run(() => ParseCodes(p), p.CancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}

/***********************
 * CompileSyntaxTrees  *
 ***********************/
namespace Boostable.WhatRoslynTalkAbout.CompileSyntaxTrees
    {
        using Boostable.WhatRoslynTalkAbout.ArrangeCodes;
        using Boostable.WhatRoslynTalkAbout.ParseCodes;
    using Boostable.WhatTalkAbout.Abstractions;
    using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp;
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;

        // Prompt: 参照を持たせ、AppDomain マージの可否を切替
        public interface ICompileSyntaxTreesPromptForTalking<out TSelf> : IParseCodesPromptForTalking<TSelf>
        where TSelf : class, ICompileSyntaxTreesPromptForTalking<TSelf>
    {
        /// <summary>明示的に与える参照</summary>
        IReadOnlyList<MetadataReference> References { get; }

        /// <summary>AppDomain.CurrentDomain の参照を自動マージするか</summary>
        bool MergeAppDomainReferences { get; }

        /// <summary>CompilationOptions</summary>
        CSharpCompilationOptions CompilationOptions { get; }
    }

    // Artifacts: Compilation と 実際に使った参照
    public interface IReadOnlyCompileSyntaxTreesArtifacts : IReadOnlyParseCodesArtifacts
    {
        CSharpCompilation? Compilation { get; }
        IReadOnlyList<MetadataReference>? UsedReferences { get; }
    }

    public interface ICompileSyntaxTreesArtifacts : IParseCodesArtifacts, IReadOnlyCompileSyntaxTreesArtifacts
    {
        void SetCompilation(CSharpCompilation compilation, IReadOnlyList<MetadataReference> usedReferences);
    }

    public class CompileSyntaxTreesTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : ParseCodesTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, ICompileSyntaxTreesPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyCompileSyntaxTreesArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, ICompileSyntaxTreesArtifacts, IArtifacts, new()
    {
        private static ITalkChapter CurrentTalkChapter { get; } = new TalkChapter("Compiling Syntax Trees Chapter");
        private int _hasCompiled;

        protected internal CompileSyntaxTreesTalkOutlineBase(
            Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
            IReadOnlyList<TPrompt> prompts,
            params ITalkChapter[] chapters
        ) : base(funcToArrangeCodes, prompts, [CurrentTalkChapter, .. chapters]) { }

        protected internal virtual void CompileSyntaxTrees(TPrompt prompt)
        {
            try
            {
                // まず Parse 済みか確認
                var trees = Prerequisite.Artifacts.ParsedTrees;
                if (trees is null)
                {
                    // Parse が失敗していれば証言に残っているはずなので静かにスキップ
                    return;
                }

                prompt.CancellationToken.ThrowIfCancellationRequested();

                // 参照の解決（明示 + オプションで AppDomain マージ）
                var usedRefs = ResolveReferences(prompt);

                var options = prompt.CompilationOptions;
                options = OnUseCompilationOptions(options, prompt);

                // 一意なアセンブリ名
                var asmName = "InMemoryAssembly_" + Guid.NewGuid().ToString("N");

                var compilation = CSharpCompilation.Create(
                    assemblyName: asmName,
                    syntaxTrees: trees,
                    references: usedRefs,
                    options: options
                );

                // 診断を収集して、Error は証言として残す
                var diagnostics = compilation.GetDiagnostics(prompt.CancellationToken);
                foreach (var d in diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        // 今の枠は Exception を証言に載せる設計なので、簡易的に包む
                        AddTestimony(new InvalidOperationException(d.ToString()), CurrentTalkChapter, prompt);
                    }
                }

                // 成果物を保存
                Prerequisite.Artifacts.SetCompilation(compilation, usedRefs);
            }
            catch (OperationCanceledException oce)
            {
                AddTestimony(oce, CurrentTalkChapter, prompt);
                throw;
            }
            catch (Exception ex)
            {
                AddTestimony(ex, CurrentTalkChapter, prompt);
            }
        }

        protected override void PrepareForTalk()
        {
            if (Interlocked.CompareExchange(ref _hasCompiled, 1, 0) != 0)
                throw new InvalidOperationException("The syntax trees have already been compiled. Let them compile only once per session.");

            base.PrepareForTalk();

            foreach (var p in Prerequisite.Prompts)
                CompileSyntaxTrees(p);
        }

        protected override async Task PrepareForTalkAsync()
        {
            if (Interlocked.CompareExchange(ref _hasCompiled, 1, 0) != 0)
                throw new InvalidOperationException("The syntax trees have already been compiled. Let them compile only once per session.");

            await base.PrepareForTalkAsync().ConfigureAwait(false);

            var tasks = Prerequisite.Prompts.Select(p => Task.Run(() => CompileSyntaxTrees(p), p.CancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        protected virtual CSharpCompilationOptions OnUseCompilationOptions(
            CSharpCompilationOptions options,
            TPrompt prompt
        )
        {
            // オプションを設定するためのフック。必要に応じてオーバーライドして実装。
            // 例えば、options に特定のフラグを設定するなど。
            // アセンブリを Execute する場合などに WithOutputKind(OutputKind.DynamicallyLinkedLibrary) する想定。
            return options;
        }

        /// <summary>
        /// Prompt 指定の参照をベースに、必要なら AppDomain の参照をマージして返す（重複除去）。
        /// </summary>
        protected virtual IReadOnlyList<MetadataReference> ResolveReferences(TPrompt prompt)
        {
            // ベースは Prompt の参照
            var refs = new List<MetadataReference>(prompt.References ?? []);

            if (prompt.MergeAppDomainReferences)
            {
                foreach (var mr in GetAppDomainMetadataReferences())
                {
                    refs.Add(mr);
                }
            }

            // 重複除去（パス基準）。AssemblyIdentity まで見るなら強名も見るが、まずはパスで十分なことが多い
            var dedup = new List<MetadataReference>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in refs)
            {
                if (r is PortableExecutableReference { FilePath: { Length: > 0 } path })
                {
                    if (seen.Add(path)) dedup.Add(r);
                    continue;
                }
                dedup.Add(r);
            }
            return dedup;
        }

        protected virtual IEnumerable<MetadataReference> GetAppDomainMetadataReferences()
        {
            // AppDomain にロード済みのアセンブリのうち、ファイルに紐づくものだけを拾う
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm is { IsDynamic: false } && asm.Location is { Length: > 0 } loc && File.Exists(loc))
                {
                    MetadataReference mr;
                    try { mr = MetadataReference.CreateFromFile(loc); }
                    catch { continue; }
                    yield return mr;
                }
            }
        }
    }
}

/**********************
 * EmitCompilations   *
 **********************/
namespace Boostable.WhatRoslynTalkAbout.EmitCompilations
    {
        using Boostable.WhatRoslynTalkAbout.ArrangeCodes;
        using Boostable.WhatRoslynTalkAbout.CompileSyntaxTrees;
    using Boostable.WhatTalkAbout.Abstractions;
    using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.Emit;
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;

        // Prompt: 何をどう吐くかのポリシーを持たせる
        public interface IEmitCompilationsPromptForTalking<out TSelf> : ICompileSyntaxTreesPromptForTalking<TSelf>
        where TSelf : class, IEmitCompilationsPromptForTalking<TSelf>
    {
        /// <summary>PE と一緒に PDB も出すか</summary>
        bool EmitPdb { get; }

        /// <summary>EmitOptions（null なら既定でOK）</summary>
        EmitOptions? EmitOptions { get; }

        /// <summary>（任意）エントリポイント名ヒントなどを使いたければここに追加</summary>
        // string? EntryPointHint { get; }
    }

    // Artifacts: 出力バイナリそのもの
    public interface IReadOnlyEmitCompilationsArtifacts : IReadOnlyCompileSyntaxTreesArtifacts
    {
        IReadOnlyList<byte>? PeImage { get; }
        IReadOnlyList<byte>? PdbImage { get; }
        string? AssemblyName { get; }
    }

    public interface IEmitCompilationsArtifacts : ICompileSyntaxTreesArtifacts, IReadOnlyEmitCompilationsArtifacts
    {
        void SetEmittedImages(byte[] pe, byte[]? pdb, string? assemblyName);
    }

    public class EmitCompilationsTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : CompileSyntaxTreesTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IEmitCompilationsPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyEmitCompilationsArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IEmitCompilationsArtifacts, IArtifacts, new()
    {
        private static ITalkChapter CurrentTalkChapter { get; } = new TalkChapter("Emitting Compilations Chapter");
        private int _hasEmitted;

        protected internal EmitCompilationsTalkOutlineBase(
            Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
            IReadOnlyList<TPrompt> prompts,
            params ITalkChapter[] chapters
        ) : base(funcToArrangeCodes, prompts, [CurrentTalkChapter, .. chapters]) { }

        protected internal virtual void EmitCompilation(TPrompt prompt)
        {
            try
            {
                var compilation = Prerequisite.Artifacts.Compilation;
                if (compilation is null)
                {
                    // Compile が失敗していれば証言に残っているはずなので静かにスキップ
                    return;
                }

                prompt.CancellationToken.ThrowIfCancellationRequested();

                using var pe = new MemoryStream();
                using var pdb = prompt.EmitPdb ? new MemoryStream() : null;

                var emitOptions = prompt.EmitOptions ?? new EmitOptions();

                var result = compilation.Emit(
                    peStream: pe,
                    pdbStream: pdb,
                    options: emitOptions,
                    cancellationToken: prompt.CancellationToken
                );

                // 診断を証言に流す（Error だけ or 全部、は好みで）
                foreach (var d in result.Diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                        AddTestimony(new InvalidOperationException(d.ToString()), CurrentTalkChapter, prompt);
                }

                if (!result.Success)
                    return; // 失敗時は証言だけ残し、Artifacts は更新しない

                Prerequisite.Artifacts.SetEmittedImages(
                    pe: pe.ToArray(),
                    pdb: pdb?.ToArray(),
                    assemblyName: compilation.AssemblyName
                );
            }
            catch (OperationCanceledException oce)
            {
                AddTestimony(oce, CurrentTalkChapter, prompt);
                throw;
            }
            catch (Exception ex)
            {
                AddTestimony(ex, CurrentTalkChapter, prompt);
            }
        }

        protected override void PrepareForTalk()
        {
            base.PrepareForTalk();
            if (Interlocked.CompareExchange(ref _hasEmitted, 1, 0) != 0)
                throw new InvalidOperationException("The compilation has already been emitted. Let it emit only once per session.");

            foreach (var p in Prerequisite.Prompts)
                EmitCompilation(p);
        }

        protected override async Task PrepareForTalkAsync()
        {
            await base.PrepareForTalkAsync().ConfigureAwait(false);
            if (Interlocked.CompareExchange(ref _hasEmitted, 1, 0) != 0)
                throw new InvalidOperationException("The compilation has already been emitted. Let it emit only once per session.");

            var tasks = Prerequisite.Prompts.Select(p => Task.Run(() => EmitCompilation(p), p.CancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}

/****************
 * AnalyzeIL    *
 ****************/
namespace Boostable.WhatRoslynTalkAbout.AnalyzeIL
    {
        using Boostable.WhatRoslynTalkAbout.ArrangeCodes;
        using Boostable.WhatRoslynTalkAbout.EmitCompilations;
    using Boostable.WhatTalkAbout.Abstractions;
    using Microsoft.CodeAnalysis;
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Reflection;
        using System.Threading;
        using System.Threading.Tasks;

        // =======================
        // Prompt / Artifacts
        // =======================

        // Prompt: 何を測るかのスイッチ（最低限）
        public interface IAnalyzeILPromptForTalking<out TSelf> : IEmitCompilationsPromptForTalking<TSelf>
        where TSelf : class, IAnalyzeILPromptForTalking<TSelf>
    {
        bool CollectTypeCounts { get; }          // 型/メソッド数などざっくり
        bool CollectAssemblyIdentity { get; }    // AssemblyName / Version
    }

    // Artifacts: メトリクスを格納（必要に応じて拡張）
    public interface IReadOnlyAnalyzeILArtifacts : IReadOnlyEmitCompilationsArtifacts
    {
        IReadOnlyDictionary<string, object>? IlMetrics { get; }
    }

    public interface IAnalyzeILArtifacts : IEmitCompilationsArtifacts, IReadOnlyAnalyzeILArtifacts
    {
        void SetIlMetrics(IReadOnlyDictionary<string, object> metrics);
    }

    // =======================
    // Talk Outline
    // =======================

    public class AnalyzeILTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : EmitCompilationsTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IAnalyzeILPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyAnalyzeILArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IAnalyzeILArtifacts, IArtifacts, new()
    {
        private static ITalkChapter CurrentTalkChapter { get; } = new TalkChapter("Analyzing IL Chapter");
        private int _hasAnalyzed;
        private IAssemblyLoader AssemblyLoader { get; }

        protected internal AnalyzeILTalkOutlineBase(
            Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
            IAssemblyLoader asmLoader,
            IReadOnlyList<TPrompt> prompts,
            params ITalkChapter[] chapters
        ) : base(funcToArrangeCodes, prompts, [CurrentTalkChapter, .. chapters])
        {
            AssemblyLoader = asmLoader ?? throw new ArgumentNullException(nameof(asmLoader));
        }

        protected internal virtual void AnalyzeIl(TPrompt prompt)
        {
            try
            {
                var pe = Prerequisite.Artifacts.PeImage;
                if (pe is null) return; // Emit 失敗時はスキップ

                prompt.CancellationToken.ThrowIfCancellationRequested();

                var metrics = new Dictionary<string, object>
                {
                    ["PeSize"] = pe.Count,
                    ["PdbSize"] = Prerequisite.Artifacts.PdbImage?.Count ?? 0,
                    ["AssemblyName"] = Prerequisite.Artifacts.AssemblyName ?? "(unknown)"
                };

                using var peStream = new MemoryStream([.. pe]);
                using var pdbStream = Prerequisite.Artifacts.PdbImage is { } pdbBytes
                    ? new MemoryStream([.. pdbBytes])
                    : null;

                // ざっくり反射ベースの軽量解析（生ILでなくてもまずはOK）
                if (prompt.CollectTypeCounts || prompt.CollectAssemblyIdentity)
                {
                    var options = new AssemblyLoadOptions
                    {
                        LogicalName = CurrentTalkChapter.Name.Replace(' ', '_'),
                        LeaveStreamsOpen = false,
                        CancellationToken = prompt.CancellationToken
                    };

                    var loaded = AssemblyLoader.LoadFromStreams(peStream, pdbStream, options);

                    try
                    {
                        var asm = loaded.Assembly;

                        if (prompt.CollectAssemblyIdentity)
                        {
                            var an = asm.GetName();
                            metrics["Identity.Name"] = an.Name ?? "";
                            metrics["Identity.Version"] = an.Version?.ToString() ?? "";
                        }

                        if (prompt.CollectTypeCounts)
                        {
                            var types = asm.DefinedTypes.ToArray();
                            metrics["TypeCount"] = types.Length;

                            var methodCount = 0;
                            foreach (var t in types)
                            {
                                prompt.CancellationToken.ThrowIfCancellationRequested();
                                methodCount += t.GetMethods(
                                    BindingFlags.Public | BindingFlags.NonPublic |
                                    BindingFlags.Instance | BindingFlags.Static |
                                    BindingFlags.DeclaredOnly).Length;
                            }
                            metrics["MethodCount"] = methodCount;
                        }
                    }
                    finally
                    {
                        // Unload可能ならここで寿命を閉じる（ALC.Unload等は実装側）
                        loaded.Lease?.Dispose();
                    }
                }

                Prerequisite.Artifacts.SetIlMetrics(metrics);
            }
            catch (OperationCanceledException oce)
            {
                AddTestimony(oce, CurrentTalkChapter, prompt);
                throw;
            }
            catch (Exception ex)
            {
                AddTestimony(ex, CurrentTalkChapter, prompt);
            }
        }

        protected override void PrepareForTalk()
        {
            base.PrepareForTalk();
            if (Interlocked.CompareExchange(ref _hasAnalyzed, 1, 0) != 0)
                throw new InvalidOperationException("The IL has already been analyzed. Let it analyze only once per session.");

            foreach (var p in Prerequisite.Prompts)
                AnalyzeIl(p);
        }

        protected override async Task PrepareForTalkAsync()
        {
            await base.PrepareForTalkAsync().ConfigureAwait(false);
            if (Interlocked.CompareExchange(ref _hasAnalyzed, 1, 0) != 0)
                throw new InvalidOperationException("The IL has already been analyzed. Let it analyze only once per session.");

            var tasks = Prerequisite.Prompts.Select(p => Task.Run(() => AnalyzeIl(p), p.CancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}

/****************
 * Execute      *
 ****************/
namespace Boostable.WhatRoslynTalkAbout.ExecuteAssemblies
    {
        using Boostable.WhatRoslynTalkAbout.AnalyzeIL;
        using Boostable.WhatRoslynTalkAbout.ArrangeCodes;
    using Boostable.WhatTalkAbout.Abstractions;
    using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp;
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Reflection;
        using System.Threading;
        using System.Threading.Tasks;

        // Prompt: 実行ポリシー（stdout 関連は削除）
        public interface IExecuteAssembliesPromptForTalking<out TSelf> : IAnalyzeILPromptForTalking<TSelf>
        where TSelf : class, IExecuteAssembliesPromptForTalking<TSelf>
    {
        /// <summary>EntryPoint の明示名（null なら Assembly.EntryPoint を使用）</summary>
        string? EntryPointName { get; }

        /// <summary>引数をどう渡すか（null なら空配列）</summary>
        IReadOnlyList<string>? Args { get; }
    }

    // Artifacts: 実行結果（stdout は持たない）
    public interface IReadOnlyExecuteAssembliesArtifacts : IReadOnlyAnalyzeILArtifacts
    {
        object? ReturnValue { get; }
    }

    public interface IExecuteAssembliesArtifacts : IAnalyzeILArtifacts, IReadOnlyExecuteAssembliesArtifacts
    {
        void SetExecutionResult(object? returnValue);
    }

    /// <summary>Assembly をどう実行するかの戦略（利用側が差し替え可能）</summary>
    public interface IAssemblyExecutor<in TPrompt>
        where TPrompt : class, IExecuteAssembliesPromptForTalking<TPrompt>
    {
        /// <summary>この Executor が EntryPoint(Main) を必要とするか。</summary>
        bool RequiresEntryPoint { get; }

        Task<object?> ExecuteAsync(Assembly assembly, TPrompt prompt, CancellationToken ct);
    }

    /// <summary>既定の実行ポリシー：EntryPoint(Main) を走らせる</summary>
    public sealed class MainEntryAssemblyExecutor<TPrompt> : IAssemblyExecutor<TPrompt>
        where TPrompt : class, IExecuteAssembliesPromptForTalking<TPrompt>
    {
        public bool RequiresEntryPoint => true; // ★ Main 実行前提

        public async Task<object?> ExecuteAsync(Assembly asm, TPrompt prompt, CancellationToken ct)
        {
            var entry = ResolveEntryPoint(asm, prompt)
                ?? throw new MissingMethodException("EntryPoint not found.");

            var args = (prompt.Args?.ToArray()) ?? [];
            return await InvokeEntryPoint(entry, args, ct).ConfigureAwait(false);
        }

        private static MethodInfo? ResolveEntryPoint(Assembly asm, TPrompt prompt)
        {
            if (!string.IsNullOrWhiteSpace(prompt.EntryPointName))
                return asm.GetType(prompt.EntryPointName!)?.GetMethod(
                    "Main",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                );

            return asm.EntryPoint;
        }

        private static async Task<object?> InvokeEntryPoint(MethodInfo entry, string[] args, CancellationToken ct)
        {
            var ps = entry.GetParameters();
            object?[] callArgs =
                ps.Length == 0 ? []
              : ps.Length == 1 && ps[0].ParameterType == typeof(string[])
                ? [args]
                : throw new NotSupportedException($"Unsupported entry point signature: {entry}");

            var ret = entry.Invoke(null, callArgs);

            // async Task / Task<int> 対応
            if (ret is Task t)
            {
                using var _ = ct.Register(() => { /* 任意でキャンセル連携 */ });
                await t.ConfigureAwait(false);
                if (t.GetType().IsGenericType)
                    return t.GetType().GetProperty("Result")!.GetValue(t);
                return null;
            }

            return ret;
        }
    }

    public class ExecuteAssembliesTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : AnalyzeILTalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IExecuteAssembliesPromptForTalking<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyExecuteAssembliesArtifacts
        where TArtifacts : class, TReadOnlyArtifacts, IExecuteAssembliesArtifacts, IArtifacts, new()
    {
        private static ITalkChapter CurrentTalkChapter { get; } = new TalkChapter("Executing Assembly Chapter");
        private int _hasExecuted;

        private readonly IAssemblyLoader AssemblyLoader;
        private readonly IAssemblyExecutor<TPrompt> Executor;

        protected internal ExecuteAssembliesTalkOutlineBase(
            Func<IEnumerable<VirtualSource>> funcToArrangeCodes,
            IAssemblyLoader asmLoader,
            IReadOnlyList<TPrompt> prompts,
            IAssemblyExecutor<TPrompt>? executor = null,
            params ITalkChapter[] chapters
        ) : base(funcToArrangeCodes, asmLoader, prompts, [CurrentTalkChapter, .. chapters])
        {
            AssemblyLoader = asmLoader ?? throw new ArgumentNullException(nameof(asmLoader));
            Executor = executor ?? new MainEntryAssemblyExecutor<TPrompt>();
        }

        protected internal virtual void ExecuteAssembly(TPrompt prompt)
        {
            try
            {
                var pe = Prerequisite.Artifacts.PeImage;
                if (pe is null) return; // Emit 失敗時はスキップ

                prompt.CancellationToken.ThrowIfCancellationRequested();

                using var peStream = new MemoryStream([.. pe]);
                using var pdbStream = Prerequisite.Artifacts.PdbImage is { } pdb
                    ? new MemoryStream([.. pdb])
                    : null;

                var options = new AssemblyLoadOptions {
                    LogicalName = CurrentTalkChapter.Name.Replace(' ', '_'),
                    LeaveStreamsOpen = false,
                    CancellationToken = prompt.CancellationToken
                };

                var loaded = AssemblyLoader.LoadFromStreams(peStream, pdbStream, options);

                try
                {
                    var asm = loaded.Assembly;
                    var result = Executor.ExecuteAsync(asm, prompt, prompt.CancellationToken)
                                         .GetAwaiter().GetResult();
                    Prerequisite.Artifacts.SetExecutionResult(result);
                }
                finally
                {
                    loaded.Lease?.Dispose();
                }
            }
            catch (OperationCanceledException oce)
            {
                AddTestimony(oce, CurrentTalkChapter, prompt);
                throw;
            }
            catch (Exception ex)
            {
                AddTestimony(ex, CurrentTalkChapter, prompt);
            }
        }

        protected override void PrepareForTalk()
        {
            base.PrepareForTalk();
            if (Interlocked.CompareExchange(ref _hasExecuted, 1, 0) != 0)
                throw new InvalidOperationException("The assembly has already been executed. Let it execute only once per session.");

            foreach (var p in Prerequisite.Prompts)
                ExecuteAssembly(p);
        }

        protected override async Task PrepareForTalkAsync()
        {
            await base.PrepareForTalkAsync().ConfigureAwait(false);
            if (Interlocked.CompareExchange(ref _hasExecuted, 1, 0) != 0)
                throw new InvalidOperationException("The assembly has already been executed. Let it execute only once per session.");

            var tasks = Prerequisite.Prompts.Select(p => Task.Run(() => ExecuteAssembly(p), p.CancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        protected override CSharpCompilationOptions OnUseCompilationOptions(
            CSharpCompilationOptions options, TPrompt prompt)
        {
            options = base.OnUseCompilationOptions(options, prompt);

            // Executor の要件に合わせて OutputKind を自動切り替え
            return Executor.RequiresEntryPoint
                ? options.WithOutputKind(OutputKind.ConsoleApplication)
                : options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);
        }
    }
}