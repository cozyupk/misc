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

        /// <summary>
        /// Object to lock access to the thread ID to label dictionary.
        /// </summary>
        private object LockThreadIdToLabel { get; } = new object();

        /// <summary>
        /// Managed thread ID of the synchronized single thread context, if it exists.
        /// </summary>
        private int? ManagedThreadIdOfSyncronizedSingleThreadContext = null;

        /// <summary>
        /// Sets the managed thread ID of the synchronized single thread context.
        /// </summary>
        public void SetManagedThreadIdOfSyncronizedSingleThreadContext(int managedThreadId)
        {
            // Set the managed thread ID of the synchronized single thread context
            ManagedThreadIdOfSyncronizedSingleThreadContext = managedThreadId;
        }

        /// <summary>
        /// Writes the label of current thread ID.
        /// </summary>
        public void WriteLineThreadID(string? message = null, int index = 0)
        {
            // Get the current managed thread ID and create a label for it
            var ThreadId = Environment.CurrentManagedThreadId;   
            string threadLabel;

            if (ManagedThreadIdOfSyncronizedSingleThreadContext.HasValue &&
                ManagedThreadIdOfSyncronizedSingleThreadContext.Value == ThreadId)
            {
                threadLabel = "SyncCtxThread";
            }
            else
            {
                lock (LockThreadIdToLabel)
                {
                    // If the thread ID is not already in the dictionary, add it with a label
                    if (!ThreadIdToLabel.TryGetValue(ThreadId, out string? value))
                    {
                        value = $"Thread {ThreadIdToLabel.Count + 1:X2}";
                        ThreadIdToLabel[ThreadId] = value;
                    }
                    threadLabel = value;
                }
            }

            // This method will be executed on a thread pool thread
            Console.WriteLine(
                $"{new string(' ', index)}[{threadLabel}: SyncCtx is {(SynchronizationContext.Current == null ? "null" : "not null")}] {message}" +
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
