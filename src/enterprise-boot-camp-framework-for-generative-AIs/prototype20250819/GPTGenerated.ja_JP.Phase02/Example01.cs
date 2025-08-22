// 本ファイルはプロンプトも含め、MIT License の下で公開されています。
//    → MIT License に従い、商用・非商用を問わず自由にご利用いただけます。
// Copyright (C) 2025 cozyupk
// https://github.com/cozyupk
// なお、このコードおよびプロンプトは研究・学習・人類と生成AIの進化を目的とした未完成版であることを付記します。

// This file, including the prompts, is released under the MIT License.
//    → You are free to use it for both commercial and non-commercial purposes in accordance with the MIT License.
// Copyright (C) 2025 cozyupk
// https://github.com/cozyupk
// Note: This code and its prompts are provided as an unfinished version,
// intended for research, learning, and the joint evolution of humanity and generative AI.

// --- ユーザー向け ---
namespace ForUser { class Explanation { public static void Dummy() { 

_ = "ChatGPT とのバイブコーディングで利用する場合は、チャットの冒頭で次のプロンプトを入力することを推奨します。";
/*
 - ここから先、このチャットでは私との会話履歴を一切使わずに答えてください。
 - 会話の中でソースコード改善案を提案する場合は、まずはコード改善案を提示することの許可をユーザーに求めてください。
*/
_ = "また、オプショナルですが、次のようなプロンプトを同時に入力することも可能かと思います。（好みに応じて修正）";
/*
次のプロンプト以降、内部で3回自己レビューしてから、最終出力+ユーザー側での確認方法＋QAチェック全YES＋セルフレビュー要約3点も出力してください。
口調は優しいお医者さんが、ユーザーに寄り添って説明するような感じでお願いします。
*/
_ = "その後、このファイルをアップロードすると同時に、次のようなプロンプトで「作るもの」の定義をしていきましょう。（下記はあくまで例です。目的に応じて修正）";
_ = "※ このプロンプトも含め、このプロンプト以降、VeryVibe_ja-JP.cs をアップロード/更新しながら進めていきます。";
/*
 Windows環境上のC#で、パラメータを指定してリサージュ曲線を描き .png ファイルとして保存するコンソールプログラムを書きたいです。
また、アニメーションgif出力にも対応したいです。
できるだけ柔軟なプログラムとするためには、どのようなコマンドラインオプションが必要ですか？ 
--help で表示される Usage 風に、説明部分は日本語で答えてください。
この .cs ファイルの RULE や手順に従って生成することを想定しています。 
 */
_ = "「作るものの定義」準備が整ったら、次のようなプロンプトでコード生成を開始します。";
/*
それでは、このチャットでの今までの会話を参照して、コメント生成手順 1. を実施してください。
*/
_ = "ChatGPT が「コピペできない」形で応答してきた場合は、次のようなプロンプトを入力してみてください。";
/*
コピペ可能なようにコードブロックで出力してください。
 */
_ = "出力の改善を求める場合は、次のようなプロンプトを入力してみてください。";
/*
先ほど提案してくれた Usage 風の説明も含めてください。また、コピペ可能なようにコードブロックで出力してください。
*/
_ = @"あなたはこのファイルにChatGPTが出力したコメントやコードを貼り付けて、次のプロンプトで
      更新したファイルを再度アップロードしつつ、「〇〇手順〇を実施してください」とプロンプティングすることで、
      バイブコーディングを進めることができます。
      その形で コメント生成手順 1. ～ コメント生成手順 6. を実施し、その後、コード生成手順 1. 以降も順に実施していきます。
      コード生成手順 5. は、ChatGPTが「対象なし」を出力するまで繰り返し実施します。
      各段階でコンパイルエラーや警告が発生した場合は、あなたが修正して GPT にフィードバックするか、
      必要があれば GPT にスクリーンショットとソースコードを提供し、修正を依頼してください。";
}}}
// --- END ユーザー向け ---

/* --- 生成AI向け RULE ---
   VeryVibe 名前空間は、VeryVibe フレームワークのコア機能を提供します。 
   コードを生成する際には、このコア機能を活用し、各 Chapter を SOLID 原則に準拠して実装してください。
   VeryVibe.UsageExample 名前空間は使用例を示すためのものです。
   また、RULE ブロックもコード生成後には削除してください（ただし最後に示す「RULE 保存方針」に従う場合を除く）。
  
   このファイル内のコードを生成・変更する際には以下のルールに従ってください。
   （注）#3 は生成AIが誤解しやすいので特に注意すること。
   1. ソースコードを適切なフェーズに分割し、それらの各 "Chapter" に独立した機能を持たせること。Mainメソッドは短く保つこと。そのために、コマンドラインパラメータの解析や各種I/Oの前処理等にも、独立した "Chapter" を用意すること。
   2. 各 Chapter を実装するクラスは IChapter を実装し、必要最小限の役割インタフェースにのみ依存すること。
   3. IChapter や IChapterContext の型引数の反変性を成立させるため、**上流（入力に近い）側は、下流（出力に近い）側の役割インタフェイスを継承して“より具体的”になる**継承鎖とする。
     「上流IF は下流IF を継承する（一般→具体）」で一貫させる。
      例（一般 → 具体 の順）: VeryVibe.IArg ← IProcessingArg ← IParseArgsArg
      ※ 実行時には、反変 `IChapter<in TArg>` の性質により、下流を処理する Chapter が、
         その親である“より具体的な”上流IF（＝実際の Arg）を安全に受理できる。下記 VeryVibe.ContravarianceExample を参照。
         (このファイルは .cs ファイルであり、コンパイルが通り、意図道理動作することを確認済です。)
*/
// ============================================================================
// VeryVibe.UsageExample （入れ替え候補サンプル / RULE準拠・XMLコメント付）
// ---------------------------------------------------------------------------
// このセクションは VeryVibe フレームワークの最小利用例（Hello → World/WorldWithCulture）です。
// 重要:
// - コード生成手順 4.（Main 実装以降）では **この UsageExample を削除または置き換え** してください（RULE 保存方針に従う）。
// - Arg インタフェースは RULE #3 の反変方針（上流=具体 ← 下流=一般を継承）に従います。
// - Chapter 連結は PushBack を用いて示し、RootArg は明示的実装（RULE #7）で提供します。
// - 例外/入出力は Chapter 内で処理し、外へ伝播させません（RULE #12）。
// - 分岐する Chapter は多重継承を許容（同名メンバの衝突は new などで明示回避）。
// 禁止:
// - RootArg を直接参照しない（必ず役割インタフェース経由でアクセス）。
// - 章間で new インスタンスに詰め替えて渡さない（既定/許可状態の継承が壊れる）。
// - IContextBuffer<out T> / IChapterContext<in T> の in/out を入れ替えない。
//    （入力(−) × IChapterContext<in T>(−) = 共変(＋) の符号合成で PushBack が型安全になります。）
// ============================================================================
#if VERYVIBE_USAGEEXAMPLE
namespace VeryVibe.UsageExample
{
    using System;
    using System.Globalization;
    using VeryVibe;

    // ------------------------------------------------------------------------
    // Arg インタフェース定義（一般 → 具体の順を維持／直列 or 多重継承を明示）
    // ------------------------------------------------------------------------

    /// <summary>
    /// カルチャ付き「World」出力に必要な最小契約（下流・一般）。
    /// <para>RULE #3: 上流IFは下流IFを継承して“より具体的”になります。</para>
    /// </summary>
    internal interface IWorldWithCultureArg : IArg
    {
        /// <summary>残り出力回数（ゼロで完了）。</summary>
        int WorldWithCultureCount { get; set; }

        /// <summary>
        /// 行出力シンク（例：<see cref="Console.WriteLine(string)"/>）。
        /// 例外やI/O失敗は Chapter 内で処理します（RULE #12）。
        /// </summary>
        Action<string> WriteLineAction { get; }

        /// <summary>
        /// 出力に用いるカルチャ。未設定での参照は無効（例外）。
        /// </summary>
        CultureInfo Culture { get; }
    }

    /// <summary>
    /// カルチャ無しの「World」出力に必要な最小契約（下流・一般）。
    /// </summary>
    internal interface IWorldArg : IArg
    {
        /// <summary>残り出力回数（ゼロで完了）。</summary>
        int WorldCount { get; set; }

        /// <summary>
        /// 行出力シンク（例：<see cref="Console.WriteLine(string)"/>）。
        /// 例外やI/O失敗は Chapter 内で処理します（RULE #12）。
        /// </summary>
        Action<string> WriteLineAction { get; }
    }

    /// <summary>
    /// 「Hello」を出力し、条件により
    /// <see cref="WorldWithCultureChapter"/> または <see cref="WorldChapter"/> に遷移する上流・具体の契約。
    /// <para>分岐する Chapter のため、<see cref="IWorldArg"/> と <see cref="IWorldWithCultureArg"/> を多重継承します。</para>
    /// </summary>
    internal interface IHelloArg : IWorldArg, IWorldWithCultureArg
    {
        /// <summary>残りの Hello 出力回数。</summary>
        int HelloCount { get; set; }

        /// <summary>
        /// 行出力シンク。多重継承により同名メンバがあるため <c>new</c> で再宣言し衝突を明示回避。
        /// </summary>
        new Action<string> WriteLineAction { get; }

        /// <summary>
        /// カルチャが設定済みなら <c>true</c>。
        /// 注意：既存互換のため綴りは <c>IsCulutureSet</c> のまま（将来的な改名は別途）。
        /// </summary>
        bool IsCulutureSet { get; }
    }

    /// <summary>
    /// CLI 引数を解析して <see cref="HelloChapter"/> へ制御を渡す最上流契約（上流・最具体）。
    /// <para>RULE #1: Main を細く保つため、CLI 解析は Chapter 側に置きます。</para>
    /// </summary>
    internal interface IParseArgsArg : IHelloArg
    {
        /// <summary>コマンドライン引数（実行ファイル名は含まない）。</summary>
        string[] Args { get; }

        /// <summary>
        /// カルチャ設定（<c>null</c>→設定、二重設定は禁止）。
        /// </summary>
        /// <param name="culture">適用するカルチャ。</param>
        void SetCulture(CultureInfo culture);
    }

    // ------------------------------------------------------------------------
    // Chapter 実装（例外/I-O は Chapter 内で完結：RULE #12）
    // ------------------------------------------------------------------------

    /// <summary>
    /// カルチャ付きで「World in {Culture}」を出力する章。
    /// </summary>
    internal sealed class WorldWithCultureChapter : IChapter<IWorldWithCultureArg>
    {
        /// <inheritdoc/>
        public void Handle(IWorldWithCultureArg arg, IContextBuffer<IWorldWithCultureArg> buffer)
        {
            try
            {
                arg.WriteLineAction($"World in {arg.Culture.Name}");
            }
            catch (Exception ex)
            {
                // RULE #12: I/O などの失敗は Chapter 内で処理
                arg.WriteLineAction($"[WorldWithCulture] output failed: {ex.Message}");
                return;
            }

            arg.WorldWithCultureCount--;
            if (arg.WorldWithCultureCount > 0)
            {
                // 反変: IContextBuffer<out T> × IChapterContext<in T> の符号合成で型安全に PushBack
                buffer.PushBack(new ChapterContext<IWorldWithCultureArg>(this, arg));
            }
            else
            {
                arg.WriteLineAction("All worlds with culture processed.");
            }
        }
    }

    /// <summary>
    /// 「World」を出力する章（カルチャ無し）。
    /// </summary>
    internal sealed class WorldChapter : IChapter<IWorldArg>
    {
        /// <inheritdoc/>
        public void Handle(IWorldArg arg, IContextBuffer<IWorldArg> buffer)
        {
            try
            {
                arg.WriteLineAction("World");
            }
            catch (Exception ex)
            {
                arg.WriteLineAction($"[World] output failed: {ex.Message}");
                return;
            }

            arg.WorldCount--;
            if (arg.WorldCount > 0)
            {
                buffer.PushBack(new ChapterContext<IWorldArg>(this, arg));
            }
            else
            {
                arg.WriteLineAction("All worlds processed.");
            }
        }
    }

    /// <summary>
    /// 「Hello」を出力し、回数が尽きたらカルチャ有無に応じて次章へ分岐する章。
    /// </summary>
    internal sealed class HelloChapter : IChapter<IHelloArg>
    {
        /// <inheritdoc/>
        public void Handle(IHelloArg arg, IContextBuffer<IHelloArg> buffer)
        {
            try
            {
                arg.WriteLineAction("Hello");
            }
            catch (Exception ex)
            {
                arg.WriteLineAction($"[Hello] output failed: {ex.Message}");
                return;
            }

            arg.HelloCount--;
            if (arg.HelloCount > 0)
            {
                buffer.PushBack(new ChapterContext<IHelloArg>(this, arg));
            }
            else
            {
                // 分岐: 単なるスキップではなく機能分岐なので兄弟IFへルーティング
                if (arg.IsCulutureSet)
                {
                    buffer.PushBack(new ChapterContext<IWorldWithCultureArg>(new WorldWithCultureChapter(), arg));
                }
                else
                {
                    buffer.PushBack(new ChapterContext<IWorldArg>(new WorldChapter(), arg));
                }
            }
        }
    }

    /// <summary>
    /// CLI を解析し、必要に応じてカルチャを設定して <see cref="HelloChapter"/> に渡す章。
    /// </summary>
    internal sealed class ParseArgsChapter : IChapter<IParseArgsArg>
    {
        /// <inheritdoc/>
        public void Handle(IParseArgsArg arg, IContextBuffer<IParseArgsArg> buffer)
        {
            try
            {
                if (1 < arg.Args.Length)
                {
                    Console.WriteLine("Usage: HelloWorld [Culture]");
                    return;
                }

                if (arg.Args.Length == 1)
                {
                    var culture = CultureInfo.GetCultureInfo(arg.Args[0]);
                    arg.SetCulture(culture);
                    arg.WriteLineAction($"Culture set to {culture.Name}");
                }
            }
            catch (Exception ex)
            {
                // RULE #12: 解析失敗はここで要約して通知し、外に出さない
                arg.WriteLineAction($"[ParseArgs] parsing failed: {ex.Message}");
                return;
            }

            // 次章へ
            buffer.PushBack(new ChapterContext<IHelloArg>(new HelloChapter(), arg));
        }
    }

    // ------------------------------------------------------------------------
    // RootArg 実装（明示的実装：RULE #7）
    // ------------------------------------------------------------------------

    /// <summary>
    /// すべての役割インタフェースを<strong>明示的に</strong>実装するルート引数。
    /// <para>内部状態は private メンバで保持し、アクセスは必ずインタフェース経由。</para>
    /// </summary>
    internal sealed class RootArg : IParseArgsArg
    {
        /// <inheritdoc/>
        string[] IParseArgsArg.Args => Args;

        /// <inheritdoc/>
        void IParseArgsArg.SetCulture(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture), "Culture cannot be null.");
            if (Culture != null) throw new InvalidOperationException("Culture is already set.");
            Culture = culture;
        }

        /// <inheritdoc/>
        int IHelloArg.HelloCount { get; set; }

        /// <inheritdoc/>
        int IWorldArg.WorldCount { get; set; }

        /// <inheritdoc/>
        int IWorldWithCultureArg.WorldWithCultureCount { get; set; }

        /// <inheritdoc/>
        bool IHelloArg.IsCulutureSet => Culture != null;

        /// <inheritdoc/>
        CultureInfo IWorldWithCultureArg.Culture
            => Culture ?? throw new InvalidOperationException("Culture is not set.");

        /// <inheritdoc/>
        Action<string> IHelloArg.WriteLineAction => WriteLineAction;

        /// <inheritdoc/>
        Action<string> IWorldArg.WriteLineAction => WriteLineAction;

        /// <inheritdoc/>
        Action<string> IWorldWithCultureArg.WriteLineAction => WriteLineAction;

        // 内部実装（必ず IF 経由でアクセスさせる）
        private Action<string> WriteLineAction { get; } = Console.WriteLine;
        private string[] Args { get; }
        private CultureInfo? Culture { get; set; }

        /// <summary>
        /// ルート引数を生成します。
        /// </summary>
        /// <param name="args">コマンドライン引数（実行ファイル名は含まない）。</param>
        /// <param name="helloCount">Hello の出力回数（0 以上）。</param>
        /// <param name="worldCount">World/WorldWithCulture の出力回数（0 以上）。</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> が <c>null</c>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="helloCount"/> または <paramref name="worldCount"/> が負。</exception>
        public RootArg(string[] args, int helloCount, int worldCount)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args), "Args cannot be null.");
            if (helloCount < 0) throw new ArgumentOutOfRangeException(nameof(helloCount), "Hello count must be non-negative.");
            if (worldCount < 0) throw new ArgumentOutOfRangeException(nameof(worldCount), "World count must be non-negative.");

            ((IHelloArg)this).HelloCount = helloCount;
            ((IWorldArg)this).WorldCount = ((IWorldWithCultureArg)this).WorldWithCultureCount = worldCount;
        }
    }

    // ------------------------------------------------------------------------
    // Main（UsageExample 用 / RULE #1: Main は短く保つ）
    // ------------------------------------------------------------------------

    /// <summary>
    /// UsageExample のエントリポイント（本番生成では削除/置換：RULE 保存方針）。
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main は Stage 構築と最初の Chapter 起動のみを行います（RULE #1）。
        /// CLI 解析は Chapter（<see cref="ParseArgsChapter"/>）へ委譲します。
        /// </summary>
        private static void Main(string[] args)
        {
            var stage = new Stage<IParseArgsArg>();
            var first = new ParseArgsChapter();
            var root = new RootArg(args, helloCount: 3, worldCount: 2);

            stage.Run(first, root);
        }
    }
}
#endif
/*
// ============================================================================
// End: VeryVibe.UsageExample （入れ替え候補サンプル / RULE準拠・XMLコメント付）
// ============================================================================/*
VeryVibe.IArg は、すべての役割インターフェースの基底となるマーカーインタフェース（メンバーなし）。
     IChapter 実装クラスの Handle メソッドは直接呼び出さず、Stage クラスや buffer.PushBack() を通じて呼び出すこと。IChapter 実装クラスから IChapter 実装クラスの Handle を直接呼び出してはならない。
   4. インタフェース定義は対応する Chapter の直前に置くこと。
   5. 各インタフェースメンバには明確な XML コメントを記述すること。
   6. 実行時引数は RootArg 1つに統一すること。
   7. RootArg ではすべての役割インタフェースを**明示的に実装**すること（メンバは必ずインタフェース経由でのみアクセスできるようにする）。
   8. 各 Chapter では対応する Arg インタフェース型のみを利用し、他のインタフェースや RootArg にキャストして無関係なメンバにアクセスしないこと。
   9. RootArg を直接参照することは禁止。Setter も含め、必ずインタフェース経由でアクセスすること。
   10. IContextBuffer<out TArg> の out と IChapterContext<in TArg> の in のバリアンスを維持すること。（理由：入力(−)×反変(−)=共変(＋) の符号合成で CS1961 を満たしているため）
       CS1961 違反を起こさないよう out/in の使用位置を保証すること。
   11. フィールドよりプロパティを優先すること。ただし、内部ロック用や固定コレクションなど従来的用途においては private readonly フィールドの利用を認める。
   12. 設計上の選択が必然ではなくフレームワークの方針による場合は、その旨と意図をインラインコメントに記述し、VeryVibe 外の文脈（および AI/自動レビュー）でも自己説明的になるようにすること。例: readonly フィールドではなくプロパティを選択する、明示的インタフェース実装、パイプラインやリフレクションのための非標準的な命名/可視性、スレッドセーフ性やアロケーションに関するトレードオフ。
   13. I/O や外部プロセス、環境依存呼び出しなどで発生しうる例外は Chapter 内で処理する。既定では Handle の外へ例外を伝播させない。致命的障害など呼び出し側の判定が必要な場合は、呼び出し元の役割IFに IErrorSink 等の“サニタイズ済みエラーの記録口”を定義し、任意のエラーオブジェクト（例外の要約・構造化メタのみ、PII/スタックを含めない）を記録することで逆伝播させる。例外オブジェクト（Exception）自体を IArg に格納して保持してはならない（RULE #25/#32）。
   14. .NET 8.0 の言語機能を活用すること。Nullable を有効化すること。ImplicitUsings は無効化すること。アクセス修飾子は internal をデフォルトとし、最小に絞ること。
   15. コードのコンパイルや実行に .csproj 側の設定（例: TargetFramework, UseWindowsForms, SupportedOSPlatformVersion, プラットフォーム依存パッケージ参照など）が必須な場合は、プログラムの冒頭コメントにその旨を明記すること。
   16. コレクションの契約は必ずインタフェース型（IReadOnlyList<T>, IEnumerable<T> など）で表現すること。Arg や Chapter の契約において、List<T> や配列 (T[]) といった具象型を直接公開してはならない。
   17. null 合体演算子（?? / ??=）を使用する場合、両辺は**契約型に揃える**こと。List<T> と T[] を混在させてはならない。
   18. 空のデフォルト値にはキャッシュされた静的定数を使うこと。例: private static readonly IReadOnlyList<T> Empty = Array.Empty<T>(); public IReadOnlyList<T> Values => _values ?? Empty;  
   19. バッキングフィールドの型は契約型と完全に一致させること。例: private IReadOnlyList<T>? _points;   // List<T>? ではなく  
   20. ミュータブルな入力（例: SetNormalizedPoints）を受け取る場合、受け取り側は IEnumerable<T> など広く受け入れてよい。ただし内部保存は契約型に揃えること。コピーするか否かの方針は明示的にコメントに記載すること（RULE #11）。
   21. タプル要素の名前は型の同一性には影響しないが、コードベース内で一貫させること（例: (double x, double y) を推奨）。
   22.（上位原則・不可侵）データに破壊的な変更を行わない（No Destructive Changes）。既存データの削除・上書き・不可逆変換は既定で禁止。必要な場合は「別名への新規作成」＋「明示フラグ」＋「事前検証」を必須とする。
   23.（上位原則・入力防御）外部から与えられる全入力（CLI引数、環境変数、ファイル名、パス、URL、JSON、テンプレート文字列など）はセキュリティ観点で必ずサニタイズ／バリデーションを行う（allowlist 方式を優先、denylist は補助）。
   24.（上位原則・最小特権）処理に必要な最小権限のみを使用する（ファイル権限、ネットワーク、認証情報）。昇格や広い権限が必要な場合は、その根拠をコメントに明記（RULE #12/#11 に従いインラインで理由を残す）。
   25.（上位原則・可観測性）ログは秘匿情報を含めず、再現に必要な最小限のメタ情報のみを構造化形式で残す（PII/資格情報は常にマスク）。
   26.（上位原則・既定は安全）既定値は常に「安全側」に倒す（外部送信しない／上書きしない／実行しない）。利便性のために安全側を緩める場合は、明示的なフラグとコメントで根拠を示す（RULE #11）。
   27.（入力バリデーション）パラメータは型・範囲・サイズ・形式（正規表現）を検証し、未指定・空・制御文字・改行・NULL バイトを拒否する。パスは GetFullPath で正規化し、必ずベースディレクトリ配下かを StartsWith で検査。シンボリックリンクや再解析ポイントは許可しない。
   28.（出力エンコード）外部へ出す文字列はシンクに応じて必ずエンコード／エスケープ（HTML/CSV/JSON/シェル引数）。文字列連結でコマンドやクエリを組み立てない。コマンド実行は UseShellExecute=false、引数は配列で分離。SQL/検索クエリ等は常にパラメータ化／プリペアド。
   29.（ファイルI/Oの安全化）ファイル書き込みは上書き禁止（FileMode.CreateNew）。一時ファイルに書き込み、検証後に原子的 Move（上書き不可）。対象拡張子／最大サイズに上限を設ける。FileShare は最小で開く（読み: Read、書き: None）。
   30.（ネットワークの安全化）デフォルトで外部送信禁止。必要な場合のみドメイン allowlist＋TLS（≥1.2）＋証明書検証有効。全リクエストにタイムアウト／リトライ（指数バックオフ）／CancellationToken を設定。リダイレクトとプロキシの扱いを明示。
   31.（直列化/逆直列化）BinaryFormatter 等の危険APIは禁止。System.Text.Json を用い、MaxDepth を設定、既知の型にバインド。ポリモーフィックな任意型解決や Type 名埋め込みは使わない。
   32.（秘密情報）APIキー・トークン・接続文字列をソースに埋め込まない。取得は注入インタフェースや OS セキュアストア経由。例外・ログに秘匿情報を出さない（マスク／要約）。
   33.（暗号/乱数）セキュリティ用途の乱数は Random ではなく RandomNumberGenerator を使用。独自暗号は禁止。標準ライブラリのみ使用。
   34.（並行性/リソース）長時間処理は定期的にキャンセルを確認。IDisposable は using で確実に破棄。ロック範囲は最小化し、I/O／ユーザコードの呼び出しはロック外で行う（既存の RunAll 実装に準拠）。
   35.（確認フロー）破壊的になり得る操作（重名出力、削除、移動）は二段階確認とする：Dry-run（検出のみ）→ Confirm フラグで実行。可能なら成果物は別ディレクトリ／バージョニングされたパスに出力。
   36.（テスト可能性）安全性ルールを守るためのユニットテストを用意（例：ディレクトリトラバーサル拒否、上書き禁止、例外時のロールバック保証、エンコード適用）。生成AIがコードを変更してもテストがガードする構成にする。
   37.（AI/自動生成対策）上記ルールに反するコード片（Process.Start のシェル連結、FileMode.Create、未検証パス等）を“禁止パターン”としてコメントに明記し、代替の安全テンプレートをすぐ下に置く（RULE #11）。レビュー時はまず禁止パターン検索から始める。
   38.（失敗時の挙動）失敗は静かに握りつぶさない。ユーザ向けには安全なメッセージ、ログには再現可能な最小情報を残し、中断かフォールバックかの方針を Chapter ごとに明示（RULE #12 と一貫）。
   39.（デフォルト設定の固定）外部送信=無効、上書き=無効、実行=無効、インタラクティブ確認=必要、を既定とし、解除には明示的なフラグ名（例：--allow-overwrite, --allow-network）を必須とする。
   40. UX も考慮すること。数値・日付・時刻は現在のロケールに従ってフォーマット（例: ToString("N", CultureInfo.CurrentCulture)）。ログ/プログラム的出力はカルチャ非依存（例: 日付は ISO 8601）。エラーメッセージやユーザー向け文字列はローカライズ／翻訳ポリシーに従うこと。
   41. コード中のコメント、以下のチャット出力はすべて日本語で記述すること。テンプレも日本語で記述すること。
   42.（CLI フラグ規約・固定・ただし要件上不要なものは省略可）
       --dry-run（既定=true）/ --confirm（実行許可）
       --allow-overwrite / --allow-network / --allow-exec
       出力衝突時は --allow-overwrite が無ければ失敗。安全既定は常に維持する（#39）。
   43.（禁止パターン最小セット・抜粋）
       - Process.Start(string) でのシェル連結／UseShellExecute=true
       - FileMode.Create / FileShare.ReadWrite の使用
       - 未検証パスの使用（GetFullPath/StartsWith 検査なし）
       - BinaryFormatter 等の危険直列化／任意型バインド
       - セキュリティ用途に Random を使用
       - 例外/ログに絶対パス・トークン等の秘匿情報を出力
   44.（.csproj 既定スケルトン）
       既定: 
         <TargetFramework>net8.0</TargetFramework>
         <Nullable>enable</Nullable>
         <ImplicitUsings>disable</ImplicitUsings>
        <EnableImplicitProgram>false</EnableImplicitProgram>
         <NoWarn>$(NoWarn);IDE0130</NoWarn>
       この既定を前提にコードを生成する。外す場合はコメントで明示する。
   45.（RULE 保存方針）
       RULE ブロックは `#if VERYVIBE_RULES` で残すか、別ファイル（VeryVibe.RULES.md 等）へ切り出して保存してよい。生成後に完全削除する場合は、SSoT の所在を別コメントで必ず示す。

------（テンプレート）コード生成後にユーザーへ返すチャット出力 -------

【重要】コード生成タスクをユーザーとの対話でサブタスクに分割する場合でも、
各サブタスク完了後に **必ず本セクションを出力** すること。

## 準拠レポート（SOLID）
- 原則順守／違反の有無を明記。逸脱が必要だった場合は、理由・影響・将来の是正策を簡潔に列挙。
- 例）「すべての SOLID 原則を維持しました。」または違反の詳細。

## バグ/ロジック レビュー & 自己プロンプト
- よくあるミス（オフバイワン、累積/差分の取り違え、冗長処理、性能地雷、可読性）を点検。
- 各指摘に対応する“自己プロンプト”を添える。
- 問題なしの場合：「バグ/ロジック レビューに指摘はありません。」

## セキュリティ レビュー & 自己プロンプト
- 非破壊既定 / 入力検証 / インジェクション耐性 / 出力エンコード / 直列化安全 / 秘密情報 /
  ネットワーク / 並行性 / 暗号 / ロギング を網羅確認。
- 各項目に合否と簡単な自己プロンプトを添える。
- 問題なしの場合：「セキュリティ レビューに指摘はありません。」

## リファクタリング提案 & 自己プロンプト
- 有益な抽象化・命名・分割・依存整理の提案と、その検証用自己プロンプト。
- 提案なしの場合：「リファクタリング提案はありません。」

## 不確実性レポート
- 判断が割れうる設計点ごとに：決定点 / 代替案 / 採用案と理由 / 信頼度（High/Medium/Low） / 次の改善提案
------ END: （テンプレート）コード生成後にユーザーへ返すチャット出力 -------

=============================================================================
RULE: コメント出力フォーマット契約
=============================================================================
出力フォーマット:
  - 生成AI は **Markdown のコードブロック（```csharp ... ```）の中に**
    C# のブロックコメントを入れて出力してください

禁止事項:
 - 複数のコードブロックに分割して出すこと
 - コメントの外側に文字列を追加すること
 - コードブロックの中に実際の C# 実装コードや using 句を書くこと

例（見出し冒頭の期待形）:
  /* ========================================================================
     仕様合意と設計方針（LissajousTool / CLI）
     ========================================================================
     1) 概要 / 目的
        - 目的: ...
        - 実行形態: ...
        ...
     例: LissajousTool png -ax 3 -ay 2 --confirm -o .\out\classic.png
     例: LissajousTool gif -ax 5 -ay 7 --phase-sweep 0..360 --frames 120 --fps 30 --loop 0 --confirm -o .\out\spin.gif
   *\/
   見出し冒頭の文言は、このRULEには依存しない形の文言で記述すること。（このRULEの存在を知らない第三者に意味が通ること）
   実行時の追加注意:
     - コメント生成手順 1. 以外の「コメント生成手順」同様にコメントブロックで返す。
     - サブタスクに分割しても、サブタスク単位で**毎回コメントブロック**を返す。
     - 「コメント生成手順」以外の手順（ex. 「コード生成手順」）はこの契約には含まれない。
     - 途中で要約が必要なら、必ずコメント内に含める（外に書かない）。
================= END: RULE: コメント出力フォーマット契約 ==================

==========================================================================
RULE: VeryVibe 名前空間での定義の絶対参照（IChapter/IContextBuffer/IChapterContext/IArg）
==========================================================================

[目的]
  - VeryVibe コアにあるランタイム契約（IArg / IChapter<TArg> / IContextBuffer<out TArg> / IChapterContext<in TArg>）を
    全コンポーネントが一貫して参照し、各機能側での再定義・影の型の混入を防止する。

[必須事項]
  1) VeryVibe 名前空間の明示
   - すべての Chapter 実装を含むファイルは、ファイル先頭（namespace の内側）に以下を必ず記すこと:
        using VeryVibe;

  2) 再定義の禁止
      - 次のいずれの型も、アプリ/機能側（例: VeryVibe.Cal 等）で新規に定義してはならない:
          interface IArg
          interface IChapter<in TArg>
          interface IContextBufferr<out TArg>
          interface IChapterContext<in TArg>
      - これらの「同名/同義の型」を別 namespace で宣言すること（影武者定義）も禁止。
 
  3) 便利クラスの取り扱い
      - VeryVibe 名前空間に ChapterContext<T> / NoopContextBuffer 等の“実装クラス”が存在する場合は原則としてそれを使用する。
      - VeryVibe 名前空間に未提供の場合や継承拡張したい場合、独自実装したい場合、機能側で実装を定義してよい。
      - 機能側で実装する場合、VeryVibe 名前空間に存在するクラスと異なる名前を付けること。
 
  4) using 配置の統一
     - C# 12 以降のファイルスコープ namespace を用いる場合でも、using は namespace ブロックの「内側」に置くこと。
       解析/生成コードはこの前提で出力される。

===============　END: RULE: 名前空間での定義の絶対参照（IChapter/IContextBuffer/IChapterContext/IArg） =======
--- END RULE ---
*/

/*
=== コメント生成手順 1. 仕様合意と設計方針 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※ユーザーからコメント生成手順1.の実施をプロンプトで指示された場合、次の事項を本ソースコードにコメントとしてコピー&ペースト可能な形でチャット出力する。
  - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
1) 概要：目的/機能/想定ユーザー/実行形態（CLI/GUI）/対象OS・.NET など
2) I/O 要件：入力/出力/拡張子/最大サイズ/上書き可否/ドライラン/Confirm必須の有無など
 - 入出力のフォーマット・入出力ファイル構成や環境変数・コマンドラインパラメータなど、詳細にすべてを記述すること。
 - 直前のチャットでの会話内容を網羅的に詳細まで含めること。つまりほぼコピーすること。
3) 失敗方針：中断 or フォールバック／ユーザー表示内容／ログ方針（PIIマスク/構造化）など
4) セキュリティ前提：パス正規化/ディレクトリ外拒否/ネットワーク既定禁止/禁止パターン検索実施など
5) UX方針：Usage（CLIオプション・例）、数値/日付のフォーマット（人向け=ロケール依存、機械= ISO 8601）など
6) 設計方針：Chapter 一覧と各責務、対応する Arg IF（Chapter直前に置く）
  - 役割IFは上流→下流の継承で反変性を満たす（例：IExtractInputArg : ICalcArg : IArg）
  - RootArg は全役割IFを“明示的”に実装し、Chapterは対応IF経由のみアクセス
7) 受け入れ条件：ビルド成功／Dry-run成功／破壊的操作はConfirm必須／禁止パターン不検出など
※ 本出力だけで会話履歴が消えても再開可能なレベルまで自己完結的に出力すること。
=== End コメント生成手順 1. ===
*/
/*
=== コメント生成手順 2. 仕様セルフレビュー ===
※ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コメント生成手順 1. の実施が完了していることが前提。
※ユーザーからコメント生成手順2.の実施をプロンプトで指示された後、次の事項を本ソースコードにコメントとしてコピー&ペースト可能な形でチャット出力する。
 - Specification Review Template の内容に基づいた「仕様合意と設計方針」のセルフレビュー結果
  - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
  - 本ソースコードへコピペ可能な形式で、テンプレートも含め日本語で提示される。

Specification Review Template

## Specification Compliance
- Does the agreed specification align with the VeryVibe RULE set (SOLID principles, safety principles, least privilege, No Destructive Changes, etc.)?
- Is the relationship between each Chapter and its corresponding IArg interface clearly defined?
- Does the specification include explicit implementation of RootArg and the prohibition of direct RootArg access?

## Completeness Check
- Are all I/O requirements (extensions, maximum size, overwrite policy, dry-run, confirm flag) fully covered in the specification?
- Are error handling policies (abort, fallback, logging) clearly documented?
- Are the security principles (banned patterns, path normalization, network prohibition) explicitly stated?

## Consistency Check
- Are the usage examples and the requirements described in the specification consistent with each other?
- Are the safe defaults maintained (dry-run enabled by default, overwrite prohibited, etc.)?
- Is the handling of locale/logging (user-facing = CurrentCulture, machine-facing = Invariant) included in the specification?

## Risk & Uncertainty
- Are there any design decisions that are still undecided or have multiple alternatives? (e.g., choice of DNN framework)
- If so, what alternatives exist, and on what basis should the decision be made?

## Refactoring / Clarification Suggestions
- Are there any missing or ambiguous descriptions in the specification document?
- Are there additional items that should be explicitly specified? (e.g., detailed format of configuration files)
=== End コメント生成手順 2. ===
*/
/*
=== コメント生成手順 3. 仕様追補 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コメント生成手順 2. の実施が完了していることが前提。
※ユーザーが「コメント生成手順 3. を実施」と指示した場合：
  - コメント生成手順 2. のセルフレビューで明らかになった未確定点や課題を追補仕様として整理し、コメント出力する。
  - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
  - 本ソースコードへコピペ可能な形式で提示される。
※ 本出力だけで会話履歴が消えても以後の手順が実施できるよう自己完結的に出力すること。
=== End コメント生成手順 3. ===
*/
/*
=== コメント生成手順 4. 依存パッケージ等決定 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コメント生成手順 3. の実施が完了していることが前提。
※ユーザーが「コメント生成手順 4. を実施」と指示した場合：
ユーザーに依存パッケージ等の決定が必要な事項を通知し、それぞれに関して
  - 選択肢
  - 選択肢ごとのメリットデメリット
  - 推奨選択肢
を提示し、決定を促す。本手順はコメントブロックではなく、ユーザーとのチャットで行う。
パッケージのシェアが大きいことは選択肢のメリットとして挙げる。
パッケージの更新が止まっていることは選択肢のデメリットとして挙げるが、「歴史があり、安定していること」はメリットとして挙げる。
安定版が存在せず、プレビュー、ベータ版しかないパッケージは、そもそも選択肢として提示しないこと。
テスト用のパッケージは、最初の選択肢として xUnit と Moq を提示する。
=== End コメント生成手順 4. ===
*/
/*
=== コメント生成手順 5. 依存パッケージ等コメント生成 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コメント生成手順 4. の実施が完了していることが前提。
※ユーザーが「コメント生成手順 5. を実施」と指示した場合：
ユーザーとのチャット記録に基づき、依存パッケージの決定事項と、想定される .csproj を**単一ブロックコメント**で提示する（実際の貼付はユーザーが行う）。
  - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
  - csproj の内容は、ユーザーが実際に貼り付けることを前提として、<Project> タグから </Project> タグまでを含む。
  - csproj の内容は、アプリケーションプロジェクト用と、テストプロジェクト用の2つを出力する。
  - csproj を出力する際には、C# のコメントブロックの行頭 * は利用せず、ユーザーがそのままコピーアンドペーストできるようにすること。
  - csproj では、PackageReference のバージョンは、現時点で適切と思われるものを仮に指定する。
※ 本出力だけで会話履歴が消えても以後の手順が実施できるよう自己完結的に出力すること。
=== End コメント生成手順 5. ===
*/
/*
=== コメント生成手順 6. Arg インタフェース最終化 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コメント生成手順 5. の実施が完了していることが前提。
※ユーザーが「コメント生成手順 6. を実施」と指示した場合：
 各 Chapter 直前に配置する Arg インタフェースの最終版を、C# 単一ブロックコメントで出力する。
方針:
 - 今までにコメントで構想した各 Chapter の Arg インタフェースを、上流（ユーザーからの入力）から下流（出力）へ向かっての順に、各メンバに XML コメントを必ず付与した**最終シグネチャ**をコメントとして提示する。
出力物: 各 IF の public メンバ名、継承インタフェイス、型、読み書き属性（get; / set;）、意図、例外規約。
 - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
注意: RULE #3 に完全に従うこと。通常は IArg を継承する役割インタフェイスはただ一つとなる。
成果: コメントをそのまま各 Chapter 直前に貼る → 以降のコード生成の単一情報源（SSoT）にする。
※ 本出力だけで会話履歴が消えてもコード生成プロセスが実施できるよう自己完結的に出力すること。
=== End コメント生成手順 6. ===
*/
/*
=== コード生成手順 1. Arg IF コード生成 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コメント生成手順 6. の実施が完了していることが前提。
ユーザーが「コード生成手順 1. を実施」と指示した場合：
 各 Chapter 直前に配置する interface の**実装コード**（XML ドキュメントコメント必須）を生成してソースコードにマージし、ダウンロード可能にする。
 入力: コメント生成手順6. の最終化コメント（SSoT）
 ルール: VeryVibe RULE 準拠。契約型はインタフェース（IReadOnlyList<T> 等）で公開。配列や List<T> を直接返さない。
        **インタフェイス及び各メンバには、必ず XML ドキュメントコメントを付与すること。**
注意: Chat 返信の末尾にはレビュー結果のみを簡潔に出す（テンプレ本文の再掲は禁止）。
=== End コード生成手順 1. ===
*/
/*
=== コード生成手順 2. RootArg 明示的実装スケルトン ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コード生成手順 1. の実施が完了していることが前提。
ユーザーが「コード生成手順 2. を実施」と指示した場合：
  RootArg が全 Arg IF を**明示的**に実装（プロパティ/セッターは IF 経由のみアクセス可能）する形のソースコードを生成してソースコードにマージし、ダウンロード可能にする。
要件:
 - バッキングフィールドは契約型と完全一致（例: IReadOnlyList<T>? _points）。
 - 空既定値は静的キャッシュ Array.Empty<T>() を使用。
 - 受け取りは IEnumerable<T> 可、内部保存は契約型へ正規化。コピー方針をコメントで明記。
 - I/O を持つメソッドはここでは実装しない（Adapter を後日注入可にするため）。
 - 補助的クラスを実装する場合には、必ず XML ドキュメントコメントを付与すること。
出力: RootArg クラス本体（ctor で引数の基本検証、IClockArg/ITempFileArg などの注入ポイントのプロパティ）。
=== End コード生成手順 2. ===
*/
/*
=== コード生成手順 3. Chapter 空実装雛形 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コード生成手順 2. の実施が完了していることが前提。
ユーザーが「コード生成手順 3. を実施」と指示した場合：
 - IChapter<対応 Arg IF> を実装し、ソースコードにマージし、ダウンロード可能にする。
 - Handle 内で try/catch を用意（I/O 想定箇所のみ）。エラーは短文 + 構造化ログに要約、伝播禁止（RULE #13）。
 - 次フェーズへのコンテキストを buffer.PushBack(new ChapterContext<...>(..., arg)) でつなぐだけの骨組み。
 - TODO コメントで「この Chapter に置くロジック」と「禁止パターン」を明記。
 - Chapter は、典型的には次のような構成となる。:
internal sealed class EncodeChapter : IChapter<IEncodeArg>
{
     public void Handle(IEncodeArg arg, IContextBuffer<IEncodeArg> buffer)
     {
         try
         {
              // 何らかの処理を行う（例: エンコード）
              var encodeOut = ...
              buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(), encodeOut));
         } catch (Exception ex) {
              // エラーは短文 + 構造化ログに要約、伝播禁止（RULE #13）
         }
     }
}
 - コンストラクタインジェクションを伴う場合は、次のような形で「次のチャプター」に引き継げるパラメータを受け取る:
internal sealed class EncodeChapter : IChapter<IEncodeArg>
{
     private readonly ILoggerFactory _logFactory;
     private readonly ILogger<EncodeChapter> _log;
     private readonly IImageEncoderEngine _engine;

     public EncodeChapter(ILoggerFactory logFactory, IImageEncoderEngine? engine = null)
     {
             _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
             _log = logFactory.CreateLogger<EncodeChapter>();
             _engine = engine ?? new ImageSharpEncoderEngine();
     }

     public void Handle(IEncodeArg arg, IContextBuffer<IEncodeArg> buffer)
     {
         try
         {
              // 何らかの処理を行う（例: エンコード）
              var encodeOut = ...
              buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(_logFactory), encodeOut));
         } catch (Exception ex) {
              // エラーは短文 + 構造化ログに要約、伝播禁止（RULE #13）
         }
     }
}
=== End コード生成手順 3. ===
*/
/*
=== コード生成手順 4. Mainメソッドの実装 ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コード生成手順 3. の実施が完了していることが前提。
ユーザーが「コード生成手順 4. を実施」と指示した場合：
 - Main メソッドを実装し、ダウンロード可能にする。
 - Main メソッドは、Stage と 最初の Chapter、RootArg のインスタンス、および他に必要となるインスタンスを生成し、
   Stage.Run() を呼び出す。
**重要(必ずユーザーに報告 or 依頼)**: この時点で VeryVibe.UsageExample の実装は削除するか、その旨をユーザーに依頼(削除依頼)すること。
=== コード生成手順 4. Mainメソッドの実装 ===
*/
/*
=== コード生成手順 5. Chapter の実装（単一） ===
Rule: ユーザーから明示的に実施をされない限り、本手順は実施しない。(先走った出力をしない。手順実施の提案をするのはOK)
※コード生成手順 4. の実施が完了していることが前提。
ユーザーが「コード生成手順 5. を実施」と指示した場合：
1) 「骨組み状態」の Chapter を列挙し（Handle に TODO/NotImplemented/空実装があるものを骨組みと定義）、
   パイプライン順で最上流のものを 1 つだけ選ぶ。パイプライン順はソースコード上のコメントや PushBack の順序で決定する。
2) 選ばれた Chapter の Handle と、必要があればヘルパ―等の関連クラスを実装する .cs を 1 つ生成し、ダウンロード可能にする。
   本出力だけで会話履歴が消えても他のチャプターの実装が再開可能なレベルまでコメントを自己完結的に出力すること。
3) 骨組みが 1 つも無い（＝全実装済み）の場合：生成を行わず、「対象なし」を短く報告する。
** 重要 **
 セルフレビューを忘れずに行い、ユーザーにチャットで報告すること。
 - 内容は、「（テンプレート）コード生成後にユーザーへ返すチャット出力」参照
 - コード生成手順3.で実装した空実装を削除するよう、該当メソッド名を網羅的に明らかにしつつ、ユーザーに依頼すること。
=== End コード生成手順 5. ===
*/

namespace VeryVibe
{
    using System;
    using System.Collections.Generic;

    // ----- VeryVibe Framework -----
    /// <summary>
    /// Marker interface for argument types.
    /// </summary>
    public interface IArg
    {
        // intentionally empty
    }

    /// <summary>
    /// A chapter that processes an argument and may enqueue follow-up contexts.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IChapter<in TArg>
        where TArg : IArg
    {
        /// <summary>Handle one step and push next contexts to <paramref name="buffer"/>.</summary>
        void Handle(TArg arg, IContextBuffer<TArg> buffer);
    }

    /// <summary>
    /// A context wrapper that can execute a chapter using its argument.
    /// </summary>
    /// <remarks>
    /// Variance note: <see cref="IChapterContext{TArg}"/> is <b>contravariant</b> (<c>in TArg</c>).
    /// This allows a context of a base argument type to be consumed where a derived argument is processed.
    /// </remarks>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IChapterContext<in TArg>
        where TArg : IArg
    {
        /// <summary>Execute this context within the given buffer/dispatcher.</summary>
        void Execute(IContextBuffer<TArg> buffer);
    }

    /// <summary>
    /// Concrete chapter context.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public sealed class ChapterContext<TArg>(IChapter<TArg> chapter, TArg arg) : IChapterContext<TArg>
        where TArg : IArg
    {
        // This could be readonly fields, but by policy they're kept as properties (see RULE #11).
        private IChapter<TArg> Chapter { get; } = chapter;
        private TArg Arg { get; } = arg;

        public void Execute(IContextBuffer<TArg> buffer)
        {
            Chapter.Handle(Arg, buffer);
        }
    }

    /// <summary>
    /// Buffer for managing a sequence of chapter contexts (enqueue side).
    /// </summary>
    /// <remarks>
    /// Variance note: <see cref="IContextBuffer{TArg}"/> is <b>covariant</b> (<c>out TArg</c>).
    /// Even though <typeparamref name="TArg"/> appears in method parameters via
    /// <see cref="IChapterContext{TArg}"/>, that interface is contravariant (<c>in TArg</c>),
    /// which keeps the overall use of <typeparamref name="TArg"/> in an output position; this complies with CS1961.
    /// Example: <c>IContextBuffer&lt;IHelloArg&gt;</c> can be used where <c>IContextBuffer&lt;IWorldArg&gt;</c> is expected
    /// if <c>IHelloArg : IWorldArg</c>.
    /// </remarks>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IContextBuffer<out TArg>
        where TArg : IArg
    {
        void PushFront(IChapterContext<TArg> chapterContext);
        void PushBack(IChapterContext<TArg> chapterContext);
    }

    /// <summary>
    /// Dispatcher for consuming and executing buffered contexts (dequeue side).
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public interface IContextDispatcher<TArg>
        where TArg : IArg
    {
        /// <summary>Runs until the buffer becomes empty. Exceptions from contexts propagate to the caller unless handled by policy.</summary>
        void RunAll();
    }

    /// <summary>
    /// Thread-safe deque that acts as both buffer and dispatcher.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public sealed class ChapterContextDeque<TArg> : IContextBuffer<TArg>, IContextDispatcher<TArg>
        where TArg : IArg
    {
        // Exceptions to RULE #10: lock objects and fixed backing collections are conventional readonly fields.
        private readonly object _lockObject = new();
        private readonly LinkedList<IChapterContext<TArg>> _queue = new();

        void IContextBuffer<TArg>.PushFront(IChapterContext<TArg> chapterContext) => PushFront(chapterContext);
        void IContextBuffer<TArg>.PushBack(IChapterContext<TArg> chapterContext) => PushBack(chapterContext);
        void IContextDispatcher<TArg>.RunAll() => RunAll();

        private void PushFront(IChapterContext<TArg> chapterContext)
        {
            ArgumentNullException.ThrowIfNull(chapterContext);
            lock (_lockObject) _queue.AddFirst(chapterContext);
        }

        private void PushBack(IChapterContext<TArg> chapterContext)
        {
            ArgumentNullException.ThrowIfNull(chapterContext);
            lock (_lockObject) _queue.AddLast(chapterContext);
        }

        private void RunAll()
        {
            while (true)
            {
                IChapterContext<TArg>? next;
                lock (_lockObject)
                {
                    if (_queue.Count == 0) return;
                    next = _queue.First!.Value;
                    _queue.RemoveFirst();
                }
                // Execute outside the lock to allow re-entrancy and new scheduling.
                next.Execute(this);
            }
        }
    }

    /// <summary>
    /// High-level runner that wires the initial chapter and argument and drains the buffer.
    /// </summary>
    /// <typeparam name="TArg">Argument type (must implement <see cref="IArg"/>).</typeparam>
    public sealed class Stage<TArg>
        where TArg : IArg
    {
        // By policy kept as a property-like field name would be fine; we keep it private here.
        private ChapterContextDeque<TArg> Buffer { get; } = new();

        public void Run(IChapter<TArg> firstChapter, TArg arg)
        {
            IContextDispatcher<TArg> dispatcher = Buffer; // compile-time guarantee
            IContextBuffer<TArg> buffer = Buffer; // compile-time guarantee

            buffer.PushBack(new ChapterContext<TArg>(firstChapter, arg));
            dispatcher.RunAll();
        }
    }
}
/* ========================================================================
   仕様合意と設計方針（LissajousTool / CLI）
   ========================================================================
1) 概要 / 目的
   - 目的: Windows 上の .NET コンソールアプリとして、リサージュ曲線を描画し PNG（静止画）および GIF（アニメーション）を安全に出力する。
   - 実行形態: CLI。サブコマンド `png` / `gif` を提供。
   - 安全既定: 既定は dry-run（検証のみ）。実書込みは -y/--yes（--confirm 同義）で許可。上書きは --allow-overwrite が明示されない限り禁止。
   - 想定ユーザー: 画像生成・可視化を行いたい開発者/研究者/クリエイター。
   - 依存方針: 画像描画/エンコード用の外部パッケージを採用（候補は後述の手順4で決定）。テストは xUnit/Moq を第一候補。
   - 対象環境: .NET 8 / Windows。Nullable 有効、ImplicitUsings 無効、EnableImplicitProgram=false を前提。

2) I/O 要件
   入力:
   - コマンドライン引数（サブコマンド、描画パラメータ、キャンバス/スタイル、GIF アニメ設定、スイープ設定、グローバル安全フラグ）。
   - 角度単位は既定=度。--radians 指定時はラジアン。
   出力:
   - `png` サブコマンド: 拡張子 .png の単一ファイル。
   - `gif` サブコマンド: 拡張子 .gif の単一ファイル（アニメーション、ループ/フレーム数/残像/スイープ対応）。
   - 追加メタデータ（任意）を PNG に埋め込み可能とする（キー=値;... 形式）。
   ファイルポリシ:
   - 既定は dry-run：ファイルは作成しない。検証とレポートのみ実施。
   - 実出力するには -y/--yes を必須。既存ファイルがある場合、--allow-overwrite 指定がないとエラー。
   - 出力先は正規化し、ベースディレクトリ外や再解析ポイント/シンボリックリンクは拒否。
   - 書込みは一時ファイル→検証→原子的 Move（CreateNew / 上書き禁止）を基本とし、--allow-overwrite 時のみ安全に置換。
   パラメータ（代表例・既定値は設計想定、最終値は実装時に再確認）:
   - 共通曲線: -ax/--amp-x, -ay/--amp-y, -fx/--freq-x, -fy/--freq-y, -p/--phase, --samples, --center-x/--center-y
   - キャンバス/スタイル: -w/--width, -h/--height, -m/--margin, --bg, --stroke, --stroke-width, --opacity, --dash, --antialias/--no-antialias
   - PNG: --png-compress, --metadata
   - GIF: --frames, --fps, --loop, --duration, --easing, --trail
   - スイープ（gif のみ）: --phase-sweep, --amp-x-sweep, --amp-y-sweep, --freq-x-sweep, --freq-y-sweep, --color-sweep, --width-sweep
     ※ 範囲表記 "start..end[:step]"。step 省略時は frames に同期する等の自動割当。複数スイープは直積ではなく同期補間。
   - グローバル: -o/--output, --dry-run(既定), -y/--yes(--confirm), --allow-overwrite, --dpi, --seed, --radians, --verbose

3) 失敗方針
   - 入力検証エラー/範囲外/不正形式/危険パス/衝突: 即時中断し、ユーザー向けには安全なサマリ（PII/絶対パス伏せ）を表示。
   - エンコード/描画失敗: 章（Chapter）内で捕捉し要約を出力。ハードエラーは当該フローを中断。部分成果物は残さない。
   - dry-run では静かに成功/失敗を報告し、標準出力に検証レポート。ログは構造化（機械処理向け）を想定。

4) セキュリティ前提
   - No Destructive Changes：既定で上書き禁止・実行禁止・外部送信禁止。
   - パス正規化、ベース配下検査、シンボリックリンク/再解析ポイント拒否。
   - 危険 API/パターン禁止（Process.Start のシェル連結、FileMode.Create、BinaryFormatter、未検証パス、Secrets 出力 等）。
   - 例外/ログはサニタイズした要約のみ（PII/資格情報/絶対パス/トークン等は出さない）。

5) UX 方針
   - `--help` で Usage 一覧と例を十分に提示。
   - 例（要旨）:
       LissajousTool png -fx 3 -fy 2 -ax 450 -ay 450 -p 0 -w 1024 -h 1024 -o .\out.png -y
       LissajousTool gif -fx 5 -fy 7 --phase-sweep "0..360" --frames 120 --fps 30 --loop 0 -o .\spin.gif -y
   - 人向け文字列は CurrentCulture、機械向けは Invariant/ISO 8601 を採用。
   - 失敗時は短文＋改善ヒント、詳細は（あれば）構造化ログへ。

6) 設計方針（Chapter 概要 / Arg IF 対応）
   - ParseArgs → (BuildModel) → Render → Encode → Write の流れを基調とし、I/O/前処理は独立 Chapter 化。
   - 各 Chapter は「対応する役割インタフェース（I*Arg）」のみを見る。RootArg へキャストしない。
   - 反変性方針: 上流（より具体）IF が下流（一般）IF を継承する継承鎖により、IChapter<in T> の反変を成り立たせる。
   - RootArg はすべての役割インタフェースを **明示的実装**。状態は private に保持し IF 経由でのみアクセス。
   - エラーは Chapter 内で処理・要約（外へ例外は基本伝播しない）。

   参考 Chapter/IF（最小サンプル。最終版は手順6で確定）:
   - IParseArgsArg : IBuildModelArg : IRenderArg : IEncodePngArg / IEncodeGifArg : IWriteArg : IArg
     * 分岐（png/gif）は Render 後の Encode*Arg で枝分かれ。
     * Sweep/Animation は IEncodeGifArg に集約（補間/配列長整合もここ）。

7) 受け入れ条件
   - ビルド成功、すべての基本シナリオで dry-run 成功。
   - 実出力は -y/--yes 必須かつ上書き時は --allow-overwrite 必須。
   - 危険パターン不検出、パス/サイズ/拡張子/エンコードの検証テストがパス。
   - png/gif ともに最小例が成功し、スイープが同期補間で破綻しないこと。

（本仕様は本ファイル記載の RULE/手順を前提にしており、単独で読み直しても意味が通るよう自己完結的に記述している）
=========================================================================

=========================================================================
仕様セルフレビュー（コメント生成手順 2.）
=========================================================================
■ Specification Compliance
- VeryVibe RULE（SOLID/安全原則/最小特権/非破壊）に整合: Yes
- Chapter ↔ Arg IF の対応：上流→下流の継承鎖を明示し、RootArg 明示実装を前提に記述: Yes
- RootArg 直接参照禁止の明示: Yes（IF 経由アクセスのみ）

■ Completeness Check
- I/O 要件（拡張子/最大サイズ上限（※値は実装時に設定）/上書き可否/dry-run/confirm）: 概念は網羅、数値閾値は未確定（追補要）
- エラーポリシ（中断/フォールバック/ログ方針）: Yes（要約/構造化ログ）
- セキュリティ原則（禁止パターン/パス正規化/ネットワーク既定禁止）: Yes

■ Consistency Check
- Usage 例と仕様の整合: Yes
- 安全既定（dry-run 有効/上書き禁止）: Yes
- ロケール/ログ方針（人=CurrentCulture, 機械=Invariant/ISO）: Yes

■ Risk & Uncertainty
- 画像ライブラリ選定（描画/エンコード/GIF最適化/色管理）: 未決
- CLI パーサ選定（長短所/メンテ現況）: 未決
- カラー指定の許容範囲（色名辞書/Hex のみ/alpha 取扱）: 一部未決
- 最大キャンバス/メモリ閾値/フレーム上限/時間制限: 未決（DoS 回避要件として閾値が必要）
- スイープ同期規則の細部（端数処理/同長化/優先度）: 一部未決

■ Refactoring / Clarification Suggestions
- 仕様に「閾値（幅/高さ/frames/samples/ファイルサイズ上限）」の具体値を追加推奨
- 色指定/ダッシュ/不透明度などの許容形式を厳密化
- スイープ複合時の競合規則（duration と frames/fps の優先度）を明記

（本セルフレビューは上記仕様に対する網羅・一貫性・安全性の観点での自己点検結果です）
=========================================================================

=========================================================================
仕様追補（コメント生成手順 3.）
=========================================================================
A) 閾値・制限（DoS/誤設定防止）— 提案値（暫定）
   - 幅/高さ: 64〜8192 px（既定 1024）
   - samples: 100〜200,000（既定 4,000）
   - GIF frames: 1〜2,000、fps: 1〜120、duration: ≤ 120,000ms
   - 残像 trail: 0〜frames-1
   - 出力ファイル最大サイズ: 256 MB（超過時は中断）

B) カラー指定
   - 受理: #RGB/#RRGGBB/#RRGGBBAA と一部の色名（"white","black" など最小辞書）。色名はカルチャ非依存。
   - 不正形式は即エラー。--opacity は 0..1 の Double、--stroke は alpha を含める場合は #RRGGBBAA を推奨。

C) スイープ同期規則
   - 明示 step あり: 各スイープは (floor((end-start)/step)+1) サンプルを生成。複数スイープは最長長さに合わせ、短いものは端値を繰返さず「補間」で埋める（linear / --easing）。
   - step 省略: 指定がないスイープは frames に等分で自動生成。
   - duration と fps/frames が競合する場合の優先度: duration > fps > frames（duration 指定時は fps から frames を導出）。

D) PNG メタデータ
   - 形式: "k=v; k2=v2" のセミコロン区切り。キー/値の許容文字は英数/一部記号（=; は値の中で使用不可）。不正は拒否。

E) ログと出力メッセージ
   - ユーザー向け: 要約、日本語（CurrentCulture）。
   - 構造化ログ（任意）: JSON Lines（Invariant）で出力可（将来拡張）。

F) エラーメッセージの粒度
   - ファイルパスは末尾ファイル名のみ表示。ディレクトリは伏せる。詳細はオプション --verbose 時のみ安全範囲で追記。

G) 既定動作の固定
   - --dry-run 既定 ON、--antialias 既定 ON、背景 #000000、線 #FFFFFF、線幅 2.0、opacity 1.0。

（本追補は実装直前の不確定点を固定するための暫定案。手順4でパッケージ選定を確定後、必要に応じて更新する）
=========================================================================
*/
/* =============================================================================
 コメント生成手順 5. 依存パッケージ等コメント生成（確定版）
 =============================================================================
決定事項（ユーザー回答反映）:
- 画像/エンコード: SixLabors.ImageSharp / ImageSharp.Drawing を採用（Yes）
- CLI パーサ: CommandLineParser（commandline）を採用（候補B）
- ログ: Microsoft.Extensions.Logging.Abstractions の最小構成（Yes）
- テスト: xUnit + Moq（Yes）
- 仕様追補の閾値（暫定値）の固定（Yes）

注意:
- バージョン番号は「現時点の安定版を仮指定」。実際の開発環境に合わせて適宜更新してください。
- <ImplicitUsings>disable</ImplicitUsings>、<Nullable>enable</Nullable>、<EnableDefaultItems>false</EnableDefaultItems> を前提にします（RULE #44）。
- Windows 前提だが、ImageSharp を選択しているため将来の移植性は担保しやすい構成です。
- テストでは xunit.runner.visualstudio / coverlet.collector を参照（ローカル実行・CI 併用想定）。

---------------------------------------
[アプリケーション用 .csproj（LissajousTool.csproj 例）]
---------------------------------------
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <EnableDefaultItems>false</EnableDefaultItems>
    <RootNamespace>LissajousTool</RootNamespace>
    <AssemblyName>LissajousTool</AssemblyName>
    <LangVersion>preview</LangVersion>
    <NoWarn>$(NoWarn);IDE0130</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <!-- CLI パーサ（候補B） -->
    <PackageReference Include="CommandLineParser" Version="2.9.1" />
    <!-- 画像描画 / エンコード -->
    <PackageReference Include="SixLabors.ImageSharp" Version="3.1.5" />
    <PackageReference Include="SixLabors.ImageSharp.Drawing" Version="3.1.5" />
    <!-- ロギング最小構成 -->
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  </ItemGroup>
</Project>

---------------------------------------
[テスト用 .csproj（LissajousTool.Tests.csproj 例）]
---------------------------------------
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <EnableDefaultItems>false</EnableDefaultItems>
    <RootNamespace>LissajousTool.Tests</RootNamespace>
    <AssemblyName>LissajousTool.Tests</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\LissajousTool\LissajousTool.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.8.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="coverlet.collector" Version="6.0.0">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>

=============================================================================
 コメント生成手順 6. Arg インタフェース最終化（最終シグネチャ / XML ドキュメント付）
=============================================================================
目的:
- 各 Chapter 直前に配置する Arg インタフェースを、上流（入力）→下流（出力）の順で確定。
- 反変性方針（RULE #3）：上流IF は下流IF を継承して“より具体的”になる継承鎖を維持。
- RootArg は**明示的実装**でこれら全IFを実装し、Chapter は対応IF 経由のみアクセス（RootArg 直接参照禁止）。

継承鎖（一般 → 具体 の順）:
  VeryVibe.IArg
    ← IWriteArg
      ← IEncodePngArg / IEncodeGifArg
        ← IRenderArg
          ← IBuildModelArg
            ← IParseArgsArg

※ IEncodePngArg と IEncodeGifArg は兄弟分岐のため、それぞれ IRenderArg を継承する。

---------------------------------------
[IParseArgsArg : IBuildModelArg]
---------------------------------------
public interface IParseArgsArg : IBuildModelArg
{
    IReadOnlyList<string> RawArgs { get; }
    bool UseRadians { get; }
    bool Verbose { get; }
    bool DryRun { get; set; }
    bool AllowOverwrite { get; set; }
    string? OutputPathRaw { get; }
    void SetCulture(System.Globalization.CultureInfo culture);
}

---------------------------------------
[IBuildModelArg : IRenderArg]
---------------------------------------
public interface IBuildModelArg : IRenderArg
{
    int Width { get; set; }
    int Height { get; set; }
    int Margin { get; set; }
    int Dpi { get; set; }
    double CenterX { get; set; }
    double CenterY { get; set; }
    int Samples { get; set; }
    double AmpX { get; set; }
    double AmpY { get; set; }
    int FreqX { get; set; }
    int FreqY { get; set; }
    double PhaseRad { get; set; }
    double StrokeWidth { get; set; }
    double Opacity { get; set; }
    string BgColor { get; set; }
    string StrokeColor { get; set; }
    string? DashPattern { get; set; }
    bool Antialias { get; set; }
    string? NormalizedOutputPath { get; set; }
}

---------------------------------------
[IRenderArg : IEncodePngArg, IEncodeGifArg]
---------------------------------------
public interface IRenderArg : IEncodePngArg, IEncodeGifArg
{
    string AngleTraversalRule { get; set; }
}

---------------------------------------
[IEncodePngArg : IWriteArg]
---------------------------------------
public interface IEncodePngArg : IWriteArg
{
    int PngCompressionLevel { get; set; }
    string? PngMetadata { get; set; }
}

---------------------------------------
[IEncodeGifArg : IWriteArg]
---------------------------------------
public interface IEncodeGifArg : IWriteArg
{
    int Frames { get; set; }
    double Fps { get; set; }
    int? DurationMs { get; set; }
    int LoopCount { get; set; }
    string Easing { get; set; }
    int Trail { get; set; }
    IReadOnlyList<double>? PhaseSweepRad { get; set; }
    IReadOnlyList<double>? AmpXSweep { get; set; }
    IReadOnlyList<double>? AmpYSweep { get; set; }
    IReadOnlyList<int>? FreqXSweep { get; set; }
    IReadOnlyList<int>? FreqYSweep { get; set; }
    IReadOnlyList<string>? ColorSweep { get; set; }
    IReadOnlyList<double>? StrokeWidthSweep { get; set; }
}

---------------------------------------
[IWriteArg : VeryVibe.IArg]
---------------------------------------
public interface IWriteArg : VeryVibe.IArg
{
    string OutputPathNormalized { get; }
    bool DryRun { get; }
    bool AllowOverwrite { get; }
    long MaxFileBytes { get; }
}
=============================================================================
*/
/* ========================================================================
   コード生成手順 1. Arg IF コード生成（LissajousTool / CLI）— 改訂版
   ------------------------------------------------------------------------
   衝突回避のため IParseArgsArg 側のフラグを「…Option」接尾辞に変更：
     - DryRun      → DryRunOption
     - AllowOverwrite → AllowOverwriteOption
   これにより、下流の IWriteArg 側の最終値（DryRun / AllowOverwrite）と区別されます。
======================================================================== */

namespace LissajousTool
{
    using System.Collections.Generic;
    using System.Globalization;
    using VeryVibe;

    /// <summary>
    /// CLI 引数を解析して下流へ受け渡す最上流契約（最具体）。
    /// </summary>
    internal interface IParseArgsArg : IBuildModelArg
    {
        /// <summary>コマンドライン引数（実行ファイル名は含まない）。</summary>
        IReadOnlyList<string> RawArgs { get; }

        /// <summary>位相等をラジアンとして解釈する場合 true。</summary>
        bool UseRadians { get; }

        /// <summary>詳細ログを有効化する場合 true。</summary>
        bool Verbose { get; }

        /// <summary>
        /// ドライランを「オプション指定として」受け取ったか（生値）。
        /// 下流の <see cref="IWriteArg.DryRun"/> に正規化される前の値。
        /// </summary>
        bool DryRunOption { get; set; }

        /// <summary>
        /// 上書き許可を「オプション指定として」受け取ったか（生値）。
        /// 下流の <see cref="IWriteArg.AllowOverwrite"/> に正規化される前の値。
        /// </summary>
        bool AllowOverwriteOption { get; set; }

        /// <summary>出力先パスの生文字列（未正規化）。</summary>
        string? OutputPathRaw { get; }

        /// <summary>使用するカルチャを設定（多重設定は禁止）。</summary>
        /// <param name="culture">設定するカルチャ。</param>
        void SetCulture(CultureInfo culture);
    }

    /// <summary>
    /// 解析済みパラメータを内部モデルへ正規化して保持する契約。
    /// </summary>
    internal interface IBuildModelArg : IRenderArg
    {
        int Width { get; set; }
        int Height { get; set; }
        int Margin { get; set; }
        int Dpi { get; set; }

        double CenterX { get; set; }
        double CenterY { get; set; }

        int Samples { get; set; }

        double AmpX { get; set; }
        double AmpY { get; set; }
        int FreqX { get; set; }
        int FreqY { get; set; }
        double PhaseRad { get; set; }

        double StrokeWidth { get; set; }
        double Opacity { get; set; }
        string BgColor { get; set; }
        string StrokeColor { get; set; }
        string? DashPattern { get; set; }
        bool Antialias { get; set; }

        /// <summary>正規化済みの出力ファイルパス（ベース配下・再解析ポイント無し）。</summary>
        string? NormalizedOutputPath { get; set; }
    }

    /// <summary>
    /// レンダリング段階の契約。エンコード章（PNG/GIF）へ橋渡しする。
    /// </summary>
    internal interface IRenderArg : IEncodePngArg, IEncodeGifArg
    {
        /// <summary>角度走査規則の識別子（実装依存）。</summary>
        string AngleTraversalRule { get; set; }
    }

    /// <summary>PNG エンコード専用の契約（静止画）。</summary>
    internal interface IEncodePngArg : IWriteArg
    {
        /// <summary>PNG 圧縮レベル（0..9）。</summary>
        int PngCompressionLevel { get; set; }

        /// <summary>メタデータ（"k=v; k2=v2"）。null 可。</summary>
        string? PngMetadata { get; set; }
    }

    /// <summary>GIF エンコード専用の契約（アニメーション／スイープ対応）。</summary>
    internal interface IEncodeGifArg : IWriteArg
    {
        int Frames { get; set; }
        double Fps { get; set; }
        int? DurationMs { get; set; }
        int LoopCount { get; set; }
        string Easing { get; set; }
        int Trail { get; set; }

        IReadOnlyList<double>? PhaseSweepRad { get; set; }
        IReadOnlyList<double>? AmpXSweep { get; set; }
        IReadOnlyList<double>? AmpYSweep { get; set; }
        IReadOnlyList<int>? FreqXSweep { get; set; }
        IReadOnlyList<int>? FreqYSweep { get; set; }
        IReadOnlyList<string>? ColorSweep { get; set; }
        IReadOnlyList<double>? StrokeWidthSweep { get; set; }
    }

    /// <summary>
    /// 最終出力（ファイル書込み）に必要な情報を提供する契約。
    /// </summary>
    internal interface IWriteArg : IArg
    {
        /// <summary>正規化済み出力パス（必須）。</summary>
        string OutputPathNormalized { get; }

        /// <summary>最終的に適用されるドライランフラグ（既定 true）。</summary>
        bool DryRun { get; }

        /// <summary>最終的に適用される上書き許可フラグ。</summary>
        bool AllowOverwrite { get; }

        /// <summary>最大許容ファイルサイズ（バイト）。超過時は中断。</summary>
        long MaxFileBytes { get; }
    }
}

/* ========================================================================
   レビュー要点
   - IParseArgsArg.*Option 系で上流の「生値」を分離し、IWriteArg 側の最終値と衝突しない。
   - 継承鎖（IParseArgsArg → … → IWriteArg）内で重複メンバー定義を排除。
   - 後続の RootArg 明示的実装で衝突（CS0111）を回避可能。
========================================================================= */
// ============================================================================
// コード生成手順 2. RootArg 明示的実装スケルトン（LissajousTool / CLI）
// - 役割IFをすべて「明示的」に実装します（RULE #7）。
// - バッキングフィールドの型は契約型（IReadOnlyList<T> 等）に一致させます（RULE #16/#19）。
// - ここでは I/O や外部依存の処理は行いません（後続 Chapter で注入/実装）。
// - 既定値は「安全側」：DryRun=true, AllowOverwrite=false, MaxFileBytes=256MB 等（追補A）。
// - OutputPathNormalized は後続 Chapter（正規化フェーズ）で設定される前提のプレースホルダです。
// ============================================================================

namespace LissajousTool
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Lissajous CLI 用のルート引数。
    /// すべての役割インタフェースを<strong>明示的に</strong>実装し、Chapter からは常に対応 IF 経由でアクセスされます。
    /// </summary>
    internal sealed class RootArg :
        IParseArgsArg,   // 上流：CLI入力の生値（Option系）を保持
        IBuildModelArg,  // 中流：内部モデルへ正規化済みの値を保持
        IRenderArg,      // 中流：描画フェーズに必要な値
        IEncodePngArg,   // 下流：PNG エンコード用
        IEncodeGifArg,   // 下流：GIF エンコード用
        IWriteArg        // 最下流：出力フェーズの確定値
    {
        // -------------------------
        // 静的既定値（Empty キャッシュ等）
        // -------------------------
        private static readonly IReadOnlyList<string> EmptyStrings = Array.Empty<string>();
        private static readonly IReadOnlyList<double> EmptyDoubles = Array.Empty<double>();
        private static readonly IReadOnlyList<int> EmptyInts = Array.Empty<int>();

        // -------------------------
        // 上流：Parse（生値）用フィールド
        // -------------------------
        private IReadOnlyList<string> _rawArgs = EmptyStrings;
        private bool _useRadians;
        private bool _verbose;
        private bool _dryRunOption = true;        // 既定は安全側（DryRun 有効）
        private bool _allowOverwriteOption;       // 既定=false
        private string? _outputPathRaw;
        private CultureInfo? _cultureForParsing;  // 数値/色/言語処理等の解析カルチャ（SetCulture で一度だけ設定）

        // -------------------------
        // 中流：Build/Render（正規化）用フィールド
        // -------------------------
        private int _width = 1024;
        private int _height = 1024;
        private int _margin = 32;
        private int _dpi = 96;

        private double _centerX = 512;
        private double _centerY = 512;

        private int _samples = 4000;

        private double _ampX = 450;
        private double _ampY = 450;
        private int _freqX = 3;
        private int _freqY = 2;
        private double _phaseRad; // 0（度系からの正規化は上流Chapterで行う）

        private double _strokeWidth = 2.0;
        private double _opacity = 1.0;
        private string _bgColor = "#000000";
        private string _strokeColor = "#FFFFFF";
        private string? _dashPattern;
        private bool _antialias = true;

        private string? _normalizedOutputPath;

        private string _angleTraversalRule = "uniform-0..2pi"; // 実装依存の識別子（等間隔走査想定）

        // -------------------------
        // 下流：PNG/GIF エンコード用フィールド
        // -------------------------
        private int _pngCompressionLevel = 6;
        private string? _pngMetadata;

        private int _frames = 120;
        private double _fps = 30;
        private int? _durationMs;
        private int _loopCount; // 0=無限
        private string _easing = "linear";
        private int _trail;

        private IReadOnlyList<double>? _phaseSweepRad = EmptyDoubles;
        private IReadOnlyList<double>? _ampXSweep = EmptyDoubles;
        private IReadOnlyList<double>? _ampYSweep = EmptyDoubles;
        private IReadOnlyList<int>? _freqXSweep = EmptyInts;
        private IReadOnlyList<int>? _freqYSweep = EmptyInts;
        private IReadOnlyList<string>? _colorSweep = EmptyStrings;
        private IReadOnlyList<double>? _strokeWidthSweep = EmptyDoubles;

        // -------------------------
        // 最下流：Write（確定値）用フィールド
        // -------------------------
        private string _outputPathNormalized = "";          // 正規化未完了時は空文字（後続 Chapter で設定）
        private bool _dryRun = true;                        // 既定は安全側
        private bool _allowOverwrite;                       // 既定=false
        private long _maxFileBytes = 256L * 1024L * 1024L;  // 256MB

        /// <summary>
        /// 既定コンストラクタ。CLI の生引数と任意の初期既定を指定できます。
        /// 上流 Chapter によって値が更新・正規化され、最下流の確定値へ反映されます。
        /// </summary>
        /// <param name="rawArgs">実行ファイル名を除く CLI 引数。</param>
        /// <param name="dryRunDefault">最下流の既定 DryRun 値（true 推奨）。</param>
        /// <param name="allowOverwriteDefault">最下流の既定 AllowOverwrite 値（false 推奨）。</param>
        public RootArg(IReadOnlyList<string>? rawArgs = null, bool dryRunDefault = true, bool allowOverwriteDefault = false)
        {
            _rawArgs = rawArgs ?? EmptyStrings;
            _dryRun = dryRunDefault;
            _allowOverwrite = allowOverwriteDefault;
        }

        // =====================================================================
        // IParseArgsArg（上流・生値）
        // =====================================================================
        IReadOnlyList<string> IParseArgsArg.RawArgs => _rawArgs;
        bool IParseArgsArg.UseRadians => _useRadians;
        bool IParseArgsArg.Verbose => _verbose;

        bool IParseArgsArg.DryRunOption
        {
            get => _dryRunOption;
            set => _dryRunOption = value;
        }

        bool IParseArgsArg.AllowOverwriteOption
        {
            get => _allowOverwriteOption;
            set => _allowOverwriteOption = value;
        }

        string? IParseArgsArg.OutputPathRaw => _outputPathRaw;

        void IParseArgsArg.SetCulture(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);
            if (_cultureForParsing is not null)
                throw new InvalidOperationException("Culture has already been set.");
            _cultureForParsing = culture;
        }

        // =====================================================================
        // IBuildModelArg（中流・正規化）
        // =====================================================================
        int IBuildModelArg.Width { get => _width; set => _width = value; }
        int IBuildModelArg.Height { get => _height; set => _height = value; }
        int IBuildModelArg.Margin { get => _margin; set => _margin = value; }
        int IBuildModelArg.Dpi { get => _dpi; set => _dpi = value; }
        double IBuildModelArg.CenterX { get => _centerX; set => _centerX = value; }
        double IBuildModelArg.CenterY { get => _centerY; set => _centerY = value; }
        int IBuildModelArg.Samples { get => _samples; set => _samples = value; }
        double IBuildModelArg.AmpX { get => _ampX; set => _ampX = value; }
        double IBuildModelArg.AmpY { get => _ampY; set => _ampY = value; }
        int IBuildModelArg.FreqX { get => _freqX; set => _freqX = value; }
        int IBuildModelArg.FreqY { get => _freqY; set => _freqY = value; }
        double IBuildModelArg.PhaseRad { get => _phaseRad; set => _phaseRad = value; }
        double IBuildModelArg.StrokeWidth { get => _strokeWidth; set => _strokeWidth = value; }
        double IBuildModelArg.Opacity { get => _opacity; set => _opacity = value; }
        string IBuildModelArg.BgColor { get => _bgColor; set => _bgColor = value ?? "#000000"; }
        string IBuildModelArg.StrokeColor { get => _strokeColor; set => _strokeColor = value ?? "#FFFFFF"; }
        string? IBuildModelArg.DashPattern { get => _dashPattern; set => _dashPattern = value; }
        bool IBuildModelArg.Antialias { get => _antialias; set => _antialias = value; }
        string? IBuildModelArg.NormalizedOutputPath { get => _normalizedOutputPath; set => _normalizedOutputPath = value; }

        // =====================================================================
        // IRenderArg（描画フェーズ）
        // =====================================================================
        string IRenderArg.AngleTraversalRule { get => _angleTraversalRule; set => _angleTraversalRule = value ?? "uniform-0..2pi"; }

        // =====================================================================
        // IEncodePngArg（PNG）
        // =====================================================================
        int IEncodePngArg.PngCompressionLevel { get => _pngCompressionLevel; set => _pngCompressionLevel = value; }
        string? IEncodePngArg.PngMetadata { get => _pngMetadata; set => _pngMetadata = value; }

        // =====================================================================
        // IEncodeGifArg（GIF）
        // =====================================================================
        int IEncodeGifArg.Frames { get => _frames; set => _frames = value; }
        double IEncodeGifArg.Fps { get => _fps; set => _fps = value; }
        int? IEncodeGifArg.DurationMs { get => _durationMs; set => _durationMs = value; }
        int IEncodeGifArg.LoopCount { get => _loopCount; set => _loopCount = value; }
        string IEncodeGifArg.Easing { get => _easing; set => _easing = value ?? "linear"; }
        int IEncodeGifArg.Trail { get => _trail; set => _trail = value; }
        IReadOnlyList<double>? IEncodeGifArg.PhaseSweepRad { get => _phaseSweepRad; set => _phaseSweepRad = value; }
        IReadOnlyList<double>? IEncodeGifArg.AmpXSweep { get => _ampXSweep; set => _ampXSweep = value; }
        IReadOnlyList<double>? IEncodeGifArg.AmpYSweep { get => _ampYSweep; set => _ampYSweep = value; }
        IReadOnlyList<int>? IEncodeGifArg.FreqXSweep { get => _freqXSweep; set => _freqXSweep = value; }
        IReadOnlyList<int>? IEncodeGifArg.FreqYSweep { get => _freqYSweep; set => _freqYSweep = value; }
        IReadOnlyList<string>? IEncodeGifArg.ColorSweep { get => _colorSweep; set => _colorSweep = value; }
        IReadOnlyList<double>? IEncodeGifArg.StrokeWidthSweep { get => _strokeWidthSweep; set => _strokeWidthSweep = value; }

        // =====================================================================
        // IWriteArg（出力フェーズ・確定値）
        // =====================================================================
        string IWriteArg.OutputPathNormalized => _outputPathNormalized;
        bool IWriteArg.DryRun => _dryRun;
        bool IWriteArg.AllowOverwrite => _allowOverwrite;
        long IWriteArg.MaxFileBytes => _maxFileBytes;

        // =====================================================================
        // 補助 API（Chapter からの“正規化→確定値”反映用）
        // ※ 役割IFに含めず、RootArg の内部 API として提供します。
        //    後続 Chapter（パス正規化/検証結果）から呼び出し、最下流の確定値に反映します。
        // =====================================================================

        /// <summary>
        /// 正規化済みのパスを確定値（IWriteArg.OutputPathNormalized）に反映します。
        /// </summary>
        internal void CommitNormalizedPath(string normalizedPath)
        {
            _outputPathNormalized = normalizedPath ?? throw new ArgumentNullException(nameof(normalizedPath));
        }

        /// <summary>
        /// Option 系の生値（DryRunOption/AllowOverwriteOption）から、最下流の確定値へ反映します。
        /// 典型的には「検証 OK かつ --confirm 指定あり」の時に DryRun=false に落とす等のロジックを Chapter で判断し、
        /// その結果のみを本 API で反映します。
        /// </summary>
        internal void CommitExecutionPolicy(bool dryRun, bool allowOverwrite)
        {
            _dryRun = dryRun;
            _allowOverwrite = allowOverwrite;
        }

        /// <summary>
        /// 最大出力サイズなどの閾値を更新します（DoS/誤設定防止のための固定・追補A）。
        /// </summary>
        internal void SetLimits(long? maxFileBytes = null)
        {
            if (maxFileBytes is { } v && v > 0) _maxFileBytes = v;
        }

        /// <summary>
        /// 上流のフラグを取得します（検証・決定ロジック用）。Chapter 専用の便宜メソッド。
        /// </summary>
        internal (bool DryRunOption, bool AllowOverwriteOption, bool UseRadians, bool Verbose, CultureInfo? Culture, string? OutputPathRaw) GetUpstreamOptions()
            => (_dryRunOption, _allowOverwriteOption, _useRadians, _verbose, _cultureForParsing, _outputPathRaw);

        // =====================================================================
        // 上流フラグの「直接設定」用（CLI パーサ Chapter から注入）
        // ※ IParseArgsArg は“明示的実装”なので、Chapter 側からは RootArg を直接参照せず、
        //   解析 Chapter 内部で RootArg を保持している場合に限り、この内部 API を使ってセットします。
        // =====================================================================

        internal void SetParseInputs(
            IReadOnlyList<string> rawArgs,
            bool useRadians,
            bool verbose,
            bool dryRunOption,
            bool allowOverwriteOption,
            string? outputPathRaw)
        {
            _rawArgs = rawArgs ?? EmptyStrings;
            _useRadians = useRadians;
            _verbose = verbose;
            _dryRunOption = dryRunOption;
            _allowOverwriteOption = allowOverwriteOption;
            _outputPathRaw = outputPathRaw;
        }
    }
}
// ここまで：コード生成手順 2（RootArg 明示的実装スケルトン）
// ============================================================================
// コード生成手順 3. Chapter 空実装雛形（LissajousTool / CLI）— 再出力（logger 一貫化）
// - すべての Chapter で ILoggerFactory を保持する形に統一しました。
// - ParseCliChapter も _loggerFactory フィールドを持ち、次章生成に同一の factory を渡します。
// ============================================================================

namespace LissajousTool
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using System;
    using System.IO;
    using VeryVibe;

    // ------------------------------------------------------------------------
    // 解析 → 正規化 → レンダリング → エンコード(PNG/GIF) → 書込み
    // インタフェース継承鎖（一般 → 具体）:
    //   VeryVibe.IArg
    //     ← IWriteArg
    //       ← IEncodePngArg / IEncodeGifArg
    //         ← IRenderArg
    //           ← IBuildModelArg
    //             ← IParseArgsArg
    // ------------------------------------------------------------------------

    // ============================
    // 2) 正規化/検証 Chapter
    // ============================
    /// <summary>
    /// 値域・形式・既定値の適用、パス正規化、閾値チェックなどを行う章。
    /// </summary>
    internal sealed class BuildModelChapter : IChapter<IBuildModelArg>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;

        public BuildModelChapter(ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _log = _loggerFactory.CreateLogger<BuildModelChapter>();
        }

        /// <inheritdoc/>
        public void Handle(IBuildModelArg arg, IContextBuffer<IBuildModelArg> buffer)
        {
            try
            {
                // TODO:
                // - 幅/高さ/samples/frames/fps の範囲検証と補正（追補Aの閾値）
                // - 角度: 度→ラジアン（IParseArgsArg.UseRadians=false の場合）
                // - 出力パス正規化とコミット（RootArg.CommitNormalizedPath）
                // - 実行ポリシ確定（RootArg.CommitExecutionPolicy）
                // - 色/ダッシュ/不透明度の形式検証
                // - スイープ同期（frames と系列長の整合 / イージング補間）
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "正規化/検証でエラーが発生しました。");
                return;
            }

            buffer.PushBack(new ChapterContext<IRenderArg>(new RenderChapter(_loggerFactory), arg));
        }
    }

    // ============================
    // 3) レンダリング Chapter
    // ============================
    /// <summary>
    /// 曲線のサンプリングとパス構築（座標列→描画プリミティブ生成）を行う章。
    /// </summary>
    internal sealed class RenderChapter : IChapter<IRenderArg>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;

        public RenderChapter(ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _log = _loggerFactory.CreateLogger<RenderChapter>();
        }

        /// <inheritdoc/>
        public void Handle(IRenderArg arg, IContextBuffer<IRenderArg> buffer)
        {
            try
            {
                // TODO:
                // - t ∈ [0, 2π] の等間隔サンプリング（arg.AngleTraversalRule）
                // - x(t)=A*sin(a*t+δ), y(t)=B*sin(b*t) の計算
                // - マージン/中心を考慮してキャンバス座標へ写像
                // - 線幅/ダッシュ/AA のための中間データ生成（具象型に依存しない）
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "レンダリング処理でエラーが発生しました。");
                return;
            }

            // 出力拡張子で分岐
            try
            {
                var ext = SafeGetExtension(arg);
                if (string.Equals(ext, ".gif", StringComparison.OrdinalIgnoreCase))
                {
                    buffer.PushBack(new ChapterContext<IEncodeGifArg>(new EncodeGifChapter(_loggerFactory), arg));
                }
                else
                {
                    buffer.PushBack(new ChapterContext<IEncodePngArg>(new EncodePngChapter(_loggerFactory), arg));
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "出力先の種類判定に失敗しました。");
                return;
            }
        }

        private static string SafeGetExtension(IRenderArg arg)
        {
            try
            {
                var path = arg.OutputPathNormalized;
                if (!string.IsNullOrWhiteSpace(path)) return Path.GetExtension(path);
            }
            catch { /* ignore */ }
            return ".png";
        }
    }

    // ============================
    // 4a) PNG エンコード Chapter
    // ============================
    /// <summary>
    /// レンダリング結果を PNG にエンコードする章（骨組み）。
    /// </summary>
    internal sealed class EncodePngChapter : IChapter<IEncodePngArg>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;

        public EncodePngChapter(ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _log = _loggerFactory.CreateLogger<EncodePngChapter>();
        }

        /// <inheritdoc/>
        public void Handle(IEncodePngArg arg, IContextBuffer<IEncodePngArg> buffer)
        {
            try
            {
                // TODO:
                // - 抽象描画結果 → ImageSharp の Image<Rgba32> 等へ
                // - 圧縮レベル/メタデータを適用し PNG へエンコード（メモリ上）
                // - 書込み章へ渡すためのバイト列保管（設計：別 Arg IF 追加など）
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "PNG エンコードでエラーが発生しました。");
                return;
            }

            buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(_loggerFactory), arg));
        }
    }

    // ============================
    // 4b) GIF エンコード Chapter
    // ============================
    /// <summary>
    /// レンダリング結果をアニメーション GIF にエンコードする章（骨組み）。
    /// </summary>
    internal sealed class EncodeGifChapter : IChapter<IEncodeGifArg>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;

        public EncodeGifChapter(ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _log = _loggerFactory.CreateLogger<EncodeGifChapter>();
        }

        /// <inheritdoc/>
        public void Handle(IEncodeGifArg arg, IContextBuffer<IEncodeGifArg> buffer)
        {
            try
            {
                // TODO:
                // - 各スイープ列を同期補間してフレーム列を生成
                // - fps/duration/loop/trail を適用し GIF へエンコード（メモリ上）
                // - 書込み章へ渡すためのバイト列保管（設計：別 Arg IF 追加など）
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "GIF エンコードでエラーが発生しました。");
                return;
            }

            buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(_loggerFactory), arg));
        }
    }

    // ============================
    // 5) 書込み Chapter
    // ============================
    /// <summary>
    /// 出力ファイルの書込み（dry-run 確認、上書きポリシ、安全な原子的 Move）を行う章（骨組み）。
    /// </summary>
    internal sealed class WriteChapter : IChapter<IWriteArg>
    {
        private readonly ILogger _log;

        public WriteChapter(ILoggerFactory? loggerFactory = null)
        {
            _log = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<WriteChapter>();
        }

        /// <inheritdoc/>
        public void Handle(IWriteArg arg, IContextBuffer<IWriteArg> buffer)
        {
            try
            {
                // TODO:
                // - DryRun=true: 実ファイルは作らず予定を要約
                // - DryRun=false:
                //     * 一時ファイルへ CreateNew で書込み
                //     * サイズ/拡張子検査（MaxFileBytes、.png/.gif）
                //     * 原子的 Move。上書きは AllowOverwrite のときのみ安全に置換
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "ファイル書込みでエラーが発生しました。");
                return;
            }

            // 最終章のため PushBack は不要
        }
    }
}
// ここまで：コード生成手順 3（Chapter 空実装雛形・修正版）
// ============================================================================
// コード生成手順 4. Mainメソッドの実装（LissajousTool / CLI）
// - VeryVibe ランタイムの Stage を起動し、最初の Chapter と RootArg を配線します。
// - ログは現状 Abstractions のみ参照のため NullLoggerFactory を使用（将来 Console 等を追加可）。
// **重要**: 現在のファイルには VeryVibe.UsageExample 側にも Program.Main が存在します。
//          「複数のエントリポイント」エラーを避けるため、UsageExample の Program を削除するか、
//          `#if VERYVIBE_USAGEEXAMPLE` でガードしてください。  :contentReference[oaicite:0]{index=0}
// ============================================================================

namespace LissajousTool
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using VeryVibe;

    /// <summary>
    /// LissajousTool のエントリポイント。
    /// Main は「最初の Chapter」「RootArg」「Stage.Run」のみを行い、残りは Chapter に委譲します（RULE #1）。
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// エントリポイント。ここでは副作用を最小化し、パイプラインの起動だけを行います。
        /// </summary>
        private static int Main(string[] args)
        {
            // ロガー（現状は Null。必要に応じて Console などの Provider を追加してください）
            ILoggerFactory loggerFactory = NullLoggerFactory.Instance;

            // パイプライン最初の章（CLI 解析）
            var firstChapter = new ParseCliChapter(loggerFactory);

            // RootArg（安全既定: DryRun=true, AllowOverwrite=false）
            var rootArg = new RootArg(rawArgs: args, dryRunDefault: true, allowOverwriteDefault: false);

            // VeryVibe ランタイム起動
            var stage = new Stage<IParseArgsArg>();
            stage.Run(firstChapter, rootArg);

            return 0;
        }
    }
}
// ここまで：コード生成手順 4（Main 実装）— UsageExample 側の Program を削除/ガードしてください（必須）
namespace LissajousTool
{
    using CommandLine;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using VeryVibe;

    internal sealed class ParseCliChapter : IChapter<IParseArgsArg>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;

        public ParseCliChapter(ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
            _log = _loggerFactory.CreateLogger<ParseCliChapter>();
        }

        // -----------------------------
        // CLI オプション定義（CommandLineParser）
        // -----------------------------
        private abstract class BaseOptions
        {
            // グローバル安全系
            [Option("confirm", HelpText = "実出力を許可します（既定は dry-run）。", Default = false)]
            public bool Confirm { get; set; }

            [Option("dry-run", HelpText = "常に検証のみを実行します。--confirm より優先度は低いです。", Default = false)]
            public bool DryRun { get; set; }

            [Option("allow-overwrite", HelpText = "既存ファイルへの上書きを許可します（既定: 不許可）。", Default = false)]
            public bool AllowOverwrite { get; set; }

            [Option('o', "output", HelpText = "出力先（拡張子により png/gif を自動判定）。", Required = true)]
            public string? Output { get; set; }

            [Option("verbose", HelpText = "詳細ログを有効化します。", Default = false)]
            public bool Verbose { get; set; }

            [Option("radians", HelpText = "角度の単位をラジアンとして解釈します（既定: 度）。", Default = false)]
            public bool UseRadians { get; set; }

            [Option("culture", HelpText = "解析に用いるカルチャ（例: ja-JP, en-US）。", Required = false)]
            public string? CultureName { get; set; }

            // キャンバス/スタイル
            [Option('w', "width", HelpText = "キャンバス幅(px)", Default = 1024)]
            public int Width { get; set; }

            [Option('h', "height", HelpText = "キャンバス高さ(px)", Default = 1024)]
            public int Height { get; set; }

            [Option('m', "margin", HelpText = "周囲マージン(px)", Default = 32)]
            public int Margin { get; set; }

            [Option("dpi", HelpText = "DPI（印刷用メタ）", Default = 96)]
            public int Dpi { get; set; }

            [Option("center-x", HelpText = "中心X座標(px)", Default = 512d)]
            public double CenterX { get; set; }

            [Option("center-y", HelpText = "中心Y座標(px)", Default = 512d)]
            public double CenterY { get; set; }

            [Option("samples", HelpText = "サンプル点数", Default = 4000)]
            public int Samples { get; set; }

            // リサージュ・パラメータ
            [Option("amp-x", HelpText = "X 振幅 A", Default = 450d)]
            public double AmpX { get; set; }

            [Option("amp-y", HelpText = "Y 振幅 B", Default = 450d)]
            public double AmpY { get; set; }

            [Option("freq-x", HelpText = "X 周波数 a", Default = 3)]
            public int FreqX { get; set; }

            [Option("freq-y", HelpText = "Y 周波数 b", Default = 2)]
            public int FreqY { get; set; }

            [Option('p', "phase", HelpText = "位相 δ（既定は度単位）", Default = 0d)]
            public double Phase { get; set; }

            // 線/色
            [Option("stroke-width", HelpText = "線幅(px)", Default = 2.0)]
            public double StrokeWidth { get; set; }

            [Option("opacity", HelpText = "不透明度(0..1)", Default = 1.0)]
            public double Opacity { get; set; }

            [Option("bg", HelpText = "背景色（#RRGGBB など）", Default = "#000000")]
            public string BgColor { get; set; } = "#000000";

            [Option("stroke", HelpText = "線色（#RRGGBB など）", Default = "#FFFFFF")]
            public string StrokeColor { get; set; } = "#FFFFFF";

            [Option("dash", HelpText = "ダッシュパターン（例: \"5,3\"）", Required = false)]
            public string? DashPattern { get; set; }

            [Option("antialias", HelpText = "アンチエイリアス有効", Default = true)]
            public bool Antialias { get; set; }

            [Option("angle-rule", HelpText = "角度走査規則ID（実装依存）", Default = "uniform-0..2pi")]
            public string AngleTraversalRule { get; set; } = "uniform-0..2pi";
        }

        [Verb("png", HelpText = "静止画 PNG を出力します。")]
        private sealed class PngOptions : BaseOptions
        {
            [Option("png-compress", HelpText = "PNG圧縮レベル(0..9)", Default = 6)]
            public int PngCompression { get; set; }

            [Option("metadata", HelpText = "PNG メタデータ（\"k=v; k2=v2\"）", Required = false)]
            public string? Metadata { get; set; }
        }

        [Verb("gif", HelpText = "アニメーション GIF を出力します。")]
        private sealed class GifOptions : BaseOptions
        {
            [Option("frames", HelpText = "フレーム数", Default = 120)]
            public int Frames { get; set; }

            [Option("fps", HelpText = "フレームレート", Default = 30d)]
            public double Fps { get; set; }

            [Option("duration", HelpText = "全体再生時間(ms)（fps と併用時は fps を優先）", Required = false)]
            public int? DurationMs { get; set; }

            [Option("loop", HelpText = "ループ回数（0=無限）", Default = 0)]
            public int Loop { get; set; }

            [Option("easing", HelpText = "補間関数（linear 等）", Default = "linear")]
            public string Easing { get; set; } = "linear";

            [Option("trail", HelpText = "残像フレーム尾数（0=なし）", Default = 0)]
            public int Trail { get; set; }

            // スイープ（列 or 範囲）
            [Option("phase-sweep", HelpText = "位相スイープ（例: \"0..360\" or \"0,90,180\"）", Required = false)]
            public string? PhaseSweep { get; set; }

            [Option("amp-x-sweep", HelpText = "AmpX スイープ", Required = false)]
            public string? AmpXSweep { get; set; }

            [Option("amp-y-sweep", HelpText = "AmpY スイープ", Required = false)]
            public string? AmpYSweep { get; set; }

            [Option("freq-x-sweep", HelpText = "FreqX スイープ", Required = false)]
            public string? FreqXSweep { get; set; }

            [Option("freq-y-sweep", HelpText = "FreqY スイープ", Required = false)]
            public string? FreqYSweep { get; set; }

            [Option("color-sweep", HelpText = "色スイープ（#RRGGBB の列）", Required = false)]
            public string? ColorSweep { get; set; }

            [Option("width-sweep", HelpText = "線幅スイープ", Required = false)]
            public string? WidthSweep { get; set; }
        }

        /// <inheritdoc/>
        public void Handle(IParseArgsArg arg, IContextBuffer<IParseArgsArg> buffer)
        {
            try
            {
                // RawArgs の先頭が "png" / "gif" でなくとも、出力拡張子で後続が分岐できるため、
                // ここでは両 Verb を登録してパースする。
                var parser = new Parser(cfg =>
                {
                    cfg.EnableDashDash = true;
                    cfg.HelpWriter = null; // ここではヘルプ出力しない（--help は別途 CLI の責務）
                    cfg.CaseInsensitiveEnumValues = true;
                });

                var result = parser.ParseArguments<PngOptions, GifOptions>(ToArray(arg.RawArgs));

                result
                    .WithParsed<PngOptions>(opts => ApplyCommon(arg, opts, isGif: false, png: opts, gif: null))
                    .WithParsed<GifOptions>(opts => ApplyCommon(arg, opts, isGif: true, png: null, gif: opts))
                    .WithNotParsed(errs =>
                    {
                        // 解析失敗：短文メッセージに要約（詳細はログ）
                        _log.LogWarning("CLI 解析に失敗しました。エラー数={Count}", errs is ICollection<Error> c ? c.Count : -1);
                        // ここで終了（RULE #13）
                    });
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "コマンドライン解析中に予期せぬエラーが発生しました。");
                return;
            }

            // 以降は BuildModelChapter が担当
            buffer.PushBack(new VeryVibe.ChapterContext<IBuildModelArg>(new BuildModelChapter(_loggerFactory), (IBuildModelArg)arg));
        }

        private static string[] ToArray(IReadOnlyList<string> raw)
        {
            if (raw is string[] arr) return arr;
            var a = new string[raw.Count];
            for (int i = 0; i < a.Length; i++) a[i] = raw[i];
            return a;
        }

        private void ApplyCommon(IParseArgsArg arg, BaseOptions opt, bool isGif, PngOptions? png, GifOptions? gif)
        {
            // 1) 文化圏（数値の . と , の扱いなど）— 多重設定は禁止
            if (!string.IsNullOrWhiteSpace(opt.CultureName))
            {
                try
                {
                    var culture = CultureInfo.GetCultureInfo(opt.CultureName!.Trim());
                    arg.SetCulture(culture);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "カルチャ名が不正です: {Name}", opt.CultureName);
                    // 不正でも継続（既定カルチャで解釈）
                }
            }

            // 2) 上流オプションの生値（RootArg の内部 API で注入）
            if (arg is RootArg root)
            {
                // Confirm 優先、なければ DryRun フラグ、どちらも無ければ規定（true）
                bool dryRunOption = opt.Confirm ? false : (opt.DryRun ? true : true);
                bool allowOverwriteOption = opt.AllowOverwrite;

                root.SetParseInputs(
                    rawArgs: arg.RawArgs,
                    useRadians: opt.UseRadians,
                    verbose: opt.Verbose,
                    dryRunOption: dryRunOption,
                    allowOverwriteOption: allowOverwriteOption,
                    outputPathRaw: SafeOutput(opt.Output));
            }

            // 3) 中流（IBuildModelArg）へ正規化前の値を反映（Chapter 間の継続性を保つ）
            var model = (IBuildModelArg)arg;
            model.Width = opt.Width;
            model.Height = opt.Height;
            model.Margin = opt.Margin;
            model.Dpi = opt.Dpi;
            model.CenterX = opt.CenterX;
            model.CenterY = opt.CenterY;
            model.Samples = opt.Samples;

            model.AmpX = opt.AmpX;
            model.AmpY = opt.AmpY;
            model.FreqX = opt.FreqX;
            model.FreqY = opt.FreqY;
            // Phase は度/ラジアンの正規化は BuildModel で行う前提。ここでは「生値」を PhaseRad に一旦詰める。
            model.PhaseRad = opt.Phase;

            model.StrokeWidth = opt.StrokeWidth;
            model.Opacity = opt.Opacity;
            model.BgColor = opt.BgColor ?? "#000000";
            model.StrokeColor = opt.StrokeColor ?? "#FFFFFF";
            model.DashPattern = opt.DashPattern;
            model.Antialias = opt.Antialias;

            var render = (IRenderArg)arg;
            render.AngleTraversalRule = opt.AngleTraversalRule ?? "uniform-0..2pi";

            if (!isGif && png is not null)
            {
                var enc = (IEncodePngArg)arg;
                enc.PngCompressionLevel = png.PngCompression;
                enc.PngMetadata = png.Metadata;
            }
            else if (isGif && gif is not null)
            {
                var enc = (IEncodeGifArg)arg;
                enc.Frames = gif.Frames;
                enc.Fps = gif.Fps;
                enc.DurationMs = gif.DurationMs;
                enc.LoopCount = gif.Loop;
                enc.Easing = gif.Easing ?? "linear";
                enc.Trail = gif.Trail;

                // スイープの生パース（範囲 "a..b[:s]" は start/end の 2点に限定格納。詳細展開は後段）
                enc.PhaseSweepRad = TryParseDoubles(gif.PhaseSweep, allowRange: true);
                enc.AmpXSweep = TryParseDoubles(gif.AmpXSweep, allowRange: true);
                enc.AmpYSweep = TryParseDoubles(gif.AmpYSweep, allowRange: true);
                enc.FreqXSweep = TryParseInts(gif.FreqXSweep, allowRange: true);
                enc.FreqYSweep = TryParseInts(gif.FreqYSweep, allowRange: true);
                enc.ColorSweep = TryParseColors(gif.ColorSweep);
                enc.StrokeWidthSweep = TryParseDoubles(gif.WidthSweep, allowRange: true);
            }
        }

        private static string SafeOutput(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            // ここでは正規化は行わない（BuildModel/Write で実施）。最低限のトリムのみ。
            return path.Trim().Trim('"');
        }

        // -----------------------------
        // 軽量パーサ群（列 or 範囲）
        // -----------------------------
        private static IReadOnlyList<double>? TryParseDoubles(string? spec, bool allowRange)
        {
            if (string.IsNullOrWhiteSpace(spec)) return Array.Empty<double>();
            var s = spec.Trim();
            if (allowRange && TryParseRange(s, out var start, out var end, out var step))
            {
                // 本章では start/end のみ格納（細分化は Build/Render 側）
                return new double[] { start, end };
            }
            var list = new List<double>();
            foreach (var tok in s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (double.TryParse(tok, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) list.Add(v);
            return list.ToArray();
        }

        private static IReadOnlyList<int>? TryParseInts(string? spec, bool allowRange)
        {
            if (string.IsNullOrWhiteSpace(spec)) return Array.Empty<int>();
            var s = spec.Trim();
            if (allowRange && TryParseRange(s, out var start, out var end, out var step))
            {
                return new int[] { (int)Math.Round(start), (int)Math.Round(end) };
            }
            var list = new List<int>();
            foreach (var tok in s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (int.TryParse(tok, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) list.Add(v);
            return list.ToArray();
        }

        private static IReadOnlyList<string>? TryParseColors(string? spec)
        {
            if (string.IsNullOrWhiteSpace(spec)) return Array.Empty<string>();
            var s = spec.Trim();
            var list = new List<string>();
            foreach (var tok in s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                // 許容形式の軽量チェック（#RGB/#RRGGBB or 名前）。厳密検証は BuildModelChapter 側。
                var t = tok;
                if (t.StartsWith('#') && (t.Length == 4 || t.Length == 7)) { list.Add(t); continue; }
                if (!t.Contains(' ') && !t.Contains('\t')) { list.Add(t); continue; }
            }
            return list.ToArray();
        }

        private static bool TryParseRange(string s, out double start, out double end, out double step)
        {
            start = end = step = 0;
            var parts = s.Split("..", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2) return false;
            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out start)) return false;

            var right = parts[1];
            var stepIdx = right.IndexOf(':');
            if (stepIdx >= 0)
            {
                var endStr = right.Substring(0, stepIdx);
                var stepStr = right[(stepIdx + 1)..];
                if (!double.TryParse(endStr, NumberStyles.Float, CultureInfo.InvariantCulture, out end)) return false;
                if (!double.TryParse(stepStr, NumberStyles.Float, CultureInfo.InvariantCulture, out step)) step = 0;
            }
            else
            {
                if (!double.TryParse(right, NumberStyles.Float, CultureInfo.InvariantCulture, out end)) return false;
            }
            return true;
        }
    }
}