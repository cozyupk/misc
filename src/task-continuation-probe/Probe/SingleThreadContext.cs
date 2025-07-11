using System.Collections.Concurrent;

namespace Probe
{
    /// <summary>
    /// A synchronization context that processes callbacks on a single thread.
    /// </summary>
    public sealed class SingleThreadSyncContext : SynchronizationContext
    {
        /// <summary>
        /// A thread-safe collection to hold callbacks and their states to be processed by the single thread.
        /// </summary>
        private BlockingCollection<(SendOrPostCallback, object?)> Queue { get; } = [];

        /// <summary>
        /// The thread that processes the callbacks in the synchronization context.
        /// </summary>
        private Thread Thread { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleThreadSyncContext"/> class.
        /// </summary>
        public SingleThreadSyncContext()
        {
            Thread = new Thread(Run);
            Thread.Start();
        }

        /// <summary>
        /// Posts a callback to be executed on the single thread of this synchronization context.
        /// </summary>
        public override void Post(SendOrPostCallback d, object? state)
        {
            Queue.Add((d, state));
        }

        /// <summary>
        /// Sends a callback to be executed on the single thread of this synchronization context.
        /// </summary>
        private void Run()
        {
            SetSynchronizationContext(this);
            foreach (var (callback, state) in Queue.GetConsumingEnumerable())
            {
                callback(state);
            }
        }

        public void Complete() => Queue.CompleteAdding();
    }
}
