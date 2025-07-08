using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Docodemo.Async.Tasks.Extentions.AsyncTaskDoorCallingHelper;

namespace Docodemo.Async.Tasks.Extentions
{
    public static class AsyncTaskDoor
    {
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            params Func<CancellationToken, Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed,
            params Func<CancellationToken, Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            params Func<CancellationToken, Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed,
            params Func<CancellationToken, Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            params Func<CancellationToken, Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            params Func<CancellationToken, Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            params Func<Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed,
            params Func<Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            params Func<Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), onAllTasksProcessedAsync);
        }

        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed,
            params Func<Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), Taskify(onAllTasksProcessed));
        }

        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            params Func<Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask));
        }

        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            params Func<Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask));
        }
    }
}
