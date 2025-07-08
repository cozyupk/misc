using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docodemo.Async.Tasks.Abstractions;
using Docodemo.Async.Tasks.DefaultDoor;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Represents a base class for building an asynchronous door context for tasks that return a result of type <typeparamref name="TResult"/>.
    /// </summary>
    public class AsyncTaskDoorContextBuilderBase<TResult>
    {
        /// <summary>
        /// Represents the context for the asynchronous door, including cancellation token, semaphore for task completion and so on.
        /// </summary>
        protected IAsyncTaskDoorBuilderContext<TResult>? Context { get; private set; }

        /// <summary>
        /// Represents a lock object to ensure thread safety when accessing the context.
        /// </summary>
        protected object ContextLock { get; } = new object();

        /// <summary>
        /// Re-entrant guard to prevent nested calls to LetThemGo and ShallWeGo methods within the same context.
        /// </summary>
        protected int _isRunning = 0;            // 0: idle, 1: running

        /// <summary>
        /// Represents a collection of asynchronous tasks to be executed.
        /// </summary>
        protected IEnumerable<Func<CancellationToken, Task<TResult>>> Tasks { get; }

        /// <summary>
        /// Callback that is invoked when all tasks have been processed.
        /// </summary>
        protected Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task>? OnAllTasksProcessedAsync { get; }

        /// <summary>
        /// Creates the initial context for the tasks, including cancellation token and semaphore for task completion.
        /// </summary>
        protected IAsyncTaskDoorBuilderContext<TResult> CreateInitialContext<TAsyncTaskDoorContext>()
            where TAsyncTaskDoorContext : IAsyncTaskDoorBuilderContext<TResult>, new()
        {
            // Create the context for the tasks
            return new TAsyncTaskDoorContext();
        }

        /// <summary>
        /// Creates the initial context for the tasks using the default implementation of <see cref="IAsyncTaskDoorRunnerContext{TResult}"/>.
        /// </summary>
        protected virtual IAsyncTaskDoorBuilderContext<TResult> CreateInitialContext()
        {
            // Use the default context implementation
            return CreateInitialContext<DefaultAsyncTaskDoorContext<TResult>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilderBase{TResult}"/> class with a collection of tasks and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilderBase(IEnumerable<Func<CancellationToken, Task<TResult>>> tasks, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task>? onAllTasksProcessedAsync = null)
        {
            // Store the snapshot of the tasks
            Tasks = tasks?.ToArray() ?? throw new ArgumentNullException(nameof(tasks), "Tasks cannot be null.");

            // Validate the contents of the tasks
            if (Tasks.Any(task => task == null))
            {
                throw new ArgumentException("Tasks cannot contain null elements.", nameof(tasks));
            }

            // Store the callback for when all tasks are processed
            OnAllTasksProcessedAsync = onAllTasksProcessedAsync;

            // Create the context for the tasks
            Context = CreateInitialContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilderBase{TResult}"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilderBase(Func<CancellationToken, Task<TResult>> task, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task>? onAllTasksProcessedAsync = null)
        {
            // Validate and store the task
            Tasks = new[] { task ?? throw new ArgumentNullException(nameof(task), "Task cannot be null.") };

            // Store the callback for when all tasks are processed
            OnAllTasksProcessedAsync = onAllTasksProcessedAsync;

            // Create the context for the tasks
            Context = CreateInitialContext();
        }

        /// <summary>
        /// Invokes the asynchronous door to execute all tasks and returns the results and exceptions.
        /// </summary>
        protected (IEnumerable<TResult>, IEnumerable<AggregateException>?) InvokeDoor<TAsyncTaskDoor>(bool isBlockingMode)
            where TAsyncTaskDoor : IAsyncTaskDoor, new()
        {
            // ───── Re-entrant guard ─────
            if (Interlocked.Exchange(ref _isRunning, 1) == 1)
                throw new InvalidOperationException("Re-entrant Request detected.");

            IEnumerable<TResult> results;
            IEnumerable<AggregateException>? exceptions;

            // Ensure the context is initialized
            lock (ContextLock)
            {
                try
                {
                    // Ensure the context is not yet Disposed
                    if (Context == null)
                    {
                        throw new InvalidOperationException("The context is already disposed.");
                    }

                    // Create an instance of the async door
                    var asyncDoor = new TAsyncTaskDoor();

                    // Cast the context for runner side
                    if (Context is not IAsyncTaskDoorRunnerContext<TResult> crs)
                    {
                        throw new InvalidOperationException(
                            $"The context must implement {nameof(IAsyncTaskDoorRunnerContext<TResult>)} interface.");
                    }
                    // Execute the tasks and get the results and exceptions
                    (results, exceptions) = asyncDoor.Invoke(crs, isBlockingMode, Tasks.ToArray());
                }
                finally
                {
                    // The context will disposed by runner side, so we just set it to null here.
                    Context = null;

                    // Reset the re-entrant guard
                    _ = Interlocked.Exchange(ref _isRunning, 0);
                }
            }

            // return the results and exceptions
            return (results, exceptions);
        }

        /// <summary>
        /// Starts executing all tasks through the asynchronous door.  
        /// This method does not propagate exceptions.  
        /// If you need to handle results or exceptions, use the OnAllTasksProcessed callback.
        /// </summary>
        protected void LetThemGo<TAsyncTaskDoor>()
            where TAsyncTaskDoor : IAsyncTaskDoor, new()
        {
            lock(ContextLock)
            {
                // TODO: Make the context let it mode.

                // Open the async door safely and return the results and exceptions
                _ = InvokeDoor<TAsyncTaskDoor>(false);
            }
        }

        /// <summary>
        /// Using the default AsyncTaskDoor implementation, let tasks go the asynchronous door, executing all tasks and returning the results and exceptions.
        /// </summary>
        public virtual void LetThemGo()
        {
            LetThemGo<DefaultAsyncTaskDoor>();
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
        public AsyncTaskDoorContextBuilder(IEnumerable<Func<CancellationToken, Task>> tasks, Func<IEnumerable<AggregateException>?, Task>? onAllTasksProcessedAsync = null)
            : base(
                tasks.Select(WrapTask),
                onAllTasksProcessedAsync != null ? IgnoreResultsCallback(onAllTasksProcessedAsync) : null
              )
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(Func<CancellationToken, Task> task, Func<IEnumerable<AggregateException>?, Task>? onAllTasksProcessedAsync = null)
            : base(
                WrapTask(task),
                onAllTasksProcessedAsync != null ? IgnoreResultsCallback(onAllTasksProcessedAsync) : null
              )
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Wraps a Func<Task> into a Func<Task<object?>> to allow for uniform handling of results.
        /// </summary>
        protected static Func<CancellationToken, Task<object?>> WrapTask(Func<CancellationToken, Task> task)
            => async (ct) => {
                                await task(ct);
                                return default; // default(object?)
                             };

        /// <summary>
        /// Wraps a Func<Task> into a Func<Task<object?>> to allow for uniform handling of results.
        /// </summary>
        protected static Func<Task<object?>> WrapTask(Func<Task> task)
            => async () => { await task(); return default; };

        /// <summary>
        /// Wraps a Func<Task<TResult>> into a Func<Task<object?>> to allow for uniform handling of results.
        /// </summary>
        protected static Func<IEnumerable<object?>, IEnumerable<AggregateException>?, Task> IgnoreResultsCallback(Func<IEnumerable<AggregateException>?, Task> callback)
        {
            return (_, exceptions) => callback.Invoke(exceptions);
        }

        /// <summary>
        /// Blocks the current thread and walks with all tasks until they are done.
        /// Returns the collected exceptions, if any.
        /// </summary>
        protected IEnumerable<AggregateException>? ShallWeGo<TAsyncTaskDoor>()
            where TAsyncTaskDoor : IAsyncTaskDoor, new()
        {
            lock (ContextLock)
            {
                // TODO: Make the context shall we mode.

                // Open the async door safely and return the results and exceptions
                return InvokeDoor<TAsyncTaskDoor>(true).Item2;
            }
        }

        /// <summary>
        /// Takes the default path and waits for all tasks to complete before returning.
        /// Returns any exceptions that occurred during the journey.
        /// </summary>
        public virtual IEnumerable<AggregateException>? ShallWeGo()
        {
            return ShallWeGo<DefaultAsyncTaskDoor>();
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
        public AsyncTaskDoorContextBuilder(IEnumerable<Func<CancellationToken, Task<TResult>>> tasks, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task>? onAllTasksProcessedAsync = null)
            : base(tasks, onAllTasksProcessedAsync)
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder{TResult}"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(Func<CancellationToken, Task<TResult>> task, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, Task>? onAllTasksProcessedAsync = null)
            : base(task, onAllTasksProcessedAsync)
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Blocks the current thread until all tasks are completed, returning the results and exceptions.
        /// </summary>
        protected (IEnumerable<TResult>, IEnumerable<AggregateException>?) ShallWeGo<TAsyncTaskDoor>()
            where TAsyncTaskDoor : IAsyncTaskDoor, new()
        {
            lock (ContextLock)
            {
                // TODO: Make the context blocking mode.

                // Open the async door safely and return the results and exceptions
                return InvokeDoor<TAsyncTaskDoor>(true);
            }
        }

        /// <summary>
        /// Using the default AsyncTaskDoor implementation, blocks the current thread until all tasks are completed, returning the exceptions.
        /// </summary>
        public virtual (IEnumerable<TResult>, IEnumerable<AggregateException>?) ShallWeGo()
        {
            return ShallWeGo<DefaultAsyncTaskDoor>();
        }
    }
}
