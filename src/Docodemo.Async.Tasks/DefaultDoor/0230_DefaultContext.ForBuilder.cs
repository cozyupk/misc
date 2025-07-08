using System.Collections.Generic;
using System;
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
		: IBuilderContext<TResult>
	{
        /// <summary>
        /// Sets the action to be executed when all tasks are processed.
        /// </summary>
        public void SetOnAllTasksProcessedAsync(
			Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? asyncTask
		) {
            OnAllTasksProcessedAsyncField
                .Set(
                    asyncTask ?? throw new ArgumentNullException(nameof(asyncTask))
                );
        }
    }
}
