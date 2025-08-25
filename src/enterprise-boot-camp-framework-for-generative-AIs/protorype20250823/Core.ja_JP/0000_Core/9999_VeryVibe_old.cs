namespace VeryVibe.Core.ja_JP_old
{
    public interface ISimpleContainer
    {
        T Resolve<T>();
        void Register<TService, TImplementation>() where TImplementation : TService;
    }

    public interface ISimpleNotification
    {
        void Notify(string message);
    }

    public interface ISimpleLogger
    {
        void Log(string message);
    }

    public interface ILocalizationService
    {
        string GetString(string key);
        string GetString(string key, params object[] arguments);
    }

    public interface IChapterArg;
    public interface IChapterCommand : IChapterArg;
    public interface IChapterQuery : IChapterArg;

    public interface IChapterCommandHandler<in TCommand>
        where TCommand : IChapterCommand
    {
        Func<Task> HandleCommand(TCommand command);
    }

    public interface IChapterQueryHandler<out TResult , in TUpstreamResult, in TQuery>
        where TResult : TUpstreamResult
        where TQuery : IChapterQuery
    {
        Func<TUpstreamResult, TResult> HandleQuery(TQuery query);
    }

    public interface IChapterContext
    {
    }

    public interface IChapterContext<TOut> : IChapterContext
    {
        TOut Execute(IChapterQueryContextDispatcher<TOut> dispatcher);
    }


    public interface IChapterCommandContext
        : IChapterContext<Task>
    {
    }

    public interface IChapterQueryContext<TResult>
        : IChapterContext<TResult>
    {
    }

    public interface IChapterQueryContextBuffer<TResult>
    {
        void PushQueryFront(IChapterQueryContext<TResult> context);
        void PushQueryBack(IChapterQueryContext<TResult> context);
    }

    public interface IChapterCommandContextBuffer
    {
        void RunCommand(IChapterCommandContext context);
    }

    public interface IChapterCommandContextDispatcher
    {
        void AwaitAll();
    }

    public interface IChapterQueryContextDispatcher<out TResult>
    {
        TResult RunAll();
    }

    public interface IChapterContextDispatcher<out TResult>
        : IChapterCommandContextDispatcher,
          IChapterQueryContextDispatcher<TResult>
    {
    }

    public sealed class ChapterContextDeque<TQuery, TResult, TCommand>
        : IChapterQueryContextBuffer<TResult>,
          IChapterCommandContextBuffer,
          IChapterContextDispatcher<TResult>
        where TQuery : IChapterQuery
        where TCommand : IChapterCommand
    {
        private readonly object _lockQueueObject = new();
        private readonly LinkedList<IChapterQueryContext<TResult>> _queue = new();
        private readonly IList<Task> tasks = new List<Task>();
        private readonly object _lockTasksObject = new();

        void IChapterQueryContextBuffer<TResult>.PushQueryFront(IChapterQueryContext<TResult> context)
        {
            ArgumentNullException.ThrowIfNull(context);
            lock (_lockQueueObject) _queue.AddFirst(context);
        }

        void IChapterQueryContextBuffer<TResult>.PushQueryBack(IChapterQueryContext<TResult> context)
        {
            ArgumentNullException.ThrowIfNull(context);
            lock (_lockQueueObject) _queue.AddLast(context);
        }

        void IChapterCommandContextBuffer.RunCommand(IChapterCommandContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var task = context.Execute(this);
            lock (_lockTasksObject) tasks.Add(task);
        }

        void IChapterCommandContextDispatcher.AwaitAll()
        {
            throw new NotImplementedException();
        }

        TResult IChapterQueryContextDispatcher<TResult>.RunAll()
        {
            TResult? result = default;
            while (true)
            {
                IChapterContext<TResult>? next;
                lock (_lockQueueObject)
                {
                    if (_queue.Count == 0) return result;
                    next = _queue.First!.Value;
                    _queue.RemoveFirst();
                }
                // Execute outside the lock to allow re-entrancy and new scheduling.
                next.Execute(this);
            }
        }
    }
}


