using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Helper class to convert parameters to adapt them for asynchronous task doors.
    /// </summary>
    internal static class AsyncTaskDoorCallingHelper
    {
        /// <summary>
        /// Converts an action that processes results and exceptions into an asynchronous function.
        /// </summary>
        public static Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> Taskify<TResult>(Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> action)
        {
            return (results, exceptions) =>
            {
                action(results, exceptions);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Converts an action that processes exceptions into an asynchronous function.
        /// </summary>
        public static Func<IEnumerable<AggregateException>?, Task> Taskify(Action<IEnumerable<AggregateException>?> action)
        {
            return (exceptions) =>
            {
                action(exceptions);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Wraps a Func<Task<TResult>> into a Func<CancellationToken, Task<TResult>> to allow for uniform handling of results.
        /// </summary>
        public static Func<CancellationToken, Task<TResult>> AsCancellableFuncTask<TResult>(Func<Task<TResult>> task)
            => ct => task();

        /// <summary>
        /// Wraps a Func<Task> into a Func<CancellationToken, Task> to allow for uniform handling of results.
        /// </summary>
        public static Func<CancellationToken, Task> AsCancellableActionTask(Func<Task> task)
            => ct => task();
    }
}
