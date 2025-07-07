using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docodemo.Async.Tasks.Abstractions;

namespace Docodemo.Async.Tasks.Extentions
{
    /*
    internal class AsyncDoorContextBuilderBase<TAsyncDoor>
        where TAsyncDoor : IAsyncDoor, new()
    {
        public AsyncDoorContextBuilderBase()
        {
            // TODO: Impl
        }

        public void Fire()
        {
            // TODO: Impl
        }

        public void Block()
        {
            // TODO: Impl
        }

        public AsyncDoorContextBuilderBase<TAsyncDoor> Timeout(TimeSpan timeout)
        {
            // TODO: Impl
            return this;
        }
        public AsyncDoorContextBuilderBase<TAsyncDoor> WithCancellationToken(int millisecondsTimeout)
        {
            // TODO: Impl
            return this;
        }
    }
    */

    /// <summary>
    /// Represents a base class for building an asynchronous door context for tasks that return a result of type <typeparamref name="TResult"/>.
    /// </summary>
    public class AsyncTaskDoorContextBuilderBase<TResult>
    {
        /// <summary>
        /// Represents the context for the asynchronous door, including cancellation token, semaphore for task completion and so on.
        /// </summary>
        private IAsyncTaskDoorContext<TResult> Context { get; }

        /// <summary>
        /// Represents a collection of asynchronous tasks to be executed.
        /// </summary>
        protected IEnumerable<Func<Task<TResult>>> Tasks { get; }

        /// <summary>
        /// Callback that is invoked when all tasks have been processed.
        /// </summary>
        protected Action<IEnumerable<TResult>, IEnumerable<AggregateException>>? OnAllTasksProcessed { get; }

        /// <summary>
        /// Creates the initial context for the tasks, including cancellation token and semaphore for task completion.
        /// </summary>
        private IAsyncTaskDoorContext<TResult> CreateInitialContext()
        {
            // Create the context for the tasks
            return new AsyncTaskDoorContext<TResult>(
                           cancellationToken : default,
                           isBlocking: true,
                           numTasks: Tasks.Count()
                       );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilderBase{TResult}"/> class with a collection of tasks and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilderBase(IEnumerable<Func<Task<TResult>>> tasks, Action<IEnumerable<TResult>, IEnumerable<AggregateException>>? onAllTasksProcessed = null)
        {
            // Store the snapshot of the tasks
            Tasks = tasks?.ToArray() ?? throw new ArgumentNullException(nameof(tasks), "Tasks cannot be null.");

            // Validate the contents of the tasks
            if (Tasks.Any(task => task == null))
            {
                throw new ArgumentException("Tasks cannot contain null elements.", nameof(tasks));
            }

            // Store the callback for when all tasks are processed
            OnAllTasksProcessed = onAllTasksProcessed;

            // Create the context for the tasks
            Context = CreateInitialContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilderBase{TResult}"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilderBase(Func<Task<TResult>> task, Action<IEnumerable<TResult>, IEnumerable<AggregateException>>? onAllTasksProcessed = null)
        {
            // Validate and store the task
            Tasks = new[] { task ?? throw new ArgumentNullException(nameof(task), "Task cannot be null.") };

            // Create the context for the tasks
            Context = CreateInitialContext();
        }

        /// <summary>
        /// Wraps a Func<Task> into a Func<Task<object?>> to allow for uniform handling of results.
        /// </summary>
        protected static Func<Task<object?>> WrapTask(Func<Task> task)
            => async () => { await task(); return default; };

        /// <summary>
        /// Wraps a Func<Task<TResult>> into a Func<Task<object?>> to allow for uniform handling of results.
        /// </summary>
        protected static Action<IEnumerable<object?>, IEnumerable<AggregateException>> IgnoreResultsCallback(Action<IEnumerable<AggregateException>>? callback)
        {
            return (results, exceptions) => callback?.Invoke(exceptions);
        }

        /// <summary>
        /// Fires the asynchronous door with the context, executing all tasks and invoking the callback when all tasks have been processed.
        /// </summary>
        /// <typeparam name="TAsyncDoor"></typeparam>
        public void Fire<TAsyncDoor>()
            where TAsyncDoor : IAsyncTaskDoor, new()
        {
            // Create an instance of the async door
            var asyncDoor = new TAsyncDoor();
            // Execute the tasks and get the results and exceptions
            var (results, exceptions) = asyncDoor.Investigate(Tasks.ToArray());
            // Invoke the callback if provided
            OnAllTasksProcessed?.Invoke(results, exceptions);
            // Dispose the context if needed
            Context.Dispose();
        }

        /// <summary>
        /// Fires the default asynchronous door with the context, executing all tasks and invoking the callback when all tasks have been processed.
        /// </summary>
        public void Fire()
        {
            Fire<AsyncTaskDoor>();
        }
    }

    /// <summary>
    /// Represents a builder for creating an asynchronous door context for Func<Task> by fluently chaining methods.
    /// </summary>
    public class AsyncTaskDoorContextBuilder : AsyncTaskDoorContextBuilderBase<object?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder"/> class with a collection of tasks and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(IEnumerable<Func<Task>> tasks, Action<IEnumerable<AggregateException>>? onAllTasksProcessed = null)
            : base(
                tasks.Select(WrapTask),
                IgnoreResultsCallback(onAllTasksProcessed)
              )
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(Func<Task> task, Action<IEnumerable<AggregateException>>? onAllTasksProcessed = null)
            : base(
                WrapTask(task),
                IgnoreResultsCallback(onAllTasksProcessed)
              )
        {
            // No additional initialization needed here
        }
    }

    /// <summary>
    /// Represents a builder for creating an asynchronous door context for Func<Task<typeparamref name="TResult"/>> by fluently chaining methods.
    /// </summary>
    public class AsyncTaskDoorContextBuilder<TResult> : AsyncTaskDoorContextBuilderBase<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder{TResult}"/> class with a collection of tasks and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(IEnumerable<Func<Task<TResult>>> tasks, Action<IEnumerable<TResult>, IEnumerable<AggregateException>>? onAllTasksProcessed = null)
            : base(tasks, onAllTasksProcessed)
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder{TResult}"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(Func<Task<TResult>> task, Action<IEnumerable<TResult>, IEnumerable<AggregateException>>? onAllTasksProcessed = null)
            : base(task, onAllTasksProcessed)
        {
            // No additional initialization needed here
        }
    }
}
