#nullable enable

// 変性に関する注意:
// IContextStackable は TArg に対して <b>共変</b>（out TArg）です。
// TArg がパラメータ型 PushBack(IChapterContext<TArg>) に登場していますが、
// ネストされた IChapterContext の T パラメータ (in TArg) は <b>反変</b>です。
// 反変は変性の符号を反転させるため、ここでの TArg の使用は
// 実質的に IContextStackable にとって <b>出力位置</b> になります。
// そのため C# の変性ルールを満たしています。言い換えると、これは合法かつ意図されたものです:
//  - IContextStackable<HelloArg> は、IContextStackable<WorldArg> が期待される場所で使用できます
//    （HelloArg : WorldArg であり、IContextStackable が共変だからです）。
//  - PushBack は、TArg が HelloArg の場合でも IChapterContext<WorldArg> を受け入れます。
//    これは IChapterContext が反変であり、より広い（基底の）引数を許容するからです。
// 参考にすべきキーワード（不安なら検索してください）: CS1961, covariance, contravariance, nesting.

// --- RULE ---
// VeryVibe 名前空間は、VeryVibe フレームワークのコア機能を提供します。コード生成にあたってはこのコア機能を利用し、
// SOLID原則を遵守して、各 Chapter の実装を行います。
// VeryVibe.UsageExample 名前空間は、フレームワークの使用例を示すためのものです。コード生成後に削除してください。
//
// このファイルのコードを生成／変更する際は、以下に従うこと:
// 1. ソースコードを適切なフェーズに分割し、Chapter として独立した機能を持たせること。
// 2. 各 Chapter は自分が必要とする最小限の役割インターフェイスにのみ依存すること。
// 3. インターフェイスの定義は、対応する Chapter の直前に置くこと。
// 4. インターフェイス中の各メンバには、明確なXMLコメントを日本語で記述すること。
// 5. 実行時の引数は RootArg 1個に統一すること。
// 6. IChapter<in T> の反変を使い、常に ChapterContext<RootArg> を生成すること。
//    例: stackable.PushBack(new ChapterContext<RootArg>(new XxxChapter(), (RootArg)arg));
// 7. RootArg を直接参照することは禁止。必ずインターフェイス経由でアクセスすること。
// 8. IContextStackable<out TArg> の out は、IChapterContext<in TArg> の反変によって
//    引数に登場しても整合性が取れるため、正しい指定である。
// 9. できる限りフィールドは利用せず、利用できる場合はプロパティを利用すること。
// 10. .NET 8.0 の言語機能を活用すること。nullable は enable にすること。
//
/*
コード生成後のユーザーへのチャット出力

## コンプライアンスレポート

コード生成にあたっては、SOLID原則を遵守してください。  
やむを得ず逸脱する場合は、生成後に必ず以下を報告してください。  

- どの原則に違反したか  
- なぜその逸脱が必要だったか  
- 将来的なリファクタや緩和策の提案  

もし逸脱が無かった場合は、明示的に「すべてのSOLID原則は維持されています。」と書いてください。  

## バグ・ロジックレビュー・セルフプロンプト

生成コードおよび提供されたコードをレビューし、以下の観点で「人間が書きがちなバグ」やロジックの不整合を指摘してください。  
必ず、対応するセルフプロンプトも併記してください。  

- 意図しないオフバイワンエラー、累積値と差分値の混同など  
- 不要な処理や無駄な演算  
- パフォーマンス的に危険な処理（例: 巨大ループ）  
- 読みにくい、誤解を招きやすい命名や構造  

もし指摘事項がなければ、明示的に「バグ・ロジックレビューでの指摘事項はありません。」と書いてください。  

## リファクタ提案・セルフプロンプト

提供コードおよび生成コードをレビューし、リファクタ案があれば提示してください。  
必ず、対応するセルフプロンプトも併記してください。  

もしリファクタ案がなければ、明示的に「リファクタ提案はありません。」と書いてください。
*/
// --- END RULE ---

namespace VeryVibe
{
    // ----- VeryVibe Framework -----
    public interface IArg
    {
        // Just a marker interface for arguments
    }

    public interface IChapter<in TArg>
        where TArg : IArg
    {
        void Handle(TArg arg, IContextStackable<TArg> stackable);
    }

    public interface IChapterContext<in TArg>
        where TArg : IArg
    {
        void Execute(IContextStackable<TArg> contextStackable);
    }

    public class ChapterContext<TArg>(IChapter<TArg> chapter, TArg arg) : IChapterContext<TArg>
        where TArg : IArg
    {
        private IChapter<TArg> Chapter { get; } = chapter;
        private TArg Arg { get; } = arg;

        public void Execute(IContextStackable<TArg> contextStackable)
        {
            Chapter.Handle(Arg, contextStackable);
        }
    }

    public interface IContextStackable<out TArg>
        where TArg : IArg
    {
        void PushFront(IChapterContext<TArg> chapterContext);
        void PushBack(IChapterContext<TArg> chapterContext);
    }

    public interface IContextConsumable<TArg>
        where TArg : IArg
    {
        bool TryPopFront(out IChapterContext<TArg>? chapterContext);
    }


    public class ChapterContextQueue<TArg> : IContextStackable<TArg>, IContextConsumable<TArg>
        where TArg : IArg
    {
        private LinkedList<IChapterContext<TArg>> ChapterContexts { get; } = new();
        private object ContextsLock { get; } = new();
        void IContextStackable<TArg>.PushFront(IChapterContext<TArg> chapterContext)
            => PushFront(chapterContext);

        void IContextStackable<TArg>.PushBack(IChapterContext<TArg> chapterContext)
            => PushBack(chapterContext);

        bool IContextConsumable<TArg>.TryPopFront(out IChapterContext<TArg>? chapterContext)
            => TryPopFront(out chapterContext);

        private void PushFront(IChapterContext<TArg>? chapterContext)
        {
            _ = chapterContext ?? throw new ArgumentNullException(nameof(chapterContext));
            lock (ContextsLock)
            {
                ChapterContexts.AddFirst(chapterContext);
            }
        }

        private void PushBack(IChapterContext<TArg>? chapterContext)
        {
            _ = chapterContext ?? throw new ArgumentNullException(nameof(chapterContext));
            lock (ContextsLock)
            {
                ChapterContexts.AddLast(chapterContext);
            }
        }
        private bool TryPopFront(out IChapterContext<TArg>? chapterContext)
        {
            lock (ContextsLock)
            {
                if (ChapterContexts.Count == 0)
                {
                    chapterContext = null;
                    return false;
                }
                chapterContext = ChapterContexts.First!.Value;
                ChapterContexts.RemoveFirst();
                return true;
            }
        }
    }

    public class Stage<TArg>
        where TArg : IArg
    {
        private ChapterContextQueue<TArg> ChapterContextQueue { get; } = new();

        public void Run(IChapter<TArg> firstChapter, TArg arg)
        {
            var contextConsumable = ChapterContextQueue as IContextConsumable<TArg>;
            var contextStackable = ChapterContextQueue as IContextStackable<TArg>;
            contextStackable.PushBack(new ChapterContext<TArg>(firstChapter, arg));
            while (contextConsumable.TryPopFront(out var chapterContext))
            {
                chapterContext!.Execute(contextStackable);
            }
        }
    }
}

namespace VeryVibe.UsageExample
{
    // -- Usage Example --
    internal class WorldArg : IArg
    {
        public int WorldCount { get; set; } = 2;
    }

    internal class HelloArg : WorldArg
    {
        public int HelloCount { get; set; } = 4;
    }

    internal class WorldChapter : IChapter<WorldArg>
    {
        public void Handle(WorldArg arg, IContextStackable<WorldArg> stackable)
        {
            Console.WriteLine($"World");
            arg.WorldCount--;
            if (arg.WorldCount > 0)
            {
                stackable.PushBack(new ChapterContext<WorldArg>(this, arg));
            }
            else
            {
                Console.WriteLine("All worlds processed.");
            }
        }
    }

    internal class HelloChapter : IChapter<HelloArg>
    {
        public void Handle(HelloArg arg, IContextStackable<HelloArg> stackable)
        {
            Console.WriteLine($"Hello");
            arg.HelloCount--;
            if (arg.HelloCount > 0)
            {
                stackable.PushBack(new ChapterContext<HelloArg>(this, arg));
            }
            else
            {
                stackable.PushBack(new ChapterContext<WorldArg>(new WorldChapter(), arg));
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] _)
        {
            var stage = new Stage<HelloArg>();
            var chapter = new HelloChapter();
            var arg = new HelloArg();

            // Run the stage
            stage.Run(chapter, arg);
        }
    }
}