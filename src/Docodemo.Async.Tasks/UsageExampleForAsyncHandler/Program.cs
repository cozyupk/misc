using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Docodemo.Async.Tasks.Abstractions;
using Docodemo.Async.Tasks.Extentions;

namespace Docodemo.Async.Tasks.UsageExampleForAsyncHandler
{
    internal class Program
    {
        /// <summary>
        /// A static lazy instance of a Stopwatch to measure elapsed time.
        /// </summary>
        private static Lazy<Stopwatch> Sw { get; }
                                    = new Lazy<Stopwatch>(() => {
                                        var sw = new Stopwatch();
                                        sw.Start();
                                        InitialCpuUsageMs = Proc.TotalProcessorTime.TotalMilliseconds;
                                        return sw;
                                    });

        /// <summary>
        /// A static instance of the current process to track CPU time.
        /// </summary>
        private static Process Proc { get; } = Process.GetCurrentProcess();

        /// <summary>
        /// A static variable to store the CPU elapsed time offset in milliseconds.
        /// </summary>
        private static double InitialCpuUsageMs = 0.0;

        /// <summary>
        /// Writes a message to the console with the elapsed time since the stopwatch started.
        /// </summary>
        public static void WriteLineMessage(string message)
        {
            Proc.Refresh();
            Console.WriteLine(
                $"{Sw.Value.ElapsedMilliseconds,5:N0}ms: {message}" +
                $" - (Total CPU time elapsed: {Proc.TotalProcessorTime.TotalMilliseconds - InitialCpuUsageMs:N0}ms)");
        }

        /// <summary>
        /// Main entry point of the program that demonstrates the usage of Awaiter class
        /// </summary>
        static void Main(string[] _)
        {
            // Prepare new awaiter instance.
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1859 // Use concrete types where possible to improve performance
            IAsyncTaskDoor awaiter = new AsyncTaskDoor();
#pragma warning restore CA1859 // Use concrete types where possible to improve performance
#pragma warning restore IDE0079 // Remove unnecessary suppression

            // Define an async tasks.
            async Task<int> ExampleAsyncTaskA()
            {
                WriteLineMessage("[A] Hello from async lambda");
                await Task.Delay(2500);
                WriteLineMessage("[A] Async lambda completed");
                return 42; // Example return value
            }

            // Define another async task.
            async Task<int> ExampleAsyncTaskB()
            {
                WriteLineMessage("[B] Hello from async lambda");
                await Task.Delay(1000);
                WriteLineMessage("[B] Now waiting something...");
                await Task.Delay(1000);
                WriteLineMessage("[B] Async lambda completed");
                return 84; // Example return value
            }

            // Define an "awaitable sync task".
            Task<int> ExampleTaskC()
            {
                WriteLineMessage("[C] Hello from async lambda");
                WriteLineMessage("[C] Async lambda completed");
                return Task.FromResult(100); // Example return value
            }

            // Define an asyncTask that throws Exception.
            async Task<int> ExampleAsyncTaskD()
            {
                WriteLineMessage("[D] Hello from async lambda");
                WriteLineMessage("[D] Async lambda completed");
                await Task.Delay(1000);
                throw new InvalidOperationException("This is a test exception from async task D.");
            }

            // Define an "awaitable sync task" that throws an exception immediately.
            Task<int> ExampleTaskE()
            {
                WriteLineMessage("[E] Hello from async lambda");
                WriteLineMessage("[E] Async lambda completed");
                throw new InvalidOperationException("This is a test exception from sync task E.");
            }

            // Await for the all tasks above to complete.
            var (results, exceptions) = awaiter.Investigate(
                ExampleAsyncTaskA, ExampleAsyncTaskB, ExampleTaskC,
                ExampleAsyncTaskD, ExampleTaskE
            );

            Console.WriteLine();
            Console.WriteLine("All tasks completed. Results:");
            Console.WriteLine(string.Join(", ", results));

            if (exceptions != null)
            {
                Console.WriteLine("");
                Console.WriteLine("FYI: The following exceptions occurred:");
                foreach (var ex in exceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    foreach (var innerEx in ex.InnerExceptions)
                    {
                        Console.WriteLine($"  Inner Exception: {innerEx.Message}");
                        Console.WriteLine("--- Begin Stack Trace ---");
                        Console.WriteLine(innerEx.StackTrace);
                        Console.WriteLine("--- End Stack Trace ---");
                    }
                }
            }

            // Care the stopwatch.
            Sw.Value.Stop();
        }
    }
}
