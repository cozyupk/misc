using System.Collections.Concurrent;
using System.Threading;
using System;
using Docodemo.Async.Tasks.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docodemo.Async.Tasks.DefaultRunner
{
    /// <summary>
    /// Represents the context for an investigation, including cancellation token and semaphore for task completion.
    /// In this partial file, we implement the <see cref="IRunnerContext{TResult}"/> interface.
    /// </summary>
    partial class DefaultContext<TResult>
        : IRunnerContext<TResult>
    {
        /// <summary>
        /// A queue to store results of the tasks.
        /// </summary>
        public ConcurrentQueue<TResult> Results { get; } = new();

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
        /// A Property to get the action to be executed when all tasks are processed.
        /// </summary>
        public Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? OnAllTasksProcessedAsync
            => OnAllTasksProcessedAsyncField.Value;

        /// <summary>
        /// Get the TaskContinuationOptions used for Task.ContinueWith() in the runner.
        /// </summary>
        public TaskContinuationOptions TaskContinuationOptions
            => TaskContinuationOptionsField.IsSet ?
                    TaskContinuationOptionsField.Value : TaskContinuationOptions.None;

        /// <summary>
        /// Get the TaskScheduler used for Task.ContinueWith() in the runner.
        /// </summary>
        public TaskScheduler TaskScheduler
            => TaskSchedulerField.IsSet ?
                    TaskSchedulerField.Value ?? TaskScheduler.Default
                    : TaskScheduler.Default;

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
