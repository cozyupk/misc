using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docodemo.Async.Tasks.Abstractions
{
    public interface IAsyncTaskDoor
    {
        /// <summary>
        /// Invokes a set of asynchronous tasks and waits for their completion.
        /// </summary>
        (IEnumerable<TResult> Results, IEnumerable<AggregateException>? Exceptions)
            Invoke<TResult>(IAsyncTaskDoorRunnerContext<TResult> context,
            bool isBlockingMode,
            params Func<CancellationToken, Task<TResult>>[] asyncTasks);
    }
}
