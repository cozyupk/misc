using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docodemo.Async.Tasks.Abstractions
{
    /// <summary>
    /// Represents the context for async door context builders, including cancellation token and semaphore for task completion.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IBuilderContext<TResult>
    {
        /// <summary>
        /// Sets the action to be executed when all tasks are processed.
        /// </summary>
        /// <param name="asyncTask">The asynchronous task to execute.</param>
        void SetOnAllTasksProcessedAsync(
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? asyncTask
        );
    }
}
