using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Docodemo.Async.Tasks.Extentions.CallingHelper;

namespace Docodemo.Async.Tasks.Extentions
{
    public static class AsyncTaskDoor
    {
        /// <summary>
        /// Registers asynchronous tasks and sets an async callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true,
            params Func<CancellationToken, Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, checkEmptyTasks, onAllTasksProcessedAsync);
        }

        /// <summary>
        /// Registers asynchronous tasks and sets a synchronous callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true,
            params Func<CancellationToken, Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Registers non-result tasks and sets an async callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Func<IEnumerable<AggregateException>?, CancellationToken, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true,
            params Func<CancellationToken, Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, checkEmptyTasks, onAllTasksProcessedAsync);
        }

        /// <summary>
        /// Registers non-result tasks and sets a synchronous callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true,
            params Func<CancellationToken, Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Prepares result-returning asynchronous tasks for execution without setting a completion callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            bool checkEmptyTasks = true,
            params Func<CancellationToken, Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, checkEmptyTasks);
        }

        /// <summary>
        /// Prepares non-result asynchronous tasks for execution without setting a completion callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            bool checkEmptyTasks = true,
            params Func<CancellationToken, Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, checkEmptyTasks);
        }

        /// <summary>
        /// Registers asynchronous tasks (without CancellationToken) and sets an async callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true,
            params Func<Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), checkEmptyTasks, AsCancellableFuncTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Registers asynchronous tasks (without CancellationToken) and sets a synchronous callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true,
            params Func<Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Registers non-result asynchronous tasks (without CancellationToken) and sets an async callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true,
            params Func<Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), checkEmptyTasks, AsCancellableActionTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Registers non-result asynchronous tasks (without CancellationToken) and sets a synchronous callback to be invoked when all tasks have completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true,
            params Func<Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Prepares result-returning asynchronous tasks (without CancellationToken) for execution without setting a completion callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            bool checkEmptyTasks = true,
            params Func<Task<TResult>>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), checkEmptyTasks);
        }

        /// <summary>
        /// Prepares non-result asynchronous tasks (without CancellationToken) for execution without setting a completion callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            bool checkEmptyTasks = true,
            params Func<Task>[] tasks
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), checkEmptyTasks);
        }
    }
}
