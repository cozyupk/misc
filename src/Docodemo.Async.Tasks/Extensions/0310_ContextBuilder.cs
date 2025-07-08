using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docodemo.Async.Tasks.Abstractions;
using Docodemo.Async.Tasks.DefaultRunner;

namespace Docodemo.Async.Tasks.Extentions
{
    /// <summary>
    /// Represents a base class for building an asynchronous door context for tasks that return a result of type <typeparamref name="TResult"/>.
    /// </summary>
    public class ContextBuilder<TResult>
    {
        /// <summary>
        /// Represents the context for the asynchronous door, including cancellation token, semaphore for task completion and so on.
        /// </summary>
        protected IBuilderContext<TResult>? Context { get; private set; }

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
        /// Creates the initial context for the tasks, including cancellation token and semaphore for task completion.
        /// </summary>
        protected IBuilderContext<TResult> CreateInitialContext<TAsyncTaskDoorContext>()
            where TAsyncTaskDoorContext : IBuilderContext<TResult>, new()
        {
            // Create the context for the tasks
            return new TAsyncTaskDoorContext();
        }

        /// <summary>
        /// Creates the initial context for the tasks using the default implementation of <see cref="IRunnerContext{TResult}"/>.
        /// </summary>
        protected virtual IBuilderContext<TResult> CreateInitialContext()
        {
            // Use the default context implementation
            return CreateInitialContext<DefaultContext<TResult>>();
        }

        protected void CreateAndInitializaContext(Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? asyncTask)
        {
            lock (ContextLock)
            {
                // Create the context for the tasks
                Context = CreateInitialContext();

                // Store the callback for when all tasks are processed
                Context.SetOnAllTasksProcessedAsync(
                    asyncTask
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextBuilder{TResult}"/> class with a collection of tasks and an optional callback for when all tasks are processed.
        /// </summary>
        public ContextBuilder(IEnumerable<Func<CancellationToken, Task<TResult>>> tasks, bool checkEmptyTasks, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? onAllTasksProcessedAsync = null)
        {
            // Store the snapshot of the tasks
            Tasks = tasks?.ToArray() ?? throw new ArgumentNullException(nameof(tasks), "Tasks cannot be null.");

            // Validate the tasks collection if required
            if (checkEmptyTasks && !Tasks.Any())
            {
                throw new ArgumentException("No tasks were provided. You can bypass this check by setting checkEmptyTasks: false.", nameof(tasks));
            }

            // Validate the contents of the tasks
            if (Tasks.Any(task => task == null))
            {
                throw new ArgumentException("Tasks cannot contain null elements.", nameof(tasks));
            }

            // Create the context for the tasks and set the callback for when all tasks are processed
            CreateAndInitializaContext(onAllTasksProcessedAsync);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextBuilder{TResult}"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public ContextBuilder(Func<CancellationToken, Task<TResult>> task, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? onAllTasksProcessedAsync = null)
        {
            // Validate and store the task
            Tasks = new[] { task ?? throw new ArgumentNullException(nameof(task), "Task cannot be null.") };

            // Create the context for the tasks and set the callback for when all tasks are processed
            CreateAndInitializaContext(onAllTasksProcessedAsync);
        }

        /// <summary>
        /// Invokes the asynchronous door to execute all tasks and returns the results and exceptions.
        /// </summary>
        protected (IEnumerable<TResult>, IEnumerable<AggregateException>?) InvokeDoor<TAsyncTaskDoor>(bool isBlockingMode)
            where TAsyncTaskDoor : IRunner, new()
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
                    if (Context is not IRunnerContext<TResult> crs)
                    {
                        throw new InvalidOperationException(
                            $"The context must implement {nameof(IRunnerContext<TResult>)} interface.");
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
            where TAsyncTaskDoor : IRunner, new()
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
            LetThemGo<DefaultRunner.DefaultRunner>();
        }
    }

    /// <summary>
    /// Represents a builder for creating an asynchronous door context for Func<Task> by fluently chaining methods.
    /// </summary>
    public class AsyncTaskDoorContextBuilder : ContextBuilder<object?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder"/> class with a collection of tasks and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(IEnumerable<Func<CancellationToken, Task>> tasks, bool checkEmptyTasks, Func<IEnumerable<AggregateException>?, CancellationToken, Task>? onAllTasksProcessedAsync = null)
            : base(
                tasks.Select(WrapTask),
                checkEmptyTasks,
                onAllTasksProcessedAsync != null ? IgnoreResultsCallback(onAllTasksProcessedAsync) : null
              )
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(Func<CancellationToken, Task> task, Func<IEnumerable<AggregateException>?, CancellationToken, Task>? onAllTasksProcessedAsync = null)
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
        protected static Func<IEnumerable<object?>, IEnumerable<AggregateException>?, CancellationToken, Task> IgnoreResultsCallback(Func<IEnumerable<AggregateException>?, CancellationToken, Task> callback)
        {
            return (_, exceptions, ct) => callback.Invoke(exceptions, ct);
        }

        /// <summary>
        /// Blocks the current thread and walks with all tasks until they are done.
        /// Returns the collected exceptions, if any.
        /// </summary>
        protected IEnumerable<AggregateException>? ShallWeGo<TAsyncTaskDoor>()
            where TAsyncTaskDoor : IRunner, new()
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
            return ShallWeGo<DefaultRunner.DefaultRunner>();
        }
    }

    /// <summary>
    /// Represents a builder for creating an asynchronous door context for Func<Task<typeparamref name="TResult"/>> by fluently chaining methods.
    /// </summary>
    public class AsyncTaskDoorContextBuilder<TResult> : ContextBuilder<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder{TResult}"/> class with a collection of tasks and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(IEnumerable<Func<CancellationToken, Task<TResult>>> tasks, bool checkEmptyTasks, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? onAllTasksProcessedAsync = null)
            : base(tasks, checkEmptyTasks, onAllTasksProcessedAsync)
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTaskDoorContextBuilder{TResult}"/> class with a single task and an optional callback for when all tasks are processed.
        /// </summary>
        public AsyncTaskDoorContextBuilder(Func<CancellationToken, Task<TResult>> task, Func<IEnumerable<TResult>, IEnumerable<AggregateException>?, CancellationToken, Task>? onAllTasksProcessedAsync = null)
            : base(task, onAllTasksProcessedAsync)
        {
            // No additional initialization needed here
        }

        /// <summary>
        /// Blocks the current thread until all tasks are completed, returning the results and exceptions.
        /// </summary>
        protected (IEnumerable<TResult>, IEnumerable<AggregateException>?) ShallWeGo<TAsyncTaskDoor>()
            where TAsyncTaskDoor : IRunner, new()
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
            return ShallWeGo<DefaultRunner.DefaultRunner>();
        }
    }
}
