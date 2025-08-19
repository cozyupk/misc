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
// 3. Place interface definitions directly before the corresponding Chapter.
// 4. Write clear XML comments for each interface member.
// 5. Runtime arguments must be unified into a single RootArg.
// 6. In RootArg, implement all role interfaces explicitly (use explicit interface implementation so members are only reachable via their interfaces).
// 7. Within each Chapter, consume only its corresponding Arg-interface type; do not cast the argument to other interfaces or to RootArg to reach unrelated members.
// 8. Direct references to RootArg are prohibited. Always access it via an interface.
// 9. The `out` variance of IContextBuffer<out TArg> and the `in` variance of IChapterContext<in TArg> must be preserved; this pairing satisfies CS1961.
// 10. Prefer properties over fields. Exceptions: it is acceptable to use private readonly fields for conventional purposes such as internal lock objects, fixed backing collections, or other synchronization/perf-critical internals.
// 11. When a design choice is driven by framework policy rather than necessity, include a brief inline comment stating the policy and intent so the code remains self-explanatory outside the VeryVibe context (and to AI/automated reviewers). Examples include: choosing a property over a readonly field, using explicit interface implementation, adopting non-standard naming/visibility for pipeline/reflection needs, or specific thread-safety/allocations trade-offs.
// 12. Handle exceptions that may arise from I/O, external processes, or environment-dependent calls inside the Chapter. Do not allow such exceptions to escape `Handle` unless explicitly documented by policy.
// 13. Take advantage of .NET 8.0 language features. Enable nullable.
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

## Refactoring Suggestions & Self-Prompt

Review both the provided and generated code, and suggest refactorings if applicable.  
Always include a corresponding self-prompt for each suggestion.  

If there are no refactoring suggestions, explicitly state:  
"There are no refactoring suggestions."
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
