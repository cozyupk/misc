using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docodemo.Async.Tasks.Abstractions
{
    /// <summary>
    /// Represents the context for async door runners, including cancellation token and semaphore for task completion.
    /// </summary>
    public interface IRunnerContext<TResult> : IDisposable
    {
        /// <summary>
        /// A queue to store results of the tasks.
        /// </summary>
        ConcurrentQueue<TResult> Results { get; }

        /// <summary>
        /// A queue to store exceptions that occurred during the investigation.
        /// </summary>
        ConcurrentQueue<AggregateException> Exceptions { get; }

        /// <summary>
        /// A cancellation token that can be used to cancel the investigation.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Get the task that should be executed when all tasks are processed.
        /// </summary>
        Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? OnAllTasksProcessedAsync { get; }

        /// <summary>
        /// Get the TaskContinuationOptions used for Task.ContinueWith() in the runner.
        /// </summary>
        TaskContinuationOptions TaskContinuationOptions { get; }

        /// <summary>
        /// Get the TaskScheduler used for Task.ContinueWith() in the runner.
        /// </summary>
        TaskScheduler TaskScheduler { get; }
    }
}
