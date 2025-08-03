# The Ultimate .NET async/await Visualization Plan (just good old print debugging)

> TL;DR: This repo shows how to visualize thread switching in async/await code — and why you should be careful with `.ContinueWith()`!

## Thread Pool Stirring Process Introduction

### Problem

In C#, you can use `Environment.CurrentManagedThreadId` to get “the managed thread ID of the thread currently executing that code.” By using so-called "printf debugging" (which in practice means `Console.WriteLine`, etc.), you can observe the behavior of asynchronous code.

*(In the following text, we will refer to the "managed thread ID" simply as "thread ID.")*

For example, consider the following code:

```csharp
/// <summary>
/// A dummy asynchronous method. It outputs the thread ID, then awaits with a given label.
/// </summary>
static async Task DummyMethod(ThreadProbe tp, string label)
{
    tp.WriteLineThreadID($"{label} - before await()", 2);
    await Task.Delay(200);
    tp.WriteLineThreadID($"{label} - after await()", 2);
}

/// <summary>
/// Case using ContinueWith and returning only the original Task
/// </summary>
static (string, Func<Task>) Case_AwaitAndReturnOriginalTask(ThreadProbe tp)
{
    var task = Task.Run(() => DummyMethod(tp, "In Task.Run()"));
    task.ContinueWith(t => DummyMethod(tp, "In ContinueWith()"));
    return ("Return only the original Task after ContinueWith", () => task);
}
````

And if we write a method like the following to _execute_ the returned Task from `Case_AwaitAndReturnOriginalTask`:

```csharp
/// <summary>
/// Executes the specified case and outputs the thread states.
/// </summary>
static async Task ExecuteCase(ThreadProbe threadProbe, (string, Func<Task>) caseToExecute)
{
        tp.WriteLineThreadID($"await started: {caseExec.Item1}");
        await caseExec.Item2();
        tp.WriteLineThreadID($"await completed: {caseExec.Item1}");
        await Task.Delay(1000);
        tp.WriteLineThreadID($"post-wait: {caseExec.Item1}");
        Console.WriteLine();
}
```

If we run it like that (“execute” it), we can get output like the following:

```text
[Thread 01: SyncCtx is null] await started: Return only the original Task after ContinueWith
  [Thread 02: SyncCtx is null] In Task.Run() - before await()
  [Thread 02: SyncCtx is null] In Task.Run() - after await()
[Thread 02: SyncCtx is null] await completed: Return only the original Task after ContinueWith
  [Thread 03: SyncCtx is null] In ContinueWith() - before await()
  [Thread 03: SyncCtx is null] In ContinueWith() - after await()
[Thread 03: SyncCtx is null] post-wait: Return only the original Task after ContinueWith
```

As a result, we obtain output like above.

_Note: `WriteLineThreadID` is implemented separately._

However, it won’t necessarily turn out like this every time. The reason is that .NET has a "thread pool" mechanism, meaning threads that have been used are not discarded but **reused**.

Specifically, threads used for asynchronous tasks or background work (like `Task.Run`) are not destroyed when done, but returned to a “thread pool” and may be reused for future tasks.

Because of that, even if a thread switches before and after an `await`, the same thread ID might _appear_ again “by coincidence because it was reused.”  
And the likelihood of that happening is—

> If your program is a simple console app of only a few dozen lines,  
> it’ll happen _extremely_ often!

_(We’ll talk about the behavior of `ContinueWith` later.)_

This leads to the challenge that even if the output thread IDs are the same, you cannot easily tell whether “they ran on the same thread by design” or “they could have run on different threads by design but just happened to use the same thread this time.”

For example, look at the two lines below. You can’t determine whether “they ran on the same thread by design” or “they might run on different threads by design but just happened to run on the same thread this time.” For instance, what if in production code other tasks are also running in parallel? Or if another `Task` completed during that `Task.Delay(200)`?

```text
  [Thread 02: SyncCtx is null] In Task.Run() - before await()
  [Thread 02: SyncCtx is null] In Task.Run() - after await()
```

I want to do something about this problem. We might not achieve “anything that _could_ run on a different thread _always_ prints a different thread ID,” but at least we want to **increase the chance** that “anything that could run on a different thread prints a different thread ID.”

### Implementation Idea

So, we prepare an implementation like this. Simplifying (omitting try/catch and `CancellationToken` handling), it looks like:

```csharp
/// <summary>
/// Stirs the thread pool by creating a burst of tasks that sleep for a certain duration.
/// </summary>
public static async Task StirThreadPoolAsync()
{
    // Run the following in an infinite loop
    while (true)
    {
        // Create tasks that sleep for different durations
        // Task that sleeps 1ms, 2ms, ..., up to 60ms
        var tasks = Enumerable.Range(1, 60).Select(t => Task.Run(() =>
        {
                var sleepMs = t;
                Thread.Sleep(sleepMs);
        })).ToArray();

        // Start all tasks and wait for all to complete
        await Task.WhenAll(tasks);
    }
}
```

The idea is:

> “By intentionally keeping the ThreadPool busy such that the **same thread is less likely to be reused**, we reduce the chance of thread reuse in the user’s code and flush out thread switches.”

If we run a few instances of this in the background (ignoring their returned Tasks) while executing the same code as before, we might get output like below (it won’t necessarily happen every time):

```text
=== When SynchronizationContext.Current is null ===
[Thread 01: SyncCtx is null] await started: Return only the original Task after ContinueWith
  [Thread 02: SyncCtx is null] In Task.Run() - before await()
  [Thread 03: SyncCtx is null] In Task.Run() - after await()
[Thread 03: SyncCtx is null] await completed: Return only the original Task after ContinueWith
  [Thread 03: SyncCtx is null] In ContinueWith() - before await()
  [Thread 04: SyncCtx is null] In ContinueWith() - after await()
[Thread 03: SyncCtx is null] post-wait: Return only the original Task after ContinueWith
```

In other words (in one run on my machine):

**Before introducing StirThreadPoolAsync:**

```text
  [Thread 02: SyncCtx is null] In Task.Run() - before await()
  [Thread 02: SyncCtx is null] In Task.Run() - after await()

  [Thread 03: SyncCtx is null] In ContinueWith() - before await()
  [Thread 03: SyncCtx is null] In ContinueWith() - after await()
```

**After introducing StirThreadPoolAsync:**

```text
  [Thread 02: SyncCtx is null] In Task.Run() - before await()
  [Thread 03: SyncCtx is null] In Task.Run() - after await()

  [Thread 03: SyncCtx is null] In ContinueWith() - before await()
  [Thread 04: SyncCtx is null] In ContinueWith() - after await()
```

So we were able to clearly observe, “Ah, the thread _can_ change before and after `await`.” (In this example, the thing awaited is just a `Task.Delay(200)`.)

`StirThreadPoolAsync` is meant to “flush out as many ‘could-be-different’ cases as possible,” but there are patterns that it still cannot flush out frequently. Therefore, from any observation results, you can conclude “ah, here it _could_ be different,” but you cannot conclude “ah, here it is never different” (except for known behaviors like the custom SynchronizationContext discussed later).

As noted in the output example, the observations in this section were done in an environment where `SynchronizationContext.Current == null` (in other words, a regular console application `async Task Main`, not a WPF UI thread, etc.). Unless explicitly stated otherwise, the rest of this article assumes the same context.

## Observing Task.ContinueWith Behavior

Now, let’s shift perspective a bit and look at the common problem of “I thought I was waiting, but actually I wasn’t” when using `.ContinueWith()`.

Consider code like below:

```csharp
// Pattern A
// Case using ContinueWith and returning only the original Task
async Task PatternA() {
    var task = var task = SomeDummyAsyncMethod(); // Simulated async work (e.g., Task.Delay(200))
    task.ContinueWith(/* continuation Task */);
    return task;
}

// Pattern B
// Case using ContinueWith and returning the Task returned by ContinueWith (Unwrapなし)
async Task PatternB() {
    var task = var task = SomeDummyAsyncMethod(); // Simulated async work (e.g., Task.Delay(200))
    task = task.ContinueWith(/* continuation Task */);
    return task;
}

// Pattern C
// Case using ContinueWith and returning the Task returned by ContinueWith (Unwrapあり)
async Task PatternC() {
    var task = var task = SomeDummyAsyncMethod(); // Simulated async work (e.g., Task.Delay(200))
    task = task.ContinueWith(/* continuation Task */).Unwrap();
    return task;
}
```

For simplicity, let’s assume that inside both the “some Task” and the “continuation Task”, we just do a `Task.Delay(200)`.

If we run this in the same way as before, the logs output will be as follows:

```text
=== When SynchronizationContext.Current is null ===
[Thread 01: SyncCtx is null] await started: Return only the original Task after ContinueWith
  [Thread 02: SyncCtx is null] In Task.Run() - before await()
  [Thread 03: SyncCtx is null] In Task.Run() - after await()
[Thread 03: SyncCtx is null] await completed: Return only the original Task after ContinueWith
  [Thread 03: SyncCtx is null] In ContinueWith() - before await()
  [Thread 04: SyncCtx is null] In ContinueWith() - after await()
[Thread 03: SyncCtx is null] post-wait: Return only the original Task after ContinueWith

[Thread 05: SyncCtx is null] await started: Return the Task returned by ContinueWith (no Unwrap)
  [Thread 05: SyncCtx is null] In Task.Run() - before await()
  [Thread 02: SyncCtx is null] In Task.Run() - after await()
  [Thread 02: SyncCtx is null] In ContinueWith() - before await()
[Thread 02: SyncCtx is null] await completed: Return the Task returned by ContinueWith (no Unwrap)
  [Thread 02: SyncCtx is null] In ContinueWith() - after await()
[Thread 06: SyncCtx is null] post-wait: Return the Task returned by ContinueWith (no Unwrap)

[Thread 06: SyncCtx is null] await started: Return the unwrapped Task from ContinueWith
  [Thread 06: SyncCtx is null] In Task.Run() - before await()
  [Thread 07: SyncCtx is null] In Task.Run() - after await()
  [Thread 07: SyncCtx is null] In ContinueWith() - before await()
  [Thread 08: SyncCtx is null] In ContinueWith() - after await()
[Thread 08: SyncCtx is null] await completed: Return the unwrapped Task from ContinueWith
[Thread 04: SyncCtx is null] post-wait: Return the unwrapped Task from ContinueWith
```

_(In the above, blank lines separate the logs of Pattern A, Pattern B, and Pattern C in order.)_

These three patterns differ, in a word, in **whether the continuation’s Task is properly awaited or not**.

*   In **Pattern A: “Wait only on the original Task”**
    
    *   Only the original `task` is awaited.
        
    *   The continuation runs in a fire-and-forget manner.
        
    *   An `await` inside the continuation is not being waited on by the outside.
        

In fact, you can tell from the logs that the “outer `await`” has completed _before_ the continuation Task even starts (i.e. before it reaches its own first `await`):

```text
[Thread 03: SyncCtx is null] await completed: Return only the original Task after ContinueWith
  [Thread 03: SyncCtx is null] In ContinueWith() - before await()
```

*   In **Pattern B: “Await the Task returned by ContinueWith (without Unwrap)”**
    
    *   `ContinueWith(...)` returns a wrapper `Task<Task>`.
        
    *   When you `await` it, you are only guaranteed to wait until the continuation is _invoked_.
        
    *   However, you _do not wait_ for the `await` inside the continuation to complete.  
        → In other words, “you only wait until the continuation reaches its first `await`.”
        

That is, even though “the Task completed,” it doesn’t necessarily mean the asynchronous work inside it is fully finished.

In fact, from the logs you can see that by the time the continuation Task hits its `await`, the “outer `await`” has already completed:

```text
  [Thread 02: SyncCtx is null] In ContinueWith() - before await()
[Thread 02: SyncCtx is null] await completed: Return the Task returned by ContinueWith (no Unwrap)
```

*   In **Pattern C: “Use Unwrap() to wait for the inner Task as well”**
    
    *   `Unwrap()` flattens the `Task<Task>`, so you properly wait for the inner continuation to finish.
        
    *   If you want correct behavior with async methods in continuations, this is recommended.
        

Here too, from the logs you can see that once the `await` inside the “continuation Task” completes, the “outer `await`” also completes at that same time:

```text
[Thread 08: SyncCtx is null] await completed: Return the unwrapped Task from ContinueWith
[Thread 04: SyncCtx is null] post-wait: Return the unwrapped Task from ContinueWith
```

Usually, this is the behavior one would expect from `Task.ContinueWith`! (Of course, one can’t say there will never be cases where one intentionally does Pattern A/B, but...)

___________

### Conclusion: If you use ContinueWith with async, don’t forget Unwrap!

`.ContinueWith()` is an older continuation model from before `async/await`, and it has the trap of unintentionally returning a `Task<Task>`.

Therefore, if you use an async method in a continuation, using `.Unwrap()` is almost mandatory.

*   Pattern A: The continuation is not awaited; async side-effects happen at indeterminate timing.
    
*   Pattern B: Only the “invocation” of the continuation is awaited.
    
*   Pattern C: The continuation is waited “until it fully finishes” ← **Correct**
    

Conversely, if you use `.ContinueWith()` without `.Unwrap()`, you must accept that “nobody is awaiting the asynchronous content” inside, so to speak.

This is especially problematic in test code or script-like batch processes, because even if you accidentally end up with an `await Task<Task>`, it compiles without error — leading to a bug where “it appears to work, but some async processing is left stranded in the background.”

___________

Incidentally, in modern C# development, using `await` (with sequential or parallel composition) is more common and readable than `.ContinueWith()`, so unless you need a special control flow, it’s generally better to avoid using `.ContinueWith()` at all.

## Summary of this Article

*   In asynchronous operations like `Task.Delay`, there is a possibility that the thread will switch.
    
*   But because of thread pool reuse, sometimes the ID ends up being the same.  
    → So sometimes things _look_ the same thread-wise even when they were actually different.
    
*   `ContinueWith` is a pre-`async/await` era construct and has the trap of returning an unintended `Task<Task>`.  
    → When writing asynchronous continuations, don’t forget `.Unwrap()`!
