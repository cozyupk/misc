using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docodemo
{
    /// <summary>
    /// Provides methods to wait for the completion of multiple asynchronous tasks synchronously.
    /// </summary>
    public class AsyncDoor : IAsyncDoor
    {
        /// <summary>
        /// Indicates whether the door is blocked until all tasks are completed.
        /// </summary>
        protected bool IsBlocking { get; } = false;

        /// <summary>
        /// The default timeout for the asynchronous tasks, if specified.
        /// </summary>
        protected TimeSpan? DefaultTimeout { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDoor"/> class with an optional default timeout.
        /// </summary>
        public AsyncDoor(bool isBlocking = true, TimeSpan? defaultTimeout = null)
        {
            // Set the blocking behavior of the door
            IsBlocking = isBlocking;

            // Initialize the default timeout if provided
            DefaultTimeout = defaultTimeout;
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks and returns their results and exceptions if any occurred.
        /// </summary>
        public (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions)
            Investigate<T>(params Func<Task<T>>[] asyncTasks)
        {
            // Convert the async tasks to a collection of Func<CancellationToken.
            Func<CancellationToken, Task<T>>[] converted
                = asyncTasks.Select<Func<Task<T>>, Func<CancellationToken, Task<T>>>(
                    f => ct => f()
                ).ToArray();

            // Call the Investigate method with a default CancellationToken
            return Investigate(CancellationToken.None, converted);
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks. If this instance was constructed with a <see cref="DefaultTimeout"/>, 
        /// each task will be subject to that timeout via <see cref="CancellationToken"/>.
        /// </summary>
        public (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions)
            Investigate<T>(params Func<CancellationToken, Task<T>>[] asyncTasks)
        {
            // Check if the DefaultTimeout is set
            if (DefaultTimeout != null)
            {
                // If a DefaultTimeout is specified, use it to create a CancellationTokenSource
                TimeSpan timeout = DefaultTimeout.Value;

                // Call the Investigate method with the effective timeout
                return Investigate(timeout, asyncTasks);
            }

            // Otherwise, call the Investigate method with a default CancellationToken
            return Investigate(CancellationToken.None, asyncTasks);
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks with a specified timeout and returns their results and exceptions if any occurred.
        /// </summary>
        public (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions)
            Investigate<T>(TimeSpan timeout, params Func<CancellationToken, Task<T>>[] asyncTasks)
        {
            // Create a CancellationTokenSource with the specified timeout
            CancellationTokenSource cts = new(timeout);

            // Call the Investigate method with the CancellationToken from the source
            return Investigate(cts.Token, asyncTasks);
        }

        /// <summary>
        /// Represents the context for an investigation, including cancellation token and semaphore for task completion.
        /// </summary>
        internal interface IInvestigationContext<T> : IDisposable
        {
            /// <summary>
            /// A queue to store results of the tasks.
            /// </summary>
            ConcurrentQueue<T> Results { get; }

            /// <summary>
            /// A queue to store exceptions that occurred during the investigation.
            /// </summary>
            ConcurrentQueue<AggregateException> Exceptions { get; }

            /// <summary>
            /// A semaphore that is used to block the investigation until all tasks are completed.
            /// </summary>
            SemaphoreSlim? Semaphore { get; set; }

            /// <summary>
            /// A cancellation token that can be used to cancel the investigation.
            /// </summary>
            CancellationToken CancellationToken { get; }

            /// <summary>
            /// Decrements the number of tasks left to be processed and returns the new count.
            /// Note: this method should thread-safe.
            /// </summary>
            int DecrementNumLeftTasks();
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks with cancellation support and returns their results and exceptions if any occurred.
        /// </summary>
        public (IEnumerable<T> Results, IEnumerable<AggregateException>? Exceptions) 
            Investigate<T>(CancellationToken ct, params Func<CancellationToken, Task<T>>[] asyncTasks)
        {
            // Validate asyncTasks parameter and count number of tasks
            var numTasks = asyncTasks?.Count()
                ?? throw new ArgumentNullException(nameof(asyncTasks));

            // Prepare to run tasks synchronously
            using IInvestigationContext<T> context = new InvestigationContext<T>(ct, IsBlocking, numTasks);
            var exceptions = context.Exceptions;
            var semaphore = context.Semaphore;
            var results = context.Results;

            // Fire each of the tasks
            foreach (var task in asyncTasks)
            {
                try
                {
                    // Fire the task and collect its result or exception
                    SafeFireAndCollectResult(task, context);
                }
                catch (Exception ex)
                {
                    // If an exception occurs while creating the task, we enqueue it
                    // Note: This is a defensive check, as SafeFireAndCollectResult should handle all exceptions.
                    exceptions.Enqueue(new AggregateException(ex));
                    semaphore?.Release();
                }
            }

            // If blocking is enabled, we need to wait for all tasks to complete
            if (semaphore != null)
            {
                // Wait for all async state machines to complete and release the semaphore
                for (int i = 0; i < numTasks; i++)
                {
                    WaitSemaphore(semaphore);
                }
            }

            // Return results and any exceptions that occurred
            // Note: return Enumerable snapshot of results and exceptions to avoid issues with concurrent modifications.
            return (results.ToArray(), exceptions.Any() ? exceptions.ToArray() : null);
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(params Func<Task>[] asyncTasks)
        {
            // Convert to token-aware form
            Func<CancellationToken, Task>[] converted = asyncTasks
                .Select<Func<Task>, Func<CancellationToken, Task>>(f => ct => f())
                .ToArray();

            return Explore(CancellationToken.None, converted);
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results,
        /// using the default timeout if configured.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(params Func<CancellationToken, Task>[] asyncTasks)
        {
            if (DefaultTimeout is { TotalMilliseconds: > 0 })
            {
                return Explore(DefaultTimeout.Value, asyncTasks);
            }

            return Explore(CancellationToken.None, asyncTasks);
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results,
        /// with a specified timeout.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(TimeSpan timeout, params Func<CancellationToken, Task>[] asyncTasks)
        {
            using CancellationTokenSource cts = new(timeout);
            return Explore(cts.Token, asyncTasks);
        }

        /// <summary>
        /// Runs a collection of asynchronous tasks that do not return results,
        /// with cancellation support.
        /// </summary>
        public IEnumerable<AggregateException>? Explore(CancellationToken ct, params Func<CancellationToken, Task>[] asyncTasks)
        {
            // Wrap each task to return dummy result
            Func<CancellationToken, Task<int>>[] converted = asyncTasks
                .Select<Func<CancellationToken, Task>, Func<CancellationToken, Task<int>>>(f => async token =>
                {
                    await f(token);
                    return 0; // Dummy result
                }).ToArray();

            // Discard results, return only exceptions
            var (_, exceptions) = Investigate(ct, converted);
            return exceptions;
        }

        /// <summary>
        /// Processes the result of a completed task.
        /// This method may also serve as a hook that runs after a task completes; it returns the original result by default.
        /// </summary>
        public virtual T TransformResult<T>(T originalResult)
        {
            // This method can be overridden to perform actions on the result of a completed task.
            // By default, it simply returns the original result.
            return originalResult;
        }

        /// <summary>
        /// Called when all tasks have completed.
        /// Note: This method is never called if the door is not blocking mode.
        /// </summary>
        public virtual void OnAllTaskProcessed<T>(IEnumerable<T> results, IEnumerable<AggregateException> exceptions)
        {
            // This method can be overridden to perform actions after all tasks have completed.
            // By default, it does nothing.
        }

        /// <summary>
        /// Enqueues an AggregateException to the provided collection.
        /// </summary>
        protected virtual void EnqueueAggregateException(ConcurrentQueue<AggregateException> exceptions, AggregateException ex)
        {
            // This method can be overridden to customize how exceptions are added
            // to the collection. (ex. to flatten the exceptions, log them, etc.)
            // By default, it simply enqueues the exception.
            // This method can also be used just as a hook of the exception occurrence,
            // in that case, please don't forget to call base.EnqueueAggregateException(exceptions, ex).
            exceptions.Enqueue(ex);
        }

        /// <summary>
        /// Waits for the semaphore to be released.
        /// </summary>
        protected virtual void WaitSemaphore(SemaphoreSlim semaphore)
        {
            // This method can be overridden to customize how the semaphore is waited on.
            // By default, it simply waits for the semaphore to be released. (ex. use timeout, etc.)
            semaphore.Wait();
        }

        /// <summary>
        /// Gets the task continuation options for the tasks being awaited.
        /// </summary>
        protected virtual TaskContinuationOptions GetTaskContinuationOptions()
        {
            // This property can be overridden to customize the task continuation options.
            // By default, it uses the default options.
            return TaskContinuationOptions.None;
        }

        /// <summary>
        /// Sceheduler used for continuations of the tasks being awaited.
        /// </summary>
        /// <returns></returns>
        protected virtual TaskScheduler GetTaskScheduler()
        {
            // This property can be overridden to customize the task scheduler used for continuations.
            // By default, it uses the default task scheduler.
            return TaskScheduler.Default;
        }

        /// <summary>
        /// Represents the context for an investigation, including cancellation token and semaphore for task completion.
        /// </summary>
        internal class InvestigationContext<T> : IInvestigationContext<T>
        {
            /// <summary>
            /// A queue to store results of the tasks.
            /// </summary>
            public ConcurrentQueue<T> Results { get; } = new();

            /// <summary>
            /// A queue to store exceptions that occurred during the investigation.
            /// </summary>
            public ConcurrentQueue<AggregateException> Exceptions { get; } = new();

            /// <summary>
            /// A semaphore that is used to block the investigation until all tasks are completed.
            /// </summary>
            public SemaphoreSlim? Semaphore { get; set; }

            /// <summary>
            /// A cancellation token that can be used to cancel the investigation.
            /// </summary>
            public CancellationToken CancellationToken { get; }

            /// <summary>
            /// Decrements the number of tasks left to be processed and returns the new count.
            /// </summary>
            public int DecrementNumLeftTasks()
            {
                return Interlocked.Decrement(ref NumLeftTasks);
            }

            /// <summary>
            /// The number of tasks that are still left to be processed.
            /// Note: We use field insted of property to use Interlocked operations for thread safety.
            /// </summary>
            private int NumLeftTasks;

            /// <summary>
            /// Initializes a new instance of the <see cref="InvestigationContext"/> class.
            /// </summary>
            public InvestigationContext(CancellationToken ct, bool IsBlocking, int numTasks)
            {
                // store the cancellation token
                CancellationToken = ct;
                // store the number of tasks
                NumLeftTasks = numTasks;
                // If the door is blocking, we create a semaphore to wait for all tasks to complete.
                if (IsBlocking)
                {
                    Semaphore = new(0, numTasks);
                }
            }

            /// <summary>
            /// Disposes the semaphore if it is not null.
            /// </summary>
            public void Dispose()
            {
                Semaphore?.Dispose();
                Semaphore = null;
            }
        }

        /// <summary>
        /// Safely fires an asynchronous task and collects its result or exception.
        /// </summary>
        private void SafeFireAndCollectResult<T>(
            Func<CancellationToken, Task<T>> task,
            IInvestigationContext<T> context
        )
        {
            // This method is responsible for firing the task and collecting its result or exception.
            try
            {
                var ct = context.CancellationToken;
                // Check if the cancellation is requested before starting the task
                if (ct.IsCancellationRequested)
                {
                    // If the cancellation is requested, we throw an OperationCanceledException
                    // to indicate that the operation was canceled.
                    throw new OperationCanceledException(ct);
                }

                // Fire the task and continue processing its result or exception
                task(ct).ContinueWith((Task<T> taskResult) => 
                                    {
                                        // This continuation is called when the task completes.
                                        PostProcessTask(
                                            taskResult, context
                                        );
                                        // Atomically decrement the number of remaining tasks.
                                        // If this is the last task, invoke the final callback.
                                        if (context.DecrementNumLeftTasks() == 0 && context.Semaphore != null)
                                        {
                                            OnAllTaskProcessed(
                                                context.Results.ToList(),
                                                context.Exceptions.ToArray()
                                            );
                                        }
                                    },
                                    ct, GetTaskContinuationOptions(), GetTaskScheduler());
            }
            catch (Exception ex)
            {
                // If an exception occurs while creating the task, we enqueue it
                EnqueueAggregateException(context.Exceptions, new AggregateException(ex));
                context.Semaphore?.Release();
                return;
            }
        }

        /// <summary>
        /// Processes the completed task, handling its result or exception.
        /// </summary>
        private void PostProcessTask<T>(
            Task<T> task, IInvestigationContext<T> context

        ) {
            // This method is called when the task completes.
            try
            {
                var ct = context.CancellationToken;
                if (ct.IsCancellationRequested)
                {
                    // If the cancellation is requested, we throw an OperationCanceledException
                    // to indicate that the operation was canceled.
                    // Note: This is a defensive check, as the task should not complete if cancellation is requested.
                    throw new OperationCanceledException(ct);
                }
                if (task.IsFaulted)
                {
                    // Validate that the task has an exception
                    // Note: This is a defensive check, as IsFaulted should imply an exception exists
                    var ex = task.Exception
                        ?? throw new InvalidOperationException("Task faulted without any exception.");
                    EnqueueAggregateException(context.Exceptions, ex);
                }
                else if (task.IsCanceled)
                {
                    // If the task was canceled, we create a TaskCanceledException
                    throw new TaskCanceledException(task);
                }
                else if (task.Status == TaskStatus.RanToCompletion)
                {
                    // If the task completed successfully, we process its result
                    var result = TransformResult(task.Result);

                    // Check the consistency of the result with the type T
                    if (!IsNullable<T>())
                    {
                        // If T is not nullable, we check if the result is null
                        // Note: Because of for test coverage, we use nested if statement here.
                        if (result == null)
                        {
                            throw new InvalidOperationException($"{nameof(TransformResult)} returned null, but T is not nullable.");
                        }
                    }

                    // Add the result to the results list
                    context.Results.Enqueue(result);
                }
                else
                {
                    // If the task is not faulted or completed, it might be canceled or still running.
                    // This is a defensive check, but in practice, we expect tasks to either complete or fault.
                    throw new InvalidOperationException("Task did not complete or fault as expected.");
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs while processing the task, we enqueue it

                // and release the semaphore to avoid deadlocks.
                EnqueueAggregateException(context.Exceptions, new AggregateException(ex));
            } 
            finally
            {
                // Release the semaphore to signal that this task has finished processing.
                // This applies whether the task completed successfully or an exception was caught and enqueued.
                // If an exception escapes this method, it may cause deadlocks in blocking mode.
                // Always ensure the semaphore is released in all code paths.
                context.Semaphore?.Release();
            }
        }

        /// <summary>
        /// Checks if the type T is nullable.
        /// </summary>
        private static bool IsNullable<T>()
        {
            var type = typeof(T);
            if (!type.IsValueType)
                return true; // Reference types are always nullable

            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}