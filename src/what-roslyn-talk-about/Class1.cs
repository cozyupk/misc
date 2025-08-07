using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Boostable.WhatRoslynTalkAbout
{
    /// <summary>
    /// Serves as an alias interface for <see cref="IRoslynTestimony"/>, enabling alternative naming or future extension
    /// scenarios.
    /// </summary>
    /// <remarks>This interface currently duplicates the contract of <see cref="IRoslynTestimony"/> and is
    /// intended for use in contexts where a distinct type name or semantic differentiation is desired. It may be useful
    /// for code organization, expressive intent, or as a placeholder for future enhancements.</remarks>
    public interface IWhatRoslynTalkedAbout : IRoslynTestimony
    {
        // Just an alias for me.

        // Yes, I know this is redundant.
        // But Roslyn speaks, and sometimes I need to listen in a different voice.
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

    public interface IRoslynTestimony
    {
        bool IsMeaningful { get; }
        IPrerequisite Prerequisite { get; }

        public interface IPrerequisite {
            IEnumerable<RoslynTalkOutlinePrompt> Prompts { get; }

            bool IsILAnalysisEnabled { get; }
            string? TargetCodes { get; }
        }
    }

    public class RoslynTalkOutlinePrompt
    {
        public string Label { get; }
        public CSharpParseOptions ParseOptions { get; }
        public CSharpCompilationOptions CompilationOptions { get; }
        public System.Text.Encoding EncodingForParse { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public string Path { get; set; }

        public RoslynTalkOutlinePrompt(
            string label,
            CSharpParseOptions? parseOptions = null,
            CSharpCompilationOptions? compilationOptions = null,
            System.Text.Encoding? encodingForParse = null,
            string? path = null,
            CancellationToken cancellationToken = default
        ) {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            ParseOptions = parseOptions ?? CSharpParseOptions.Default;
            CompilationOptions = compilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            EncodingForParse = encodingForParse ?? System.Text.Encoding.UTF8;
            CancellationToken = cancellationToken;
            Path = path ?? $"{label}.cs";
        }

        public RoslynTalkOutlinePrompt(
            RoslynTalkOutlinePrompt source
        ) {
            Label = source.Label;
            ParseOptions = source.ParseOptions;
            CompilationOptions = source.CompilationOptions;
            EncodingForParse = source.EncodingForParse;
            CancellationToken = source.CancellationToken;
            Path = source.Path;
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
            CancellationTokenInjector(Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>> prompts, CancellationToken ct)
        {
            return (baseParseOpt, baseCompilationOpt) =>
            {
                return [.. prompts(baseParseOpt, baseCompilationOpt).Select(prompt =>
                    new RoslynTalkOutlinePrompt(prompt){ CancellationToken= ct }
                )];
            };
        }

        public static Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>
            PathProjector(Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>> prompts, Func<string, string> pathProjection)
        {
            return (baseParseOpt, baseCompilationOpt) =>
            {
                return [.. prompts(baseParseOpt, baseCompilationOpt).Select(prompt => {
                    var retval = new RoslynTalkOutlinePrompt(prompt);
                    retval.Path = pathProjection(retval.Label);
                    return retval;
                })];
            };
        }

        public static Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>
            TimeoutInjector(this Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>> prompts, TimeSpan timeout)
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

        ) {
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
            CreatePromptVariationBuilderForAllLangVersions(IReadOnlyCollection<LanguageVersion>? languageVersionsToExclude = null)
                => (baseParseOpt, baseCompilationOpt) => ForAllLangVersions(baseParseOpt, baseCompilationOpt, languageVersionsToExclude);
    }

    // This class represents a lightweight entry point for testing Roslyn code analysis.
    // It abstracts away most of the complexity — think of it as “Roslyn Talks for Humans”.
    public class SimpleRoslynTalkSession : RoslynTalkSession
    {
        public SimpleRoslynTalkSession(
            string arrangeCode, Action<string>? actForTheCode = null,
            CSharpParseOptions? baseParseOpt = null, CSharpCompilationOptions? baseCompilationOpt = null,
            bool enableILAnalysis = true,
            Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>? promptVariationBuilder = null,
            Func<Func<string>, Action<string>?, IEnumerable<RoslynTalkOutlinePrompt>, bool, RoslynTalkOutline>? outlineFactory = null
        ) : base(
            () => arrangeCode, actForTheCode,
            baseParseOpt, baseCompilationOpt, enableILAnalysis, promptVariationBuilder, outlineFactory
        )
        {
            // No additional initialization required.
        }

        public SimpleRoslynTalkSession(
            string arrangeCode, IEnumerable<RoslynTalkOutlinePrompt> prompts,
            Action<string>? actForTheCode = null,
            bool enableILAnalysis = true,
            Func<Func<string>, Action<string>?, IEnumerable<RoslynTalkOutlinePrompt>, bool, RoslynTalkOutline>? outlineFactory = null
        ) : base(
            () => arrangeCode, actForTheCode,
            null, null, enableILAnalysis, (_1, _2) => prompts, outlineFactory
        )
        {
            // No additional initialization required.
        }
    }

    public class RoslynTalkSession
    {
        protected RoslynTalkOutline Outline { get; }

        private int _hasRun = 0;

        public RoslynTalkSession(
            Func<string> arrangeCode, Action<string>? actForTheCode = null,
            CSharpParseOptions? baseParseOpt = null, CSharpCompilationOptions? baseCompilationOpt = null,
            bool enableILAnalysis = true,
            Func<CSharpParseOptions, CSharpCompilationOptions, IEnumerable<RoslynTalkOutlinePrompt>>? promptVariationBuilder = null,
            Func<Func<string>, Action<string>?, IEnumerable<RoslynTalkOutlinePrompt>, bool, RoslynTalkOutline>? outlineFactory = null
        ) {
            _ = arrangeCode ?? throw new ArgumentNullException(nameof(arrangeCode));
            baseParseOpt ??= CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            baseCompilationOpt ??= new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            promptVariationBuilder ??= (baseParseOpt, baseCompilationOpt) => DefaultVariationBuilder(baseParseOpt, baseCompilationOpt);
            if (enableILAnalysis)
            {
                baseCompilationOpt = baseCompilationOpt.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);  
            }
            var prompts = promptVariationBuilder(baseParseOpt, baseCompilationOpt);
            outlineFactory ??= DefaultOutlineFactory; // // Allow the factory to be overridden, e.g. for outline injection or behavior customization.
            Outline = outlineFactory(arrangeCode, actForTheCode, prompts, enableILAnalysis);
        }

        public IWhatRoslynTalkedAbout TalkAbout()
        {
            if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
            {
                throw new InvalidOperationException("Roslyn has already spoken. Let it speak only once per session.");
            }

            Outline.CompileAndAnalyzeIL();
            // TestCase.ExecuteIL(); // TODO: Implement synchronous execution of IL.
            return Outline;
        }

        public async Task<IWhatRoslynTalkedAbout> TalkAboutAsync()
        {
            if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
            {
                throw new InvalidOperationException("Roslyn has already spoken. Let it speak only once per session.");
            }

            // await Outline.CompileAndAnalyzeILAsync(); // TODO: Implement async compilation and IL analysis.
            // await TestCase.ExecuteILAsync(); // TODO: Implement async execution of IL.
            return Outline;
        }

        // The default variation builder — when Roslyn talks without being asked to vary.
        protected virtual IEnumerable<RoslynTalkOutlinePrompt> DefaultVariationBuilder(
            CSharpParseOptions baseParseOpt,
            CSharpCompilationOptions baseCompilationOpt
        ) {
            return [ 
                new RoslynTalkOutlinePrompt(
                    "Default Prompt (without promptVariationBuilder)",
                    baseParseOpt,
                    baseCompilationOpt
                )
            ];
        }

        // This factory drafts Roslyn’s outline, including her prompts and test points.
        protected virtual RoslynTalkOutline DefaultOutlineFactory(
            Func<string> arrangeCode,
            Action<string>? actForTheCode,
            IEnumerable<RoslynTalkOutlinePrompt> promptsToActCode,
            bool enableILAnalysis
        ) {
            return new RoslynTalkOutline(arrangeCode, actForTheCode, promptsToActCode, enableILAnalysis);
        }
    }


    public class RoslynTalkOutline : IWhatRoslynTalkedAbout
    {
        bool IRoslynTestimony.IsMeaningful => false; // TODO: Implement this property
        IRoslynTestimony.IPrerequisite IRoslynTestimony.Prerequisite => Prerequisite;

        protected int _hasCompiled = 0;
        protected virtual Parameters Prerequisite { get; }

        protected class Parameters(
            Func<string> arrangeCode,
            Action<string>? actForTheCode,
            IEnumerable<RoslynTalkOutlinePrompt> prompts,
            bool isILAnalysisEnabled
            ) : IRoslynTestimony.IPrerequisite
        {
            IEnumerable<RoslynTalkOutlinePrompt> IRoslynTestimony.IPrerequisite.Prompts => Prompts;

            bool IRoslynTestimony.IPrerequisite.IsILAnalysisEnabled => IsILAnalysisEnabled;

            string? IRoslynTestimony.IPrerequisite.TargetCodes => TargetCodes;

            public virtual Func<string> ArrangeCode { get; } = arrangeCode ?? throw new ArgumentNullException(nameof(arrangeCode));
            public virtual Action<string>? ActForTheCode { get; } = actForTheCode;
            public virtual IEnumerable<RoslynTalkOutlinePrompt> Prompts { get; } = prompts ?? throw new ArgumentNullException(nameof(prompts));
            public virtual bool IsILAnalysisEnabled { get; } = isILAnalysisEnabled;
            public virtual string? TargetCodes { get; set; }
        }


        protected internal RoslynTalkOutline(
                Func<string> arrangeCode,
                Action<string>? actForTheCode,
                IEnumerable<RoslynTalkOutlinePrompt> prompts,
                bool enableILAnalysis
            )
        {
            _ = arrangeCode ?? throw new ArgumentNullException(nameof(arrangeCode));
            _ = actForTheCode;
            _ = prompts ?? throw new ArgumentNullException(nameof(prompts));
            if (!prompts.Any())
            {
                throw new ArgumentException("At least one prompt must be provided.", nameof(prompts));
            }

            Prerequisite = new Parameters(arrangeCode, actForTheCode, prompts, enableILAnalysis);
        }

        public void CompileAndAnalyzeIL()
        {
            if (Interlocked.CompareExchange(ref _hasCompiled, 1, 0) != 0)
            {
                throw new InvalidOperationException("The code has already been compiled. Let it compile only once per session.");
            }

            try
            {
                Prerequisite.TargetCodes = Prerequisite.ArrangeCode();
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
                    Compile(Prerequisite.TargetCodes, prompt);
                }
                catch (Exception ex)
                {
                    // TODO: Let the exception be into the IWhatRoslynTalkedAbout instance.
                    Console.WriteLine($"An error occurred while compiling the code: {ex.Message}");
                    throw;
                }
            }
        }

        // TODO: Implement async version of CompileAndAnalyzeIL
        /*
        public Task CompileAndAnalyzeILAsync()
        {
            if (Interlocked.CompareExchange(ref _hasCompiled, 1, 0) != 0)
            {
                throw new InvalidOperationException("The code has already been compiled. Let it compile only once per session.");
            }
        }
        */

        protected internal virtual void Compile(string code, RoslynTalkOutlinePrompt prompt)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                                    code, 
                                    prompt.ParseOptions, 
                                    encoding: prompt.EncodingForParse,
                                    cancellationToken: prompt.CancellationToken,
                                    path: prompt.Path);

            var references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Cast<MetadataReference>();

            var compilation = CSharpCompilation.Create(
                assemblyName: "DynamicAssembly",
                syntaxTrees: [syntaxTree],

                references: references,
                options: prompt.CompilationOptions);

            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                var diagnostics = emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString());

                Console.WriteLine("Compilation failed:\n" + string.Join("\n", diagnostics));
                return;
            }
        }
    }
}
