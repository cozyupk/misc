using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Docodemo.Async.Tasks.Extentions.CallingHelper;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Extensions for creating and managing asynchronous doors with a fluent interface.
    /// </summary>
    public static class AsyncTaskDoorExtensions
    {
        /// <summary>
        /// Schedules a set of cancellable asynchronous tasks and sets a final callback to run after all complete.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<CancellationToken, Task<TResult>>> tasks,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, checkEmptyTasks, AsCancellableFuncTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Runs cancellable async tasks and executes the specified action once all are done.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<CancellationToken, Task<TResult>>> tasks,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Runs cancellable async tasks without results, invoking the final async callback when finished.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<CancellationToken, Task>> tasks,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, checkEmptyTasks, AsCancellableActionTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Runs cancellable async tasks without results and calls the provided action when all complete.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<CancellationToken, Task>> tasks,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Sets up a single cancellable async task and final callback to be run once completed.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<CancellationToken, Task<TResult>> task,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task, AsCancellableFuncTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Sets up a single cancellable async task and calls the specified action after it finishes.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<CancellationToken, Task<TResult>> task,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Runs a single cancellable async task with a final callback executed after completion.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<CancellationToken, Task> task,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder(task, AsCancellableActionTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Runs a single cancellable async task and calls an action after it completes.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<CancellationToken, Task> task,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(task, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Runs multiple cancellable async tasks that return results, without a final callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this IEnumerable<Func<CancellationToken, Task<TResult>>> tasks,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks, checkEmptyTasks);
        }

        /// <summary>
        /// Runs multiple cancellable async tasks without returning results, without any final callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this IEnumerable<Func<CancellationToken, Task>> tasks,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks, checkEmptyTasks);
        }

        /// <summary>
        /// Runs a single cancellable async task that returns a result, without a final callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this Func<CancellationToken, Task<TResult>> task
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(task);
        }

        /// <summary>
        /// Runs a single cancellable async task without returning a result or specifying a callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this Func<CancellationToken, Task> task
        )
        {
            return new AsyncTaskDoorContextBuilder(task);
        }

        /// <summary>
        /// Schedules non-cancellable async tasks with results and executes a final async callback when all complete.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), checkEmptyTasks, AsCancellableFuncTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Runs non-cancellable async tasks with results and invokes an action when all tasks complete.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Schedules non-cancellable async tasks without results and executes a final async callback when all complete.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<Task>> tasks,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), checkEmptyTasks, AsCancellableActionTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Runs non-cancellable async tasks without results and calls the provided action when all tasks complete.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this IEnumerable<Func<Task>> tasks,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), checkEmptyTasks, Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Schedules a non-cancellable async task with result and a final async callback to run after it completes.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<Task<TResult>> task,
            Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(AsCancellableFuncTask(task), AsCancellableFuncTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Runs a non-cancellable async task with result and calls an action when it finishes.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncHandler<TResult>(
            this Func<Task<TResult>> task,
            Action<IEnumerable<TResult>, IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(AsCancellableFuncTask(task), Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Schedules a non-cancellable async task and invokes a final async callback after it completes.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<Task> task,
            Func<IEnumerable<AggregateException>?, Task> onAllTasksProcessedAsync
        )
        {
            return new AsyncTaskDoorContextBuilder(AsCancellableActionTask(task), AsCancellableActionTask(onAllTasksProcessedAsync));
        }

        /// <summary>
        /// Runs a non-cancellable async task and calls an action after it completes.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncHandler(
            this Func<Task> task,
            Action<IEnumerable<AggregateException>?> onAllTasksProcessed
        )
        {
            return new AsyncTaskDoorContextBuilder(AsCancellableActionTask(task), Taskify(onAllTasksProcessed));
        }

        /// <summary>
        /// Runs non-cancellable async tasks with results, without defining a final callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this IEnumerable<Func<Task<TResult>>> tasks,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(tasks.Select(AsCancellableFuncTask), checkEmptyTasks);
        }

        /// <summary>
        /// Runs non-cancellable async tasks without results or callbacks.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this IEnumerable<Func<Task>> tasks,
            bool checkEmptyTasks = true
        )
        {
            return new AsyncTaskDoorContextBuilder(tasks.Select(AsCancellableActionTask), checkEmptyTasks);
        }

        /// <summary>
        /// Runs a single non-cancellable async task with result and no final callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder<TResult> ToAsyncRunner<TResult>(
            this Func<Task<TResult>> task
        )
        {
            return new AsyncTaskDoorContextBuilder<TResult>(AsCancellableFuncTask(task));
        }

        /// <summary>
        /// Runs a single non-cancellable async task without result or final callback.
        /// </summary>
        public static AsyncTaskDoorContextBuilder ToAsyncRunner(
            this Func<Task> task
        )
        {
            return new AsyncTaskDoorContextBuilder(AsCancellableActionTask(task));
        }
    }
}