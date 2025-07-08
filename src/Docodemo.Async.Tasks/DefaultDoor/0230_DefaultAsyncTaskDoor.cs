using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docodemo.Async.Tasks.Abstractions;

namespace Docodemo.Async.Tasks.DefaultDoor
{
    /// <summary>
    /// Provides methods to wait for the completion of multiple asynchronous tasks synchronously.
    /// </summary>
    public class DefaultAsyncTaskDoor : IAsyncTaskDoor
    {
        public (IEnumerable<TResult> Results, IEnumerable<AggregateException>? Exceptions) Invoke<TResult>(
            IAsyncTaskDoorRunnerContext<TResult> context,
            bool isBlockingMode,
            params Func<CancellationToken, Task<TResult>>[] asyncTasks)
        {
            // Validate asyncTasks parameter and count number of tasks
            var numTasks = asyncTasks?.Length
                ?? throw new ArgumentNullException(nameof(asyncTasks));
            context.SetNumLeftTasks(numTasks);

            // Treat the case of blocking mode
            if (isBlockingMode)
            {
                // If blocking mode is enabled, we need to create a semaphore
                // to block the current thread until all tasks are completed.
                context.Semaphore = new SemaphoreSlim(0, numTasks);
            }

            // Prepare to run tasks synchronously
            var exceptions = context.Exceptions;
            var semaphore = context.Semaphore;
            var results = context.Results;

            // Let each of the tasks go
            foreach (var task in asyncTasks)
            {
                try
                {
                    // Let the tasks go and collect its result or exception
                    SafeLetItGoAndCollectResult(task, context);
                }
                catch (Exception ex)
                {
                    // If an exception occurs while creating the task, we enqueue it
                    // Note: This is a defensive check, as SafeLetItGoAndCollectResult should handle all exceptions.
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
        /// Safely let an asynchronous task go and collects its result or exception.
        /// </summary>
        private void SafeLetItGoAndCollectResult<T>(
            Func<CancellationToken, Task<T>> task,
            IAsyncTaskDoorRunnerContext<T> context
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
                task(ct).ContinueWith((taskResult) => 
                                    {
                                        // This continuation is called when the task completes.
                                        PostProcessTask(
                                            taskResult, context
                                        );
                                        // Atomically decrement the number of remaining tasks.
                                        // If this is the last task, invoke the final callback.
                                        if (context.DecrementNumLeftTasks() == 0)
                                        {
                                            OnAllTasksProcessed(
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
            Task<T> task, IAsyncTaskDoorRunnerContext<T> context

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