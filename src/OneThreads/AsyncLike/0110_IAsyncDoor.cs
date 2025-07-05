using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docodemo
{
    public interface IAsyncDoor
    {
        /// <summary>
        /// Runs a collection of asynchronous tasks and returns their results and exceptions if any occurred.
        /// </summary>
        (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions)
            Investigate<T>(params Func<Task<T>>[] asyncTasks);

        /// <summary>
        /// Runs a collection of asynchronous tasks. If this instance was constructed with a <see cref="DefaultTimeout"/>, 
        /// each task will be subject to that timeout via <see cref="CancellationToken"/>.
        /// </summary>
        (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions)
            Investigate<T>(params Func<CancellationToken, Task<T>>[] asyncTasks);

        /// <summary>
        /// Runs a collection of asynchronous tasks with a specified timeout and returns their results and exceptions if any occurred.
        /// </summary>
        (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions)
            Investigate<T>(TimeSpan timeout, params Func<CancellationToken, Task<T>>[] asyncTasks);

        /// <summary>
        /// Runs a collection of asynchronous tasks with cancellation support and returns their results and exceptions if any occurred.
        /// </summary>
        (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions)
            Investigate<T>(CancellationToken ct, params Func<CancellationToken, Task<T>>[] asyncTasks);

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(params Func<Task>[] asyncTasks);

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results,
        /// using the default timeout if configured.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(params Func<CancellationToken, Task>[] asyncTasks);

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results,
        /// with a specified timeout.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(TimeSpan timeout, params Func<CancellationToken, Task>[] asyncTasks);

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results,
        /// with cancellation support.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(CancellationToken ct, params Func<CancellationToken, Task>[] asyncTasks);
    }
}
