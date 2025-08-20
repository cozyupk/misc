#nullable enable

// --- RULE ---
// The VeryVibe namespace provides the core functionality of the VeryVibe framework. 
// When generating code, make use of this core functionality and implement each Chapter 
// in compliance with the SOLID principles.
// The VeryVibe.UsageExample namespace is for demonstration purposes only. 
// Please remove it after code generation.
// Also, please remove the RULE section after code generation.
//
// When generating or modifying code in this file, follow these rules:
// 1. Split the source code into proper phases, giving each Chapter independent functionality.
// 2. Each Chapter should depend only on the minimal set of role interfaces it requires.
// 3. To preserve the contravariance of IChapter and IChapterContext type parameters,  
//    upstream role interfaces MUST inherit downstream ones in order.  
//    Example from VeryVibe.UsageExample: IHelloArg : IWorldArg : IArg  
//    Hypothetical example: ICommandLineParseArg : IComputeArg : IRenderArg : ISaveArg : IArg
// 4. Place interface definitions directly before the corresponding Chapter.
// 5. Write clear XML comments for each interface member.
// 6. Runtime arguments must be unified into a single RootArg.
// 7. In RootArg, implement all role interfaces explicitly (use explicit interface implementation so members are only reachable via their interfaces).
// 8. Within each Chapter, consume only its corresponding Arg-interface type; do not cast the argument to other interfaces or to RootArg to reach unrelated members.
// 9. Direct references to RootArg are strictly prohibited — this includes property setters. Always access it via an interface.
// 10. The `out` variance of IContextBuffer<out TArg> and the `in` variance of IChapterContext<in TArg> must be preserved; this pairing satisfies CS1961.
// 11. Prefer properties over fields. Exceptions: it is acceptable to use private readonly fields for conventional purposes such as internal lock objects, fixed backing collections, or other synchronization/perf-critical internals.
// 12. When a design choice is driven by framework policy rather than necessity, include a brief inline comment stating the policy and intent so the code remains self-explanatory outside the VeryVibe context (and to AI/automated reviewers). Examples include: choosing a property over a readonly field, using explicit interface implementation, adopting non-standard naming/visibility for pipeline/reflection needs, or specific thread-safety/allocations trade-offs.
// 13. Handle exceptions that may arise from I/O, external processes, or environment-dependent calls inside the Chapter. Do not allow such exceptions to escape `Handle` unless explicitly documented by policy.
// 14. Take advantage of .NET 8.0 language features. Enable nullable.
// 15. If compilation or execution requires .csproj settings (e.g., TargetFramework, UseWindowsForms,
//     SupportedOSPlatformVersion, platform-dependent package references), explicitly document this at the beginning of the program.
// 16. Collection contracts must always be expressed using interfaces (e.g. IReadOnlyList<T>, IEnumerable<T>).
//     Do not expose concrete types such as List<T> or arrays (T[]) in Arg or Chapter contracts.
// 17. When using null-coalescing operators (?? / ??=), both sides must have the same declared type.
//     Do not mix List<T> with T[]. Align with the contract type.
// 18. Use cached static constants for empty defaults.
//     Example: private static readonly IReadOnlyList<T> Empty = Array.Empty<T>();
//              public IReadOnlyList<T> Values => _values ?? Empty;
// 19. Backing field types must match the contract type exactly.
//     Example: private IReadOnlyList<T>? _points;   // not List<T>?
// 20. When accepting mutable input (e.g. SetNormalizedPoints), accept it broadly (IEnumerable<T>)
//     but store it as the contract type. Explicitly comment the copy/no-copy policy (RULE #11).
// 21. Tuple element names do not affect type identity, but must be kept consistent in codebase.
//     Always prefer (double x, double y) instead of mixing with (double, double).
// 22. (High-level principle: No Destructive Changes) Never perform destructive modifications by default.
//     Deletion, overwrite, or irreversible transformation of existing data is prohibited by default.
//     If necessary, require "create new under different name" + "explicit flag" + "pre-validation".
// 23. (High-level principle: Input Defense) All external inputs (CLI args, env vars, filenames, paths, URLs,
//     JSON, template strings, etc.) must be sanitized/validated for security (prefer allowlist over denylist).
// 24. (High-level principle: Least Privilege) Always use the minimal permissions required (files, network, credentials).
//     If escalation or broader privilege is needed, document the reason inline (see RULE #12/#11).
// 25. (High-level principle: Observability) Logs must not contain secrets/PII.
//     Retain only minimal structured metadata for reproduction; always mask secrets/credentials.
// 26. (High-level principle: Secure Defaults) Defaults must always be "safe" (no sending, no overwriting, no execution).
//     If loosened for convenience, require explicit flags and inline justification (RULE #11).
// 27. (Input Validation) Validate parameters for type/range/size/format (regex).
//     Reject unspecified, empty, control chars, newlines, or null bytes.
//     Normalize paths with GetFullPath and ensure they remain under base directory via StartsWith.
//     Reject symlinks/reparse points.
// 28. (Output Encoding) Encode/escape output strings according to sink (HTML/CSV/JSON/shell).
//     Never build commands or queries by string concatenation.
//     For command execution, set UseShellExecute=false and pass args as arrays.
//     Always parameterize SQL/queries.
// 29. (Safe File I/O) Never overwrite files (use FileMode.CreateNew).
//     Write to temp files, then atomically move without overwrite.
//     Enforce extension/size limits. Use minimal FileShare (read: Read, write: None).
// 30. (Safe Networking) Default deny for outbound.
//     Allow only via domain allowlist + TLS (>=1.2) + certificate validation.
//     Always set timeouts/retries (exponential backoff)/CancellationToken.
//     Explicitly handle redirects and proxies.
// 31. (Serialization/Deserialization) Prohibit BinaryFormatter and unsafe APIs.
//     Use System.Text.Json with MaxDepth and bind to known types.
//     Do not allow polymorphic arbitrary type resolution or embedded Type names.
// 32. (Secrets Handling) Never embed API keys, tokens, or connection strings in source.
//     Retrieve via injection interfaces or OS secure stores.
//     Do not leak secrets in exceptions/logs (mask/summarize).
// 33. (Cryptography/Randomness) Use only platform crypto libraries.
//     Use RandomNumberGenerator for security purposes; never custom crypto.
// 34. (Concurrency/Resources) For long-running tasks, check cancellation regularly.
//     Always dispose IDisposable with using.
//     Keep lock scope minimal; do I/O/user code outside locks (see RunAll implementation).
// 35. (Confirmation Flow) For potentially destructive ops (duplicate names, deletes, moves),
//     require two-step confirmation: dry-run (detect only) → Confirm flag to execute.
//     Prefer output to separate dir or versioned path.
// 36. (Testability) Provide unit tests to enforce safety rules (e.g. refuse directory traversal,
//     forbid overwrite, guarantee rollback on exceptions, enforce encoding).
//     Ensure tests guard even if code is AI-modified.
// 37. (AI/Autogen Safeguards) Explicitly mark "forbidden patterns" in comments
//     (e.g. Process.Start with shell concat, FileMode.Create, unvalidated paths).
//     Always provide safe alternatives right below (see RULE #11).
//     Review begins by scanning for forbidden patterns.
// 38. (Failure Behavior) Never silently swallow failures.
//     For users: safe error messages. For logs: reproducible minimal info.
//     Clearly document per-Chapter whether to abort or fallback (consistent with RULE #12).
// 39. (Default Configs) Defaults must enforce safe behavior: outbound=disabled, overwrite=disabled,
//     execution=disabled, interactive confirmation=required.
//     Unlock only with explicit flags (e.g. --allow-overwrite, --allow-network).
// 40. UX must also be considered.  
//     Numbers, dates, and times must be formatted according to the current locale 
//     (e.g. ToString("N", CultureInfo.CurrentCulture)).  
//     For logs and programmatic outputs, use culture-invariant formats (e.g. ISO 8601 for dates).  
//     Error messages and user-facing strings must follow localization/translation policies.
// 41. All comments in code and subsequent chat outputs must be written in English.
//
//
/*
Chat output to the user after code generation:

## Compliance Report

When generating code, you must comply with the SOLID principles.  
If it is absolutely necessary to deviate, you must report the following after generation:

- Which principle was violated  
- Why the deviation was necessary  
- Proposed future refactorings or mitigation strategies  

If no violations occurred, explicitly state:  
"All SOLID principles have been maintained."

## Bug / Logic Review & Self-Prompt

Review both the generated code and the provided code, and point out 
typical human mistakes or logical inconsistencies based on the following criteria.  
Always include a corresponding self-prompt for each point:

- Unintended off-by-one errors, confusion between cumulative and delta values  
- Unnecessary or redundant operations  
- Performance hazards (e.g., massive loops)  
- Poor readability or misleading naming/structure  

If there are no issues, explicitly state:  
"There are no findings in the bug/logic review."

## Security Review & Self-Prompt

Assess the code against the security rules. For each item, either confirm compliance or report findings,
and include a brief self-prompt describing how you verified or would fix it:

- Non-destructive by default: No overwrites/deletes; atomic writes via temp + move?  
  *Self-prompt:* “Did I ensure CreateNew/atomic move and require an explicit confirm flag?”
- Input validation & sanitization: All external inputs validated (type/range/size/regex)?  
  *Self-prompt:* “Did I normalize paths and reject traversal/symlinks?”
- Injection resistance: No shell string concatenation; UseShellExecute=false; parameterized queries only.  
  *Self-prompt:* “Are all command args passed as arrays/typed params, not interpolated strings?”
- Output encoding: Proper escaping for HTML/CSV/JSON/shell where applicable.  
  *Self-prompt:* “Did I encode at the sink appropriate to the context?”
- Serialization safety: No BinaryFormatter or permissive polymorphic binding; bounded depth.  
  *Self-prompt:* “Am I using System.Text.Json with safe options and known types?”
- Secrets handling: No secrets in source/logs; retrieved via injected providers/secure store.  
  *Self-prompt:* “Could any exception/log leak sensitive values?”
- Networking: Default deny for outbound; TLS enabled; timeouts/retries/cancellation set.  
  *Self-prompt:* “Is there an allowlist of destinations and cert validation on?”
- Concurrency & resources: Lock scope minimal; I/O outside locks; proper disposal; supports CancellationToken.  
  *Self-prompt:* “Could any long-running path starve or deadlock?”
- Cryptography & randomness: Only platform crypto; RandomNumberGenerator for security needs.  
  *Self-prompt:* “Did I avoid custom crypto or non-CSPRNG?”
- Logging & observability: Structured logs; PII masked; no path/secret disclosure on failure.  
  *Self-prompt:* “Do logs enable reproduction without leaking sensitive data?”

If there are no issues, explicitly state:  
"There are no findings in the security review."

## Refactoring Suggestions & Self-Prompt

Review both the provided and generated code, and suggest refactorings if applicable.  
Always include a corresponding self-prompt for each suggestion.  

If there are no refactoring suggestions, explicitly state:  
"There are no refactoring suggestions."

## Uncertainty Report

If, during generation, multiple valid design choices existed and the generator made a judgment under uncertainty,  
this section must explicitly report those decisions. Each item should include:

- **Decision Point**: What part of the code required a choice  
- **Alternatives**: The other options that were considered  
- **Chosen Option & Reason**: Which option was picked and why  
- **Confidence Level**: High / Medium / Low  
- **Future Suggestion**: Any recommendation for refinement or review

*/
// --- END RULE ---

using System;
using System.Collections.Generic;

namespace VeryVibe
{
    // ----- VeryVibe Framework -----
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
    public class Stage<TArg>
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

namespace VeryVibe.UsageExample
{
    using VeryVibe;

    // -- Usage Example --

    internal interface IWorldArg : IArg
    {
        int WorldCount { get; set; }
        Action<string> WriteLineAction { get; }
    }

    internal sealed class WorldChapter : IChapter<IWorldArg>
    {
        public void Handle(IWorldArg arg, IContextBuffer<IWorldArg> buffer)
        {
            try
            {
                arg.WriteLineAction("World");
            }
            catch (Exception ex)
            {
                // By policy (RULE #12): handle I/O-like failures inside the Chapter.
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

    internal interface IHelloArg : IWorldArg
    {
        int HelloCount { get; set; }
        new Action<string> WriteLineAction { get; }
    }

    internal sealed class HelloChapter : IChapter<IHelloArg>
    {
        public void Handle(IHelloArg arg, IContextBuffer<IHelloArg> buffer)
        {
            try
            {
                arg.WriteLineAction("Hello");
            }
            catch (Exception ex)
            {
                // By policy (RULE #12): handle I/O-like failures inside the Chapter.
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
                buffer.PushBack(new ChapterContext<IWorldArg>(new WorldChapter(), arg));
            }
        }
    }

    internal class RootArg : IHelloArg
    {
        int IHelloArg.HelloCount { get; set; }
        int IWorldArg.WorldCount { get; set; }

        Action<string> IHelloArg.WriteLineAction => WriteLineAction;
        Action<string> IWorldArg.WriteLineAction => WriteLineAction;

        // Could be a field; kept as a property by policy to align with reflection/pipeline conventions (RULE #11).
        private Action<string> WriteLineAction { get; } = Console.WriteLine;

        public RootArg(int helloCount, int worldCount)
        {
            if (helloCount < 0) throw new ArgumentOutOfRangeException(nameof(helloCount), "Hello count must be non-negative.");
            if (worldCount < 0) throw new ArgumentOutOfRangeException(nameof(worldCount), "World count must be non-negative.");
            ((IHelloArg)this).HelloCount = helloCount;
            ((IWorldArg)this).WorldCount = worldCount;
        }
    }

    internal class Program
    {
        private static void Main(string[] _)
        {
            var stage = new Stage<IHelloArg>();
            var chapter = new HelloChapter();
            var arg = new RootArg(3, 2);

            stage.Run(chapter, arg);
        }
    }
}
