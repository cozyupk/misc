using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Extensions for creating and managing asynchronous doors with a fluent interface.
    /// </summary>
    public static class AsyncTaskDoorExtensions
    {
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks,
        Action<IEnumerable<TResult>, IEnumerable<AggregateException>> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, onAllTasksProcessed);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<Task>> tasks,
            Action<IEnumerable<AggregateException>> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, onAllTasksProcessed);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<Task<TResult>> task,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task, onAllTasksProcessed);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<Task> task,
            Action<IEnumerable<AggregateException>> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(task, onAllTasksProcessed);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this IEnumerable<Func<Task>> tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this Func<Task<TResult>> task
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this Func<Task> task
        )
        {
            return new AsyncTaskDoorContextBuilder(task);
        }
    }
}
