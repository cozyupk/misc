// This file, including the prompts, is released under the MIT License.
//    → You are free to use it for both commercial and non-commercial purposes in accordance with the MIT License.
// Copyright (C) 2025 cozyupk
// https://github.com/cozyupk
// Note: This code and its prompts are provided as an unfinished version,
// intended for research, learning, and the joint evolution of humanity and generative AI.

// --- For Users ---
namespace ForUser { class Explanation { public static void Dummy() {

_ = "When using vibe coding with ChatGPT, it is recommended to enter the following prompt at the beginning of the chat.";
/*
 - From this point forward, please answer without using any conversation history with me in this chat.
 - If you propose improvements to the source code during the conversation, please first ask the user for permission to present your code improvement suggestions.
*/
_ = "Additionally, optionally, you may also enter a prompt like the following at the same time (modify to taste).";
/*
From the next prompt onward, please internally review yourself three times before outputting: the final result plus instructions for the user on how to verify it, plus a QA checklist with all yes answers, plus a summary of three self-review points.
Use a gentle, empathetic, and reassuring tone, like a kind doctor carefully guiding the user.
*/
_ = "After that, let’s define what to create by uploading this file and using a prompt like the following (the example below is just that; adjust it according to your goals).";
/*
I want to write a console program in C# on Windows that draws a Lissajous curve given parameters and saves it as a .png file.
Additionally, I want it to support animated GIF output.
What command-line options are needed to make the program as flexible as possible?
Please answer in a Usage-like format that would be displayed by --help, with the explanation part in Japanese.
Assume that you will generate the program according to the RULE and steps in this .cs file.
*/
_ = "Once the definition of what to create is ready, start code generation with a prompt like the following.";
_ = "Note: Including this prompt and from this prompt onward, we will proceed while uploading/updating ChapterVibe_ja-JP.cs.";
/*
Now, referring to the conversation up to this point in this chat, please carry out Comment Generation Step 1.
*/
_ = "If ChatGPT responds in a form that cannot be copy‑pasted, try inputting a prompt like the following.";
/*
Please output it in a code block so that it can be copy‑pasted.
*/
_ = "If you want to improve the output, try inputting a prompt like the following.";
/*
Please also include the Usage-like explanation you proposed earlier. Also, output it in a code block so that it can be copy‑pasted.
*/
_ = @"You can advance vibe coding by pasting the comments and code output by ChatGPT into this file and, in your next prompt, prompting with ""Please carry out ○○ Step ○"" while uploading the updated file again.
      In this way, carry out Comment Generation Steps 1 through 6, and then proceed in order with Code Generation Step 1 and onward.
      Repeat Code Generation Step 5 until ChatGPT outputs ""No items.""
      At each stage, if compilation errors or warnings occur, either fix them and provide feedback to GPT, or if necessary provide GPT with screenshots and source code and ask for fixes.";
}}}
// --- END For Users ---

/* --- RULES for Generative AI ---
   The ChapterVibe namespace provides the core functions of the ChapterVibe framework.
   When generating code, use this core functionality and implement each chapter in compliance with the SOLID principles.
   The ChapterVibe.UsageExample namespace shows usage examples.
   Also, delete the RULE block after code generation (unless following the “RULE retention policy” shown later).

   When generating or modifying code in this file, follow the rules below.
   (Note) Rule #3 is particularly prone to misinterpretation by generative AI, so pay extra attention.
   1. Divide the source code into appropriate phases, giving each “Chapter” an independent function. Keep the Main method short. To achieve this, prepare independent chapters for parsing command‑line parameters and for pre‑processing various I/O operations.
   2. Classes implementing each Chapter must implement IChapter and depend only on the minimum necessary role interfaces.
   3. To satisfy the contravariance of the type parameters of IChapter and IChapterContext, the inheritance chain should be such that the upstream (closer to input) side inherits the role interfaces of the downstream (closer to output) side, thereby becoming “more specialized.” Maintain consistency that “Upstream interfaces inherit downstream interfaces (general → specific).”
     Example (general → specific): ChapterVibe.IArg ← IProcessingArg ← IParseArgsArg
     * At runtime, due to the contravariant nature of `IChapter<in TArg>`, a Chapter that processes downstream can safely accept its parent, the “more specific” upstream interface (i.e., the actual Arg). See the ChapterVibe.ContravarianceExample below. (This file is a .cs file, and it has been confirmed to compile and operate as intended.)
*/
// ============================================================================
// ChapterVibe.UsageExample (sample candidate to be replaced / conforms to RULE with XML comments)
// ---------------------------------------------------------------------------
// This section is the minimal usage example of the ChapterVibe framework (Hello → World/WorldWithCulture).
// Important:
// - In Code Generation Step 4 (after implementing Main), **delete or replace this UsageExample** (in accordance with the RULE retention policy).
// - Arg interfaces follow the contravariance policy of RULE #3 (upstream = specific ← downstream = general).
// - Chapter chaining is shown using PushBack, and RootArg is provided via explicit implementation (RULE #7).
// - Exceptions/I/O are handled within chapters and are not propagated outward (RULE #12).
// - Chapters that branch allow multiple inheritance (collisions of members with the same name are explicitly avoided using new, etc.).
// Prohibited:
// - Do not refer to RootArg directly (always access via a role interface).
// - Do not repack and pass new instances between chapters (inheritance of default/allowed state would be broken).
// - Do not swap the in/out of IContextBuffer<out T> / IChapterContext<in T>.
//    (because contravariance (−) combined with contravariance (−) results in covariance (+), ensuring type safety for PushBack.)
// ============================================================================

namespace ChapterVibe.UsageExample
{
    using System;
    using System.Globalization;
    using ChapterVibe;

    // ------------------------------------------------------------------------
    // Arg interface definitions (maintain order from general → specific / use serial or multiple inheritance)
    // ------------------------------------------------------------------------

    /// <summary>
    /// Minimal contract required to output “World with culture” (downstream/general).
    /// <para>RULE #3: Upstream interfaces inherit downstream interfaces and thus become “more specific.”</para>
    /// </summary>
    internal interface IWorldWithCultureArg : IArg
    {
        /// <summary>Remaining number of outputs (zero means complete).</summary>
        int WorldWithCultureCount { get; set; }

        /// <summary>
        /// Line output sink (e.g., <see cref="Console.WriteLine(string)"/>).
        /// Exceptions and I/O failures are handled within the chapter (RULE #12).
        /// </summary>
        Action<string> WriteLineAction { get; }

        /// <summary>
        /// The culture to use for output. Accessing it when unset is invalid (throws).
        /// </summary>
        CultureInfo Culture { get; }
    }

    /// <summary>
    /// Minimal contract required to output “World” without culture (downstream/general).
    /// </summary>
    internal interface IWorldArg : IArg
    {
        /// <summary>Remaining number of outputs (zero means complete).</summary>
        int WorldCount { get; set; }

        /// <summary>
        /// Line output sink (e.g., <see cref="Console.WriteLine(string)"/>).
        /// Exceptions and I/O failures are handled within the chapter (RULE #12).
        /// </summary>
        Action<string> WriteLineAction { get; }
    }

    /// <summary>
    /// Contract for outputting “Hello” and branching to <see cref="WorldWithCultureChapter"/> or <see cref="WorldChapter"/> depending on conditions (upstream/specific).
    /// <para>Because the chapter branches, it multiply inherits <see cref="IWorldArg"/> and <see cref="IWorldWithCultureArg"/>.</para>
    /// </summary>
    internal interface IHelloArg : IWorldArg, IWorldWithCultureArg
    {
        /// <summary>Remaining number of “Hello” outputs.</summary>
        int HelloCount { get; set; }

        /// <summary>
        /// Line output sink. Because of multiple inheritance and identical members, the member is redeclared with <c>new</c> to explicitly avoid collisions.
        /// </summary>
        new Action<string> WriteLineAction { get; }

        /// <summary>
        /// <c>true</c> if the culture is set.
        /// </summary>
        bool IsCultureSet { get; }
    }

    /// <summary>
    /// The most upstream contract that parses CLI arguments and delegates control to <see cref="HelloChapter"/> (upstream/most specific).
    /// <para>RULE #1: To keep Main slim, CLI parsing is placed in a chapter.</para>
    /// </summary>
    internal interface IParseArgsArg : IHelloArg
    {
        /// <summary>Command‑line arguments (does not include the executable name).</summary>
        string[] Args { get; }

        /// <summary>
        /// Set the culture (<c>null</c> → set; double setting is prohibited).
        /// </summary>
        /// <param name="culture">The culture to apply.</param>
        void SetCulture(CultureInfo culture);
    }

    // ------------------------------------------------------------------------
    // Chapter implementations (exceptions/I‑O are self‑contained: RULE #12)
    // ------------------------------------------------------------------------

    /// <summary>
    /// Chapter that outputs “World in {Culture}” with culture.
    /// </summary>
    internal sealed class WorldWithCultureChapter : IChapter<IWorldWithCultureArg>
    {
        /// <inheritdoc/>
        public void Handle(IWorldWithCultureArg arg, IContextBuffer<IWorldWithCultureArg> buffer)
        {
            try
            {
                arg.WriteLineAction($"World in {arg.Culture.Name}");
            }
            catch (Exception ex)
            {
                // RULE #12: Handle I/O and other failures within the chapter
                arg.WriteLineAction($"[WorldWithCulture] output failed: {ex.Message}");
                return;
            }

            arg.WorldWithCultureCount--;
            if (arg.WorldWithCultureCount > 0)
            {
                // Contravariance: IContextBuffer<out T> × IChapterContext<in T> combine to allow type‑safe PushBack
                buffer.PushBack(new ChapterContext<IWorldWithCultureArg>(this, arg));
            }
            else
            {
                arg.WriteLineAction("All worlds with culture processed.");
            }
        }
    }

    /// <summary>
    /// Chapter that outputs “World” (without culture).
    /// </summary>
    internal sealed class WorldChapter : IChapter<IWorldArg>
    {
        /// <inheritdoc/>
        public void Handle(IWorldArg arg, IContextBuffer<IWorldArg> buffer)
        {
            try
            {
                arg.WriteLineAction("World");
            }
            catch (Exception ex)
            {
                arg.WriteLineAction($"[World] output failed: {ex.Message}");
                return;
            }

            arg.WorldCount--;
            if (arg.WorldCount > 0)
            {
                buffer.PushBack(new ChapterContext<IWorldArg>(this, arg));
            }
            else
            {
                arg.WriteLineAction("All worlds processed.");
            }
        }
    }

    /// <summary>
    /// Chapter that outputs “Hello” and, when the count is exhausted, branches to the next chapter according to whether culture is set.
    /// </summary>
    internal sealed class HelloChapter : IChapter<IHelloArg>
    {
        /// <inheritdoc/>
        public void Handle(IHelloArg arg, IContextBuffer<IHelloArg> buffer)
        {
            try
            {
                arg.WriteLineAction("Hello");
            }
            catch (Exception ex)
            {
                arg.WriteLineAction($"[Hello] output failed: {ex.Message}");
                return;
            }

            arg.HelloCount--;
            if (arg.HelloCount > 0)
            {
                buffer.PushBack(new ChapterContext<IHelloArg>(this, arg));
            }
            else
            {
                // Branch: this is a functional branch, not just a skip, so route to the appropriate branch interface
                if (arg.IsCultureSet)
                {
                    buffer.PushBack(new ChapterContext<IWorldWithCultureArg>(new WorldWithCultureChapter(), arg));
                }
                else
                {
                    buffer.PushBack(new ChapterContext<IWorldArg>(new WorldChapter(), arg));
                }
            }
        }
    }

    /// <summary>
    /// Chapter that parses the CLI, sets the culture if necessary, and passes control to <see cref="HelloChapter"/>.
    /// </summary>
    internal sealed class ParseArgsChapter : IChapter<IParseArgsArg>
    {
        /// <inheritdoc/>
        public void Handle(IParseArgsArg arg, IContextBuffer<IParseArgsArg> buffer)
        {
            try
            {
                if (1 < arg.Args.Length)
                {
                    Console.WriteLine("Usage: HelloWorld [Culture]");
                    return;
                }

                if (arg.Args.Length == 1)
                {
                    var culture = CultureInfo.GetCultureInfo(arg.Args[0]);
                    arg.SetCulture(culture);
                    arg.WriteLineAction($"Culture set to {culture.Name}");
                }
            }
            catch (Exception ex)
            {
                // RULE #12: Summarize parse failures here and do not propagate
                arg.WriteLineAction($"[ParseArgs] parsing failed: {ex.Message}");
                return;
            }

            // Next chapter
            buffer.PushBack(new ChapterContext<IHelloArg>(new HelloChapter(), arg));
        }
    }

    // ------------------------------------------------------------------------
    // RootArg implementation (explicit implementation: RULE #7)
    // ------------------------------------------------------------------------

    /// <summary>
    /// Root argument that <strong>explicitly</strong> implements all role interfaces.
    /// <para>Internal state is held by private members, and access is always via the interfaces.</para>
    /// </summary>
    internal sealed class RootArg : IParseArgsArg
    {
        /// <inheritdoc/>
        string[] IParseArgsArg.Args => Args;

        /// <inheritdoc/>
        void IParseArgsArg.SetCulture(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture), "Culture cannot be null.");
            if (Culture != null) throw new InvalidOperationException("Culture is already set.");
            Culture = culture;
        }

        /// <inheritdoc/>
        int IHelloArg.HelloCount { get; set; }

        /// <inheritdoc/>
        int IWorldArg.WorldCount { get; set; }

        /// <inheritdoc/>
        int IWorldWithCultureArg.WorldWithCultureCount { get; set; }

        /// <inheritdoc/>
        bool IHelloArg.IsCultureSet => Culture != null;

        /// <inheritdoc/>
        CultureInfo IWorldWithCultureArg.Culture
            => Culture ?? throw new InvalidOperationException("Culture is not set.");

        /// <inheritdoc/>
        Action<string> IHelloArg.WriteLineAction => WriteLineAction;

        /// <inheritdoc/>
        Action<string> IWorldArg.WriteLineAction => WriteLineAction;

        /// <inheritdoc/>
        Action<string> IWorldWithCultureArg.WriteLineAction => WriteLineAction;

        // Internal implementation (always accessed via interface)
        private Action<string> WriteLineAction { get; } = Console.WriteLine;
        private string[] Args { get; }
        private CultureInfo? Culture { get; set; }

        /// <summary>
        /// Create a root argument.
        /// </summary>
        /// <param name="args">Command‑line arguments (does not include the executable name).</param>
        /// <param name="helloCount">Number of Hello outputs (0 or more).</param>
        /// <param name="worldCount">Number of World/WorldWithCulture outputs (0 or more).</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="helloCount"/> or <paramref name="worldCount"/> is negative.</exception>
        public RootArg(string[] args, int helloCount, int worldCount)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args), "Args cannot be null.");
            if (helloCount < 0) throw new ArgumentOutOfRangeException(nameof(helloCount), "Hello count must be non‑negative.");
            if (worldCount < 0) throw new ArgumentOutOfRangeException(nameof(worldCount), "World count must be non‑negative.");

            ((IHelloArg)this).HelloCount = helloCount;
            ((IWorldArg)this).WorldCount = ((IWorldWithCultureArg)this).WorldWithCultureCount = worldCount;
        }
    }

    // ------------------------------------------------------------------------
    // Main (for UsageExample / RULE #1: Keep Main short)
    // ------------------------------------------------------------------------

    /// <summary>
    /// Entry point for UsageExample (delete/replace in production generation: see RULE retention policy).
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main only constructs the Stage and starts the first chapter (RULE #1).
        /// CLI parsing is delegated to the chapter (<see cref="ParseArgsChapter"/>).
        /// </summary>
        private static void Main(string[] args)
        {
            var stage = new Stage<IParseArgsArg>();
            var first = new ParseArgsChapter();
            var root = new RootArg(args, helloCount: 3, worldCount: 2);

            stage.Run(first, root);
        }
    }
}
/*
// ============================================================================
// End: ChapterVibe.UsageExample (sample candidate to be replaced / conforms to RULE with XML comments)
// ============================================================================/*
ChapterVibe.IArg is the base marker interface for all role interfaces (it has no members).
     Do not call the Handle method of IChapter implementation classes directly; call them through the Stage class or buffer.PushBack(). One IChapter implementation must not directly call the Handle of another IChapter implementation.
   4. Place interface definitions immediately before their corresponding chapter.
   5. Provide clear XML comments for each interface member.
   6. At runtime unify arguments into a single RootArg.
   7. RootArg must implement all role interfaces **explicitly** (members must be accessed only via the interface).
   8. Each chapter must only use its corresponding Arg interface type; do not cast to other interfaces or RootArg to access unrelated members.
   9. Directly referencing RootArg is prohibited. Including setters, always access via the interface.
   10. Maintain the variance of IContextBuffer<out TArg> (out) and IChapterContext<in TArg> (in). (Reason: input (−) × contravariant (−) = covariant (+); this satisfies CS1961.) Guarantee the out/in usage positions to avoid CS1961 violations.
   11. Prefer properties over fields. However, using private readonly fields is permitted for conventional uses such as internal locks or fixed collections.
   12. When a design choice is not inevitable but a framework policy, note this and its intent in an inline comment, so that it is self‑explanatory even outside the ChapterVibe context (and for AI/automated reviews). Examples: choosing properties rather than readonly fields, explicit interface implementation, non‑standard naming/visibility for pipelines or reflection, trade‑offs between thread safety and allocations.
   13. Handle exceptions arising from I/O, external processes, or environment‑dependent calls within the chapter. By default, do not propagate exceptions outside Handle. If a fatal failure requires the caller to decide, define a sanitized error recording interface such as IErrorSink on the caller’s role interface to record a sanitized error object (only a summary or structured metadata of the exception, no PII/stack) to propagate back. Do not store the exception object itself in IArg (see RULE #25/#32).
   14. Leverage .NET 8.0 language features. Enable nullable reference types. Disable ImplicitUsings. Use internal as the default access modifier and minimize exposure.
   15. If certain .csproj settings (e.g., TargetFramework, UseWindowsForms, SupportedOSPlatformVersion, platform‑specific package references) are required for compiling or running the code, note this at the beginning of the program in comments.
   16. Always express collection contracts with interface types (IReadOnlyList<T>, IEnumerable<T>, etc.). Do not expose concrete types like List<T> or arrays (T[]) directly in Arg or chapter contracts.
   17. When using null‑coalescing operators (?? / ??=), ensure both sides are of the contract type. Do not mix List<T> and T[].
   18. Use cached static constants for empty defaults. Example: private static readonly IReadOnlyList<T> Empty = Array.Empty<T>(); public IReadOnlyList<T> Values => _values ?? Empty;
   19. The type of backing fields must exactly match the contract type. Example: private IReadOnlyList<T>? _points; (not List<T>?).
   20. When accepting mutable input (e.g., SetNormalizedPoints), accept a broad type such as IEnumerable<T>. However, ensure internal storage is normalized to the contract type. Explicitly comment on whether you copy the input (see RULE #11).
   21. Tuple element names do not affect type identity but should be consistent across the code base (e.g., (double x, double y) is recommended).
   22. (Higher Principle – Non‑Destructive) Do not make destructive changes to data. Deleting, overwriting, or irreversible conversion of existing data is prohibited by default. When necessary, require “create under a different name” plus an explicit flag plus pre‑validation.
   23. (Higher Principle – Input Defense) Always sanitize/validate all external inputs (CLI args, environment variables, file names, paths, URLs, JSON, template strings, etc.) for security. Prefer allowlists; denylists are supplementary.
   24. (Higher Principle – Least Privilege) Use only the minimum privileges necessary (file permissions, network, credentials). If elevation or broad privileges are required, explain why in comments (leave inline reasoning per RULE #12/#11).
   25. (Higher Principle – Observability) Do not include sensitive information in logs. Only record the minimal metadata necessary for reproduction in structured form (always mask PII/credentials).
   26. (Higher Principle – Safe by Default) Default values should always be “safe” (no external sends, no overwrite, no execution, require interactive confirmation). If relaxing for convenience, provide explicit flags and justify them in comments (see RULE #11).
   27. (Input Validation) Validate parameters by type, range, size, and format (regular expression). Reject unspecified, empty, control characters, newlines, and null bytes. Normalize paths with GetFullPath and verify they are under the base directory using StartsWith. Do not allow symbolic links or reparse points.
   28. (Output Encoding) Always encode/escape strings sent externally according to the sink (HTML/CSV/JSON/shell arguments). Do not build commands or queries by concatenating strings. Execute commands with UseShellExecute=false and separate arguments via an array. Always parameterize/preprepare SQL/search queries.
   29. (Safe File I/O) Prohibit overwriting files (FileMode.CreateNew). Write to a temporary file and atomically Move after verification (no overwrite). Set limits on allowed extensions and maximum size. Use minimum FileShare (read: Read, write: None).
   30. (Safe Networking) By default, disallow external transmission. Only allow if necessary with domain allowlists plus TLS (≥ 1.2) plus certificate validation enabled. Set timeouts/retries (exponential backoff)/CancellationToken on all requests. Explicitly define handling of redirects and proxies.
   31. (Serialization/Deserialization) Prohibit dangerous APIs such as BinaryFormatter. Use System.Text.Json with MaxDepth set and bind to known types. Do not use polymorphic arbitrary type resolution or embed Type names.
   32. (Secrets) Do not embed API keys, tokens, or connection strings in source. Obtain them via injection interfaces or OS secure stores. Do not output secrets (mask/summary) in exceptions or logs.
   33. (Cryptography/Randomness) For security‑related randomness, use RandomNumberGenerator instead of Random. Custom cryptography is prohibited; use only standard libraries.
   34. (Concurrency/Resources) For long‑running processes, periodically check for cancellation. Dispose of IDisposable with using. Minimize lock scope, and invoke I/O or user code outside of locks (following existing RunAll implementation).
   35. (Confirmation Flow) For operations that could be destructive (e.g., overwriting names, deletion, moving), use a two‑step confirmation: dry‑run (detection only) → execute with Confirm flag. If possible, output to a different directory or a versioned path.
   36. (Testability) Provide unit tests to enforce safety rules (e.g., refuse directory traversal, prevent overwriting, guarantee rollback on exceptions, apply encoding). Ensure tests guard the code so that generative AI cannot inadvertently compromise safety.
   37. (AI/Auto‑Generation Countermeasures) Explicitly comment any code snippets that violate the above rules (e.g., Process.Start with shell concatenation, FileMode.Create, unvalidated paths) as “banned patterns” and immediately place a safe alternative template below (see RULE #11). In reviews, start by searching for banned patterns.
   38. (Behavior on Failure) Do not silently swallow failures. For users, provide a safe message; for logs, leave minimal information needed to reproduce. Specify whether to abort or fall back on a per‑chapter basis (consistent with RULE #12).
   39. (Default Setting Fixation) The defaults are: external transmission disabled, overwrite disabled, execution disabled, interactive confirmation required. Overriding requires an explicit flag name (e.g., --allow-overwrite, --allow-network).
   40. Consider user experience (UX). Format numbers, dates, and times according to the current locale (e.g., ToString("N", CultureInfo.CurrentCulture)). Log/programmatic output should be culture‑independent (e.g., dates in ISO 8601). Error messages and user‑facing strings should follow the localization/translation policy.
   41. Write all comments in the code and subsequent chat output in English. Templates should also be written in English.
   42. (CLI flag conventions – fixed; omit those unnecessary for requirements)
       --dry-run (default=true) / --confirm (permission to execute)
       --allow-overwrite / --allow-network / --allow-exec
       If there is an output collision, it fails unless --allow-overwrite is present. Always maintain safe defaults (see #39).
   43. (Minimum set of banned patterns – excerpt)
       - Using Process.Start(string) with shell concatenation / UseShellExecute=true
       - Using FileMode.Create / FileShare.ReadWrite
       - Using unvalidated paths (without GetFullPath/StartsWith checks)
       - Dangerous serialization or arbitrary type binding (BinaryFormatter, etc.)
       - Using Random for security purposes
       - Outputting absolute paths, tokens, or other sensitive information in exceptions/logs
   44. (.csproj default skeleton)
       Default:
         <TargetFramework>net8.0</TargetFramework>
         <Nullable>enable</Nullable>
         <ImplicitUsings>disable</ImplicitUsings>
        <EnableImplicitProgram>false</EnableImplicitProgram>
         <NoWarn>$(NoWarn);IDE0130</NoWarn>
       Assume this default when generating code. If removing it, explicitly note it in comments.
   45. (RULE retention policy)
       Keep the RULE block under `#if VERYVIBE_RULES` or extract into a separate file (ChapterVibe.RULES.md, etc.) for preservation. If completely deleted after generation, indicate the single source of truth’s location in another comment.

------ (Template) Chat output to return to the user after code generation -------

【Important】Even if a code generation task is divided into subtasks through conversation with the user,
be sure to **output this section after each subtask is completed**.

## Compliance Report (SOLID)
- State whether the principles are adhered to or violated. If deviations are necessary, briefly list reasons, impact, and future corrective actions.
- Example: “All SOLID principles are maintained.” or details of violations.

## Bug/Logic Review & Self‑Prompt
- Check for common mistakes (off‑by‑one errors, confusion between accumulation and differences, redundant processing, performance pitfalls, readability).
- Attach a “self‑prompt” corresponding to each issue.
- If none: “There are no bug/logic review findings.”

## Security Review & Self‑Prompt
- Conduct a comprehensive check for non‑destructive defaults, input validation, injection resistance, output encoding, serialization safety, secrets,
  network, concurrency, cryptography, and logging.
- For each item, include a pass/fail and a brief self‑prompt.
- If none: “There are no security review findings.”

## Refactoring Suggestions & Self‑Prompt
- Suggest useful abstractions, naming, splitting, and dependency organization, along with a self‑prompt to validate them.
- If none: “There are no refactoring suggestions.”

## Uncertainty Report
- For each design decision where judgement could diverge: decision point / alternatives / chosen option and reasoning / confidence level (High/Medium/Low) / proposal for next improvement
------ END: (Template) Chat output to return to the user after code generation -------

=============================================================================
RULE: Comment Output Format Contract
=============================================================================
Output format:
  - Generative AI must put a C# block comment inside a **Markdown code block (```csharp … ``` )**.

Prohibited:
 - Do not split into multiple code blocks.
 - Do not add strings outside the comment.
 - Do not write actual C# implementation code or using clauses inside the code block.

Example (expected form at the beginning of the heading):
  /* ========================================================================
     Specification Agreement and Design Policy (LissajousTool / CLI)
     ========================================================================
     1) Overview / Purpose
        - Purpose: …
        - Execution mode: …
        …
     Example: LissajousTool png -ax 3 -ay 2 --confirm -o .\out\classic.png
     Example: LissajousTool gif -ax 5 -ay 7 --phase-sweep 0..360 --frames 120 --fps 30 --loop 0 --confirm -o .\out\spin.gif
   *\/
   The wording at the beginning of the heading should be written in a way that does not depend on this RULE (so that it makes sense to a third party unaware of this RULE).
   Additional notes on execution:
     - Return other “comment generation steps” besides Comment Generation Step 1 as comment blocks as well.
     - Even if you split into subtasks, return a comment block for each subtask.
     - Steps other than “comment generation steps” (e.g., “code generation steps”) are not covered by this contract.
     - If a summary is necessary mid‑way, include it within the comment (do not write outside).
================= END: RULE: Comment Output Format Contract ==================

==========================================================================
RULE: Absolute reference for definitions in the ChapterVibe namespace (IChapter/IContextBuffer/IChapterContext/IArg)
==========================================================================

[Purpose]
  - Ensure that all components consistently reference the runtime contracts in the ChapterVibe core (IArg / IChapter<TArg> / IContextBuffer<out TArg> / IChapterContext<in TArg>) and prevent redefinition or shadow types mixing in the application/function side.

[Requirements]
  1) Explicitly use the ChapterVibe namespace
   - All files containing Chapter implementations must include the following at the beginning of the file (inside the namespace):
        using ChapterVibe;

  2) Prohibit redefinitions
      - The following types must not be newly defined on the application/function side (e.g., ChapterVibe.Cal):
          interface IArg
          interface IChapter<in TArg>
          interface IContextBuffer<out TArg>
          interface IChapterContext<in TArg>
      - Declaring types with the same name/meaning in another namespace (shadow definitions) is also prohibited.

  3) Treatment of utility classes
      - When implementation classes such as ChapterContext<T> / NoopContextBuffer are provided in the ChapterVibe namespace, use them in principle.
      - If they are not provided in the ChapterVibe namespace, or you want to extend them via inheritance, or you want to implement your own, you may define implementations in the function side.
      - When implementing on the function side, give it a different name than classes existing in the ChapterVibe namespace.

  4) Unified placement of using directives
     - Even if using file‑scoped namespace in C# 12 and later, place using directives inside the namespace block. The analysis/generation code will output based on this premise.

=============== END: RULE: Absolute reference for definitions in the namespace (IChapter/IContextBuffer/IChapterContext/IArg) =======
--- END RULE ---
*/

/*
=== Comment Generation Step 1: Specification Agreement and Design Policy ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* If the user instructs to perform Comment Generation Step 1 via a prompt, output the following items as a comment that can be copied and pasted into this source code.
  - The output must strictly follow the “Comment Output Format Contract” and be returned as a single C# block comment.
1) Overview: purpose/features/target users/execution mode (CLI/GUI)/target OS and .NET, etc.
2) I/O requirements: input/output/extensions/maximum size/overwrite permission/dry‑run/confirm‑required flag, etc.
 - Describe all details thoroughly, including input/output formats, structure of input/output files, environment variables, command‑line parameters, etc.
 - Include in detail everything from the conversation in the immediately preceding chat; in other words, almost copy it.
3) Failure policy: abort or fallback, user‑facing messages, logging policy (PII masking/structured).
4) Security assumptions: path normalization, rejection of directories outside allowed areas, default prohibition of network access, search for banned patterns, etc.
5) UX policy: Usage (CLI options, examples), number/date formatting (human‑facing = locale‑dependent, machine = ISO 8601).
6) Design policy: list of chapters and their responsibilities, corresponding Arg interfaces (placed immediately before each chapter)
  - Role interfaces should inherit from upstream to downstream to satisfy contravariance (e.g., IExtractInputArg : ICalcArg : IArg).
  - RootArg must implement all role interfaces explicitly, and chapters must access members only through their corresponding interfaces.
7) Acceptance criteria: successful build; successful dry‑run; destructive operations require Confirm; no banned patterns detected.
* Ensure that this output is self‑contained so that the process can be resumed even if the chat history disappears.
=== End Comment Generation Step 1 ===
*/
/*
=== Comment Generation Step 2: Specification Self‑Review ===
* Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Comment Generation Step 1 is assumed.
* After the user instructs to perform Comment Generation Step 2 via a prompt, output the following items as a comment that can be copied and pasted into this source code:
 - The self‑review result of “Specification Agreement and Design Policy” based on the Specification Review Template.
  - The output must strictly follow the “Comment Output Format Contract” and be returned as a single C# block comment.
  - It will be presented in English, including the template, in a format that can be copied and pasted into this source code.

Specification Review Template

## Specification Compliance
- Does the agreed specification align with the ChapterVibe RULE set (SOLID principles, safety principles, least privilege, No Destructive Changes, etc.)?
- Is the relationship between each Chapter and its corresponding IArg interface clearly defined?
- Does the specification include explicit implementation of RootArg and the prohibition of direct RootArg access?

## Completeness Check
- Are all I/O requirements (extensions, maximum size, overwrite policy, dry‑run, confirm flag) fully covered in the specification?
- Are error handling policies (abort, fallback, logging) clearly documented?
- Are the security principles (banned patterns, path normalization, network prohibition) explicitly stated?

## Consistency Check
- Are the usage examples and the requirements described in the specification consistent with each other?
- Are the safe defaults maintained (dry‑run enabled by default, overwrite prohibited, etc.)?
- Is the handling of locale/logging (user‑facing = CurrentCulture, machine‑facing = Invariant) included in the specification?

## Risk & Uncertainty
- Are there any design decisions that are still undecided or have multiple alternatives? (e.g., choice of DNN framework)
- If so, what alternatives exist, and on what basis should the decision be made?

## Refactoring / Clarification Suggestions
- Are there any missing or ambiguous descriptions in the specification document?
- Are there additional items that should be explicitly specified? (e.g., detailed format of configuration files)
=== End Comment Generation Step 2 ===
*/
/*
=== Comment Generation Step 3: Additional Specifications ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Comment Generation Step 2 is assumed.
* If the user instructs “Please perform Comment Generation Step 3”:
  - Organize the unresolved points and issues revealed in the self‑review of Comment Generation Step 2 into an additional specification and output it as a comment.
  - The output must strictly follow the “Comment Output Format Contract” and be returned as a single C# block comment.
  - It will be presented in a form that can be copied and pasted into this source code.
* Ensure that this output is self‑contained so that subsequent steps can be performed even if the chat history disappears.
=== End Comment Generation Step 3 ===
*/
/*
=== Comment Generation Step 4: Determining Dependency Packages ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Comment Generation Step 3 is assumed.
* If the user instructs “Please perform Comment Generation Step 4”:
Notify the user of items where they need to decide dependency packages or similar, and for each:
  - Options
  - Advantages and disadvantages of each option
  - Recommended option
Prompt them to decide. This step is conducted in chat with the user, not in a comment block.
List a package’s large share as an advantage of that option.
List the fact that a package has stopped being updated as a disadvantage, but list “having a history and being stable” as an advantage.
Do not present packages that lack stable releases (only preview or beta) as options at all.
For test packages, present xUnit and Moq as the first options.
=== End Comment Generation Step 4 ===
*/
/*
=== Comment Generation Step 5: Generating Dependency Package Comments ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Comment Generation Step 4 is assumed.
* If the user instructs “Please perform Comment Generation Step 5”:
Based on the chat record with the user, present the decisions on dependency packages and the anticipated .csproj as a single block comment (the user will actually paste it).
  - The output must strictly follow the “Comment Output Format Contract” and be returned as a single C# block comment.
  - The csproj content should include everything from the <Project> tag to the </Project> tag, assuming that the user will paste it.
  - Output two csproj contents: one for the application project and one for the test project.
  - When outputting csproj, do not use the leading * of C# block comments, so that the user can copy and paste as is.
  - For PackageReference versions, specify versions that appear appropriate at this time as tentative.
* Ensure that this output is self‑contained so that subsequent steps can be performed even if the chat history disappears.
=== End Comment Generation Step 5 ===
*/
/*
=== Comment Generation Step 6: Finalizing Arg Interfaces ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Comment Generation Step 5 is assumed.
* If the user instructs “Please perform Comment Generation Step 6”:
 Output the final version of the Arg interfaces placed immediately before each chapter as a single C# block comment.
Policy:
 - For each chapter’s Arg interface conceptualized in the comments so far, present the final signature (with XML comments attached to each member) in order from upstream (user input) to downstream (output).
The output should include: public member names of each interface, inherited interfaces, types, read/write attributes (get; / set;), intent, and exception contract.
 - The output must strictly follow the “Comment Output Format Contract” and be returned as a single C# block comment.
Note: Fully adhere to RULE #3. Typically there is only one role interface that inherits from IArg.
Result: Paste the comment directly before each chapter → it becomes the single source of truth (SSoT) for subsequent code generation.
* Ensure that this output is self‑contained so that the code generation process can proceed even if the chat history disappears.
=== End Comment Generation Step 6 ===
*/
/*
=== Code Generation Step 1: Generating Arg Interface Code ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Comment Generation Step 6 is assumed.
If the user instructs “Please perform Code Generation Step 1”:
 Generate the implementation code (with required XML documentation comments) for the interfaces placed immediately before each chapter, merge it into the source code, and make it available for download.
 Input: the finalized comment (SSoT) from Comment Generation Step 6.
 Rules: Comply with the ChapterVibe RULE. Publicly expose contract types via interfaces (IReadOnlyList<T>, etc.). Do not return arrays or List<T> directly.
        **All interfaces and each of their members must have XML documentation comments attached.**
Note: At the end of the chat reply, output only the review results concisely (do not reprint the template text).
=== End Code Generation Step 1 ===
*/
/*
=== Code Generation Step 2: Skeleton Explicit Implementation of RootArg ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Code Generation Step 1 is assumed.
If the user instructs “Please perform Code Generation Step 2”:
 Generate source code in which RootArg explicitly implements all Arg interfaces (properties/setters accessible only via the interface), merge it into the source, and make it available for download.
Requirements:
 - The backing fields must match the contract type exactly (e.g., IReadOnlyList<T>? _points).
 - Use a cached static Array.Empty<T>() for empty defaults.
 - Accept IEnumerable<T>, but internally normalize to the contract type. Explicitly comment on copy policy.
 - Do not implement methods that perform I/O here (so that adapters can be injected later).
 - If you implement auxiliary classes, attach XML documentation comments.
Output: the RootArg class body (constructor performs basic validation of arguments, injection point properties like IClockArg/ITempFileArg).
=== End Code Generation Step 2 ===
*/
/*
=== Code Generation Step 3: Chapter Empty Implementation Skeleton ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Code Generation Step 2 is assumed.
If the user instructs “Please perform Code Generation Step 3”:
 - Implement IChapter<corresponding Arg interface>, merge it into the source code, and make it available for download.
 - In Handle, prepare try/catch (only for I/O‑expected sections). Summarize errors with a short message plus structured log; do not propagate (RULE #13).
 - Skeleton only wires the next‑phase context using buffer.PushBack(new ChapterContext<...>(..., arg)).
 - Use TODO comments to specify “logic to place in this chapter” and “banned patterns.”
 - A typical chapter has the following structure:
internal sealed class EncodeChapter : IChapter<IEncodeArg>
{
     public void Handle(IEncodeArg arg, IContextBuffer<IEncodeArg> buffer)
     {
         try
         {
              // Perform some processing (e.g., encoding)
              var encodeOut = ...
              buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(), encodeOut));
         } catch (Exception ex) {
              // Summarize error with short message + structured log; do not propagate (RULE #13)
         }
     }
}
 - If constructor injection is involved, receive parameters that can be passed to the “next chapter” as follows:
internal sealed class EncodeChapter : IChapter<IEncodeArg>
{
     private readonly ILoggerFactory _logFactory;
     private readonly ILogger<EncodeChapter> _log;
     private readonly IImageEncoderEngine _engine;

     public EncodeChapter(ILoggerFactory logFactory, IImageEncoderEngine? engine = null)
     {
             _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
             _log = logFactory.CreateLogger<EncodeChapter>();
             _engine = engine ?? new ImageSharpEncoderEngine();
     }

     public void Handle(IEncodeArg arg, IContextBuffer<IEncodeArg> buffer)
     {
         try
         {
              // Perform some processing (e.g., encoding)
              var encodeOut = ...
              buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(_logFactory), encodeOut));
         } catch (Exception ex) {
              // Summarize error with short message + structured log; do not propagate (RULE #13)
         }
     }
}
=== End Code Generation Step 3 ===
*/
/*
=== Code Generation Step 4: Implementing the Main Method ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Code Generation Step 3 is assumed.
If the user instructs “Please perform Code Generation Step 4”:
 - Implement the Main method and make it available for download.
 - The Main method should create instances of Stage, the first Chapter, RootArg, and any other necessary instances, and call Stage.Run().
**Important (must report to or request from the user)**: At this point, delete the ChapterVibe.UsageExample implementation or request the user to delete it.
=== End Code Generation Step 4: Implementing the Main Method ===
*/
/*
=== Code Generation Step 5: Implementing a Single Chapter ===
Rule: Do not carry out this step unless explicitly instructed by the user (do not output prematurely; proposing to run a step is OK).
* Completion of Code Generation Step 4 is assumed.
If the user instructs “Please perform Code Generation Step 5”:
1) List chapters that are in “skeleton state” (those whose Handle has TODO/NotImplemented/empty implementations are considered skeletons), and select just one that is the most upstream in pipeline order. Determine pipeline order based on comments in the source code and the order of PushBack calls.
2) Generate one .cs file implementing the selected chapter’s Handle and any necessary helper or related classes, and make it available for download.
   Ensure that the comment output is self‑contained enough that the implementation of other chapters can be resumed even if the chat history disappears.
3) If there are no skeletons (meaning all are already implemented): do not generate, simply report “No items” briefly.
** Important **
 Do not forget to perform a self‑review and report to the user in the chat.
 - Refer to “(Template) Chat output to return to the user after code generation” for the content.
 - Ask the user to delete the empty implementations implemented in Code Generation Step 3 by listing the relevant method names comprehensively.
=== End Code Generation Step 5 ===
*/
namespace ChapterVibe
{
    using System;
    using System.Collections.Generic;

    // ----- ChapterVibe Framework -----
    /// <summary>
    /// Marker interface for argument types.
    /// </summary>
    public interface IArg
    {
        // intentionally empty
    }

    /// <summary>
    /// A chapter that processes an argument and may enqueue follow-up contexts.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IChapter<in TArg>
        where TArg : IArg
    {
        /// <summary>Handle one step and push next contexts to <paramref name="buffer"/>.</summary>
        void Handle(TArg arg, IContextBuffer<TArg> buffer);
    }

    /// <summary>
    /// A context wrapper that can execute a chapter using its argument.
    /// </summary>
    /// <remarks>
    /// Variance note: <see cref="IChapterContext{TArg}"/> is <b>contravariant</b> (<c>in TArg</c>).
    /// This allows a context of a base argument type to be consumed where a derived argument is processed.
    /// </remarks>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IChapterContext<in TArg>
        where TArg : IArg
    {
        /// <summary>Execute this context within the given buffer/dispatcher.</summary>
        void Execute(IContextBuffer<TArg> buffer);
    }

    /// <summary>
    /// Concrete chapter context.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public sealed class ChapterContext<TArg>(IChapter<TArg> chapter, TArg arg) : IChapterContext<TArg>
        where TArg : IArg
    {
        // This could be readonly fields, but by policy they're kept as properties (see RULE #11).
        private IChapter<TArg> Chapter { get; } = chapter;
        private TArg Arg { get; } = arg;

        public void Execute(IContextBuffer<TArg> buffer)
        {
            Chapter.Handle(Arg, buffer);
        }
    }

    /// <summary>
    /// Buffer for managing a sequence of chapter contexts (enqueue side).
    /// </summary>
    /// <remarks>
    /// Variance note: <see cref="IContextBuffer{TArg}"/> is <b>covariant</b> (<c>out TArg</c>).
    /// Even though <typeparamref name="TArg"/> appears in method parameters via
    /// <see cref="IChapterContext{TArg}"/>, that interface is contravariant (<c>in TArg</c>),
    /// which keeps the overall use of <typeparamref name="TArg"/> in an output position; this complies with CS1961.
    /// Example: <c>IContextBuffer&lt;IHelloArg&gt;</c> can be used where <c>IContextBuffer&lt;IWorldArg&gt;</c> is expected
    /// if <c>IHelloArg : IWorldArg</c>.
    /// </remarks>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IContextBuffer<out TArg>
        where TArg : IArg
    {
        void PushFront(IChapterContext<TArg> chapterContext);
        void PushBack(IChapterContext<TArg> chapterContext);
    }

    /// <summary>
    /// Dispatcher for consuming and executing buffered contexts (dequeue side).
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IContextDispatcher<TArg>
        where TArg : IArg
    {
        /// <summary>Runs until the buffer becomes empty. Exceptions from contexts propagate to the caller unless handled by policy.</summary>
        void RunAll();
    }

    /// <summary>
    /// Thread-safe deque that acts as both buffer and dispatcher.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public sealed class ChapterContextDeque<TArg> : IContextBuffer<TArg>, IContextDispatcher<TArg>
        where TArg : IArg
    {
        // Exceptions to RULE #10: lock objects and fixed backing collections are conventional readonly fields.
        private readonly object _lockObject = new();
        private readonly LinkedList<IChapterContext<TArg>> _queue = new();

        void IContextBuffer<TArg>.PushFront(IChapterContext<TArg> chapterContext) => PushFront(chapterContext);
        void IContextBuffer<TArg>.PushBack(IChapterContext<TArg> chapterContext) => PushBack(chapterContext);
        void IContextDispatcher<TArg>.RunAll() => RunAll();

        private void PushFront(IChapterContext<TArg> chapterContext)
        {
            ArgumentNullException.ThrowIfNull(chapterContext);
            lock (_lockObject) _queue.AddFirst(chapterContext);
        }

        private void PushBack(IChapterContext<TArg> chapterContext)
        {
            ArgumentNullException.ThrowIfNull(chapterContext);
            lock (_lockObject) _queue.AddLast(chapterContext);
        }

        private void RunAll()
        {
            while (true)
            {
                IChapterContext<TArg>? next;
                lock (_lockObject)
                {
                    if (_queue.Count == 0) return;
                    next = _queue.First!.Value;
                    _queue.RemoveFirst();
                }
                // Execute outside the lock to allow re-entrancy and new scheduling.
                next.Execute(this);
            }
        }
    }

    /// <summary>
    /// High-level runner that wires the initial chapter and argument and drains the buffer.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public sealed class Stage<TArg>
        where TArg : IArg
    {
        // By policy kept as a property-like field name would be fine; we keep it private here.
        private ChapterContextDeque<TArg> Buffer { get; } = new();

        public void Run(IChapter<TArg> firstChapter, TArg arg)
        {
            IContextDispatcher<TArg> dispatcher = Buffer; // compile-time guarantee
            IContextBuffer<TArg> buffer = Buffer; // compile-time guarantee

            buffer.PushBack(new ChapterContext<TArg>(firstChapter, arg));
            dispatcher.RunAll();
        }
    }
}
