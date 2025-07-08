using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docodemo.Async.Tasks.Abstractions;

namespace Docodemo.Async.Tasks.DefaultRunner
{
    /// <summary>
    /// Provides methods to wait for the completion of multiple asynchronous tasks synchronously.
    /// </summary>
    public class DefaultRunner : IRunner
    {
        public (IEnumerable<TResult> Results, IEnumerable<AggregateException>? Exceptions) Invoke<TResult>(
            IRunnerContext<TResult> context,
            bool isBlockingMode,
            params Func<CancellationToken, Task<TResult>>[] asyncTasks)
        {
            // Validate asyncTasks parameter and count number of tasks
            var numTasks = asyncTasks?.Length
                ?? throw new ArgumentNullException(nameof(asyncTasks));

            // Treat the case of blocking mode
            SemaphoreSlim? semaphoreForEachTask = null;
            if (isBlockingMode)
            {
                // If blocking mode is enabled, we need to create a semaphore
                // to block the current thread until all tasks are completed.
                semaphoreForEachTask= new SemaphoreSlim(0, numTasks);
            }

            // Prepare to run tasks synchronously
            var exceptions = context.Exceptions;
            var results = context.Results;

            // Let each of the tasks go
            foreach (var task in asyncTasks)
            {
                try
                {
                    // Let the tasks go and collect its result or exception
                    ScheduleAndTrackAsyncTask(task, semaphoreForEachTask, context);
                }
                catch (Exception ex)
                {
                    // If an exception occurs while creating the task, we enqueue it
                    // Note: This is a defensive check, as ScheduleAndTrackAsyncTask should handle all exceptions.
                    exceptions.Enqueue(new AggregateException(ex));
                    semaphoreForEachTask?.Release();
                }
            }

            // If blocking is enabled, we need to wait for all tasks to complete
            if (semaphoreForEachTask != null)
            {
                // Wait for all async state machines to complete and release the semaphore
                for (int i = 0; i < numTasks; i++)
                {
                    WaitSemaphore(semaphoreForEachTask, context.CancellationToken);
                }
            }

            // Get snapshot of results and exceptions
            var resultsSnapshot = results.ToArray();
            var exceptionsSnapshot = exceptions.ToArray();
            var hasExceptions = exceptionsSnapshot.Length > 0;
            exceptionsSnapshot = hasExceptions
                ? exceptionsSnapshot
                : null; // If no exceptions, set to null for clarity

            // Finally, we need to call and wait for the OnAllTasksProcessedAsync method, if it is defined.
            var semaphoreForFinalization = context.OnAllTasksProcessedAsync;
            if (semaphoreForFinalization != null)
            {
                // Using a semaphore to emulate await-style continuation and ensure the current thread waits
                // for the async callback to finish, without relying on GetAwaiter().GetResult().
                var semaphoreForAllTasksProcessedAsync = new SemaphoreSlim(0, 1);
                Task.CompletedTask.ContinueWith(async t =>
                {
                    try
                    {
                        // Call the OnAllTasksProcessedAsync method to process the results and exceptions
                        await semaphoreForFinalization(resultsSnapshot, exceptionsSnapshot, context.CancellationToken)
                            .ContinueWith(_ => semaphoreForAllTasksProcessedAsync.Release(), context.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Sorry, we cannot enqueue exceptions here, so re-throw it to the caller.
                        throw new AggregateException(
                            "Unhandled exception occurred during post-processing of task result.",
                            ex
                        );
                    }
                    finally
                    {
                        // Release the semaphore to signal that all tasks have been processed.
                        semaphoreForAllTasksProcessedAsync.Release();
                    }
                }, context.CancellationToken, context.TaskContinuationOptions, context.TaskScheduler);
                semaphoreForAllTasksProcessedAsync.WaitAsync(context.CancellationToken).GetAwaiter().GetResult();
            }

            // Return results and any exceptions that occurred
            // Note: return Enumerable snapshot of results and exceptions to avoid issues with concurrent modifications.
            return (resultsSnapshot, exceptionsSnapshot);
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
        /// </summary>
        public virtual void OnAllTasksProcessed<T>(IEnumerable<T> results, IEnumerable<AggregateException> exceptions)
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

            // The policy for flattening nested exceptions is currently under review.
            exceptions.Enqueue(ex);
        }

        /// <summary>
        /// Waits for the semaphore to be released.
        /// </summary>
        protected virtual void WaitSemaphore(SemaphoreSlim semaphore, CancellationToken ct)
        {
            // This method can be overridden to customize how the semaphore is waited on.
            // By default, it simply waits for the semaphore to be released. (ex. use timeout, etc.)
            semaphore.WaitAsync(ct).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Safely let an asynchronous task go and collects its result or exception.
        /// </summary>
        private void ScheduleAndTrackAsyncTask<T>(
            Func<CancellationToken, Task<T>> task,
            SemaphoreSlim? semaphoreForEachTask,
            IRunnerContext<T> context
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

                // Let the task go and continue processing its result or exception
                _ = task(ct).ContinueWith(async (taskResult) =>
                                    {
                                        try
                                        {
                                            // This continuation is called when the task completes.
                                            await RunPostProcessTaskAsync(
                                                taskResult, context
                                            );
                                        }
                                        catch (Exception ex)
                                        {
                                            // If an exception occurs while processing the task, we enqueue it
                                            // and release the semaphore to avoid deadlocks.
                                            EnqueueAggregateException(
                                                context.Exceptions,
                                                new AggregateException(
                                                    // TODO: Adjust message based on the ContextBuilder
                                                    "Exception thrown while processing postProcessTask", 
                                                    ex
                                                )
                                            );
                                        } finally
                                        {
                                            semaphoreForEachTask?.Release();
                                        }
                                    },
                                    ct,
                                    context.TaskContinuationOptions,
                                    context.TaskScheduler);
            }
            catch (Exception ex)
            {
                // If an exception occurs while creating the task, we enqueue it
                EnqueueAggregateException(context.Exceptions, new AggregateException(ex));
                semaphoreForEachTask?.Release();
            }
        }

        /// <summary>
        /// Processes the completed task, handling its result or exception.
        /// </summary>
        private Task RunPostProcessTaskAsync<T>(
            Task<T> task, IRunnerContext<T> context
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
                    // Note: We assume that the task has a result of type T, so if T is nullable, task.Result can be null.
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

            // Placeholder implementation: no actual asynchronous logic is run here yet.
            // Intended to be replaced by a context-driven async hook in future extensions.
            return Task.CompletedTask;
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