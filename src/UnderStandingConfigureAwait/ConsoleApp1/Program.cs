using Probe;

namespace ConsoleApp1
{
    /// <summary>
    /// ContinueWith と ConfigureAwait の挙動を観察する
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main メソッドはアプリケーションのエントリポイントです。(commented by Copilot)
        /// </summary>
        static async Task Main(string[] _1)
        {
            // 「スレッド攪拌処理」を動かしておく
            CancellationTokenSource cts = new();
            _ = ThreadProbe.StirThreadPoolAsync(cts.Token);
            _ = ThreadProbe.StirThreadPoolAsync(cts.Token);
            _ = ThreadProbe.StirThreadPoolAsync(cts.Token);

            Console.WriteLine();

            // 以下、SyncronizationContext.Current が null 前提の実行
            if (SynchronizationContext.Current != null)
            {
                throw new InvalidOperationException(
                    "This program is designed to run in a context where SynchronizationContext.Current is null. " +
                    "Please ensure you are running it in a console application or similar environment.");
            }

            // スレッドの状態を観察するためのインスタンスを作成
            var threadProbe = new ThreadProbe();

            // ContinueWith と ConfigureAwait の挙動を観察するケースを実行
            Console.WriteLine("=== SynchronizationContext.Current が null の場合 ===");
            await ExecuteCase(threadProbe, Case_AwaitAndReturnOriginalTask(threadProbe));
            await ExecuteCase(threadProbe, Case_AwaitAndReturnContinueWithTask(threadProbe));
            await ExecuteCase(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            // 以下、SynchronizationContext が null ではなく、シングルスレッドコンテキストの場合の実行
            //  ≒ UIスレッドや　ASP.NET のシングルスレッドコンテキストなど
            var syncCtx = new SingleThreadSyncContext();
            SynchronizationContext.SetSynchronizationContext(syncCtx);

            // シングルスレッドコンテキスト側に移行する
            await Task.Yield();

            Console.WriteLine($"=== SynchronizationContext.Current がシングルスレッドの場合 (ConfigureAwait 指定なし ) ===");
            await ExecuteCase(threadProbe, Case_AwaitAndReturnOriginalTask(threadProbe));
            await ExecuteCase(threadProbe, Case_AwaitAndReturnContinueWithTask(threadProbe));
            await ExecuteCase(threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe));

            // ContinueWith と ConfigureAwait の挙動を観察するケースを実行
            List<(bool, bool)> patterns = [(false, false), (false, true), (true, false), (true, true)];
            foreach (var p in patterns)
            {
                Console.WriteLine($"=== SynchronizationContext.Current がシングルスレッドの場合 (ConfigureAwait: {p.Item1}/{p.Item2}) ===");
                await ExecuteCaseWithConfigureAwait(
                    threadProbe, Case_AwaitAndReturnOriginalTask(threadProbe), p.Item1, p.Item2
                );
                await ExecuteCaseWithConfigureAwait(
                    threadProbe, Case_AwaitAndReturnContinueWithTask(threadProbe), p.Item1, p.Item2
                );
                await ExecuteCaseWithConfigureAwait(
                    threadProbe, Case_AwaitAndReturnContinueWithUnwrappedTask(threadProbe), p.Item1, p.Item2
                );
            }

            SynchronizationContext.SetSynchronizationContext(null); // SynchronizationContext を null に戻す
            syncCtx.Complete(); // 実行が終わったら完了させる

            // 「スレッド攪拌処理」を停止
            cts.Cancel();
            await Task.Delay(100); // 少し待つ
        }

        /// <summary>
        /// 指定されたケースを実行し、スレッドの状態を出力するメソッドです。
        /// </summary>
        static async Task ExecuteCase(ThreadProbe threadProbe, (string, Func<Task>) caseToExecute)
        {
            threadProbe.WriteLineThreadID($"await 開始: {caseToExecute.Item1}");
            await caseToExecute.Item2();
            threadProbe.WriteLineThreadID($"await 完了: {caseToExecute.Item1}");
            await Task.Delay(1000);
            threadProbe.WriteLineThreadID($"待機完了: {caseToExecute.Item1}");
            Console.WriteLine();
        }

        /// <summary>
        /// 指定されたケースを実行し、スレッドの状態を出力するメソッドです。
        /// </summary>
        static async Task ExecuteCaseWithConfigureAwait(
            ThreadProbe threadProbe, (string, Func<Task>) caseToExecute,
            bool cfExecute, bool cfWaiting
        )
        {
            threadProbe.WriteLineThreadID($"await 開始: {caseToExecute.Item1}");
            await caseToExecute.Item2().ConfigureAwait(cfExecute);
            threadProbe.WriteLineThreadID($"await 完了: {caseToExecute.Item1}");
            await Task.Delay(1000).ConfigureAwait(cfWaiting);
            threadProbe.WriteLineThreadID($"待機完了: {caseToExecute.Item1}");
            Console.WriteLine();
        }

        /// <summary>
        /// ダミーの非同期メソッドです。スレッドIDを出力し、指定されたラベルを付けて待機します。
        /// </summary>
        static async Task DummyMethod(ThreadProbe threadProbe, string label)
        {
            threadProbe.WriteLineThreadID($"{label} - before await()", 2);
            await Task.Delay(200); // Simulate some asynchronous work
            threadProbe.WriteLineThreadID($"{label} - after await()", 2);
        }

        /// <summary>
        /// ContinueWith を利用し、元の Task だけを返す ケース
        /// </summary>
        static (string, Func<Task>) Case_AwaitAndReturnOriginalTask(ThreadProbe threadProbe)
        {
            var task = Task.Run(() => DummyMethod(threadProbe, "In Task.Run()"));
            task.ContinueWith(t => DummyMethod(threadProbe, "In ContinueWith()"));
            return ("ContinueWith を利用し、元の Task だけを返す ケース", () => task);
        }

        /// <summary>
        /// ContinueWith を利用し、ContinueWith が返した Task を返す ケース(Unwrapなし)
        /// </summary>
        static (string, Func<Task>) Case_AwaitAndReturnContinueWithTask(ThreadProbe threadProbe)
        {
            var task = Task.Run(() => DummyMethod(threadProbe, "In Task.Run()"));
            task = task.ContinueWith(t => DummyMethod(threadProbe, "In ContinueWith()"));
            return ("ContinueWith を利用し、ContinueWith が返した Task を返す ケース(Unwrapなし)", () => task);
        }

        /// <summary>
        /// ContinueWith を利用し、ContinueWith が返した Task を返す ケース(Unwrapなし)
        /// </summary>
        static (string, Func<Task>) Case_AwaitAndReturnContinueWithUnwrappedTask(ThreadProbe threadProbe)
        {
            var task = Task.Run(() => DummyMethod(threadProbe, "In Task.Run()"));
            task = task.ContinueWith(t => DummyMethod(threadProbe, "In ContinueWith()")).Unwrap();
            return ("ContinueWith を利用し、ContinueWith が返した Task を返す ケース(Unwrapあり)", () => task);
        }
    }
}
