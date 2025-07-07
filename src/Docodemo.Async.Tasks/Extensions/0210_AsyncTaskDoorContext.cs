using System.Collections.Concurrent;
using System.Threading;
using System;
using Docodemo.Async.Tasks.Abstractions;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Represents the context for an investigation, including cancellation token and semaphore for task completion.
    /// </summary>
    internal class AsyncTaskDoorContext<T> : IAsyncTaskDoorContext<T>
    {
        /// <summary>
        /// A queue to store results of the tasks.
        /// </summary>
        public ConcurrentQueue<T> Results { get; } = new();

        /// <summary>
        /// A queue to store exceptions that occurred during the investigation.
        /// </summary>
        public ConcurrentQueue<AggregateException> Exceptions { get; } = new();

        /// <summary>
        /// A semaphore that is used to block the investigation until all tasks are completed.
        /// </summary>
        public SemaphoreSlim? Semaphore { get; set; }

        /// <summary>
        /// A cancellation token that can be used to cancel the investigation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Decrements the number of tasks left to be processed and returns the new count.
        /// </summary>
        public int DecrementNumLeftTasks()
        {
            return Interlocked.Decrement(ref NumLeftTasks);
        }

        /// <summary>
        /// The number of tasks that are still left to be processed.
        /// Note: We use field insted of property to use Interlocked operations for thread safety.
        /// </summary>
        private int NumLeftTasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationContext"/> class.
        /// </summary>
        public AsyncTaskDoorContext(CancellationToken cancellationToken, bool isBlocking, int numTasks)
        {
            // store the cancellation token
            CancellationToken = cancellationToken;
            // store the number of tasks
            NumLeftTasks = numTasks;
            // If the door is blocking, we create a semaphore to wait for all tasks to complete.
            if (isBlocking)
            {
                Semaphore = new(0, numTasks);
            }
        }

        /// <summary>
        /// Disposes the semaphore if it is not null.
        /// </summary>
        public void Dispose()
        {
            Semaphore?.Dispose();
            Semaphore = null;
        }
    }
}
