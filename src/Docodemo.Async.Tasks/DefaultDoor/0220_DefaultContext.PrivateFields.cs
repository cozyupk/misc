using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docodemo.Async.Tasks.Abstractions;

namespace Docodemo.Async.Tasks.DefaultRunner
{
	/// <summary>
	/// Represents the context for an investigation, including cancellation token and semaphore for task completion.
	/// In this partial file, we implement the <see cref="IBuilderContext{TResult}"/> interface.
	/// </summary>
	partial class DefaultContext<TResult>
	{
        /// <summary>
        /// Field that stores the action to be executed when all tasks are processed.
        /// </summary>
        private ConcurrentWriteOnceField
					<Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>>
						OnAllTasksProcessedAsyncField { get; } = new();

        /// <summary>
        /// Get TaskContinuationOptions used for Task.ContinueWith() in the runner.
        /// </summary>
        private ConcurrentWriteOnceField<TaskContinuationOptions>
                        TaskContinuationOptionsField { get; } = new();

        /// <summary>
        /// Get TaskScheduler used for Task.ContinueWith() in the runner.
        /// </summary>
        private ConcurrentWriteOnceField<TaskScheduler>
                        TaskSchedulerField { get; } = new();
    }
}
