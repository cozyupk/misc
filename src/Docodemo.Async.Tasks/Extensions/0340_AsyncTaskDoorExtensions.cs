using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Docodemo.Async.Tasks.Extentions.AsyncTaskDoorCallingHelper;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Extensions for creating and managing asynchronous doors with a fluent interface.
    /// </summary>
    public static class AsyncTaskDoorExtensions
    {
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<CancellationToken, Task<TResult>>> tasks,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<CancellationToken, Task<TResult>>> tasks,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<CancellationToken, Task>> tasks,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<CancellationToken, Task>> tasks,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<CancellationToken, Task<TResult>> task,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task, onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<CancellationToken, Task<TResult>> task,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task, Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<CancellationToken, Task> task,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder(task, onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<CancellationToken, Task> task,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(task, Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this IEnumerable<Func<CancellationToken, Task<TResult>>> tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this IEnumerable<Func<CancellationToken, Task>> tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this Func<CancellationToken, Task<TResult>> task
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this Func<CancellationToken, Task> task
        )
        {
            return new AsyncTaskDoorContextBuilder(task);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<Task>> tasks,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), onAllTasksProcessedAsync);
        }
        
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<Task>> tasks,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<Task<TResult>> task,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(AsCancellableFuncTask(task), onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<Task<TResult>> task,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(AsCancellableFuncTask(task), Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<Task> task,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder(AsCancellableActionTask(task), onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<Task> task,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(AsCancellableActionTask(task), Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this IEnumerable<Func<Task>> tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask));
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this Func<Task<TResult>> task
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(AsCancellableFuncTask(task));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this Func<Task> task
        )
        {
            return new AsyncTaskDoorContextBuilder(AsCancellableActionTask(task));
        }
    }
}
