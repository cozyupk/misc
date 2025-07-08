using System.Threading;
using Docodemo.Async.Tasks.Abstractions;

namespace Docodemo.Async.Tasks.DefaultDoor
{
	/// <summary>
	/// Represents the context for an investigation, including cancellation token and semaphore for task completion.
	/// In this partial file, we implement the <see cref="IAsyncTaskDoorBuilderContext{TResult}"/> interface.
	/// </summary>
	partial class DefaultAsyncTaskDoorContext<TResult>
		: IAsyncTaskDoorBuilderContext<TResult>
	{
        // TODO: Detect duplicated setting for the attributes.
        // TODO: Consider thread-safety.

	}
}
