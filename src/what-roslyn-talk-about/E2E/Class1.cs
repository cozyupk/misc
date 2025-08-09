using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Boostable.WhatRoslynTalkAbout
{
    public interface IWhatRoslynTalkedAbout
    {
        // Just an alias for me.

        // Yes, I know this is redundant.
        // But Roslyn talks, and sometimes I need to listen in a different voice.
        // Deal with it, Copilot

        // WhatCopilotTalkedAbout:
        // • IWhatRoslynTalkedAbout
        // → An alias-like interface for IRoslynTestimony.
        // While it currently duplicates responsibility, it is acceptable if intended for future extension or use in a different context.

        // WhatChatGPTTalkedAbout:
        // This type name is a literary homage to Haruki Murakami, chosen for its expressive tone and its ability to convey both implementation intent and emotional nuance to the reader.
        // While static analysis or naming convention checkers may flag it as overly verbose, it remains syntactically valid and contextually rich.
        // In other words: it’s fine just the way it is.
    }

    public enum TestimonyPhase
    {
        Parsing,
        Compilation,
        ILComparison,
        Execution,
    }

    public readonly record struct VirtualSource
    {
        public string Path { get; }
        public string Code { get; }
        public VirtualSource(string path, string code)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }
    }

    public readonly record struct PromptPhase
    {
        public RoslynTalkOutlinePrompt Prompt { get; }
        public TestimonyPhase Phase { get; }
        public PromptPhase(RoslynTalkOutlinePrompt prompt, TestimonyPhase phase)
        {
            Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
            Phase = phase;
        }
    };

    public readonly record struct ExecutionResult
    {
        public object? ReturnValue { get; }
        public Exception? Error { get; }
        public TimeSpan Duration { get; }

        public ExecutionResult(object returnValue, TimeSpan duration)
        {
            ReturnValue = returnValue ?? throw new ArgumentNullException(nameof(returnValue));
            Duration = duration;
            Error = null;
        }

        public ExecutionResult(Exception error, TimeSpan duration)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            Duration = duration;
            ReturnValue = null;
        }
    }

    public interface IRoslynTestimony : IWhatRoslynTalkedAbout
    {
        public interface IPrerequisite
        {
            IEnumerable<RoslynTalkOutlinePrompt> Prompts { get; }

            bool IsExecutionEnabled { get; }
            string? TargetCode { get; }
        }

        bool IsMeaningful { get; }
        IPrerequisite Prerequisite { get; }

        IReadOnlyCollection<Exception> GeneralTestimony { get; }

        IReadOnlyDictionary<
            (RoslynTalkOutlinePrompt, TestimonyPhase),
            IReadOnlyCollection<Exception>
        > TestimonyForEachPromptAndPhase
        { get; }

        IReadOnlyDictionary<
            RoslynTalkOutlinePrompt,
            IReadOnlyCollection<Exception>
        > TestimonyForEachPrompt
        { get; }

        IReadOnlyDictionary<
            TestimonyPhase,
            IReadOnlyCollection<Exception>
        > TestimonyForEachPhase
        { get; }

        IReadOnlyDictionary<
            RoslynTalkOutlinePrompt,
            IReadOnlyCollection<Diagnostic>?
        > DiagnosticsInCompilingPhase
        { get; }

        IReadOnlyDictionary<
            RoslynTalkOutlinePrompt,
            AssemblyDefinition?
        > AssemblyDefinitions
        { get; }

        IReadOnlyDictionary<
            RoslynTalkOutlinePrompt,
            string?
        > IntermediateCodes
        { get; }

        IReadOnlyDictionary<
            RoslynTalkOutlinePrompt,
            Assembly?
        > Assemblies
        { get; }

        IReadOnlyCollection<PromptPhase> TestimonyForEachPromptAndPhaseWithPhase { get; }
        IReadOnlyCollection<Exception> AllTestimony { get; }
    }

    public class RoslynTalkOutlinePrompt
    {
        public string Label { get; }
        public CSharpParseOptions ParseOptions { get; }
        public CSharpCompilationOptions CompilationOptions { get; }
        public System.Text.Encoding EncodingForParse { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public RoslynTalkOutlinePrompt(
            string label,
            CSharpParseOptions? parseOptions = null,
            CSharpCompilationOptions? compilationOptions = null,
            System.Text.Encoding? encodingForParse = null,
            CancellationToken cancellationToken = default
        )
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            ParseOptions = parseOptions ?? CSharpParseOptions.Default;
            CompilationOptions = compilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            EncodingForParse = encodingForParse ?? System.Text.Encoding.UTF8;
            CancellationToken = cancellationToken;
        }

        public RoslynTalkOutlinePrompt(
            RoslynTalkOutlinePrompt source
        )
        {
            Label = source.Label;
            ParseOptions = source.ParseOptions;
            CompilationOptions = source.CompilationOptions;
            EncodingForParse = source.EncodingForParse;
            CancellationToken = source.CancellationToken;
        }
    }

    public static class RoslynTalkOutlinePromptExtensions
    {

        public static Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>
            Inject(this Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>> prompts, System.Text.Encoding encoding)
        {
            return (baseParseOpt, baseCompilationOpt) =>
            {
                return [.. prompts(baseParseOpt, baseCompilationOpt).Select(prompt =>
                    new RoslynTalkOutlinePrompt(prompt) { EncodingForParse = encoding }
                )];
            };
        }

        public static Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>
            Inject(Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>> prompts, CancellationToken ct)
        {
            return (baseParseOpt, baseCompilationOpt) =>
            {
                return [.. prompts(baseParseOpt, baseCompilationOpt).Select(prompt =>
                    new RoslynTalkOutlinePrompt(prompt){ CancellationToken= ct }
                )];
            };
        }

        public static Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>
            Inject(this Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>> prompts, TimeSpan timeout)
        {
            return (baseParseOpt, baseCompilationOpt) =>
            {
                var cts = new CancellationTokenSource(timeout);

                return [.. prompts(baseParseOpt, baseCompilationOpt).Select(prompt =>
                    new RoslynTalkOutlinePrompt(prompt)
                    {
                        CancellationToken = cts.Token
                    }
                )];
            };
        }
    }

    public class RoslynTestBase
    {
        protected virtual IEnumerable<RoslynTalkOutlinePrompt> ForAllLangVersions(
            CSharpParseOptions baseParseOpt,
            CSharpCompilationOptions baseCompilationOpt,
            IReadOnlyCollection<LanguageVersion>? languageVersionsToExclude = null

        )
        {
            IEnumerable<LanguageVersion> allLangVersions = [.. Enum.GetValues(typeof(LanguageVersion))
                    .Cast<LanguageVersion>()
                    .Where(v => v != LanguageVersion.Default && v != LanguageVersion.Latest && v != LanguageVersion.Preview)
                    .OrderBy(v => (int)v)];
            if (languageVersionsToExclude is not null)
            {
                allLangVersions = [.. allLangVersions.Where(v => !languageVersionsToExclude.Contains(v))];
            }
            return [.. allLangVersions.Select(langVersion =>
                new RoslynTalkOutlinePrompt(
                    $"For {langVersion}",
                    baseParseOpt.WithLanguageVersion(langVersion),
                    baseCompilationOpt
                )
            )];
        }

        public Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>
            CreatePromptVariationBuilderForAllCSharpLangVersions(IReadOnlyCollection<LanguageVersion>? languageVersionsToExclude = null)
                => (baseParseOpt, baseCompilationOpt) => ForAllLangVersions(baseParseOpt, baseCompilationOpt, languageVersionsToExclude);
    }

    // This class represents a lightweight entry point for testing Roslyn code analysis.
    // It abstracts away most of the complexity — think of it as “Roslyn Talks for Humans”.
    public class SimpleRoslynTalkSession : RoslynTalkSession<IRoslynTestimony>
    {

        public SimpleRoslynTalkSession(
            IEnumerable<RoslynTalkOutlinePrompt> prompts,
            string arrangeCode,
            IReadOnlyCollection<string>? referenceLocations = null,
            Func<Assembly, ExecutionResult>? funcToExecute = null,
            bool enableExcecution = true,
            Func<Func<string>, Action<string>?, IEnumerable<RoslynTalkOutlinePrompt>, bool, IRoslynTalkOutline>? outlineFactory = null
        ) : base(
            () => [("", arrangeCode)], referenceLocations, funcToExecute,
            null, null, enableExcecution, (_1, _2) => prompts, outlineFactory
        )
        {
            // No additional initialization required.
        }
    }

    public interface IRoslynTalkOutline<out TTalkAbout>
    {
        void CompileCode();
        Task CompileCodeAsync();

        TTalkAbout TalkedAbout();
    }

    public class RoslynTalkSession<TTalkAbout>
        where TTalkAbout : class, IRoslynTestimony
    {
        protected IRoslynTalkOutline<TTalkAbout> Outline { get; }

        private int _hasRun = 0;

        public RoslynTalkSession(
            Func<IReadOnlyCollection<(string, string)>> arrangeCodes,
            IReadOnlyCollection<string>? referenceLocations = null,
            CSharpParseOptions? baseParseOpt = null, CSharpCompilationOptions? baseCompilationOpt = null,
            Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>? promptVariationBuilder = null,
            Func<Func<string>, Action<string>?, IEnumerable<RoslynTalkOutlinePrompt>, bool, IRoslynTalkOutline<TTalkAbout>>? outlineFactory = null
        )
        {
            _ = arrangeCodes ?? throw new ArgumentNullException(nameof(arrangeCodes));
            baseParseOpt ??= CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            baseCompilationOpt ??= new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            promptVariationBuilder ??= (baseParseOpt, baseCompilationOpt) => DefaultVariationBuilder(baseParseOpt, baseCompilationOpt);
            var prompts = promptVariationBuilder(baseParseOpt, baseCompilationOpt);
            outlineFactory ??= DefaultOutlineFactory; // // Allow the factory to be overridden, e.g. for outline injection or behavior customization.
            Outline = outlineFactory(arrangeCodes, referenceLocations, prompts);
        }

        public TTalkAbout TalkAbout()
        {
            if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
            {
                throw new InvalidOperationException("Roslyn has already spoken. Let it speak only once per session.");
            }

            Outline.CompileCode();
            // TestCase.ExecuteIL(); // TODO: Implement synchronous execution of IL.
            return Outline.TalkedAbout();
        }

        public async Task<TTalkAbout> TalkAboutAsync()
        {
            if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
            {
                throw new InvalidOperationException("Roslyn has already spoken. Let it speak only once per session.");
            }

            await Outline.CompileCodeAsync();
            return Outline.TalkedAbout();
        }

        // The default label injector for file paths.
        protected virtual Func<string, string, string> DefaultLabeIntoPathInjector()
        {
            return (path, label) =>
            {
                // 拡張子 .cs の直前に _{label} を挿入
                var ext = ".cs";
                if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    return path.Substring(0, path.Length - ext.Length) + $"_{label}" + ext;
                }

                // 拡張子がない or .cs で終わってない場合は末尾に追加
                return path + $"_{label}";
            };
        }

        // The default variation builder — when Roslyn talks without being asked to vary.
        protected virtual IEnumerable<RoslynTalkOutlinePrompt> DefaultVariationBuilder(
            CSharpParseOptions baseParseOpt,
            CSharpCompilationOptions baseCompilationOpt
        )
        {
            return [
                new RoslynTalkOutlinePrompt(
                    "Default Prompt (without promptVariationBuilder)",
                    baseParseOpt,
                    baseCompilationOpt
                )
            ];
        }

        // This factory drafts Roslyn’s outline, including her prompts and test points.
        protected virtual IRoslynTalkOutline<TTalkAbout> DefaultOutlineFactory(
            Func<string> arrangeCode,
            Action<string>? actForTheCode,
            IEnumerable<RoslynTalkOutlinePrompt> promptsToActCode,
            bool enableILAnalysis
        )
        {
            return new RoslynTalkOutline<TTalkAbout>(arrangeCode, actForTheCode, promptsToActCode, enableILAnalysis);
        }
    }

    public class RoslynTalkOutline<TTalkAbout> : IRoslynTalkOutline<TTalkAbout>, IRoslynTestimony
            where TTalkAbout : class, IRoslynTestimony
    {
        bool IRoslynTestimony.IsMeaningful => false; // TODO: Implement this property

        IRoslynTestimonyWithExecution.IPrerequisite IRoslynTestimony.Prerequisite => Prerequisite;

        IReadOnlyCollection<Exception> IRoslynTestimony.GeneralTestimony => throw new NotImplementedException();

        IReadOnlyDictionary<(RoslynTalkOutlinePrompt, TestimonyPhase), IReadOnlyCollection<Exception>> IRoslynTestimony.TestimonyForEachPromptAndPhase => throw new NotImplementedException();

        IReadOnlyDictionary<RoslynTalkOutlinePrompt, IReadOnlyCollection<Exception>> IRoslynTestimony.TestimonyForEachPrompt => throw new NotImplementedException();

        IReadOnlyDictionary<TestimonyPhase, IReadOnlyCollection<Exception>> IRoslynTestimony.TestimonyForEachPhase => throw new NotImplementedException();

        IReadOnlyDictionary<RoslynTalkOutlinePrompt, IReadOnlyCollection<Diagnostic>?> IRoslynTestimony.DiagnosticsInCompilingPhase => throw new NotImplementedException();

        IReadOnlyDictionary<RoslynTalkOutlinePrompt, AssemblyDefinition?> IRoslynTestimony.AssemblyDefinitions => throw new NotImplementedException();

        IReadOnlyDictionary<RoslynTalkOutlinePrompt, string?> IRoslynTestimony.IntermediateCodes => throw new NotImplementedException();

        IReadOnlyDictionary<RoslynTalkOutlinePrompt, Assembly?> IRoslynTestimony.Assemblies => throw new NotImplementedException();

        IReadOnlyCollection<PromptPhase> IRoslynTestimony.TestimonyForEachPromptAndPhaseWithPhase => throw new NotImplementedException();

        IReadOnlyCollection<Exception> IRoslynTestimony.AllTestimony => throw new NotImplementedException();

        void IRoslynTalkOutline<TTalkAbout>.CompileCode()
            => CompileCode();

        async Task IRoslynTalkOutline<TTalkAbout>.CompileCodeAsync()
            => await CompileCodeAsync().ConfigureAwait(false);

        TTalkAbout IRoslynTalkOutline<TTalkAbout>.TalkedAbout()
        {
            if (this is not TTalkAbout retval)
            {
                var actualType = GetType();
                var expectedType = typeof(TTalkAbout);
                throw new InvalidOperationException(
                    $"The instance of type '{actualType.FullName}' does not implement '{expectedType.FullName}'. " +
                    $"This likely indicates a misconfiguration of your outline class. " +
                    $"Ensure that '{actualType.Name}' implements or derives from '{expectedType.Name}'."
                );
            }
            return retval;
        }

        protected int _hasCompiled = 0;
        protected virtual Parameters Prerequisite { get; }

        protected virtual Dictionary<RoslynTalkOutlinePrompt, Stream> AssemblyBinaryStreams { get; } = [];

        protected class Parameters(
            Func<string> arrangeCode,
            Action<string>? actForTheCode,
            IEnumerable<RoslynTalkOutlinePrompt> prompts,
            bool enableExcecution
            ) : IRoslynTestimonyWithExecution.IPrerequisite
        {
            IEnumerable<RoslynTalkOutlinePrompt> IRoslynTestimonyWithExecution.IPrerequisite.Prompts => Prompts;

            bool IRoslynTestimonyWithExecution.IPrerequisite.IsExecutionEnabled => IsExecutionEnabled;

            string? IRoslynTestimonyWithExecution.IPrerequisite.TargetCode => TargetCode;

            public virtual Func<string> ArrangeCode { get; } = arrangeCode;
            public virtual Action<string>? ActForTheCode { get; } = actForTheCode;
            public virtual IEnumerable<RoslynTalkOutlinePrompt> Prompts { get; } = prompts;
            public virtual bool IsExecutionEnabled { get; } = enableExcecution;
            public virtual string? TargetCode { get; set; }
        }

        protected internal RoslynTalkOutline(
                Func<string> arrangeCode,
                Action<string>? actForTheCode,
                IEnumerable<RoslynTalkOutlinePrompt> prompts,
                bool enableExcecution
            )
        {
            _ = arrangeCode ?? throw new ArgumentNullException(nameof(arrangeCode));
            _ = actForTheCode;
            _ = prompts ?? throw new ArgumentNullException(nameof(prompts));
            if (!prompts.Any())
            {
                throw new ArgumentException("At least one prompt must be provided.", nameof(prompts));
            }

            Prerequisite = new Parameters(arrangeCode, actForTheCode, prompts, enableExcecution);
        }

        protected internal virtual void CompileCode()
        {
            if (Interlocked.CompareExchange(ref _hasCompiled, 1, 0) != 0)
            {
                throw new InvalidOperationException("The code has already been compiled. Let it compile only once per session.");
            }

            try
            {
                Prerequisite.TargetCode = Prerequisite.ArrangeCode();
            }
            catch (Exception)
            {
                // TODO: Let the exception be into the IWhatRoslynTalkedAbout instance.
                throw;
            }

            foreach (var prompt in Prerequisite.Prompts)
            {
                try
                {
                    CompileCodeInternal(Prerequisite.TargetCode, prompt);
                }
                catch (Exception ex)
                {
                    // TODO: Let the exception be into the IWhatRoslynTalkedAbout instance.
                    Console.WriteLine($"An error occurred while compiling the code: {ex.Message}");
                    throw;
                }
            }
        }

        protected internal virtual async Task CompileCodeAsync()
        {
            if (Interlocked.CompareExchange(ref _hasCompiled, 1, 0) != 0)
            {
                throw new InvalidOperationException("The code has already been compiled. Let it compile only once per session.");
            }

            try
            {
                Prerequisite.TargetCode = Prerequisite.ArrangeCode();
            }
            catch (Exception)
            {
                // TODO: Let the exception be into the IWhatRoslynTalkedAbout instance.
                throw;
            }

            List<Task> tasks = [];
            foreach (var prompt in Prerequisite.Prompts)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        CompileCodeInternal(Prerequisite.TargetCode, prompt);
                    }
                    catch (Exception)
                    {
                        // TODO: Implement proper exception handling and logging.
                    }
                }));
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        protected internal virtual (Stream, EmitResult)
            CompileCodeInternal(
                IReadOnlyCollection<(string path, string sourceText)> codes,
                IReadOnlyCollection<string> referenceLocations,
                RoslynTalkOutlinePrompt prompt
            )
        {
            var syntaxTrees = codes.Select(file =>
                    CSharpSyntaxTree.ParseText(
                        file.sourceText,
                        options: prompt.ParseOptions,
                        path: file.path,
                        cancellationToken: prompt.CancellationToken
                    )
            );

            var references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Cast<MetadataReference>()
                .Union(
                    referenceLocations.Select(path =>
                        MetadataReference.CreateFromFile(path)
                    )
                );

            var compilation = CSharpCompilation.Create(
                assemblyName: AssemblyNameEscaper.EscapeToAssemblyName(prompt.Label),
                syntaxTrees: syntaxTrees,
                references: references,
                options: prompt.CompilationOptions
            );

            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            Console.WriteLine($"{AssemblyNameEscaper.EscapeToAssemblyName(prompt.Label)}");
            foreach (var diagnostic in emitResult.Diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    Console.WriteLine($"Error: {diagnostic.GetMessage()}");
                }
                else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                {
                    Console.WriteLine($"Warning: {diagnostic.GetMessage()}");
                }
            }
            if (emitResult.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                Console.WriteLine(ILDisassembler.Disassemble(ms));
            }
            return (ms, emitResult);
        }

        protected internal static partial class AssemblyNameEscaper
        {
            private static readonly Regex SafeChars = new(@"^[a-zA-Z0-9._-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            public static string EscapeToAssemblyName(string input)
            {
                if (SafeChars.IsMatch(input))
                {
                    return input;
                }

                var sb = new StringBuilder();
                var sbSub = new StringBuilder();
                foreach (var ch in input)
                {
                    if ((ch >= 'a' && ch <= 'z') ||
                        (ch >= 'A' && ch <= 'Z') ||
                        (ch >= '0' && ch <= '9') ||
                        ch == '.' || ch == '_' || ch == '-')
                    {
                        sb.Append(ch);
                    }
                    else
                    {
                        sbSub.AppendFormat("u{0:X4}", (int)ch); // escape unicode characters
                    }
                }

                var sub = sb.ToString();
                if (sbSub.Length > 0)
                {
                    sb.Append("_");
                    sb.Append(sub);
                }
                return sb.ToString();
            }
        }

        protected internal static class ILDisassembler
        {
            public static string Disassemble(Stream assemblyStream)
            {
                var result = new StringBuilder();

                var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyStream);
                foreach (var type in assembly.MainModule.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        result.AppendLine($".method {method.Name}");
                        if (method.HasBody)
                        {
                            foreach (var instr in method.Body.Instructions)
                            {
                                result.AppendLine($"  IL_{instr.Offset:X4}: {instr.OpCode} {FormatOperand(instr.Operand)}");
                            }
                        }
                        result.AppendLine();
                    }
                }
                return result.ToString();
            }

            private static string FormatOperand(object? operand)
            {
                return operand switch
                {
                    null => "",
                    MethodReference m => m.FullName,
                    FieldReference f => f.FullName,
                    TypeReference t => t.FullName,
                    Instruction i => $"IL_{i.Offset:X4}",
                    Instruction[] targets => string.Join(", ", targets.Select(t => $"IL_{t.Offset:X4}")),
                    _ => operand.ToString() ?? ""
                };
            }
        }
    }

    /*******************************************************************************
     * Implement WithExecution version for a Proof of Extensibility (OCP concepts) *
     *******************************************************************************/

        public class SimpleRoslynTalkSessionWithExecution : RoslynTalkSessionWithExecution<IRoslynTestimonyWithExecution>
            public SimpleRoslynTalkSessionWithExecution(
            string arrangeCode, IReadOnlyCollection<string>? referenceLocations = null,
            Func<Assembly, ExecutionResult>? funcToExecute = null,
            CSharpParseOptions? baseParseOpt = null, CSharpCompilationOptions? baseCompilationOpt = null,
            bool enableExcecution = true,
            Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>? promptVariationBuilder = null,
            Func<Func<string>, Action<string>?, IEnumerable<RoslynTalkOutlinePrompt>, bool, IRoslynTalkOutline>? outlineFactory = null
        ) : base(
            () => [("", arrangeCode)], referenceLocations, funcToExecute,
            baseParseOpt, baseCompilationOpt, enableExcecution, promptVariationBuilder, outlineFactory
        )
        {
            // No additional initialization required.
        }

    public interface IRoslynTestimonyWithExecution : IRoslynTestimony
    {

        IReadOnlyDictionary<
            RoslynTalkOutlinePrompt,
            ExecutionResult?
        > ExecutionResults
        { get; }
    }

    public interface IRoslynTalkOutlineWithExecution<out TTalkAbout> : IRoslynTalkOutline<TTalkAbout>
    {
    }

    public class RoslynTalkSessionWithExecution<TTalkAbout> : RoslynTalkSession<TTalkAbout>
        where TTalkAbout : class, IWhatRoslynTalkedAbout
    {
        public RoslynTalkSessionWithExecution(
            Func<IReadOnlyCollection<(string, string)>> arrangeCodes,
            IReadOnlyCollection<string>? referenceLocations = null,
            CSharpParseOptions? baseParseOpt = null, CSharpCompilationOptions? baseCompilationOpt = null,
            Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>? promptVariationBuilder = null,
            Func<Func<string>, Action<string>?, IEnumerable<RoslynTalkOutlinePrompt>, bool, TTalkAbout>? outlineFactory = null
        ) : base(
            arrangeCodes, referenceLocations, baseParseOpt, baseCompilationOpt,
            promptVariationBuilder, outlineFactory
        )
        {

            /*
                        if (enableExcecution)
                    {
                        baseCompilationOpt = baseCompilationOpt.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);  
                    }
            */

        }

    }
    public class RoslynTalkOutlineIRoslynTestimonyWithExecution<TTalkAbout> 
        : RoslynTalkOutline<TTalkAbout>, IRoslynTalkOutlineWithExecution<TTalkAbout>, IRoslynTestimonyWithExecution
            where TTalkAbout : class, IRoslynTestimonyWithExecution
    {
        protected internal RoslynTalkOutlineIRoslynTestimonyWithExecution(Func<string> arrangeCode, Action<string>? actForTheCode, IEnumerable<RoslynTalkOutlinePrompt> prompts, bool enableExcecution) : base(arrangeCode, actForTheCode, prompts, enableExcecution)
        {
        }

        IReadOnlyDictionary<RoslynTalkOutlinePrompt, ExecutionResult?> IRoslynTestimonyWithExecution.ExecutionResults => throw new NotImplementedException();

    }

}
