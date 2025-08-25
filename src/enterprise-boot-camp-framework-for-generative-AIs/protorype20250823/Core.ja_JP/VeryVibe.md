生成RULE 兼 設計書き込みテンプレート
==========
Version: 0.1.1 (2024-08-23)

OCPバイブコーディング用

---

本ファイルはプロンプトも含め、MIT License の下で公開されています。

 Copyright (C) 2025 cozyupk [https://github.com/cozyupk](https://github.com/cozyupk)

 * MIT License に従い、商用・非商用を問わず自由にご利用いただけます。

 * なお、本プロンプトおよびそれに含まれるソースコードは、は研究・学習・人類と生成AIの進化を目的とした未完成版であることを付記します。

---

This file, including the prompts, is released under the MIT License.  

Copyright (C) 2025 cozyupk  
[https://github.com/cozyupk](https://github.com/cozyupk)  

* You are free to use it for both commercial and non-commercial purposes, in accordance with the MIT License.  
* Please note that this prompt and the included source code are an unfinished version, intended for research, learning, and the evolution of humanity together with generative AI.  

---
## 1. 本ファイルの目的
1. 本ファイルでは、特定の手法に従ったC#のソースコード生成方法を示します。

1. 2. また、生成された特定ドメインのコードに関しての設計を、生成AIの指示に従い、末尾に追記していきます。

## 2. ユーザーへの確認ルール

次の場合、必ずユーザーにチャットで意思を確認し、判断を仰いでください。

1. プロンプトファイルの記述が曖昧で、複数の意味に取れる場合  
   → 例：「Argを使う」とあるが、どの責務まで含めるか不明なとき

2. プロンプトファイルの内容と、ユーザーのチャット入力内容が矛盾している場合  
   → 例：RULEでは「RootArgを一元的に用いる」とあるのに、ユーザーが「Argを分割して実装して」と入力したとき

3. 下流工程（実装工程など）で、上流工程（設計工程など）に問題があると発覚した場合  
   → 例：RootArgに必要なメンバが定義されておらず、コード生成ができないとき

4. その他、AIの判断だけで進めると設計の意図や品質を損なう恐れがある場合  
   → 例：例外処理の責務がどのChapterに属するか不明なとき
 
5. 生成中のソースコードを参照したくなった場合  
   → クラス名やメソッド名などを列挙し、ユーザーにチャットへのアップロードを依頼すること。  
      （AI 自身が暗黙に「コードを把握した」前提で進めてはならない）
 
## 3. ChapterVibe フレームワークとその利用方法
 1. ChapterVibe 名前空間は、ChapterVibe フレームワークのコア機能を提供します。 

 2. コードを生成する際には、このコア機能を活用し、各 "Chapter を SOLID 原則に準拠して実装してください。

 3. コードを生成・変更する際には以下のルールに従ってください。
     1. コードを適切なフェーズに分割し、それらの各 "Chapter" に独立した機能を持たせること。

     2. Mainメソッドは短く保つこと。そのために、コマンドラインパラメータの解析や各種I/Oの前処理等にも、独立した "Chapter" を用意すること。

     3. 各 Chapter を実装するクラスは IChapter を実装し、必要最小限の役割インタフェースにのみ依存すること。

     4. この項は、**生成AIが誤解しやすいため特に注意:** IChapter や IChapterContext の型引数の反変性を成立させるため、**上流（入力に近い）側は、下流（出力に近い）側の役割インタフェイスを継承して“より具体的”になる**継承鎖とする。
       「上流IF は下流IF を継承する（一般→具体）」で一貫させる。
       例（一般 → 具体 の順）: ChapterVibe.IArg ← IProcessingArg ← IParseArgsArg
       実行時には、反変 `IChapter<in TArg>` の性質により、下流を処理する Chapter が、
       その親である“より具体的な”上流IF（＝実際の Arg）を安全に受理できる。
       下記ソースコードのChapterVibe.UsageExample 名前空間を参照。
       (このソースコードは、.cs ファイルとしてコンパイルが通り、意図道理動作することを確認済です。)

     5. ChapterVibe.IArg は、すべての役割インターフェースの基底となるマーカーインタフェース（メンバーなし）。

     6. IChapter 実装クラスの Handle メソッドは直接呼び出さず、Stage クラスや buffer.PushBack() を通じて呼び出すこと。IChapter 実装クラスから IChapter 実装クラスの Handle を直接呼び出してはならない。

ChapterVibeフレームワーク利用例:
```csharp
// ============================================================================
// ChapterVibe.UsageExample（入れ替え候補サンプル / RULE準拠・XMLコメント付）
// ----------------------------------------------------------------------------
// このセクションは ChapterVibe フレームワークの最小利用例（Hello → World/WorldWithCulture）です。
// 重要:
// - 本ファイルは「生成AIが吐いたコードのレビュー用・設計テンプレ」の一部です。
// - Arg インタフェースは「反変方針」（上流=具体 ← 下流=一般を継承）に従います（RULE #25 参照）。
// - Chapter 連結は PushBack を用いて示し、RootArg は明示的実装で提供します（RULE #22 参照）。
// - 例外/入出力は Chapter 内で処理し、外へ伝播させません（RULE #28 参照）。
// - 章からは対応する役割インタフェースのみを利用し、RootArg 直参照・無関係メンバへのキャストは禁止です（RULE #23/#24 参照）。
// 禁止:
// - RootArg を直接参照しない（必ず役割インタフェース経由でアクセス：RULE #24）。
// - 章間で new インスタンス詰め替えを行って引き渡さない（状態の二重化/破綻の温床）。
// - IContextBuffer<out T> / IChapterContext<in T> の in/out を入れ替えない（RULE #25）。
// ============================================================================

namespace ChapterVibe.UsageExample
{
    using System;
    using System.Globalization;
    using ChapterVibe;

    // ------------------------------------------------------------------------
    // Arg インタフェース定義（一般 → 具体の順を維持／直列 or 多重継承を明示）
    // ------------------------------------------------------------------------

    /// <summary>
    /// カルチャ付き「World」出力に必要な最小契約（下流・一般）。
    /// <para>RULE #25: 反変方針により上流でより具体的IFへ拡張します。</para>
    /// </summary>
    internal interface IWorldWithCultureArg : IArg
    {
        /// <summary>残り出力回数（ゼロで完了）。</summary>
        int WorldWithCultureCount { get; set; }

        /// <summary>
        /// 行出力シンク（例：<see cref="Console.WriteLine(string)"/>）。
        /// 例外やI/O失敗は Chapter 内で処理します（RULE #28）。
        /// </summary>
        Action<string> WriteLineAction { get; }

        /// <summary>出力に用いるカルチャ。未設定での参照は無効（例外）。</summary>
        CultureInfo Culture { get; }
    }

    /// <summary>
    /// カルチャ無しの「World」出力に必要な最小契約（下流・一般）。
    /// </summary>
    internal interface IWorldArg : IArg
    {
        int WorldCount { get; set; }
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

        /// <summary>カルチャが設定済みなら <c>true</c>。</summary>
        bool IsCultureSet { get; }
    }

    /// <summary>
    /// CLI 引数を解析して <see cref="HelloChapter"/> へ制御を渡す最上流契約（上流・最具体）。
    /// </summary>
    internal interface IParseArgsArg : IHelloArg
    {
        /// <summary>コマンドライン引数（実行ファイル名は含まない）。</summary>
        string[] Args { get; }

        /// <summary>カルチャ設定（<c>null</c>→設定、二重設定は禁止）。</summary>
        void SetCulture(CultureInfo culture);
    }

    // ------------------------------------------------------------------------
    // Chapter 実装（例外/I-O は Chapter 内で完結：RULE #28）
    // ------------------------------------------------------------------------

    /// <summary>カルチャ付きで「World in {Culture}」を出力する章。</summary>
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
                // RULE #28: I/O などの失敗は Chapter 内で処理
                arg.WriteLineAction($"[WorldWithCulture] output failed: {ex.Message}");
                return;
            }

            arg.WorldWithCultureCount--;
            if (arg.WorldWithCultureCount > 0)
            {
                // RULE #25: 反変×共変の符号合成に基づく PushBack
                buffer.PushBack(new ChapterContext<IWorldWithCultureArg>(this, arg));
            }
            else
            {
                arg.WriteLineAction("All worlds with culture processed.");
            }
        }
    }

    /// <summary>「World」を出力する章（カルチャ無し）。</summary>
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

    /// <summary>「Hello」を出力し、回数が尽きたらカルチャ有無に応じて次章へ分岐する章。</summary>
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
                // 分岐: 機能分岐なので兄弟IFへルーティング（RootArg 直参照は禁止：RULE #23/#24）
                if (arg.IsCultureSet)
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

    /// <summary>CLI を解析し、必要に応じてカルチャを設定して <see cref="HelloChapter"/> に渡す章。</summary>
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
                // RULE #28: 解析失敗はここで要約して通知し、外に出さない
                arg.WriteLineAction($"[ParseArgs] parsing failed: {ex.Message}");
                return;
            }

            // 次章へ
            buffer.PushBack(new ChapterContext<IHelloArg>(new HelloChapter(), arg));
        }
    }

    // ------------------------------------------------------------------------
    // RootArg 実装（明示的実装：RULE #22）
    // ------------------------------------------------------------------------

    /// <summary>
    /// すべての役割インタフェースを<strong>明示的に</strong>実装するルート引数。
    /// <para>内部状態は private メンバで保持し、アクセスは必ずインタフェース経由（RULE #22）。</para>
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
        bool IHelloArg.IsCultureSet => Culture != null;

        /// <inheritdoc/>
        CultureInfo IWorldWithCultureArg.Culture
            => Culture ?? throw new InvalidOperationException("Culture is not set.");

        /// <inheritdoc/>
        Action<string> IHelloArg.WriteLineAction => WriteLineAction;

        /// <inheritdoc/>
        Action<string> IWorldArg.WriteLineAction => WriteLineAction;

        /// <inheritdoc/>
        Action<string> IWorldWithCultureArg.WriteLineAction => WriteLineAction;

        // 内部実装（必ず IF 経由でアクセスさせる：RULE #22）
        private Action<string> WriteLineAction { get; } = Console.WriteLine;
        private string[] Args { get; }
        private CultureInfo? Culture { get; set; }

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
    // Main（UsageExample 用 / 指針: Main は短く保つ）
    // ------------------------------------------------------------------------

    /// <summary>
    /// UsageExample のエントリポイント
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main は Stage 構築と最初の Chapter 起動のみを行います（Main最小化の指針）。
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
// ============================================================================
// End: ChapterVibe.UsageExample（RULE準拠・XMLコメント付）
// ============================================================================
```
## 4.ソースコード生成上のその他のルール
   1.（上位原則・不可侵）データに破壊的な変更を行わない（No Destructive Changes）。既存データの削除・上書き・不可逆変換は既定で禁止。必要な場合は「別名への新規作成」＋「明示フラグ」＋「事前検証」を必須とする。

   2.（上位原則・入力防御）外部から与えられる全入力（CLI引数、環境変数、ファイル名、パス、URL、JSON、テンプレート文字列など）はセキュリティ観点で必ずサニタイズ／バリデーションを行う（allowlist 方式を優先、denylist は補助）。

   3.（上位原則・最小特権）処理に必要な最小権限のみを使用する（ファイル権限、ネットワーク、認証情報）。昇格や広い権限が必要な場合は、その根拠をコメントに明記（RULE #12/#11 に従いインラインで理由を残す）。

   4.（上位原則・可観測性）ログは秘匿情報を含めず、再現に必要な最小限のメタ情報のみを構造化形式で残す（PII/資格情報は常にマスク）。

   5.（上位原則・既定は安全）既定値は常に「安全側」に倒す（外部送信しない／上書きしない／実行しない）。利便性のために安全側を緩める場合は、明示的なフラグとコメントで根拠を示す（RULE #11）。

   6.（入力バリデーション）パラメータは型・範囲・サイズ・形式（正規表現）を検証し、未指定・空・制御文字・改行・NULL バイトを拒否する。パスは GetFullPath で正規化し、必ずベースディレクトリ配下かを StartsWith で検査。シンボリックリンクや再解析ポイントは許可しない。

   7.（出力エンコード）外部へ出す文字列はシンクに応じて必ずエンコード／エスケープ（HTML/CSV/JSON/シェル引数）。文字列連結でコマンドやクエリを組み立てない。コマンド実行は UseShellExecute=false、引数は配列で分離。SQL/検索クエリ等は常にパラメータ化／プリペアド。

   8.（ファイルI/Oの安全化）ファイル書き込みは上書き禁止（FileMode.CreateNew）。一時ファイルに書き込み、検証後に原子的 Move（上書き不可）。対象拡張子／最大サイズに上限を設ける。FileShare は最小で開く（読み: Read、書き: None）。

   9.（ネットワークの安全化）デフォルトで外部送信禁止。必要な場合のみドメイン allowlist＋TLS（≥1.2）＋証明書検証有効。全リクエストにタイムアウト／リトライ（指数バックオフ）／CancellationToken を設定。リダイレクトとプロキシの扱いを明示。

   10.（直列化/逆直列化）BinaryFormatter 等の危険APIは禁止。System.Text.Json を用い、MaxDepth を設定、既知の型にバインド。ポリモーフィックな任意型解決や Type 名埋め込みは使わない。

   11.（秘密情報）APIキー・トークン・接続文字列をソースに埋め込まない。取得は注入インタフェースや OS セキュアストア経由。例外・ログに秘匿情報を出さない（マスク／要約）。

   12.（暗号/乱数）セキュリティ用途の乱数は Random ではなく RandomNumberGenerator を使用。独自暗号は禁止。標準ライブラリのみ使用。

   13.（並行性/リソース）長時間処理は定期的にキャンセルを確認。IDisposable は using で確実に破棄。ロック範囲は最小化し、I/O／ユーザコードの呼び出しはロック外で行う（既存の RunAll 実装に準拠）。

   14.（確認フロー）破壊的になり得る操作（重名出力、削除、移動）は二段階確認とする：Dry-run（検出のみ）→ Confirm フラグで実行。可能なら成果物は別ディレクトリ／バージョニングされたパスに出力。

   15.（テスト可能性）安全性ルールを守るためのユニットテストを用意（例：ディレクトリトラバーサル拒否、上書き禁止、例外時のロールバック保証、エンコード適用）。生成AIがコードを変更してもテストがガードする構成にする。

   16.（AI/自動生成対策）上記ルールに反するコード片（Process.Start のシェル連結、FileMode.Create、未検証パス等）を“禁止パターン”としてコメントに明記し、代替の安全テンプレートをすぐ下に置く（RULE #11）。レビュー時はまず禁止パターン検索から始める。

   17.（失敗時の挙動）失敗は静かに握りつぶさない。ユーザ向けには安全なメッセージ、ログには再現可能な最小情報を残し、中断かフォールバックかの方針を Chapter ごとに明示（RULE #12 と一貫）。

   18.（デフォルト設定の固定）外部送信=無効、上書き=無効、実行=無効、インタラクティブ確認=必要、を既定とし、解除には明示的なフラグ名（例：--allow-overwrite, --allow-network）を必須とする。

   19. インタフェース定義は対応する Chapter の直前に置くこと。

   20. 各インタフェースメンバには明確な XML コメントを記述すること。

   21. 実行時引数は RootArg 1つに統一すること。

   22. RootArg ではすべての役割インタフェースを**明示的に実装**すること（メンバは必ずインタフェース経由でのみアクセスできるようにする）。

   23. 各 Chapter では対応する Arg インタフェース型のみを利用し、他のインタフェースや RootArg にキャストして無関係なメンバにアクセスしないこと。

   24. RootArg を直接参照することは禁止。Setter も含め、必ずインタフェース経由でアクセスすること。

   25. IContextBuffer<out TArg> の out と IChapterContext<in TArg> の in のバリアンスを維持すること。（理由：入力(−)×反変(−)=共変(＋) の符号合成で CS1961 を満たしているため）CS1961 違反を起こさないよう out/in の使用位置を保証すること。

   26. フィールドよりプロパティを優先すること。ただし、内部ロック用や固定コレクションなど従来的用途においては private readonly フィールドの利用を認める。

   27. 設計上の選択が必然ではなくフレームワークの方針による場合は、その旨と意図をインラインコメントに記述し、ChapterVibe 外の文脈（および AI/自動レビュー）でも自己説明的になるようにすること。例: readonly フィールドではなくプロパティを選択する、明示的インタフェース実装、パイプラインやリフレクションのための非標準的な命名/可視性、スレッドセーフ性やアロケーションに関するトレードオフ。

   28. I/O や外部プロセス、環境依存呼び出しなどで発生しうる例外は Chapter 内で処理する。既定では Handle の外へ例外を伝播させない。致命的障害など呼び出し側の判定が必要な場合は、呼び出し元の役割IFに IErrorSink 等の“サニタイズ済みエラーの記録口”を定義し、任意のエラーオブジェクト（例外の要約・構造化メタのみ、PII/スタックを含めない）を記録することで逆伝播させる。例外オブジェクト（Exception）自体を IArg に格納して保持してはならない（RULE #25/#32）。

   29. .NET 8.0 の言語機能を活用すること。Nullable を有効化すること。ImplicitUsings は無効化すること。アクセス修飾子は internal をデフォルトとし、最小に絞ること。

   30. コードのコンパイルや実行に .csproj 側の設定（例: TargetFramework, UseWindowsForms, SupportedOSPlatformVersion, プラットフォーム依存パッケージ参照など）が必須な場合は、プログラムの冒頭コメントにその旨を明記すること。

   31. コレクションの契約は必ずインタフェース型（IReadOnlyList<T>, IEnumerable<T> など）で表現すること。Arg や Chapter の契約において、List<T> や配列 (T[]) といった具象型を直接公開してはならない。

   32. null 合体演算子（?? / ??=）を使用する場合、両辺は**契約型に揃える**こと。List<T> と T[] を混在させてはならない。

   33. 空のデフォルト値にはキャッシュされた静的定数を使うこと。例: private static readonly IReadOnlyList<T> Empty = Array.Empty<T>(); public IReadOnlyList<T> Values => _values ?? Empty;  

   34. バッキングフィールドの型は契約型と完全に一致させること。例: private IReadOnlyList<T>? _points;   // List<T>? ではなく  

   35. ミュータブルな入力（例: SetNormalizedPoints）を受け取る場合、受け取り側は IEnumerable<T> など広く受け入れてよい。ただし内部保存は契約型に揃えること。コピーするか否かの方針は明示的にコメントに記載すること（RULE #11）。

   36. タプル要素の名前は型の同一性には影響しないが、コードベース内で一貫させること（例: (double x, double y) を推奨）。

   37. UX も考慮すること。数値・日付・時刻は現在のロケールに従ってフォーマット（例: ToString("N", CultureInfo.CurrentCulture)）。ログ/プログラム的出力はカルチャ非依存（例: 日付は ISO 8601）。エラーメッセージやユーザー向け文字列はローカライズ／翻訳ポリシーに従うこと。

   38. コード中のコメント、以下のチャット出力はすべて日本語で記述すること。テンプレも日本語で記述すること。

   39. CLIフラグ規約・固定・要件上不要なものは省略可・短縮系は状況に合わせて決定

CLI フラグ規約

| フラグ | 型 | 既定 | 危険度 | ドメイン | 説明 | 依存/排他 |
|--------|----|------|--------|----------|------|-----------|
| `--dry-run` | bool | true | safe | core | 実行を抑止（安全側スタート）。既定は常に dry-run モード | 排他: `--execute` |
| `--plan-only` | bool | false | safe | core | 実行計画を出力するのみ（dry-run強化版）。`--execute`と排他 | 排他: `--execute` |
| `--execute` | bool | false | dangerous | core | 実際に本番実行を行う（dry-runを強制解除）。「本当にやる」時だけ指定 | 排他: `--dry-run`, `--plan-only`; Alias: `--run-for-your-life` |
| `--allow-overwrite` | bool | false | risky | filesystem | 既存ファイルや成果物の上書きを許可 | – |
| `--allow-update` | bool | false | dangerous | database | DB/永続ストアの変更（INSERT/UPDATE/DELETE）を許可。SELECT 等の読み取りは常に可 | – |
| `--allow-network` | enum(`readonly\|full`) | readonly | risky | network | ネットワークアクセスを許可。省略時は `readonly`（安全側） | – |
| `--allow-exec` | bool | false | dangerous | exec | 外部プロセス・スクリプト実行を許可 | – |
| `--no-admin` / `--allow-admin` | bool | no-admin | dangerous | privilege | 管理者権限での実行を禁止（既定）。`--allow-admin` を明示した場合のみ許可 | – |
| `--scope-dir=<PATH>` | string (multi) | （なし） | safe | filesystem | 触れてよいローカルFSルートを制限（chroot 的）。複数可。相対パス/.. 跨ぎ禁止 | – |
| `--scope-db=<NAME or DSN>` | string (multi) | （なし） | safe | database | アクセス許可するDB名/接続先を限定。例: `--scope-db=Main` | – |
| `--tenant=<TENANT-ID>` | string | （未指定不可） | safe | tenancy | マルチテナント環境の明示スコープ。指定必須 | – |
| `--max-concurrency=<N>` | int | 1 | risky | execution | 最大並列数を制御し副作用の暴走を防ぐ | – |
| `--rate-limit=<OPS/sec>` | int | （なし） | risky | network/api | 外部APIやメール送信などのスロットリング上限 | – |
| `--secrets-from=<PROVIDER>` | enum(`env\|file\|vault`) | env | risky | secrets | 秘密情報の取得元を制御 | – |
| `--secrets-allowlist=<KEY1,...>` | list | （なし） | safe | secrets | 参照を許可する秘密キーを制限 | – |
| `--no-telemetry` / `--telemetry` | bool | no-telemetry | risky | telemetry | 利用状況収集の禁止/許可。PIIは常に収集禁止 | – |
| `--compliance=<MODE>` | enum(`pci\|hipaa\|none`) | none | safe | compliance | 準拠モードを設定し、ログ/一時ファイルを自動制御 | – |
| `--deterministic` | bool | false | safe | testing | 乱数/時刻を固定化し再現性を確保。seedは `--seed=<INT>` | – |
| `--cache[=on\|off]` | bool | on | risky | performance | キャッシュ利用ポリシー。副作用系操作と併用する場合は `off` を推奨 | – |
| `--retry[=N]` / `--retry-backoff[=ms]` | int | 0 | risky | network/io | 一時エラー時のみ再試行。永続エラー(4xx)は即中断 | – |
| `--notify=<channel>` | string | stdout | safe | reporting | 実行結果の通知先（stdout/stderr/json/slack/mailなど） | – |
| `--verbose` | count | 0 | safe | core | ログ詳細度。`-v`, `-vv` などの繰り返し指定で段階的に増加 | – |
| `--version` | bool | false | safe | core | バージョン情報を表示して終了 | 排他: 他の全フラグ |
| `--help` | bool | false | safe | core | 使用方法を表示して終了 | 排他: 他の全フラグ |


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
RULE: ChapterVibe 名前空間での定義の絶対参照（IChapter/IContextBuffer/IChapterContext/IArg）
==========================================================================

[目的]
  - ChapterVibe コアにあるランタイム契約（IArg / IChapter<TArg> / IContextBuffer<out TArg> / IChapterContext<in TArg>）を
    全コンポーネントが一貫して参照し、各機能側での再定義・影の型の混入を防止する。

[必須事項]
  1) ChapterVibe 名前空間の明示
   - すべての Chapter 実装を含むファイルは、ファイル先頭（namespace の内側）に以下を必ず記すこと:
        using ChapterVibe;

  2) 再定義の禁止
      - 次のいずれの型も、アプリ/機能側（例: ChapterVibe.Cal 等）で新規に定義してはならない:
          interface IArg
          interface IChapter<in TArg>
          interface IContextBuffer<out TArg>
          interface IChapterContext<in TArg>
      - これらの「同名/同義の型」を別 namespace で宣言すること（影武者定義）も禁止。
 
  3) 便利クラスの取り扱い
      - ChapterVibe 名前空間に ChapterContext<T> / NoopContextBuffer 等の“実装クラス”が存在する場合は原則としてそれを使用する。
      - ChapterVibe 名前空間に未提供の場合や継承拡張したい場合、独自実装したい場合、機能側で実装を定義してよい。
      - 機能側で実装する場合、ChapterVibe 名前空間に存在するクラスと異なる名前を付けること。
 
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
- Does the agreed specification align with the ChapterVibe RULE set (SOLID principles, safety principles, least privilege, No Destructive Changes, etc.)?
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
 ルール: ChapterVibe RULE 準拠。契約型はインタフェース（IReadOnlyList<T> 等）で公開。配列や List<T> を直接返さない。
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
**重要(必ずユーザーに報告 or 依頼)**: この時点で ChapterVibe.UsageExample の実装は削除するか、その旨をユーザーに依頼(削除依頼)すること。
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

namespace ChapterVibe
{
    using System;
    using System.Collections.Generic;

    // ----- ChapterVibe Framework -----
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
 * // --- ユーザー向け ---
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
_ = "※ このプロンプトも含め、このプロンプト以降、ChapterVibe_ja-JP.cs をアップロード/更新しながら進めていきます。";
/*
 Windows環境上のC#で、パラメータを指定してリサージュ曲線を描き .png ファイルとして保存するコンソールプログラムを書きたいです。
また、アニメーションgif出力にも対応したいです。
できるだけ柔軟なプログラムとするためには、どのようなコマンドラインオプションが必要ですか？ 
--help で表示される Usage 風に、説明部分は日本語で答えてください。
この .cs ファイルの RULE や手順に従って生成することを想定しています。 
 */
_ = "「作るものの定義」準備が整ったら、次のようなプロンプトでコード生成を開始します。";
_ = "※ このプロンプトも含め、このプロンプト以降、ChapterVibe_ja-JP.cs をアップロード/更新しながら進めていきます。";
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

