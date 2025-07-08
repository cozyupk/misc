using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Helper class to convert parameters to adapt them for asynchronous task doors.
    /// </summary>
    internal static class CallingHelper
    {
        /// <summary>
        /// Converts an action that processes results and exceptions into an asynchronous function.
        /// </summary>
        public static Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task> Taskify<TResult>(Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> action)
        {
            return (results, exceptions, ct) =>
            {
                action(results, exceptions);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Converts an action that processes exceptions into an asynchronous function.
        /// </summary>
        public static Func<IEnumerable<AggregateException>?, CancellationToken, Task> Taskify(Action<IEnumerable<AggregateException>?> action)
        {
            return (exceptions, ct) =>
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
        /// Wraps a Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> into a Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>
        /// </summary>
        public static Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task> AsCancellableFuncTask<TResult>(Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> task)
            => (r, e, ct) => task(r, e);

        /// <summary>
        /// Wraps a Func<Task> into a Func<CancellationToken, Task> to allow for uniform handling of results.
        /// </summary>
        public static Func<CancellationToken, Task> AsCancellableActionTask(Func<Task> task)
            => ct => task();

        /// <summary>
        /// Wraps a Func<IEnumerable<AggregateException>?, Task> into a Func<IEnumerable<AggregateException>?, CancellationToken, Task>
        /// </summary>
        public static Func<IEnumerable<AggregateException>?, CancellationToken, Task> AsCancellableActionTask(Func<IEnumerable<AggregateException>?, Task> task)
            => (e, ct) => task(e);
    }
}
