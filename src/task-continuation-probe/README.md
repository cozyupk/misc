Thread Interleaving Visualization for .NET Async/Await
------------------------------------------------------

This repository provides a set of diagnostic examples to help **visualize the behavior of async/await and Task continuation in .NET**, especially in relation to **thread switching** and the **pitfalls of `.ContinueWith()`**.

🧪 Want to see how your `await` really behaves? Wondering why your continuations don’t seem to wait?  
This code helps demystify that – with good old **printf-style debugging**.

### 📄 Related Article (Japanese)

The code introduced here is explained in detail in the following Japanese article:  
👉 Qiita Article [さいきょうの .NET async/await 見える化計画（なお printf デバッグのもよう）](https://qiita.com/cozyupk/items/50bfa7e5ba6d6bf5121e)

___________

What's in This?
---------------

*   Examples showing how `Task.Delay()` can switch threads — even when it doesn't look like it.
    
*   Investigation of `.ContinueWith()`:
    
    *   Why it often **doesn't wait** for your async continuation.
        
    *   The difference between `ContinueWith(...).Unwrap()` and not using `Unwrap()`.
        
*   A **ThreadPool stirring** utility that increases the chance of thread switching — super useful for debugging.
    
*   Output like this:
    
    ```
    [Thread 03] In ContinueWith - before await()
    [Thread 04] In ContinueWith - after await()
    ```
    

### Why Thread IDs Can Be Misleading

Because of the .NET **ThreadPool**, the same thread ID can be reused across tasks.  
So even if `await` switches context under the hood, the logged thread ID may _look_ identical — creating false assumptions.

To address this, we simulate a busy ThreadPool to **force reuse to be less likely**, helping to better **observe real thread transitions**.

___________

Caution About `.ContinueWith()`
-------------------------------

`.ContinueWith()` predates `async/await`. If you pass an async lambda into it, it returns a `Task<Task>` — a **nested task**.  
Unless you explicitly `Unwrap()`, you'll only be waiting for the outer task to fire the continuation — **not for the async logic inside**.

```csharp
task.ContinueWith(...);         // ❌ May not wait correctly
task.ContinueWith(...).Unwrap(); // ✅ Correct way to await async continuations
```

___________

Want to Go Deeper?
------------------

In a future article, we’ll be diving into:

> 💡 What does `ConfigureAwait(false)` really do — and when does it matter?

Stay tuned!  
Or better yet, explore the [ConfigureAwait](https://github.com/cozyupk/misc/tree/main/src/task-continuation-probe) experiments already included in this repo 😉
