using System;
using System.Threading;

namespace Docodemo.Async.Tasks.DefaultRunner
{
    /// <summary>
    /// Represents the context for an investigation, including cancellation token and semaphore for task completion.
    /// In this partial file, we implement ConcurrentWriteOnceField, which allows setting a value only once in a thread-safe manner.
    /// </summary>
    public partial class DefaultContext<TResult>
    {
        /// <summary>
        /// A thread-safe field that allows setting a value only once.
        /// </summary>
        protected class ConcurrentWriteOnceField<T>
            where T : notnull
        {
            private static readonly object _unset = new();
            private object? _boxedValue = _unset;

            /// <summary>
            /// Sets the value of the field. If the field is already set, an exception is thrown.
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            public void Set(T? value)
            {
                var boxed = (object?)value ?? DBNull.Value;

                if (Interlocked.CompareExchange(ref _boxedValue, boxed, _unset) != _unset)
                    throw new InvalidOperationException("Value already set.");
            }

            /// <summary>
            /// Gets the value of the field. If the field is not set, an exception is thrown.
            /// </summary>
            public T? Value
            {
                get
                {
                    var current = _boxedValue;
                    if (current == _unset)
                        throw new InvalidOperationException("Value is not set.");

                    return ReferenceEquals(current, DBNull.Value) ? default : (T?)current;
                }
            }

            /// <summary>
            /// Whether the field has been set.
            /// </summary>
            public bool IsSet => !ReferenceEquals(_boxedValue, _unset);
        }
    }
}
