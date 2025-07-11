using Probe;

namespace ConsoleApp1_en_US
{
    /// <summary>
    /// Observe the behavior of ContinueWith and ConfigureAwait.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The Main method is the entry point of the application.
        /// </summary>
        static async Task Main(string[] _1)
        {
            // Start thread "stirring" operations.
            CancellationTokenSource cts = new();
            _ = ThreadProbe.StirThreadPoolAsync(cts.Token);
            _ = ThreadProbe.StirThreadPoolAsync(cts.Token);
            _ = ThreadProbe.StirThreadPoolAsync(cts.Token);

            Console.WriteLine();

            // Assumes SynchronizationContext.Current is null.
            if (SynchronizationContext.Current != null)
            {
                throw new InvalidOperationException(
                    "This program is designed to run in a context where SynchronizationContext.Current is null. " +
                    "Please ensure you are running it in a console application or similar environment.");
            }

            // Create an instance to observe thread state.
            var threadProbe = new ThreadProbe();

            // Run cases to observe behavior of ContinueWith and ConfigureAwait.
            Console.WriteLine("=== When SynchronizationContext.Current is null ===");
            await ExecuteCase(threadProbe, Case_AwaitAndReturnOriginalTask(threadProbe));
            await ExecuteCase(threadProbe, Case_AwaitAndReturnContinueWithTask(threadProbe));
            await ExecuteCase(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            // Below simulates environments like UI thread or ASP.NET single-thread context.
            var syncCtx = new SingleThreadSyncContext(threadProbe);
            SynchronizationContext.SetSynchronizationContext(syncCtx);

            // Switch to single-thread context.
            await Task.Yield();

            Console.WriteLine($"=== When SynchronizationContext.Current is Single Thread Sync Context (STSC), no ConfigureAwait ===");
            await ExecuteCase(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            var patterns = new List<(bool, bool)>
            {
                (false, false), (false, true), (true, false), (true, true)
            };
            foreach (var (exec, wait) in patterns)
            {
                Console.WriteLine($"=== STSC: ConfigureAwait({exec}/{wait}) ===");
                await ExecuteCaseWithConfigureAwait(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe), exec, wait);
            }

            Console.WriteLine($"=== STSC: await Task.CompletedTask.ConfigureAwait(false); ===");
            await ExecuteCaseWithEmptyAwait(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            Console.WriteLine($"=== STSC: await YieldOnlyAsync().ConfigureAwait(false) ===");
            await ExecuteCaseWithEmptyAwait2(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            Console.WriteLine($"=== STSC: Task.Delay(0).ConfigureAwait(false) ===");
            await ExecuteCaseWithTaskEmptyDelay(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            Console.WriteLine($"=== STSC: Task.Delay(1).ConfigureAwait(false) ===");
            await ExecuteCaseWithTaskNonEmptyDelay(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            SynchronizationContext.SetSynchronizationContext(null);
            syncCtx.Complete();

            // Stop thread stirring operations.
            cts.Cancel();
            await Task.Delay(100);
        }

        static async Task ExecuteCase(ThreadProbe tp, (string, Func<Task>) caseExec)
        {
            tp.WriteLineThreadID($"await started: {caseExec.Item1}");
            await caseExec.Item2();
            tp.WriteLineThreadID($"await completed: {caseExec.Item1}");
            await Task.Delay(1000);
            tp.WriteLineThreadID($"post-wait: {caseExec.Item1}");
            Console.WriteLine();
        }

        static async Task ExecuteCaseWithConfigureAwait(ThreadProbe tp, (string, Func<Task>) caseExec, bool execCfg, bool waitCfg)
        {
            tp.WriteLineThreadID($"await started: {caseExec.Item1}");
            await caseExec.Item2().ConfigureAwait(execCfg);
            tp.WriteLineThreadID($"await completed: {caseExec.Item1}");
            await Task.Delay(1000).ConfigureAwait(waitCfg);
            tp.WriteLineThreadID($"post-wait: {caseExec.Item1}");
            Console.WriteLine();
        }

        static async Task ExecuteCaseWithEmptyAwait(ThreadProbe tp, (string, Func<Task>) caseExec)
        {
            tp.WriteLineThreadID($"before await Task.CompletedTask.ConfigureAwait(false): {caseExec.Item1}");
            await Task.CompletedTask.ConfigureAwait(false);
            tp.WriteLineThreadID($"await started: {caseExec.Item1}");
            await caseExec.Item2();
            tp.WriteLineThreadID($"await completed: {caseExec.Item1}");
            await Task.Delay(1000);
            tp.WriteLineThreadID($"post-wait: {caseExec.Item1}");
            Console.WriteLine();
        }

        static async Task ExecuteCaseWithEmptyAwait2(ThreadProbe tp, (string, Func<Task>) caseExec)
        {
            static async Task YieldOnlyAsync() => await Task.Yield();
            tp.WriteLineThreadID($"before await YieldOnlyAsync().ConfigureAwait(false): {caseExec.Item1}");
            await YieldOnlyAsync().ConfigureAwait(false);
            tp.WriteLineThreadID($"await started: {caseExec.Item1}");
            await caseExec.Item2();
            tp.WriteLineThreadID($"await completed: {caseExec.Item1}");
            await Task.Delay(1000);
            tp.WriteLineThreadID($"post-wait: {caseExec.Item1}");
            Console.WriteLine();
        }

        static async Task ExecuteCaseWithTaskEmptyDelay(ThreadProbe tp, (string, Func<Task>) caseExec)
        {
            tp.WriteLineThreadID($"before await Task.Delay(0).ConfigureAwait(false): {caseExec.Item1}");
            await Task.Delay(0).ConfigureAwait(false);
            tp.WriteLineThreadID($"await started: {caseExec.Item1}");
            await caseExec.Item2();
            tp.WriteLineThreadID($"await completed: {caseExec.Item1}");
            await Task.Delay(1000);
            tp.WriteLineThreadID($"post-wait: {caseExec.Item1}");
            Console.WriteLine();
        }

        static async Task ExecuteCaseWithTaskNonEmptyDelay(ThreadProbe tp, (string, Func<Task>) caseExec)
        {
            tp.WriteLineThreadID($"before Task.Delay(1).ConfigureAwait(false): {caseExec.Item1}");
            await Task.Delay(1).ConfigureAwait(false);
            tp.WriteLineThreadID($"await started: {caseExec.Item1}");
            await caseExec.Item2();
            tp.WriteLineThreadID($"await completed: {caseExec.Item1}");
            await Task.Delay(1000);
            tp.WriteLineThreadID($"post-wait: {caseExec.Item1}");
            Console.WriteLine();
        }

        static async Task DummyMethod(ThreadProbe tp, string label)
        {
            tp.WriteLineThreadID($"{label} - before await()", 2);
            await Task.Delay(200);
            tp.WriteLineThreadID($"{label} - after await()", 2);
        }

        static (string, Func<Task>) Case_AwaitAndReturnOriginalTask(ThreadProbe tp)
        {
            var task = Task.Run(() => DummyMethod(tp, "In Task.Run()"));
            task.ContinueWith(t => DummyMethod(tp, "In ContinueWith()"));
            return ("Return only the original Task after ContinueWith", () => task);
        }

        static (string, Func<Task>) Case_AwaitAndReturnContinueWithTask(ThreadProbe tp)
        {
            var task = Task.Run(() => DummyMethod(tp, "In Task.Run()"));
            task = task.ContinueWith(t => DummyMethod(tp, "In ContinueWith()"));
            return ("Return the Task returned by ContinueWith (no Unwrap)", () => task);
        }

        static (string, Func<Task>) Case_AwaitAndReturnContinueWithUnwrappedTask(ThreadProbe tp)
        {
            var task = Task.Run(() => DummyMethod(tp, "In Task.Run()"));
            task = task.ContinueWith(t => DummyMethod(tp, "In ContinueWith()")).Unwrap();
            return ("Return the unwrapped Task from ContinueWith", () => task);
        }
    }
}