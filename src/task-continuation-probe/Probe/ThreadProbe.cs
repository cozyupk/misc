using System.Collections.Concurrent;

namespace Probe
{
    /// <summary>
    /// A class to probe and label threads in a thread-safe manner.
    /// </summary>
    public class ThreadProbe
    {
        /// <summary>
        /// A thread-safe dictionary to map thread IDs to labels.
        /// </summary>
        private ConcurrentDictionary<int, string> ThreadIdToLabel { get; } = new();

        private object LockThreadIdToLabel { get; } = new object();

        /// <summary>
        /// Writes the label of current thread ID.
        /// </summary>
        public void WriteLineThreadID(string? message = null, int index = 0)
        {
            // Get the current managed thread ID and create a label for it
            var ThreadId = Environment.CurrentManagedThreadId;   
            lock(LockThreadIdToLabel)
            {
                // If the thread ID is not already in the dictionary, add it with a label
                if (!ThreadIdToLabel.ContainsKey(ThreadId))
                {
                    ThreadIdToLabel[ThreadId] = $"Thread {ThreadIdToLabel.Count + 1:X2}";
                }
            }

            // This method will be executed on a thread pool thread
            Console.WriteLine(
                $"{new string(' ', index)}[{ThreadIdToLabel[ThreadId]}: SyncCtx is {(SynchronizationContext.Current == null ? "null" : "not null")}] {message}" +
                $"");
        }

        /// <summary>
        /// Stirs the thread pool by creating a burst of tasks that sleep for a random duration.
        /// </summary>
        public static async Task StirThreadPoolAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("[StirThreadPoolAsync] Starting thread pool chaos...");

            while (!cancellationToken.IsCancellationRequested)
            {
                var tasks = Enumerable.Range(1, 60).Select(t => Task.Run(() =>
                {
                    try
                    {
                        var sleepMs = t;
                        Thread.Sleep(sleepMs);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[StirThreadPoolAsync Task Error] {ex}");
                    }
                }, cancellationToken)).ToArray();

                try
                {
                    await Task.WhenAll(tasks); // await group completion
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[StirThreadPoolAsync] Cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[StirThreadPoolAsync Error] {ex}");
                }
            }

            Console.WriteLine("[StirThreadPoolAsync] Stopped.");
        }
    }
}
