using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Docodemo.Async.Tasks.Abstractions
{
    /// <summary>
    /// Represents the context for an investigation, including cancellation token and semaphore for task completion.
    /// </summary>
    public interface IAsyncTaskDoorContext<T> : IDisposable
    {
        /// <summary>
        /// A queue to store results of the tasks.
        /// </summary>
        ConcurrentQueue<T> Results { get; }

        /// <summary>
        /// A queue to store exceptions that occurred during the investigation.
        /// </summary>
        ConcurrentQueue<AggregateException> Exceptions { get; }

        /// <summary>
        /// A semaphore that is used to block the investigation until all tasks are completed.
        /// </summary>
        SemaphoreSlim? Semaphore { get; set; }

        /// <summary>
        /// A cancellation token that can be used to cancel the investigation.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Decrements the number of tasks left to be processed and returns the new count.
        /// Note: this method should thread-safe.
        /// </summary>
        int DecrementNumLeftTasks();
    }
}
