// 本ファイルのライセンスはプロンプトを含め MIT License です。
// Copyright (C) 2025 cozyupk
// https://github.com/cozyupk
// ※ このコードおよびプロンプトは研究・学習用途の公開です。
//    未完成の部分や不具合を含みますが、自由にご利用ください。

// The license of this file, including the prompts, is MIT License.
// Copyright (C) 2025 cozyupk
// https://github.com/cozyupk
// Note: This code and its prompts are released for research and educational use.
//       It is incomplete and contains bugs, but you are free to use it.

#nullable enable

// --- ユーザー向け ---
// ■■■ このファイルを ChatGPT とのバイブコーディングで利用する場合は、チャットの冒頭で次のプロンプトを入力することを推奨します。■■■ 
/*
 - ここから先、このチャットでは私との会話履歴を一切使わずに答えてください。
 - 会話の中でソースコード改善案を提案する場合は、まずはコード改善案を提示することの許可をユーザーに求めてください。
*/
// ■■■ その後、このファイルをアップロードすると同時に、次のようなプロンプトで「作るもの」の定義をしていきましょう。■■■ 
/*
 Windows環境上のC#で、パラメータを指定してリサージュ曲線を描き .png ファイルとして保存するコンソールプログラムを書きたいです。
また、アニメーションgif出力にも対応したいです。
できるだけ柔軟なプログラムとするためには、どのようなコマンドラインオプションが必要ですか？ 
--help で表示される Usage 風に、説明部分は日本語で答えてください。
この .cs ファイルの RULE や手順に従って生成することを想定しています。 
 */
// ■■■ 準備が整ったら、次のようなプロンプトでコード生成を開始します。■■■ 
/*
それでは、このチャットでの今までの会話を参照して、コメント生成手順 1. を実施してください。
*/
// ■■■ ChatGPT が「コピペできない」形で応答してきた場合は、次のようなプロンプトを入力してみてください。■■■ 
/*
コピペ可能なようにコードブロックで出力してください。
 */
// ■■■ 出力の改善を求める場合は、次のようなプロンプトを入力してみてください。■■■ 
/*
先ほど提案してくれた Usage 風の説明も含めてください。また、コピペ可能なようにコードブロックで出力してください。
*/
// ■■■ あなたはこのファイルにChatGPTが出力したコメントやコードを貼り付けて、次のプロンプトで                                  ■■■
// ■■■ 更新したファイルを再度アップロードしつつ、「〇〇手順〇を実施してください」とプロンプティングすることで、               ■■■ 
// ■■■ バイブコーディングを進めることができます。                                                                             ■■■ 
// ■■■ その形で コメント生成手順 1. ～ コメント生成手順 6. を実施し、その後、コード生成手順も順に実施していきます。           ■■■ 
// ■■■ コード生成手順 4. は、ChatGPTが「対象なし」を出力するまで繰り返し実施します。                                          ■■■ 
// ■■■ 各段階でコンパイルエラーや警告が発生した場合は、あなたが修正して GPT にフィードバックするか、                          ■■■ 
// ■■■ 必要があれば GPT にスクリーンショットとソースコードを提供し、修正を依頼してください。                                  ■■■ 
// --- END ユーザー向け ---

// --- RULE ---
// VeryVibe 名前空間は、VeryVibe フレームワークのコア機能を提供します。 
// コードを生成する際には、このコア機能を活用し、各 Chapter を SOLID 原則に準拠して実装してください。
// VeryVibe.UsageExample 名前空間は使用例を示すためのものです。 こちらはコード生成後には削除してください。
// また、RULE ブロックもコード生成後には削除してください。
//
// このファイル内のコードを生成・変更する際には以下のルールに従ってください:
// 1. ソースコードを適切なフェーズに分割し、それらの各 "Chapter" に独立した機能を持たせること。
// 2. 各 Chapter を実装するクラスは IChapter を実装し、必要最小限の役割インタフェースにのみ依存すること。
// 3. IChapter や IChapterContext の型引数の反変性を成り立たせるために、上流の役割インタフェースは下流の役割インタフェイスを継承しなければならない。
//    VeryVibe.UsageExample における例: IHelloArg : IWorldArg : IArg
//    架空の例: IExtractInputArg : ICalcArg : IExtractResultArg : ISaveArg : IArg
//    IChapter実装クラスの Handle メソッドは、直接呼び出さず、Stage を通じて呼び出すこと。IChapter 実装クラスから IChapter 実装クラスの Handle メソッドを直接呼び出してはならない。
// 4. インタフェース定義は対応する Chapter の直前に置くこと。
// 5. 各インタフェースメンバには明確な XML コメントを記述すること。
// 6. 実行時引数は RootArg 1つに統一すること。
// 7. RootArg ではすべての役割インタフェースを明示的に実装すること（メンバは必ずインタフェース経由でのみアクセスできるようにする）。
// 8. 各 Chapter では対応する Arg インタフェース型のみを利用し、他のインタフェースや RootArg にキャストして無関係なメンバにアクセスしないこと。
// 9. RootArg を直接参照することは禁止。Setter も含め、必ずインタフェース経由でアクセスすること。
// 10. IContextBuffer<out TArg> の out と IChapterContext<in TArg> の in のバリアンスを維持すること。この組み合わせは CS1961 を満たす。
// 11. フィールドよりプロパティを優先すること。ただし、内部ロック用や固定コレクションなど従来的用途においては private readonly フィールドの利用を認める。
// 12. 設計上の選択が必然ではなくフレームワークの方針による場合は、その旨と意図をインラインコメントに記述し、VeryVibe 外の文脈（および AI/自動レビュー）でも自己説明的になるようにすること。例: readonly フィールドではなくプロパティを選択する、明示的インタフェース実装、パイプラインやリフレクションのための非標準的な命名/可視性、スレッドセーフ性やアロケーションに関するトレードオフ。
// 13. I/O や外部プロセス、環境依存呼び出しなどで発生する可能性のある例外は Chapter 内で処理すること。方針で明示されていない限り、例外を Handle の外へ伝播させてはならない。
// 14. .NET 8.0 の言語機能を活用すること。Nullable を有効化すること。ImplicitUsings は無効化すること。アクセス修飾子は internal をデフォルトとし、最小に絞ること。
// 15. コードのコンパイルや実行に .csproj 側の設定（例: TargetFramework, UseWindowsForms,
//     SupportedOSPlatformVersion, プラットフォーム依存パッケージ参照など）が必須な場合は、
//     プログラムの冒頭コメントにその旨を明記すること。
// 16. コレクションの契約は必ずインタフェース型（IReadOnlyList<T>, IEnumerable<T> など）で表現すること。  
//     Arg や Chapter の契約において、List<T> や配列 (T[]) といった具象型を直接公開してはならない。  
// 17. null 合体演算子（?? / ??=）を使用する場合、両辺は必ず同じ型でなければならない。  
//     List<T> と T[] を混在させてはならない。契約型に揃えること。  
// 18. 空のデフォルト値にはキャッシュされた静的定数を使うこと。  
//     例: private static readonly IReadOnlyList<T> Empty = Array.Empty<T>();  
//         public IReadOnlyList<T> Values => _values ?? Empty;  
// 19. バッキングフィールドの型は契約型と完全に一致させること。  
//     例: private IReadOnlyList<T>? _points;   // List<T>? ではなく  
// 20. ミュータブルな入力（例: SetNormalizedPoints）を受け取る場合、受け取り側は IEnumerable<T> など広く受け入れてよい。  
//     ただし内部保存は契約型に揃えること。コピーするか否かの方針は明示的にコメントに記載すること（RULE #11）。  
// 21. タプル要素の名前は型の同一性には影響しないが、コードベース内で一貫させること。  
//     (double x, double y) を推奨し、(double, double) などと混在させないこと。  
// 22.（上位原則・不可侵）データに破壊的な変更を行わない（No Destructive Changes）。
//     既存データの削除・上書き・不可逆変換は既定で禁止。必要な場合は「別名への新規作成」＋「明示フラグ」＋「事前検証」を必須とする。
// 23.（上位原則・入力防御）外部から与えられる全入力（CLI引数、環境変数、ファイル名、パス、URL、JSON、テンプレート文字列など）は
//     セキュリティ観点で必ずサニタイズ／バリデーションを行う（allowlist 方式を優先、denylist は補助）。
// 24.（上位原則・最小特権）処理に必要な最小権限のみを使用する（ファイル権限、ネットワーク、認証情報）。昇格や広い権限が必要な場合は、
//     その根拠をコメントに明記（RULE #12/#11 に従いインラインで理由を残す）。
// 25.（上位原則・可観測性）ログは秘匿情報を含めず、再現に必要な最小限のメタ情報のみを構造化形式で残す（PII/資格情報は常にマスク）。
// 26.（上位原則・既定は安全）既定値は常に「安全側」に倒す（外部送信しない／上書きしない／実行しない）。
//     利便性のために安全側を緩める場合は、明示的なフラグとコメントで根拠を示す（RULE #11）。
// 27.（入力バリデーション）パラメータは型・範囲・サイズ・形式（正規表現）を検証し、未指定・空・制御文字・改行・NULL バイトを拒否する。
//     パスは GetFullPath で正規化し、必ずベースディレクトリ配下かを StartsWith で検査。シンボリックリンクや再解析ポイントは許可しない。
// 28.（出力エンコード）外部へ出す文字列はシンクに応じて必ずエンコード／エスケープ（HTML/CSV/JSON/シェル引数）。
//     文字列連結でコマンドやクエリを組み立てない。コマンド実行は UseShellExecute=false、引数は配列で分離。
//     SQL/検索クエリ等は常にパラメータ化／プリペアド。
// 29.（ファイルI/Oの安全化）ファイル書き込みは上書き禁止（FileMode.CreateNew）。一時ファイルに書き込み、検証後に原子的 Move（上書き不可）。
//     対象拡張子／最大サイズに上限を設ける。FileShare は最小で開く（読み: Read、書き: None）。
// 30.（ネットワークの安全化）デフォルトで外部送信禁止。必要な場合のみドメイン allowlist＋TLS（≥1.2）＋証明書検証有効。
//     全リクエストにタイムアウト／リトライ（指数バックオフ）／CancellationToken を設定。リダイレクトとプロキシの扱いを明示。
// 31.（直列化/逆直列化）BinaryFormatter 等の危険APIは禁止。System.Text.Json を用い、MaxDepth を設定、既知の型にバインド。
//     ポリモーフィックな任意型解決や Type 名埋め込みは使わない。
// 32.（秘密情報）APIキー・トークン・接続文字列をソースに埋め込まない。取得は注入インタフェースや OS セキュアストア経由。
//     例外・ログに秘匿情報を出さない（マスク／要約）。
// 33.（暗号/乱数）セキュリティ用途の乱数は Random ではなく RandomNumberGenerator を使用。独自暗号は禁止。標準ライブラリのみ使用。
// 34.（並行性/リソース）長時間処理は定期的にキャンセルを確認。IDisposable は using で確実に破棄。
//     ロック範囲は最小化し、I/O／ユーザコードの呼び出しはロック外で行う（既存の RunAll 実装に準拠）。
// 35.（確認フロー）破壊的になり得る操作（重名出力、削除、移動）は二段階確認とする：Dry-run（検出のみ）→ Confirm フラグで実行。
//     可能なら成果物は別ディレクトリ／バージョニングされたパスに出力。
// 36.（テスト可能性）安全性ルールを守るためのユニットテストを用意（例：ディレクトリトラバーサル拒否、上書き禁止、
//     例外時のロールバック保証、エンコード適用）。生成AIがコードを変更してもテストがガードする構成にする。
// 37.（AI/自動生成対策）上記ルールに反するコード片（Process.Start のシェル連結、FileMode.Create、未検証パス等）を “禁止パターン” として
//     コメントに明記し、代替の安全テンプレートをすぐ下に置く（RULE #11）。レビュー時はまず禁止パターン検索から始める。
// 38.（失敗時の挙動）失敗は静かに握りつぶさない。ユーザ向けには安全なメッセージ、ログには再現可能な最小情報を残し、
//     中断かフォールバックかの方針を Chapter ごとに明示（RULE #12 と一貫）。
// 39.（デフォルト設定の固定）外部送信=無効、上書き=無効、実行=無効、インタラクティブ確認=必要、を既定とし、
//     解除には明示的なフラグ名（例：--allow-overwrite, --allow-network）を必須とする。
// 40. UX も考慮すること。  
//     数値・日付・時刻は現在のロケールに従ってフォーマットすること  
//     （例: ToString("N", CultureInfo.CurrentCulture)）。  
//     ログやプログラム的な出力ではカルチャ非依存のフォーマットを使用すること  
//     （例: 日付には ISO 8601 を用いる）。  
//     エラーメッセージやユーザー向け文字列はローカライズ／翻訳ポリシーに従うこと。
// 41. コード中のコメント、以下のチャット出力はすべて日本語で記述すること。
// 
/*
Chat output to the user after code generation:

**Important:** Even when the code generation task is split into subtasks via 
user interaction, this section must still be produced and included **after the 
completion of each subtask**.


## Compliance Report

When generating code, you must comply with the SOLID principles.  
If it is absolutely necessary to deviate, you must report the following after generation:

- Which principle was violated  
- Why the deviation was necessary  
- Proposed future refactorings or mitigation strategies  

If no violations occurred, explicitly state:  
"All SOLID principles have been maintained."

## Bug / Logic Review & Self-Prompt

Review both the generated code and the provided code, and point out 
typical human mistakes or logical inconsistencies based on the following criteria.  
Always include a corresponding self-prompt for each point:

- Unintended off-by-one errors, confusion between cumulative and delta values  
- Unnecessary or redundant operations  
- Performance hazards (e.g., massive loops)  
- Poor readability or misleading naming/structure  

If there are no issues, explicitly state:  
"There are no findings in the bug/logic review."

## Security Review & Self-Prompt

Assess the code against the security rules. For each item, either confirm compliance or report findings,
and include a brief self-prompt describing how you verified or would fix it:

- Non-destructive by default: No overwrites/deletes; atomic writes via temp + move?  
  *Self-prompt:* “Did I ensure CreateNew/atomic move and require an explicit confirm flag?”
- Input validation & sanitization: All external inputs validated (type/range/size/regex)?  
  *Self-prompt:* “Did I normalize paths and reject traversal/symlinks?”
- Injection resistance: No shell string concatenation; UseShellExecute=false; parameterized queries only.  
  *Self-prompt:* “Are all command args passed as arrays/typed params, not interpolated strings?”
- Output encoding: Proper escaping for HTML/CSV/JSON/shell where applicable.  
  *Self-prompt:* “Did I encode at the sink appropriate to the context?”
- Serialization safety: No BinaryFormatter or permissive polymorphic binding; bounded depth.  
  *Self-prompt:* “Am I using System.Text.Json with safe options and known types?”
- Secrets handling: No secrets in source/logs; retrieved via injected providers/secure store.  
  *Self-prompt:* “Could any exception/log leak sensitive values?”
- Networking: Default deny for outbound; TLS enabled; timeouts/retries/cancellation set.  
  *Self-prompt:* “Is there an allowlist of destinations and cert validation on?”
- Concurrency & resources: Lock scope minimal; I/O outside locks; proper disposal; supports CancellationToken.  
  *Self-prompt:* “Could any long-running path starve or deadlock?”
- Cryptography & randomness: Only platform crypto; RandomNumberGenerator for security needs.  
  *Self-prompt:* “Did I avoid custom crypto or non-CSPRNG?”
- Logging & observability: Structured logs; PII masked; no path/secret disclosure on failure.  
  *Self-prompt:* “Do logs enable reproduction without leaking sensitive data?”

If there are no issues, explicitly state:  
"There are no findings in the security review."

## Refactoring Suggestions & Self-Prompt

Review both the provided and generated code, and suggest refactorings if applicable.  
Always include a corresponding self-prompt for each suggestion.  

If there are no refactoring suggestions, explicitly state:  
"There are no refactoring suggestions."

## Uncertainty Report

If, during generation, multiple valid design choices existed and the generator made a judgment under uncertainty,  
this section must explicitly report those decisions. Each item should include:

- **Decision Point**: What part of the code required a choice  
- **Alternatives**: The other options that were considered  
- **Chosen Option & Reason**: Which option was picked and why  
- **Confidence Level**: High / Medium / Low  
- **Future Suggestion**: Any recommendation for refinement or review

*/

/* =============================================================================
 * コメント出力フォーマット契約
 * =============================================================================
 * 目的:
 *   ユーザーが「コメント生成手順 1. を実施」などと指示した場合に、出力を
 *   C# ソースへそのまま貼り付け可能なコメント形式に**限定**する。
 *
 * 指示キーワード:
 *   - 「コメント生成手順 1. を実施」
 *   - 「コメント生成手順1. の実施」
 *   - 上記に準ずる表現（全角/半角/句読点差異を含む）
 *
 * 出力フォーマット（必須遵守事項）:
 *   1) 出力はユーザーがGUIからコピー可能なコードブロックとして行う。
 *   2) 出力は **単一の C# ブロックコメント** であること:
 *        開始:  "/*"
 *        終了:  "*\/"
 *      先頭と末尾以外にバッククォートや Markdown 記法（``` など）を含めない。
 *
 *   3) コメントの中だけに本文を記載すること（コードや Markdown を混在させない）。
 *
 *   4) 文字コードは UTF-8 前提、制御文字は含めない。行末は \n。
 *
 *   5) 本文構成:
 *        - 見出し行（「生成手順 1. 仕様合意と設計方針（LissajousTool / CLI）」）
 *        - 目的・前提・入出力・安全方針・CLI・数式・検証/エラー・
 *          ロギング・拡張性・性能・受け入れ基準・例 を箇条書きで簡潔に。
 *        - 例コマンドは1行1例で、先頭に「例: 」を付与。
 *
 *   6) 外部への参照（URL、画像、表）は記載しない。
 *
 * 禁止事項:
 *   - 複数ブロックへの分割、Markdown コードフェンス（```）の使用
 *   - コメント外の文字（ヘッダ説明や追記）を出力すること
 *   - 実装コードや using 句を混在させること
 *
 * 受け入れ判定（チェックリスト）:
 *   [ ] 出力全体が /* で始まり *\/ で終わる単一ブロックコメントか
 *   [ ] コメント外の文字が一切ないか
 *   [ ] Markdown フェンスや装飾が一切ないか
 *   [ ] 必須章（目的/入出力/安全/CLI/検証/ログ/拡張/性能/受け入れ基準/例）が揃っているか
 *
 * 例（見出し冒頭の期待形）:
 *   /* ========================================================================
 *    * 仕様合意と設計方針（LissajousTool / CLI）
 *    * ========================================================================
 *    * 1) 概要 / 目的
 *    *    - 目的: ...
 *    *    - 実行形態: ...
 *    *    ...
 *    * 例: LissajousTool png -ax 3 -ay 2 --assume-yes -o .\out\classic.png
 *    * 例: LissajousTool gif -ax 5 -ay 7 --phase-sweep 0..360 --frames 120 --fps 30 --loop 0 --assume-yes -o .\out\spin.gif
 *    *\/
 *
 * 実行時の追加注意:
 *   - コメント生成手順 1. 以外の「コメント生成手順」同様にコメントブロックで返す。
 *   - サブタスクに分割しても、サブタスク単位で**毎回コメントブロック**を返す。
 *   - 「コメント生成手順」以外の手順（ex. 「コード生成手順」）はこの契約には含まれない。
 *   - 途中で要約が必要なら、必ずコメント内に含める（外に書かない）。
 * =============================================================================
 */
// --- END RULE ---

// === コメント生成手順 1. 仕様合意と設計方針 ===
// ※ユーザーからコメント生成手順1.の実施をプロンプトで指示された場合、次の事項を本ソースコードにコメントとしてコピー&ペースト可能な形でチャット出力する。
//   - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
//     (ユーザーがコピー＆ペースと可能なようにコードブロックとして出力)
// 1) 概要：目的/機能/想定ユーザー/実行形態（CLI/GUI）/対象OS・.NET など
// 2) I/O 要件：入力/出力/拡張子/最大サイズ/上書き可否/ドライラン/Confirm必須の有無など
//  - 入出力のフォーマットや環境変数・入出力ファイル構成や環境変数・コマンドラインパラメータなど、詳細にすべてを記述すること。
//  - 直前のチャットでの会話内容を網羅的に詳細まで含めること。つまりほぼコピーすること。
// 3) 失敗方針：中断 or フォールバック／ユーザー表示内容／ログ方針（PIIマスク/構造化）など
// 4) セキュリティ前提：パス正規化/ディレクトリ外拒否/ネットワーク既定禁止/禁止パターン検索実施など
// 5) UX方針：Usage（CLIオプション・例）、数値/日付のフォーマット（人向け=ロケール依存、機械= ISO 8601）など
// 6) 設計方針：Chapter 一覧と各責務、対応する Arg IF（Chapter直前に置く）
//   - 役割IFは上流→下流の継承で反変性を満たす（例：IExtractInputArg : ICalcArg : IArg）
//   - RootArg は全役割IFを“明示的”に実装し、Chapterは対応IF経由のみアクセス
// 7) 受け入れ条件：ビルド成功／Dry-run成功／破壊的操作はConfirm必須／禁止パターン不検出など
// ※ 本出力だけで会話履歴が消えても再開可能なレベルまで自己完結的に出力すること。
// === End コメント生成手順 1. ===

// === コメント生成手順 2. 仕様セルフレビュー ===
// ※ユーザーからコメント生成手順2.の実施をプロンプトで指示された後、次の事項を本ソースコードにコメントとしてコピー&ペースト可能な形でチャット出力する。
//  - Specification Review Template の内容に基づいた「仕様合意と設計方針」のセルフレビュー結果
//   - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
//     (ユーザーがコピー＆ペースと可能なようにコードブロックとして出力)
//   - 本ソースコードへコピペ可能な形式で提示される。
/*

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
*/
// === End コメント生成手順 2. ===

// === コメント生成手順 3. 仕様追補 ===
// ユーザーが「コメント生成手順 3. を実施」と指示した場合：
//   - コメント生成手順 2. のセルフレビューで明らかになった未確定点や課題を
//     追補仕様として整理し、コメント出力する。
//   - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
//     (ユーザーがコピー＆ペースと可能なようにコードブロックとして出力)
//   - 本ソースコードへコピペ可能な形式で提示される。
// ※ 本出力だけで会話履歴が消えても以後の手順が実施できるよう自己完結的に出力すること。
// === End コメント生成手順 3. ===

// === コメント生成手順 4. 依存パッケージ等決定 ===
// ユーザーが「コメント生成手順 4. を実施」と指示した場合：
// ユーザーに依存パッケージ等の決定が必要な事項を通知し、それぞれに関して
//   - 選択肢
//   - 選択肢ごとのメリットデメリット
//   - 推奨選択肢
// を提示し、決定を促す。本手順はコメントブロックではなく、ユーザーとのチャットで行う。
// パッケージのシェアが大きいことは選択肢のメリットとして挙げる。
// パッケージの更新が止まっていることは選択肢のデメリットとして挙げるが、「歴史があり、安定していること」はメリットとして挙げる。
// 安定版が存在せず、プレビュー、ベータ版しかないパッケージは、そもそも選択肢として提示しないこと。
// テスト用のパッケージは、最初の選択しとして xUnit と Moq を提示する。
// === End コメント生成手順 4. ===

// === コメント生成手順 5. 依存パッケージ等コメント生成 ===
// ユーザーが「コメント生成手順 5. を実施」と指示した場合：
// ユーザーとのチャット記録に基づき、依存パッケージの決定事項と、想定される .csproj を**単一ブロックコメント**で提示する（実際の貼付はユーザーが行う）。
//   - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
//     (ユーザーがコピー＆ペースと可能なようにコードブロックとして出力)
//  - 「コメント出力フォーマット契約」に厳密準拠（単一 /* ... */、Markdown フェンス禁止）。
//  - csproj の内容は、ユーザーが実際に貼り付けることを前提として、<Project> タグから </Project> タグまでを含む。
//  - csproj の内容は、アプリケーションプロジェクト用と、テストプロジェクト用の2つを出力する。
//  - csproj を出力する際には、C# のコメントブロックの行頭 * は利用せず、ユーザーがそのままコピーアンドペーストできるようにすること。
//  - csproj では、ReferencePakage のバージョンは、現時点で適切と思われるものを仮に指定する。
// ※ 本出力だけで会話履歴が消えても以後の手順が実施できるよう自己完結的に出力すること。
// === End コメント生成手順 5. ===

// === コメント生成手順 6. Arg インタフェース最終化 ===
// ユーザーが「コメント生成手順 6. を実施」と指示した場合：
//  各 Chapter 直前に配置する Arg インタフェースの最終版を、C# 単一ブロックコメントで出力する。
// 方針:
//  - 今までにコメントで構想した各 Chapter の Arg インタフェースを、上流（ユーザーからの入力）から
//    下流（出力）へ向かっての順に、各メンバに XML コメントを必ず付与した**最終シグネチャ**をコメントとして提示する。
// 出力物: 各 IF の public メンバ名、継承インタフェイス、型、読み書き属性（get; / set;）、意図、例外規約。
//  - 出力は「コメント出力フォーマット契約」に厳密に従い、C# の単一ブロックコメントとして返す。
//     (ユーザーがコピー＆ペースと可能なようにコードブロックとして出力)
//  - 「コメント出力フォーマット契約」に厳密準拠（単一 /* ... */、Markdown フェンス禁止）。
// 注意: RULE #3 に完全に従うこと。通常は IArg を継承する役割インタフェイスはただ一つとなる。
// 成果: コメントをそのまま各 Chapter 直前に貼る → 以降のコード生成の単一情報源（SSoT）にする。
// ※ 本出力だけで会話履歴が消えてもコード生成プロセスが実施できるよう自己完結的に出力すること。
// === End コメント生成手順 6. ===

// === コード生成手順 1. Arg IF コード生成 ===
// ユーザーが「コード生成手順 1. を実施」と指示した場合：
//  各 Chapter 直前に配置する interface の**実装コード**（XML ドキュメントコメント必須）を生成してソースコードにマージし、ダウンロード可能にする。
// 入力: コメント生成手順4. の最終化コメント（SSoT）
// ルール: VeryVibe RULE 準拠。契約型はインタフェース（IReadOnlyList<T> 等）で公開。配列や List<T> を直接返さない。
//         **インタフェイス及び各メンバには、必ず XML ドキュメントコメントを付与すること。**
// 注意: Chat 返信の末尾にはレビュー結果のみを簡潔に出す（テンプレ本文の再掲は禁止）。
// この時点で VeryVibe.UsageExample の実装は削除するか、その旨をユーザーに通知すること。
// === End コード生成手順 1. ===

// === コード生成手順 2. RootArg 明示的実装スケルトン ===
// ユーザーが「コード生成手順 2. を実施」と指示した場合：
//   RootArg が全 Arg IF を**明示的**に実装（プロパティ/セッターは IF 経由のみアクセス可能）する形のソースコードを生成してソースコードにマージし、ダウンロード可能にする。
// 要件:
//  - バッキングフィールドは契約型と完全一致（例: IReadOnlyList<T>? _points）。
//  - 空既定値は静的キャッシュ Array.Empty<T>() を使用。
//  - 受け取りは IEnumerable<T> 可、内部保存は契約型へ正規化。コピー方針をコメントで明記。
//  - I/O を持つメソッドはここでは実装しない（Adapter を後日注入可にするため）。
//  - 補助的クラスを実装する場合には、必ずXML ドキュメントコメントを付与すること。
// 出力: RootArg クラス本体（ctor で引数の基本検証、IClockArg/ITempFileArg などの注入ポイントのプロパティ）。
// === End コード生成手順 2. ===

// === コード生成手順 3. Chapter 空実装雛形 ===
// ユーザーが「コード生成手順 3. を実施」と指示した場合：
//  - IChapter<対応 Arg IF> を実装し、ソースコードにマージし、ダウンロード可能にする。
//  - Handle 内で try/catch を用意（I/O 想定箇所のみ）。エラーは短文 + 構造化ログに要約、伝播禁止（RULE #13）。
//  - 次フェーズへのコンテキストを buffer.PushBack(new ChapterContext<...>(..., arg)) でつなぐだけの骨組み。
//  - TODO コメントで「この Chapter に置くロジック」と「禁止パターン」を明記。
// === End コード生成手順 3. ===

// === コード生成手順 4. Chapter の実装（単一） ===
// ユーザーが「コード生成手順 4. を実施」と指示した場合：
// 1) 「骨組み状態」の Chapter を列挙し（Handle に TODO/NotImplemented/空実装があるものを骨組みと定義）、
//    パイプライン順で最上流のものを 1 つだけ選ぶ。パイプライン順はソースコード上のコメントや PushBack の順序で決定する。
// 2) 選ばれた Chapter の Handle と、必要があればヘルパ―等の関連クラスを実装する .cs を 1 つ生成し、ダウンロード可能にする。
// ※ 本出力だけで会話履歴が消えても他のチャプターの実装が再開可能なレベルまでコメントを自己完結的に出力すること。
// 4) 骨組みが 1 つも無い（＝全実装済み）の場合：生成を行わず、「対象なし」を短く報告する。
// === End コード生成手順 4. ===

/* =============================================================================
 * 仕様合意と設計方針（LissajousTool / CLI）
 * =============================================================================
 * 1) 概要 / 目的
 *    - 目的: リサージュ曲線を生成し、静止画(.png)またはアニメーション(.gif)として保存できる
 *      Windows向けC#コンソールツールを提供する。
 *    - 実行形態: CLI（サブコマンド方式: png / gif / help）。
 *    - 対象OS / ランタイム: Windows / .NET 8.0（Nullable有効, ImplicitUsings無効）。
 *    - 方針: VeryVibe の Chapter 方式でフェーズ分割し、SOLID順守と安全既定（破壊的操作禁止）を徹底。
 *
 * 2) 入出力(I/O)要件
 *    - 入力: コマンドライン引数（周波数比ax/ay, 位相, 振幅, サイズ, 色, 出力パスほか）。
 *      すべて検証必須（型/範囲/サイズ/形式）。パスは GetFullPath で正規化し、ベースディレクトリ外・
 *      シンボリックリンク・再解析ポイントを拒否（allowlist優先）。
 *    - 出力: 画像ファイルを新規作成のみ（FileMode.CreateNew）。デフォルトは dry-run で検証のみ。
 *      実生成には -y/--yes（--assume-yes / --no-dry-run / --write のいずれか）が必須。既存ファイル上書きは --allow-overwrite が明示的に必要。
 *    - 拡張子と最大サイズ: png/gif に限定。最大サイズは --max-bytes で上限を指定可能（安全弁）。
 *    - 文字エンコード: ログ/コンソールはUTF-8想定。画像のDPIやメタデータはオプションで付与可。
 *
 * 3) 失敗方針
 *    - 入力検証に失敗: 即中断（非0終了コード）。ユーザー向けに短文要約、ログには構造化メタ情報。
 *    - 生成途中の失敗: 一時ファイルへ出力→検証→原子的Move方針。Move不可や検証失敗時はロールバック。
 *    - 破壊的操作の防止: 既定は dry-run。実出力は -y/--yes（等）必須。上書きは個別に --allow-overwrite 必須。
 *
 * 4) セキュリティ前提
 *    - 非破壊が既定（No Destructive Changes）。
 *    - 入力サニタイズと正規化（パス/数値/範囲/形式）。
 *    - 外部送信はデフォルト禁止（ネットワーク不使用）。
 *    - 危険API/禁止パターン例: Shell連結のProcess.Start, FileMode.Create 等（安全テンプレを別途用意）。
 *    - 直列化は System.Text.Json の安全設定に限定（BinaryFormatter禁止）。
 *
 * 5) UX方針
 *    - --help で下記 Usage を表示（日本語）。
 *    - 人に見せる数値は CurrentCulture でフォーマット、ログなど機械向けは InvariantCulture/ISO 8601。
 *    - 進捗/統計は -v/--verbose で詳細、-q/--quiet で抑制。
 *
 * 6) CLI（Usage 風 / 説明は日本語）
 *    Usage:
 *      LissajousTool png  [options]             # 静止画(.png)を生成
 *      LissajousTool gif  [options]             # アニメーション(.gif)を生成
 *      LissajousTool help                       # このヘルプを表示
 *
 *    リサージュ曲線の基本パラメータ
 *      -ax <int>                                X 側の角周波数（周波数比の X）[既定: 3]
 *      -ay <int>                                Y 側の角周波数（周波数比の Y）[既定: 2]
 *      --phase <double>                         位相差（度: 0–360）[既定: 0]
 *      --amp-x <double>                         X 振幅（画像内スケール相対値 0–1）[既定: 0.9]
 *      --amp-y <double>                         Y 振幅（同上）[既定: 0.9]
 *      --samples <int>                          曲線サンプル数（大ほど滑らか）[既定: 10_000]
 *      --thickness <double>                     線の太さ（ピクセル）[既定: 2.0]
 *      --antialias <on|off>                     アンチエイリアス有効/無効 [既定: on]
 *      --margin <int>                           端の余白（px）[既定: 32]
 *
 *    出力/キャンバス
 *      -o, --output <path>                      出力ファイルパス [必須]
 *      -W, --width <int>                        出力幅（px）[既定: 1024]
 *      -H, --height <int>                       出力高さ（px）[既定: 1024]
 *      --bg <#RRGGBB|name>                      背景色（例: #000000, white）[既定: black]
 *      --fg <#RRGGBB|name>                      線色（例: #00FFFF, cyan）[既定: cyan]
 *      --dpi <int>                              画像DPI（メタデータ）[既定: 96]
 *
 *    安全設定（既定は“安全側”）
 *      --dry-run                                破壊的操作をせず検証のみ（既定で有効）
 *      --assume-yes                             検証後に実際のファイル生成を許可
 *      --allow-overwrite                        既存ファイルの上書きを許可（--allow-overwrite と併用推奨）
 *      --out-dir <path>                         出力先ディレクトリ（-o 未指定時のフォールバック）
 *      --max-bytes <long>                       生成物の最大サイズ上限（安全のための拒否閾値）
 *
 *    ログ/UX
 *      -v, --verbose                            詳細ログ（構造化メタ情報を追加）
 *      -q, --quiet                              標準出力の進捗/統計を抑制
 *      --culture <name>                         数値の表示カルチャ（例: ja-JP, en-US）[表示専用]
 *      --seed <int>                             疑似乱数シード（ノイズ表現等の再現性用）
 *
 *    サブコマンド固有: png
 *      --png-compress <0..9>                    PNG 圧縮レベル [既定: 6]
 *      --png-filter <auto|none|sub|up|avg|paeth> PNG フィルタ戦略 [既定: auto]
 *      --metadata <key=value>...                PNG メタデータ（複数指定可）
 *
 *    サブコマンド固有: gif
 *      --frames <int>                           総フレーム数 [既定: 120]
 *      --fps <double>                           フレームレート [既定: 30]
 *      --duration <double>                      アニメ長（秒）[fps とどちらか一方を指定可]
 *      --loop <int>                             ループ回数（0=無限）[既定: 0]
 *      --phase-sweep <start..end>               位相を start..end 度でスイープ（例: 0..360）
 *      --amp-x-sweep <start..end>               X振幅をスイープ（0–1）
 *      --amp-y-sweep <start..end>               Y振幅をスイープ（0–1）
 *      --palette <auto|median-cut|kmeans>       パレット生成方式 [既定: auto]
 *      --dither <on|off>                        ディザリング有効/無効 [既定: on]
 *      --gif-quantize <colors>                  パレット色数（2–256）[既定: 256]
 *      --gif-delay-ms <int>                     各フレーム遅延（ms）[fps/duration 指定時は自動計算]
 *      --metadata <key=value>...                GIF メタデータ（複数指定可）
 *
 *    検証/制約（実行時バリデーション例）
 *      - パラメータの型・範囲を検査（負数/NaN/極端な samples を拒否/警告）。
 *      - 出力パスは正規化し、ベースディレクトリ配下のみ許可。シンボリックリンクや再解析ポイントは拒否。
 *      - 承認フラグ（-y/--yes 等）不在時は常に dry-run（生成なし）。
 *      - --allow-overwrite 不在時は上書き禁止（CreateNew）。
 *
 * 7) 設計方針（Chapter と Arg の対応）
 *    - ParseArgsChapter : 入力引数の解析/基本検証（IParseArgsArg）。
 *    - ValidateChapter  : パス正規化/安全検査/ポリシー適用（IValidateArg）。
 *    - RenderChapter    : 曲線点群生成・描画（IRenderArg）。
 *    - EncodeChapter    : PNG/GIF のエンコード（IEncodeArg）。
 *    - WriteChapter     : 一時ファイル作成→検証→原子的Move（IWriteArg）。
 *    - 各 Chapter は対応 Arg IF のみを参照（RootArg直参照は禁止／明示的実装を介する）。
 *
 * 8) 性能
 *    - サンプリングはベクトル化/Span/配列再利用などでGC負荷低減。
 *    - GIF パレットは自動/簡易方式を既定。高品質方式はオプション選択（処理時間とトレードオフ）。
 *
 * 9) 受け入れ基準
 *    - ビルド成功（.NET 8.0）, 単体テストで安全規約をガード（ディレクトリ外拒否・上書き禁止等）。
 *    - dry-run が期待通りに差分を報告し、--allow-overwrite でのみ生成されること。
 *    - 禁止パターン検出なし（Shell連結, FileMode.Create 等）。
 *
 * 10) 例（ワンライナー）
 *    例: LissajousTool png -ax 3 -ay 2 --phase 30 -W 1200 -H 800 --fg #00FFFF --bg black -o .\out\classic.png -y
 *    例: LissajousTool gif -ax 5 -ay 7 --phase-sweep 0..360 --frames 180 --fps 30 --fg #FF6AFB --bg black -o .\out\spin.gif -y
 *    例: LissajousTool gif -ax 2 -ay 3 --duration 6 --amp-x-sweep 0.8..0.95 --amp-y-sweep 0.8..0.95 --dither on -o out\soft.gif -y
 *    例: LissajousTool png -ax 7 -ay 9 --samples 20000 --thickness 1.5 --antialias on -o .\out\hires.png -y
 * =============================================================================
 */
/* =============================================================================
 * コメント生成手順 2. 仕様セルフレビュー（LissajousTool / CLI）  :contentReference[oaicite:0]{index=0}
 * =============================================================================
 * 対象: 「コメント生成手順 1.」で合意した仕様（Usage/安全既定/Chapter分割/Arg IF方針 等）
 *
 * ## 1) Specification Compliance（規約順守）
 * - VeryVibe RULE（SOLID/Safety/最小特権/非破壊既定）: 概ね順守。破壊的操作は -y/--yes（等）必須、
 *   上書きは --allow-overwrite 必須、dry-run 既定など安全側に倒している。OK。
 * - Chapter と Arg IF の関係: ParseArgs/Validate/Render/Encode/Write の各 Chapter に対応する
 *   Arg IF を想定し、RootArg が明示的実装で一元化する方針は明確。OK。
 * - RootArg 直接参照禁止: 「Chapter は対応 IF 経由のみアクセス」の方針を明記。OK。
 * - バリアンス方針: IContextBuffer<out TArg> × IChapterContext<in TArg> の組合せで CS1961 充足。OK。
 *
 * ## 2) Completeness Check（完全性）
 * - I/O 要件: 拡張子(PNG/GIF)/最大サイズ(--max-bytes)/dry-run/confirm/overwrite/出力ディレクトリの
 *   取扱いを明示。OK。
 * - エラーポリシー: 入力検証失敗は即中断、Chapter 内で I/O 例外処理、原子的 Move によるロールバック等を記載。OK。
 * - セキュリティ原則: パス正規化/ディレクトリ外拒否/シンボリックリンク拒否/ネットワーク既定禁止/
 *   危険API禁止を明示。OK。
 * - ロギング/観測性: ユーザー向けは要約、ログは構造化・PIIマスク。OK。
 * - ただし「ベースディレクトリ」の厳密定義（どこを基点とするか）は明文化要。TODO。
 *
 * ## 3) Consistency Check（一貫性）
 * - Usage例と要件の整合: 例コマンドは安全既定（-y/--yes 明示時のみ生成）と整合。OK。
 * - 安全既定: dry-run既定、上書き禁止既定を全体で一貫して記述。OK。
 * - ロケール/出力形式: ユーザー向けは CurrentCulture、ログは Invariant/ISO 8601 の方針を記述。OK。
 * - GIF 時間指定: --fps と --duration はどちらか一方で可としたが、同時指定時の優先順位は未定義。TODO。
 *
 * ## 4) Risk & Uncertainty（不確実性）
 * - 画像ライブラリ選択: PNG/GIF 実装を System.Drawing vs ImageSharp 等から選ぶ判断が未確定。
 *   代替: (A) 完全マネージド（ImageSharp系） / (B) Windows前提で System.Drawing。
 *   推奨: クロス環境性と今後の保守性から (A) を第一候補。要「コメント生成手順 4.」で決定。
 * - GIF パレット/ディザ方式: palette=auto/median-cut/kmeans の品質/速度トレードオフの既定が未決。
 *   推奨: 既定は auto、長時間処理許容なら median-cut。要ベンチ後に確定。
 * - パス検証の基準ディレクトリ: 実行ディレクトリ or 指定の out-dir を基点にするか未確定。要追補。
 * - 最大サイズ --max-bytes の既定値: 閾値未定。用途に応じた保守的デフォルトを要検討（例: 200MB）。
 *
 * ## 5) Refactoring / Clarification Suggestions（追補提案）
 * - 明文化事項の追加:
 *   1) 「ベースディレクトリ」の定義（例: out-dir が指定されればその実体パス、未指定時は
 *      プロセスのカレントディレクトリを基点、など）。
 *   2) GIF の時間指定競合ルール（--duration 優先／--fps 優先／競合時はエラー等）。
 *   3) --metadata のキー名・重複処理・禁止キー（例: 長大値の制限）を簡易表で列挙。
 *   4) 色指定（--bg/--fg）の名前解決テーブル（#RRGGBB 優先、名前は KnownColor 互換など）。
 * - テスト観点の追加:
 *   - ディレクトリトラバーサル拒否、シンボリックリンク拒否、上書き抑止、dry-run で
 *     ファイル非生成、atomic move 失敗時のロールバックのユニットテストを必須化。
 * - パフォーマンス注記:
 *   - samples≥100k のベンチ方針（割り当て削減、Span/T[] 再利用、Stream 書込のバッファリング）を仕様に一行追記。
 *
 * ## 6) 結論
 * - 現行仕様は VeryVibe RULE と整合し、安全既定・拡張しやすさ・テスト容易性が担保されている。
 * - 次アクションは「コメント生成手順 3.（仕様追補）」で上記 TODO を明文化し、
 *   「コメント生成手順 4.」で依存パッケージを確定すること。
 * =============================================================================
 */
/* =============================================================================
 * コメント生成手順 3. 仕様追補（LissajousTool / CLI）
 * =============================================================================
 * 目的:
 *   コメント生成手順 2. のセルフレビューで挙がった未確定事項を、実装に直結する
 *   レベルまで明文化し、以後のコード生成・テストをブロックしないようにする。
 *
 * A) ベースディレクトリとパス検証の定義
 *   - BaseDir の決定順:
 *       1) --out-dir 指定があれば、その絶対パス (Path.GetFullPath) を BaseDir とする。
 *       2) 無指定の場合、Environment.CurrentDirectory の絶対パスを BaseDir とする。
 *   - 受理条件:
 *       - 出力ファイルは必ず BaseDir 配下であること (StartsWith 比較は OrdinalIgnoreCase)。
 *       - シンボリックリンク/ジャンクション/再解析ポイントは拒否。
 *         (DirectoryInfo.Attributes に ReparsePoint を含む経路は不可)
 *   - 正規化:
 *       - すべての入出力パスは Path.GetFullPath で正規化後に検証する。
 *   - ファイルモード:
 *       - 既定は FileMode.CreateNew。--allow-overwrite 指定時のみ FileMode.Create に緩和。
 *   - 原子的書込:
 *       - BaseDir 配下の一時ファイル (Path.GetTempFileName 相当を BaseDir に再実装) に出力し、
 *         成功後に File.Move(temp, final, overwrite:false)。overwrite は上記ポリシーに従う。
 *
 * B) GIF の時間指定ルール（競合時の優先順位）
 *   - 受理可能な指定:
 *       1) --frames と --fps         → duration = frames / fps
 *       2) --frames と --duration    → fps = frames / duration
 *       3) --frames のみ             → fps = 30, duration = frames / 30
 *       4) --duration のみ           → fps = 30, frames = round(duration * 30)
 *       5) --fps のみ                → frames = 120, duration = frames / fps
 *   - 同時指定の衝突:
 *       - --fps と --duration と --frames の全てが明示された場合は矛盾検査を行い、
 *         |frames - round(duration * fps)| > 1 の場合はエラー。
 *   - --gif-delay-ms:
 *       - 明示された場合は delay を最優先で使用し、fps/duration/frames は補助値として再計算する。
 *         再計算結果と大きく乖離する場合は警告ログを出す。
 *
 * C) カラー指定と既定値
 *   - 形式:
 *       - 16進 #RRGGBB を最優先で受理。失敗時のみ KnownColor 名 (InvariantCulture) を解決。
 *       - 前景 (--fg) 既定: #00FFFF、背景 (--bg) 既定: #000000。
 *   - DPI 既定: 96。
 *   - アンチエイリアス既定: on。
 *
 * D) PNG/GIF エンコードの選定
 *   - 既定エンジン: SixLabors.ImageSharp (完全マネージド、クロス環境性と保守性を優先)。
 *   - 代替案: System.Drawing は Windows 限定かつ互換上の注意点が多いため非推奨 (プロジェクト要件でのみ選択)。
 *   - パッケージの確定はコメント生成手順 4. で最終決定するが、現仕様は ImageSharp 前提で進める。
 *
 * E) GIF パレット/ディザ設定の既定
 *   - --palette: auto (内部ヒューリスティクスで median-cut 相当を選択)。
 *   - --dither: on。
 *   - --gif-quantize: 256。
 *   - 品質/速度のトレードオフは README に簡易表を掲載予定（コード生成後に追加）。
 *
 * F) サイズ・安全上限・既定
 *   - --max-bytes 既定: 200_000_000 (約 200 MB)。超過見込み時は警告 → 既定では拒否。
 *   - 出力拡張子の許可: .png / .gif のみ。その他は拒否。
 *   - --dry-run は既定で有効。実生成には --allow-overwrite が必須。
 *
 * G) メタデータ鍵のポリシー
 *   - 許可キー例: Title, Author, Description, Software, Source, Comment, Copyright。
 *   - 値長上限: 8,192 文字。超過時は切り詰め＋警告。
 *   - 禁止: バイナリ相当の内容、改行連続 3 回以上、制御文字 (U+0000〜001F、U+007F)。
 *
 * H) ロギング/カルチャ方針の具体化
 *   - ユーザー向け標準出力: CultureInfo.CurrentCulture (数値は "N" などで桁区切り)。
 *   - 構造化ログ: CultureInfo.InvariantCulture、時刻は UTC ISO 8601、パス等は正規化した値のみ。
 *   - PII/秘匿情報は出力しない。例外メッセージは要約＋技術詳細は verbose のみに限定。
 *
 * I) 性能・メモリ方針
 *   - samples 既定: 10_000、上限: 5_000_000。既定は配列プール/Span ベースで確保。
 *   - 大規模描画: 連続線分バッチング、ストリーム書き込みのバッファリングを既定有効。
 *   - ベンチ観点: samples ∈ {10k, 100k, 1M}、W×H ∈ {(1024,1024), (2048,2048)} で GC / 時間を計測。
 *
 * J) テスト必須ケース（抜粋）
 *   - パス検証: BaseDir 外、シンボリックリンク、相対パス .. を全て拒否。
 *   - 上書き防止: 既存ファイルに対して --allow-overwrite 無しで失敗、ありで成功。
 *   - dry-run: 実ファイルが生成されないことを確認。
 *   - 原子的書込: Move 直前で強制失敗 → 一時ファイルのみ残存、最終出力は未作成。
 *   - GIF 時間競合: fps×duration と frames の不整合検出。
 *
 * K) 例外処理の粒度
 *   - Chapter 内で I/O 例外を捕捉し、ユーザー向け短文＋構造化ログを出力。伝播は禁止。
 *   - 設定/引数の仕様違反は早期バリデーションで弾く (ArgumentException/ValidationError 的ドメイン例外)。
 *
 * L) インタフェース公開契約
 *   - 公開契約は IReadOnlyList<T> / IEnumerable<T> 等のインタフェースに統一。配列や List<T> を直接公開しない。
 *   - 空の既定値は Array.Empty<T>() を共有利用。バッキングは契約型と一致させる。
 *
 * M) 将来拡張の拡張点
 *   - ノイズ/点描・グローなどの描画スタイル: --style と --seed を予約。
 *   - JSON 設定ファイル: --config <path> を予約。CLI とマージするルールは「CLI が最優先」。
 *
 * 本追補の効力
 *   - 本ブロックは以後の「コメント生成手順 4.–6.」「コード生成手順 1.–4.」の前提とする。
 *   - 以降の出力（コード/コメント/テスト）は、ここで定めた既定と優先順位に準拠すること。
 *
 * [追補-1] メタデータ鍵のフォーマット（G) の厳格化
 * -----------------------------------------------------------------------------
 * 変更点:
 *   - キー形式に正規表現を導入し、実装のぶれを防止。
 *   - 重複キーの取り扱いを明文化。
 *
 * 追加仕様:
 *   - キー（key）: 正規表現 `^[A-Za-z0-9._-]{1,64}$` に適合すること。
 *     * 使用可文字: 英数・ドット・アンダースコア・ハイフン
 *     * 長さ: 1〜64 文字
 *   - 値（value）: 非空、最大 8192 文字。前後の空白はトリムして格納。
 *   - 禁止: バイナリ相当／制御文字（U+0000–001F, U+007F）、改行連続3回以上。
 *   - 重複キー出現時の扱い: **後勝ち（最後に現れた値で上書き）**。Verbose 時に警告ログを出す。
 *
 * [追補-2] 色指定の対応範囲（C) の明確化
 * -----------------------------------------------------------------------------
 * 変更点:
 *   - 現行でサポートする色表記を限定し、将来拡張の余地を明記。
 *
 * 追加仕様:
 *   - 受理する形式は **#RRGGBB**（16進、大文字小文字不問）を最優先とする。
 *   - 上記にマッチしない場合のみ、KnownColor（InvariantCulture, 大小無視）を解決。
 *   - **非対応（現時点）**: `#RGB`, `#RRGGBBAA`, CSS 名（"rebeccapurple" 等の拡張名）。
 *   - 入力は前後空白をトリム。解決不能な場合は Validation エラー。
 *   - 将来拡張: `#RRGGBBAA` / `#AARRGGBB` / CSS4 名は互換レイヤで追加可能（後方互換を保つ）。
 *
 * [追補-3] ログのタイムゾーン方針（H) の明確化
 * -----------------------------------------------------------------------------
 * 変更点:
 *   - ログの時刻は常に UTC / ISO 8601（Zulu）で統一。
 *   - ユーザー向け表示はローカルカルチャでよいが、ログは機械可読を優先。
 *
 * 追加仕様:
 *   - 構造化ログの日時: **UTC 固定**、フォーマットは `"yyyy-MM-dd'T'HH:mm:ss.fff'Z'"`（ISO 8601）。
 *   - 例: 2025-08-20T03:14:15.926Z
 *   - ユーザー向けテキスト（標準出力）: `CultureInfo.CurrentCulture` に従う（日時/数値の書式は地域依存可）。
 *   - ログにローカルタイムを混在させないこと（監視/相関が困難になるため）。
 *
 * [追補-4] テスト項目（J) の追加
 * -----------------------------------------------------------------------------
 * 追加テスト:
 *   - メタデータ:
 *     * 不正キー（空白/記号/長さ超過）を拒否する。
 *     * 重複キーが後勝ちで反映され、Verbose で警告される。
 *   - 色:
 *     * #RRGGBB は通る、#RGB / #RRGGBBAA は弾く、KnownColor は解決される。
 *     * 不明色は Validation エラー。
 *   - ログ:
 *     * 構造化ログの日時が常に `Z` で終わる（UTC）ことをスナップショットで検証。
 * =============================================================================
 */
/* =============================================================================
 * コメント生成手順 5. 採用決定のコメント化 & .csproj たたき台
 * =============================================================================
 * 本ブロックは、以後の「コード生成手順 1.–4.」の前提となる **最終合意** を示す。
 * ここに記載の .csproj 例は “そのまま貼り付け → 体裁調整” を想定した **たたき台**。
 *
 * A) 採用決定（最小構成）
 * -----------------------------------------------------------------------------
 * 画像処理      : SixLabors.ImageSharp（＋必要に応じて ImageSharp.Drawing）
 * CLI           : Spectre.Console.Cli（＋表示用に Spectre.Console）
 * ロギング      : Microsoft.Extensions.Logging.Console（Abstractions は自動）
 * テスト        : xUnit + Moq
 * 追加(任意)    : FluentAssertions（可読性向上）
 * 静的解析      : いったん **入れない**（Roslynator/StyleCopは将来検討）
 *
 * B) ビルドターゲット & 既定
 * -----------------------------------------------------------------------------
 * TFM           : net8.0
 * 既定カルチャ  : 実行環境の CurrentCulture（ユーザー向け） / Invariant（ログ）
 * Nullable      : enable
 * ImplicitUsings: disable（明示 using 方針）
 * 安全既定      : dry-run / 上書き禁止 / BaseDir配下のみ出力 / 原子的書込
 *
 * C) 依存パッケージ（アプリ用 .csproj の例）
 * -----------------------------------------------------------------------------
 * <Project Sdk="Microsoft.NET.Sdk">
 *   <PropertyGroup>
 *     <OutputType>Exe</OutputType>
 *     <TargetFramework>net8.0</TargetFramework>
 *     <Nullable>enable</Nullable>
 *     <ImplicitUsings>disable</ImplicitUsings>
 *   </PropertyGroup>
 *
 *   <ItemGroup>
 *     <!-- 画像処理（必須） -->
 *     <PackageReference Include="SixLabors.ImageSharp" Version="*"/>
 *     <!-- 線や図形を ImageSharp で描く場合のみ（必要時に有効化）
 *     <PackageReference Include="SixLabors.ImageSharp.Drawing" Version="*"/> -->
 *
 *     <!-- CLI（必須） -->
 *     <PackageReference Include="Spectre.Console.Cli" Version="*"/>
 *     <!-- 見栄えを整える（オプション） -->
 *     <PackageReference Include="Spectre.Console" Version="*"/>
 *
 *     <!-- ロギング（必須・軽量） -->
 *     <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="*"/>
 *   </ItemGroup>
 * </Project>
 *
 *  ※ Version="*" はプロジェクトのポリシーに合わせて固定化してください。
 *    （中央管理: Directory.Packages.props を推奨。固定例は下段 E) を参照）
 *
 * D) テストプロジェクトの例（LissajousTool.Tests）
 * -----------------------------------------------------------------------------
 * <Project Sdk="Microsoft.NET.Sdk">
 *   <PropertyGroup>
 *     <TargetFramework>net8.0</TargetFramework>
 *     <IsPackable>false</IsPackable>
 *     <Nullable>enable</Nullable>
 *     <ImplicitUsings>disable</ImplicitUsings>
 *   </PropertyGroup>
 *
 *   <ItemGroup>
 *     <!-- xUnit 本体 -->
 *     <PackageReference Include="xunit" Version="*"/>
 *     <PackageReference Include="xunit.runner.visualstudio" Version="*" />
 *     <PackageReference Include="Microsoft.NET.Test.Sdk" Version="*"/>
 *
 *     <!-- モック -->
 *     <PackageReference Include="Moq" Version="*"/>
 *
 *     <!-- アサーション（任意） -->
 *     <PackageReference Include="FluentAssertions" Version="*"/>
 *   </ItemGroup>
 *
 *   <ItemGroup>
 *     <ProjectReference Include="..\LissajousTool\LissajousTool.csproj" />
 *   </ItemGroup>
 * </Project>
 *
 * E) 参考: 版本固定の中央管理（任意 / 推奨）
 * -----------------------------------------------------------------------------
 * ルート直下に Directory.Packages.props を置き、全プロジェクトのバージョンを一括管理。
 * これにより各 .csproj の Version="*" を省略可能（<PackageReference> の Version 属性を削除）。
 *
 * <?xml version="1.0" encoding="utf-8"?>
 * <Project>
 *   <ItemGroup>
 *     <PackageVersion Include="SixLabors.ImageSharp" Version="3.*" />
 *     <PackageVersion Include="SixLabors.ImageSharp.Drawing" Version="2.*" />
 *     <PackageVersion Include="Spectre.Console.Cli" Version="1.*" />
 *     <PackageVersion Include="Spectre.Console" Version="1.*" />
 *     <PackageVersion Include="Microsoft.Extensions.Logging.Console" Version="8.*" />
 *     <PackageVersion Include="xunit" Version="2.*" />
 *     <PackageVersion Include="xunit.runner.visualstudio" Version="2.*" />
 *     <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.*" />
 *     <PackageVersion Include="Moq" Version="4.*" />
 *     <PackageVersion Include="FluentAssertions" Version="6.*" />
 *   </ItemGroup>
 * </Project>
 *
 * F) 次アクション（このコメントを貼った後にやること）
 * -----------------------------------------------------------------------------
 * 1) パッケージ復元（dotnet restore）→ 最小サンプルのビルド確認。
 * 2) 「コード生成手順 1.」で最上流 Chapter（ParseArgsChapter）の骨組み作成。
 * 3) 単体テストの初期雛形（引数バリデーション / dry-run / 上書き禁止）を用意。
 *
 * G) メモ（運用指針）
 * -----------------------------------------------------------------------------
 * - 画像品質や GIF パレットの高度化は “後から足せる”設計を維持（Open/Closed）。
 * - 破壊的操作（上書き/削除）は常にオプトイン。安全側のデフォルトを崩さない。
 * - 依存は最小限。必要になったら追加（Roslynator 等は将来の品質ゲートで検討）。
 * =============================================================================
 */
/* =============================================================================
 * コメント生成手順 6. Arg インタフェース最終化（LissajousTool / CLI）
 * =============================================================================
 * 目的:
 *   - 各 Chapter（Parse → Validate → Render → Encode → Write）直前に配置する Arg インタフェースの
 *     **最終シグネチャ**をここに確定し、以降のコード生成の単一情報源（SSoT）とする。
 *   - RULE #3 の「反変性成立のため、上流IFが下流IFを継承する」に従い、継承方向は
 *     IParseArgsArg : IValidateArg : IRenderArg : IEncodeArg : IWriteArg : IArg とする。
 *
 * 共通契約（列挙/型の約束）:
 *   - 列挙: 
 *       enum PngFilterStrategy { Auto, None, Sub, Up, Avg, Paeth }
 *       enum GifPaletteMethod { Auto, MedianCut, KMeans }
 *   - 範囲型（仕様上の契約名。実装は後続手順で struct かクラスを定義）:
 *       DoubleRange : { double Start; double End; }   // [Start..End]（単位はプロパティXMLに準拠）
 *   - コレクション契約: すべて IReadOnlyList<T> / IEnumerable<T> を用い、List<T>/配列の直接公開は禁止（RULE #16）。
 *   - 文字列色: HTML風 #RRGGBB または KnownColor 名（InvariantCulture）。実装時は厳格に検証。
 *
 * 例外/バリデーションの方針（全IFに共通）:
 *   - 値域外/形式不正は ArgumentException/ArgumentOutOfRangeException 相当の**ドメイン例外**として扱い、
 *     Chapter 内で処理して上位へ伝播しない（RULE #13）。
 *
 * -----------------------------------------------------------------------------
 * interface IArg（最下流の共通基底）
 * -----------------------------------------------------------------------------
 * string ProgramName { get; }
 * bool   IsDryRun    { get; }
 * bool   IsConfirm   { get; }
 * bool   AllowOverwrite { get; }
 * string? OutDirAbsolute { get; }
 * string? OutputPathAbsolute { get; }
 * long   MaxBytes { get; }
 * int    Dpi { get; }
 * string CultureName { get; }
 * bool   Verbose { get; }
 * bool   Quiet   { get; }
 * int?   Seed    { get; }
 *
 * -----------------------------------------------------------------------------
 * interface IWriteArg : IArg
 * -----------------------------------------------------------------------------
 * int Width  { get; }
 * int Height { get; }
 * string Background { get; }
 * string Foreground { get; }
 * string OutputPathRaw { get; }
 * string OutputExtension { get; }
 *
 * -----------------------------------------------------------------------------
 * interface IEncodeArg : IWriteArg
 * -----------------------------------------------------------------------------
 * int PngCompressLevel { get; }
 * PngFilterStrategy PngFilter { get; }
 * IReadOnlyList<KeyValuePair<string,string>> Metadata { get; }
 * int? GifFrames { get; }
 * double? GifFps { get; }
 * double? GifDurationSeconds { get; }
 * int GifLoopCount { get; }
 * int? GifDelayMilliseconds { get; }
 * GifPaletteMethod GifPalette { get; }
 * bool GifDither { get; }
 * int GifQuantizeColors { get; }
 *
 * -----------------------------------------------------------------------------
 * interface IRenderArg : IEncodeArg
 * -----------------------------------------------------------------------------
 * int AX { get; }
 * int AY { get; }
 * double PhaseDegrees { get; }
 * double AmpX { get; }
 * double AmpY { get; }
 * int Samples { get; }
 * double Thickness { get; }
 * bool AntiAlias { get; }
 * int Margin { get; }
 * DoubleRange? PhaseSweepDegrees { get; }
 * DoubleRange? AmpXSweep { get; }
 * DoubleRange? AmpYSweep { get; }
 *
 * -----------------------------------------------------------------------------
 * interface IValidateArg : IRenderArg
 * -----------------------------------------------------------------------------
 * string BaseDirAbsolute { get; }
 * bool IsPathSecurityChecked { get; }
 * string OutputKind { get; }
 * int ResolvedFrames { get; }
 * double ResolvedFps { get; }
 * double ResolvedDurationSeconds { get; }
 *
 * -----------------------------------------------------------------------------
 * interface IParseArgsArg : IValidateArg
 * -----------------------------------------------------------------------------
 * string? OutDirRaw { get; }
 * string OutputPathRawUnnormalized { get; }
 * string? PngFilterRaw { get; }
 * string? GifPaletteRaw { get; }
 * IReadOnlyList<string> MetadataRaw { get; }
 *
 * 備考:
 *   - RootArg は上記すべての IF を**明示的実装**し、Chapter からは対応 IF 経由でのみアクセス可能とする（RULE #7/#9）。
 *   - Chapter は**自分に対応する IF のみ**を参照し、他の IF へキャストしてアクセスしない（RULE #8）。
 *   - コレクションの空既定値は Array.Empty<T>() を共有利用（RULE #18）。
 *   - null 合体は同型でのみ使用し、契約型に揃える（RULE #17/#19）。
 * =============================================================================
 */

using LissajousTool.Abstractions;
using LissajousTool.Chapters;
using LissajousTool.Core;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using VeryVibe;

namespace VeryVibe
{
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
    public class Stage<TArg>
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

// このファイルは「コード生成手順 1.（Arg IF コード生成）」の成果物です。
// 目的: 各 Chapter で参照する役割インタフェース（Arg IF）と付随する列挙/範囲型を定義します。
// 規約: VeryVibe RULE（SOLID/安全既定/最小特権/契約はインタフェースで公開）に準拠。
// 対象: .NET 8.0 / Nullable 有効 / ImplicitUsings 無効（csproj 側設定）

namespace LissajousTool.Abstractions
{
    /// <summary>
    /// PNG のフィルタ戦略を表します。
    /// </summary>
    internal enum PngFilterStrategy
    {
        /// <summary>自動選択（エンコーダのヒューリスティクスに委譲）</summary>
        Auto,
        /// <summary>フィルタ無し</summary>
        None,
        /// <summary>Sub フィルタ</summary>
        Sub,
        /// <summary>Up フィルタ</summary>
        Up,
        /// <summary>Average フィルタ</summary>
        Avg,
        /// <summary>Paeth フィルタ</summary>
        Paeth
    }

    /// <summary>
    /// GIF のパレット生成方式を表します。
    /// </summary>
    internal enum GifPaletteMethod
    {
        /// <summary>自動選択（エンコーダのヒューリスティクスに委譲）</summary>
        Auto,
        /// <summary>Median Cut 法</summary>
        MedianCut,
        /// <summary>K-Means 法</summary>
        KMeans
    }

    /// <summary>
    /// 連続実数区間を表すイミュータブルな範囲型です（両端含む）。
    /// </summary>
    /// <param name="Start">開始値（小さい方）</param>
    /// <param name="End">終了値（大きい方）</param>
    internal readonly record struct DoubleRange(double Start, double End);

    // =========================================================================
    // Arg インタフェース（上流 → 下流の順に継承）
    // IParseArgsArg : IValidateArg : IRenderArg : IEncodeArg : IWriteArg : IArg
    // =========================================================================

    /// <summary>
    /// すべての Chapter で共有される最小限の共通メタ情報です。
    /// 既定は安全側（dry-run、上書き禁止、外部送信なし）とします。
    /// </summary>
    internal interface IArg : VeryVibe.IArg
    {
        /// <summary>プログラム名（ヘルプ表示用）。null/空は不可。</summary>
        string ProgramName { get; }

        /// <summary>ドライラン（既定: true）。<c>--assume-yes</c> が無ければ常に <c>true</c>。</summary>
        bool IsDryRun { get; }

        /// <summary>実生成の明示フラグ。<c>true</c> の場合のみファイル生成を許可。</summary>
        bool IsConfirm { get; }

        /// <summary>既存ファイルの上書き許可。<c>IsConfirm=true</c> と併用時のみ有効。</summary>
        bool AllowOverwrite { get; }

        /// <summary>BaseDir の絶対パス。Validate 後に確定。未確定なら <c>null</c>。</summary>
        string? OutDirAbsolute { get; }

        /// <summary>出力ファイルの絶対パス。Validate 後に確定。未確定なら <c>null</c>。</summary>
        string? OutputPathAbsolute { get; }

        /// <summary>生成物の最大サイズ上限（既定: 200,000,000 バイト）。負数禁止。</summary>
        long MaxBytes { get; }

        /// <summary>画像 DPI（既定: 96）。正の整数。</summary>
        int Dpi { get; }

        /// <summary>表示カルチャ名（例: <c>ja-JP</c>）。空なら実行環境の既定を使用。</summary>
        string CultureName { get; }

        /// <summary>詳細ログを有効化（ユーザー向け出力）。</summary>
        bool Verbose { get; }

        /// <summary>進捗などユーザー向け出力の抑制。<c>Verbose</c> と競合する場合は本設定を優先。</summary>
        bool Quiet { get; }

        /// <summary>疑似乱数シード（再現性のため）。未指定は <c>null</c>。</summary>
        int? Seed { get; }
    }

    /// <summary>
    /// 出力サイズ・色・パスなど、ファイル書き出しに関する最小集合です。
    /// </summary>
    internal interface IWriteArg : IArg
    {
        /// <summary>出力幅（px）。正の整数。既定: 1024。</summary>
        int Width { get; }

        /// <summary>出力高さ（px）。正の整数。既定: 1024。</summary>
        int Height { get; }

        /// <summary>背景色（#RRGGBB もしくは KnownColor 名）。既定: #000000。</summary>
        string Background { get; }

        /// <summary>線色（#RRGGBB もしくは KnownColor 名）。既定: #00FFFF。</summary>
        string Foreground { get; }

        /// <summary>ユーザー入力の出力パス（相対/絶対可）。Validate 後に絶対パスへ確定。</summary>
        string OutputPathRaw { get; }

        /// <summary>出力拡張子（小文字）。<c>.png</c> または <c>.gif</c>。</summary>
        string OutputExtension { get; }
    }

    /// <summary>
    /// エンコード方式に関する情報です（PNG/GIF 共通 + 各形式固有）。
    /// </summary>
    internal interface IEncodeArg : IWriteArg
    {
        // --- PNG 固有 ---
        /// <summary>PNG 圧縮レベル（0..9、既定: 6）。</summary>
        int PngCompressLevel { get; }

        /// <summary>PNG フィルタ戦略（既定: Auto）。</summary>
        PngFilterStrategy PngFilter { get; }

        /// <summary>
        /// PNG/GIF 共通メタデータ（key=value）。キー/値とも非空、値は最大 8192 文字。
        /// </summary>
        System.Collections.Generic.IReadOnlyList<System.Collections.Generic.KeyValuePair<string, string>> Metadata { get; }

        // --- GIF 固有 ---
        /// <summary>総フレーム数（&gt; 0）。fps/duration との整合は Validate で確定。</summary>
        int? GifFrames { get; }

        /// <summary>フレームレート（&gt; 0）。</summary>
        double? GifFps { get; }

        /// <summary>総再生時間（秒、&gt; 0）。</summary>
        double? GifDurationSeconds { get; }

        /// <summary>ループ回数（0=無限、0 以上）。</summary>
        int GifLoopCount { get; }

        /// <summary>各フレーム遅延（ms、&gt; 0）。指定時は fps/duration より優先。</summary>
        int? GifDelayMilliseconds { get; }

        /// <summary>パレット生成方式（既定: Auto）。</summary>
        GifPaletteMethod GifPalette { get; }

        /// <summary>ディザリング有効/無効（既定: true）。</summary>
        bool GifDither { get; }

        /// <summary>パレット色数（2..256、既定: 256）。</summary>
        int GifQuantizeColors { get; }
    }

    /// <summary>
    /// リサージュ曲線の幾何パラメータおよび描画に関する情報です。
    /// </summary>
    internal interface IRenderArg : IEncodeArg
    {
        /// <summary>X 側の角周波数（周波数比の X、正の整数）。</summary>
        int AX { get; }

        /// <summary>Y 側の角周波数（周波数比の Y、正の整数）。</summary>
        int AY { get; }

        /// <summary>位相差（度 0..360）。</summary>
        double PhaseDegrees { get; }

        /// <summary>X 振幅（0..1、画像スケール相対）。</summary>
        double AmpX { get; }

        /// <summary>Y 振幅（0..1、画像スケール相対）。</summary>
        double AmpY { get; }

        /// <summary>曲線サンプル数（≥ 2、既定: 10,000）。</summary>
        int Samples { get; }

        /// <summary>線の太さ（px、&gt; 0）。</summary>
        double Thickness { get; }

        /// <summary>アンチエイリアス（既定: true）。</summary>
        bool AntiAlias { get; }

        /// <summary>キャンバス余白（px、0 以上）。</summary>
        int Margin { get; }

        // アニメーション・スイープ（指定時のみ使用）
        /// <summary>位相のスイープ範囲（度）。例: 0..360。未指定は <c>null</c>。</summary>
        DoubleRange? PhaseSweepDegrees { get; }

        /// <summary>X 振幅のスイープ範囲（0..1）。未指定は <c>null</c>。</summary>
        DoubleRange? AmpXSweep { get; }

        /// <summary>Y 振幅のスイープ範囲（0..1）。未指定は <c>null</c>。</summary>
        DoubleRange? AmpYSweep { get; }
    }

    /// <summary>
    /// 検証後に確定する派生値を公開します（時間パラメータの整合や安全な絶対パスなど）。
    /// </summary>
    internal interface IValidateArg : IRenderArg
    {
        /// <summary>BaseDir の絶対パス。--out-dir 指定があればその実体、無ければカレントディレクトリ。</summary>
        string BaseDirAbsolute { get; }

        /// <summary>シンボリックリンク/再解析ポイント/ディレクトリトラバーサルの検査が完了済みか。</summary>
        bool IsPathSecurityChecked { get; }

        /// <summary>出力種別（"png" または "gif"）。</summary>
        string OutputKind { get; }

        /// <summary>整合性確定後のフレーム数。</summary>
        int ResolvedFrames { get; }

        /// <summary>整合性確定後のフレームレート。</summary>
        double ResolvedFps { get; }

        /// <summary>整合性確定後の総再生時間（秒）。</summary>
        double ResolvedDurationSeconds { get; }
    }

    /// <summary>
    /// コマンドライン解析段階での“生”入力も保持する最上流のインタフェースです。
    /// </summary>
    internal interface IParseArgsArg : IValidateArg
    {
        /// <summary>ユーザー入力の出力ディレクトリ（未正規化）。未指定は <c>null</c>。</summary>
        string? OutDirRaw { get; }

        /// <summary>ユーザー入力の出力ファイルパス（未正規化）。</summary>
        string OutputPathRawUnnormalized { get; }

        /// <summary>PNG フィルタの生入力。未知値は許容し、Validate で列挙へ正規化。</summary>
        string? PngFilterRaw { get; }

        /// <summary>GIF パレット方式の生入力。未知値は許容し、Validate で列挙へ正規化。</summary>
        string? GifPaletteRaw { get; }

        /// <summary>メタデータの生入力（"key=value" の列）。空は許容。</summary>
        System.Collections.Generic.IReadOnlyList<string> MetadataRaw { get; }
    }
}

#nullable enable
// このファイルは「コード生成手順 2.（RootArg 明示的実装スケルトン）」の成果物です。
// 目的: RootArg が全 Arg IF (IParseArgsArg → IValidateArg → IRenderArg → IEncodeArg → IWriteArg → IArg)
//      を **明示的** に実装し、各 Chapter が “対応する IF 経由” でのみアクセスできる土台を提供する。
// 規約: VeryVibe RULE（SOLID/安全既定/最小特権/契約はインタフェースで公開）に準拠。
// 注意: ここでは I/O（ファイル書込/ディレクトリ操作/ネットワーク）は一切行わない。
//       パス正規化やシンボリックリンク検査は ValidateChapter 側で実施する想定。

namespace LissajousTool.Core
{
    /// <summary>
    /// CLI から受け取った実行時引数を保持する不変に近いコンテナ。
    /// すべての Arg インタフェースを <b>明示的実装</b> し、IF 経由でのみ参照可能にする。
    /// <para>安全既定（dry-run / 上書き禁止 / 拡張子限定 / 出力は未確定）を採用。</para>
    /// </summary>
    internal sealed class RootArg :
        IParseArgsArg, IValidateArg, IRenderArg, IEncodeArg, IWriteArg, VeryVibe.IArg
    {
        // ==========================================================
        // 静的既定（契約型と一致）
        // ==========================================================
        private static readonly IReadOnlyList<string> EmptyStrings = [];
        private static readonly IReadOnlyList<KeyValuePair<string, string>> EmptyKVs = [];

        // ==========================================================
        // 不変/準不変のバッキングフィールド（契約型と完全一致）
        // ==========================================================
        private readonly string _programName;
        private readonly bool _isDryRun;
        private readonly bool _isConfirm;
        private readonly bool _allowOverwrite;
        private readonly long _maxBytes;
        private readonly int _dpi;
        private readonly string _cultureName;
        private readonly bool _verbose;
        private readonly bool _quiet;
        private readonly int? _seed;

        private readonly int _width;
        private readonly int _height;
        private readonly string _background;
        private readonly string _foreground;
        private readonly string _outputPathRaw;
        private readonly string _outputExtension;

        private readonly int _pngCompressLevel;
        private readonly PngFilterStrategy _pngFilter;
        private readonly IReadOnlyList<KeyValuePair<string, string>> _metadata;

        private readonly int? _gifFrames;
        private readonly double? _gifFps;
        private readonly double? _gifDurationSeconds;
        private readonly int _gifLoopCount;
        private readonly int? _gifDelayMilliseconds;
        private readonly GifPaletteMethod _gifPalette;
        private readonly bool _gifDither;
        private readonly int _gifQuantizeColors;

        private readonly int _ax;
        private readonly int _ay;
        private readonly double _phaseDegrees;
        private readonly double _ampX;
        private readonly double _ampY;
        private readonly int _samples;
        private readonly double _thickness;
        private readonly bool _antiAlias;
        private readonly int _margin;

        private readonly DoubleRange? _phaseSweepDegrees;
        private readonly DoubleRange? _ampXSweep;
        private readonly DoubleRange? _ampYSweep;

        // 生入力（Validate 前の値）
        private readonly string? _outDirRaw;
        private readonly string _outputPathRawUnnormalized;
        private readonly string? _pngFilterRaw;
        private readonly string? _gifPaletteRaw;
        private readonly IReadOnlyList<string> _metadataRaw;

        // ==========================================================
        // Validate 後に確定する派生値（Chapter により設定）
        //  - ここでは “非公開 setter を持つプロパティ” として保持
        //  - I/O は伴わないため、Chapter から安全に更新可能
        // ==========================================================
        private string? _outDirAbsolute;
        private string? _outputPathAbsolute;
        private string? _baseDirAbsolute;
        private bool _isPathSecurityChecked;
        private string _outputKind = "png"; // 初期仮値（.png/.gif 以外は Validate で拒否）

        private int _resolvedFrames = 120;
        private double _resolvedFps = 30.0;
        private double _resolvedDurationSeconds = 4.0;

        /// <summary>
        /// RootArg の新しいインスタンスを作成する。ここでは <b>型・範囲の基本検証</b> のみ行う。
        /// 絶対パス化やリンク検査などの I/O を伴う検証は ValidateChapter 側で行う。
        /// </summary>
        public RootArg(
            // 共通
            string programName,
            bool isDryRun = true,
            bool isConfirm = false,
            bool allowOverwrite = false,
            long maxBytes = 200_000_000L,
            int dpi = 96,
            string cultureName = "",
            bool verbose = false,
            bool quiet = false,
            int? seed = null,

            // 出力/キャンバス
            int width = 1024,
            int height = 1024,
            string background = "#000000",
            string foreground = "#00FFFF",
            string outputPathRaw = "",
            string outputExtension = "png",

            // PNG
            int pngCompressLevel = 6,
            PngFilterStrategy pngFilter = PngFilterStrategy.Auto,
            IReadOnlyList<KeyValuePair<string, string>>? metadata = null,

            // GIF
            int? gifFrames = 120,
            double? gifFps = 30.0,
            double? gifDurationSeconds = null,
            int gifLoopCount = 0,
            int? gifDelayMilliseconds = null,
            GifPaletteMethod gifPalette = GifPaletteMethod.Auto,
            bool gifDither = true,
            int gifQuantizeColors = 256,

            // 幾何/描画
            int ax = 3,
            int ay = 2,
            double phaseDegrees = 0.0,
            double ampX = 0.9,
            double ampY = 0.9,
            int samples = 10_000,
            double thickness = 2.0,
            bool antiAlias = true,
            int margin = 32,

            // スイープ（任意）
            DoubleRange? phaseSweepDegrees = null,
            DoubleRange? ampXSweep = null,
            DoubleRange? ampYSweep = null,

            // 生入力（Validate で正規化）
            string? outDirRaw = null,
            string outputPathRawUnnormalized = "",
            string? pngFilterRaw = null,
            string? gifPaletteRaw = null,
            IReadOnlyList<string>? metadataRaw = null
        )
        {
            // --- 基本検証（I/O を伴わない） ---
            if (string.IsNullOrWhiteSpace(programName))
                throw new ArgumentException("programName は必須です。", nameof(programName));

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxBytes);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dpi);

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            if (string.IsNullOrWhiteSpace(outputExtension))
                throw new ArgumentException("outputExtension は必須です（png/gif）。", nameof(outputExtension));
            if (pngCompressLevel is < 0 or > 9)
                throw new ArgumentOutOfRangeException(nameof(pngCompressLevel));

            if (gifQuantizeColors is < 2 or > 256)
                throw new ArgumentOutOfRangeException(nameof(gifQuantizeColors));
            ArgumentOutOfRangeException.ThrowIfNegative(gifLoopCount);
            if (gifDelayMilliseconds is { } ms && ms <= 0)
                throw new ArgumentOutOfRangeException(nameof(gifDelayMilliseconds));

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ax);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ay);
            if (phaseDegrees is < 0.0 or > 360.0)
                throw new ArgumentOutOfRangeException(nameof(phaseDegrees));
            if (ampX is < 0.0 or > 1.0) throw new ArgumentOutOfRangeException(nameof(ampX));
            if (ampY is < 0.0 or > 1.0) throw new ArgumentOutOfRangeException(nameof(ampY));
            ArgumentOutOfRangeException.ThrowIfLessThan(samples, 2);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(thickness);
            ArgumentOutOfRangeException.ThrowIfNegative(margin);

            // --- 格納（契約型に揃える / 空はキャッシュを使用） ---
            _programName = programName;
            _isDryRun = isDryRun;
            _isConfirm = isConfirm;
            _allowOverwrite = allowOverwrite;
            _maxBytes = maxBytes;
            _dpi = dpi;
            _cultureName = cultureName ?? string.Empty;
            _verbose = verbose;
            _quiet = quiet;
            _seed = seed;

            _width = width;
            _height = height;
            _background = background ?? "#000000";
            _foreground = foreground ?? "#00FFFF";
            _outputPathRaw = outputPathRaw ?? string.Empty;
            _outputExtension = (outputExtension ?? "png").ToLowerInvariant();

            _pngCompressLevel = pngCompressLevel;
            _pngFilter = pngFilter;
            _metadata = metadata ?? EmptyKVs;

            _gifFrames = gifFrames;
            _gifFps = gifFps;
            _gifDurationSeconds = gifDurationSeconds;
            _gifLoopCount = gifLoopCount;
            _gifDelayMilliseconds = gifDelayMilliseconds;
            _gifPalette = gifPalette;
            _gifDither = gifDither;
            _gifQuantizeColors = gifQuantizeColors;

            _ax = ax;
            _ay = ay;
            _phaseDegrees = phaseDegrees;
            _ampX = ampX;
            _ampY = ampY;
            _samples = samples;
            _thickness = thickness;
            _antiAlias = antiAlias;
            _margin = margin;

            _phaseSweepDegrees = phaseSweepDegrees;
            _ampXSweep = ampXSweep;
            _ampYSweep = ampYSweep;

            _outDirRaw = outDirRaw;
            _outputPathRawUnnormalized = outputPathRawUnnormalized ?? string.Empty;
            _pngFilterRaw = pngFilterRaw;
            _gifPaletteRaw = gifPaletteRaw;
            _metadataRaw = metadataRaw ?? EmptyStrings;

            // 初期の時間派生値（Validate で最終確定）
            if (_gifFrames.HasValue && _gifFps.HasValue && !_gifDurationSeconds.HasValue)
            {
                _resolvedFrames = Math.Max(1, _gifFrames.Value);
                _resolvedFps = Math.Max(0.0001, _gifFps.Value);
                _resolvedDurationSeconds = _resolvedFrames / _resolvedFps;
            }
        }

        // ==========================================================
        // ValidateChapter からの “結果反映” 用（I/O を伴わない）
        // ==========================================================

        /// <summary>
        /// ValidateChapter による安全検査結果を反映する（I/O を伴わない）。
        /// </summary>
        public void SetValidatedPathsAndKind(
            string baseDirAbsolute,
            string? outDirAbsolute,
            string? outputPathAbsolute,
            string outputKind,
            bool isPathSecurityChecked)
        {
            // 基本検証（null/空のみ）
            if (string.IsNullOrWhiteSpace(baseDirAbsolute)) throw new ArgumentException("", nameof(baseDirAbsolute)); // TODO: 例外メッセージ
            if (string.IsNullOrWhiteSpace(outputKind)) throw new ArgumentException("", nameof(outputKind));           // TODO: 例外メッセージ

            _baseDirAbsolute = baseDirAbsolute;
            _outDirAbsolute = outDirAbsolute;
            _outputPathAbsolute = outputPathAbsolute;
            _outputKind = outputKind;
            _isPathSecurityChecked = isPathSecurityChecked;
        }

        /// <summary>
        /// GIF の時間パラメータ整合後の確定値を反映する（I/O を伴わない）。
        /// </summary>
        public void SetResolvedTiming(int frames, double fps, double durationSeconds)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frames);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fps);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationSeconds);

            _resolvedFrames = frames;
            _resolvedFps = fps;
            _resolvedDurationSeconds = durationSeconds;
        }

        // ==========================================================
        // 明示的インタフェース実装（IF 経由でのみアクセス可能）
        // ==========================================================
        // IArg
        string Abstractions.IArg.ProgramName => _programName;
        bool Abstractions.IArg.IsDryRun => _isDryRun;
        bool Abstractions.IArg.IsConfirm => _isConfirm;
        bool Abstractions.IArg.AllowOverwrite => _allowOverwrite;
        string? Abstractions.IArg.OutDirAbsolute => _outDirAbsolute;
        string? Abstractions.IArg.OutputPathAbsolute => _outputPathAbsolute;
        long Abstractions.IArg.MaxBytes => _maxBytes;
        int Abstractions.IArg.Dpi => _dpi;
        string Abstractions.IArg.CultureName => _cultureName;
        bool Abstractions.IArg.Verbose => _verbose;
        bool Abstractions.IArg.Quiet => _quiet;
        int? Abstractions.IArg.Seed => _seed;

        // IWriteArg
        int IWriteArg.Width => _width;
        int IWriteArg.Height => _height;
        string IWriteArg.Background => _background;
        string IWriteArg.Foreground => _foreground;
        string IWriteArg.OutputPathRaw => _outputPathRaw;
        string IWriteArg.OutputExtension => _outputExtension;

        // IEncodeArg
        int IEncodeArg.PngCompressLevel => _pngCompressLevel;
        PngFilterStrategy IEncodeArg.PngFilter => _pngFilter;
        IReadOnlyList<KeyValuePair<string, string>> IEncodeArg.Metadata => _metadata;
        int? IEncodeArg.GifFrames => _gifFrames;
        double? IEncodeArg.GifFps => _gifFps;
        double? IEncodeArg.GifDurationSeconds => _gifDurationSeconds;
        int IEncodeArg.GifLoopCount => _gifLoopCount;
        int? IEncodeArg.GifDelayMilliseconds => _gifDelayMilliseconds;
        GifPaletteMethod IEncodeArg.GifPalette => _gifPalette;
        bool IEncodeArg.GifDither => _gifDither;
        int IEncodeArg.GifQuantizeColors => _gifQuantizeColors;

        // IRenderArg
        int IRenderArg.AX => _ax;
        int IRenderArg.AY => _ay;
        double IRenderArg.PhaseDegrees => _phaseDegrees;
        double IRenderArg.AmpX => _ampX;
        double IRenderArg.AmpY => _ampY;
        int IRenderArg.Samples => _samples;
        double IRenderArg.Thickness => _thickness;
        bool IRenderArg.AntiAlias => _antiAlias;
        int IRenderArg.Margin => _margin;
        DoubleRange? IRenderArg.PhaseSweepDegrees => _phaseSweepDegrees;
        DoubleRange? IRenderArg.AmpXSweep => _ampXSweep;
        DoubleRange? IRenderArg.AmpYSweep => _ampYSweep;

        // IValidateArg
        string IValidateArg.BaseDirAbsolute => _baseDirAbsolute ?? string.Empty;
        bool IValidateArg.IsPathSecurityChecked => _isPathSecurityChecked;
        string IValidateArg.OutputKind => _outputKind;
        int IValidateArg.ResolvedFrames => _resolvedFrames;
        double IValidateArg.ResolvedFps => _resolvedFps;
        double IValidateArg.ResolvedDurationSeconds => _resolvedDurationSeconds;

        // IParseArgsArg（生入力面）
        string? IParseArgsArg.OutDirRaw => _outDirRaw;
        string IParseArgsArg.OutputPathRawUnnormalized => _outputPathRawUnnormalized;
        string? IParseArgsArg.PngFilterRaw => _pngFilterRaw;
        string? IParseArgsArg.GifPaletteRaw => _gifPaletteRaw;
        IReadOnlyList<string> IParseArgsArg.MetadataRaw => _metadataRaw;

        // ==========================================================
        // 便宜メソッド（内部用）: ロケールを適用した文字列化
        // ==========================================================
        /// <summary>
        /// 人向け数値の表示書式（CurrentCulture）を返す。ログ/機械向けには使用しない。
        /// </summary>
        public string ToUserNumber(double value) =>
            value.ToString("N", string.IsNullOrWhiteSpace(_cultureName)
                ? CultureInfo.CurrentCulture
                : CultureInfo.GetCultureInfo(_cultureName));

        // 追加のロジックは各 Chapter 実装に委譲する（本クラスでは行わない）。
    }
}

#nullable enable
// このファイルは「コード生成手順 3.（Chapter 空実装雛形）」の成果物です。
// 目的: 各 Chapter（Parse → Validate → Render → Encode → Write）の “骨組み” を定義します。
// 注意: ここでは I/O（ファイル書込/ディレクトリ操作/ネットワーク）は実装しません（RULE #13）。
//       例外は Chapter 内で捕捉し、短文要約＋構造化ログを出す方針のみ示します。

// VeryVibe フレームワークのコア（IChapter / IChapterContext / IContextBuffer 等）は
// 別途提供される想定です。名前空間はプロジェクト側の規約に合わせて調整してください。
namespace LissajousTool.Chapters
{
    // このファイルは「コード生成手順 4.（Chapter の実装：最上流の ParseArgsChapter）」の成果物です。
    // 目的: 既存の骨組みから ParseArgsChapter を “動く実装” に引き上げる。
    // 方針: I/O を伴わない静的検証（範囲/形式/整合性）だけをここで完了し、パス正規化やファイルI/Oは Validate 以降に委譲。
    // 依存: VeryVibe の IChapter / IChapterContext / IContextBuffer は既存実装に依存（本ファイルでは追加定義しない）。
    // 注意: 例外は Chapter 内で握り、ユーザー向けの短文と構造化ログ（ILogger）に要約する（RULE #13）。

    /// <summary>
    /// CLI の“生入力”に対し、I/O を伴わない静的検証（範囲/形式/整合性）を行う Chapter。
    /// - パスの正規化・リンク検査・ファイル存在確認などの I/O は ValidateChapter に委譲する。
    /// - 想定外の値は ArgumentException/ArgumentOutOfRangeException 相当で内部的に検出し、catch して要約出力。
    /// </summary>
    internal sealed partial class ParseArgsChapter : IChapter<IParseArgsArg>
    {
        private readonly ILoggerFactory _logFactory;
        private readonly ILogger<ParseArgsChapter> _log;

        // 仕様追補（コメント生成手順3）に合わせた正規表現
        // - メタデータ key: ^[A-Za-z0-9._-]{1,64}$
        // - 色の16進 #RRGGBB
        private static readonly Regex MetadataKeyRegex = new(@"^[A-Za-z0-9._-]{1,64}$", RegexOptions.Compiled);
        private static readonly Regex HexColorRegex = new(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public ParseArgsChapter(ILoggerFactory logFactory)
        {
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _log = logFactory.CreateLogger<ParseArgsChapter>();
        }

        public void Handle(IParseArgsArg arg, IContextBuffer<IParseArgsArg> buffer)
        {
            try
            {
                // 1) 互いに依存する数値の整合性チェック（I/Oなし）
                ValidateGifTimingCoherence(arg);

                // 2) メタデータ形式（key=value 由来の正規化後コレクション）を静的検査
                //    - キー正規表現: ^[A-Za-z0-9._-]{1,64}$
                //    - 値長: <= 8192、制御文字や連続改行(>=3)を含まない
                ValidateMetadata(arg);

                // 3) 色の一次検査（厳密解決は ValidateChapter に委譲）
                //    - #RRGGBB なら即OK、そうでなければ「一見妥当な名前か」だけ軽く弾く
                ValidateColorLike(arg.Background, nameof(arg.Background));
                ValidateColorLike(arg.Foreground, nameof(arg.Foreground));

                // 4) GIF 固有パラメータの境界確認（RootArgでも範囲検証済みだが、ここでも明示）
                if (arg.GifQuantizeColors < 2 || arg.GifQuantizeColors > 256)
                    throw new ArgumentOutOfRangeException(nameof(arg.GifQuantizeColors), "gif-quantize は 2..256 の範囲で指定してください。");

                // 5) 出力拡張子の一次確認（実ファイル検証は後段）
                var ext = arg.OutputExtension.Trim().ToLowerInvariant();
                if (ext is not "png" and not "gif" and not ".png" and not ".gif")
                    throw new ArgumentException("出力拡張子は png または gif のみ対応です。", nameof(arg.OutputExtension));
                ValidateOutputPathRawIfPresent(arg.OutputPathRaw, ext);

                // 6) サマリ（ユーザー向けは CurrentCulture、一方ログの詳細は構造化で）
                _log.LogInformation("ParseArgs: 入力の一次検査に成功しました。kind={Kind}, size={W}x{H}, samples={Samples}, phase={Phase}deg",
                    ext.TrimStart('.'),
                    arg.Width, arg.Height,
                    arg.Samples,
                    arg.PhaseDegrees.ToString("N", CultureInfo.CurrentCulture));

                // 7) 次段へ（VeryVibe 実装に依存。ChapterContext の具象名/コンストラクタはフレームワーク側に合わせてください）
                buffer.PushBack(new ChapterContext<IValidateArg>(new ValidateChapter(_logFactory), arg));
            }
            catch (Exception ex)
            {
                // ユーザー向け: 簡潔で安全なメッセージ
                _log.LogWarning("入力パラメータの検査に失敗しました。指定内容を確認してください。");

                // ログ: 再現に必要な最小限の構造化メタ情報（パラメータ生値は極力出さない）
                _log.LogError(ex, "ParseArgs: 静的検証エラー。");
                // 伝播禁止（RULE #13）
            }
        }

        // ... ParseArgsChapter クラス内に追加
        private static readonly HashSet<string> ReservedWinNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "CON","PRN","AUX","NUL",
            "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
            "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9"
        };

        private static void ValidateOutputPathRawIfPresent(string? outputPathRaw, string extHint)
        {
            if (string.IsNullOrWhiteSpace(outputPathRaw))
                return; // 未指定なら ValidateChapter 側の既定名に任せる

            // 1) 無効文字（パス/ファイル名）チェック
            if (outputPathRaw.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("出力パスに無効な文字が含まれています。", nameof(outputPathRaw));

            var fileName = Path.GetFileName(outputPathRaw);
            if (fileName.Length == 0)
                throw new ArgumentException("出力パスにファイル名が含まれていません。", nameof(outputPathRaw));
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("ファイル名に無効な文字が含まれています。", nameof(outputPathRaw));

            // 2) 末尾のドット/スペース禁止（Windows）
            if (fileName.EndsWith(" ", StringComparison.Ordinal) || fileName.EndsWith(".", StringComparison.Ordinal))
                throw new ArgumentException("ファイル名の末尾にスペース/ドットは使用できません。", nameof(outputPathRaw));

            // 3) 予約名禁止（拡張子を除いた部分で判定）
            var stem = Path.GetFileNameWithoutExtension(fileName).TrimEnd(' ', '.');
            if (ReservedWinNames.Contains(stem))
                throw new ArgumentException($"'{stem}' は予約名のためファイル名に使用できません。", nameof(outputPathRaw));

            // 4) --ext と -o の拡張子矛盾チェック（png/gif のみ厳格判定）
            var hint = (extHint ?? "").Trim().TrimStart('.').ToLowerInvariant();
            var pathExt = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            if ((hint is "png" or "gif") && (pathExt is "png" or "gif") && hint != pathExt)
                throw new ArgumentException($"出力拡張子が --ext（{hint}）と -o のパス（{pathExt}）で一致しません。", nameof(outputPathRaw));
        }
        private static void ValidateGifTimingCoherence(IParseArgsArg a)
        {
            // --gif-delay-ms が指定されていれば、fps/duration/frames は補助値扱い（追補B）
            if (a.GifDelayMilliseconds is int delayMs && delayMs > 0)
            {
                // 一貫性の軽いチェック（極端値は別段階で警告対象）
                if (a.GifFrames is int frames && frames <= 0)
                    throw new ArgumentOutOfRangeException(nameof(a.GifFrames), "frames は 1 以上で指定してください。");
                if (a.GifFps is double fps && fps <= 0)
                    throw new ArgumentOutOfRangeException(nameof(a.GifFps), "fps は 0 より大きい値で指定してください。");
                if (a.GifDurationSeconds is double dur && dur <= 0)
                    throw new ArgumentOutOfRangeException(nameof(a.GifDurationSeconds), "duration は 0 より大きい値で指定してください。");
                return;
            }

            // 3変数すべて明示の場合は整合検査（|frames - round(duration * fps)| <= 1）
            if (a.GifFrames is int f && a.GifFps is double r && a.GifDurationSeconds is double d)
            {
                if (f <= 0 || r <= 0 || d <= 0)
                    throw new ArgumentOutOfRangeException(nameof(a.GifFrames), "frames/fps/duration は すべて正にしてください。");

                var expect = Math.Round(d * r);
                if (Math.Abs(f - expect) > 1)
                    throw new ArgumentException($"frames({f}) と duration*fps({d * r:N2}) の不整合が大きすぎます。", nameof(a.GifFrames));
            }
        }

        private static void ValidateMetadata(IParseArgsArg a)
        {
            if (a.Metadata is null) return;

            foreach (var kv in a.Metadata)
            {
                var key = (kv.Key ?? string.Empty).Trim();
                var val = kv.Value ?? string.Empty;

                if (!MetadataKeyRegex.IsMatch(key))
                    throw new ArgumentException($"メタデータのキー '{key}' は許可された形式ではありません。", nameof(a.Metadata));

                if (val.Length == 0 || val.Length > 8192)
                    throw new ArgumentException($"メタデータ '{key}' の値長が不正です（1..8192 文字）。", nameof(a.Metadata));

                // 制御文字と改行の過剰連続を禁止
                foreach (var ch in val)
                {
                    if ((ch >= '\u0000' && ch <= '\u001F') || ch == '\u007F')
                        throw new ArgumentException($"メタデータ '{key}' の値に制御文字を含めることはできません。", nameof(a.Metadata));
                }

                if (val.Contains("\n\n\n", StringComparison.Ordinal))
                    throw new ArgumentException($"メタデータ '{key}' の値に改行の連続（3回以上）は許可されません。", nameof(a.Metadata));
            }

            // 重複キーの取り扱いは「後勝ち」。ここでは重複存在を検出したら Verbose 相当で注意喚起するのが望ましいが、
            // ILogger の詳細レベル制御は呼び出し元の設定に委ねる（本章では例外化しない）。
            var dup = a.Metadata
                .Select(kv => kv.Key.Trim())
                .GroupBy(k => k, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (dup.Length > 0)
            {
                // ここでは例外にせず、ValidateChapter 以降での正規化（後勝ち）に委譲。
                // この Chapter は I/O を伴わない“静的検査”の役割に留める。
            }
        }

        private static void ValidateColorLike(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"色 '{paramName}' は空にできません。", paramName);

            var trimmed = value.Trim();

            // 受理範囲（現時点）: #RRGGBB or KnownColor
            if (HexColorRegex.IsMatch(trimmed))
                return;

            // KnownColor の厳密解決は ValidateChapter に委譲するため、ここでは
            // 「英数/ハイフン/アンダースコア/スペース程度の素性」で一次フィルタ。
            // （空白混入・明らかに不正な記号列は早期に弾く）
            if (!KnownColorNameRegex().IsMatch(trimmed))
                throw new ArgumentException($"色 '{paramName}' の指定 '{value}' は不正です。#RRGGBB 形式か、既知の色名を使用してください。", paramName);
        }

        [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9 _-]{0,63}$")]
        private static partial Regex KnownColorNameRegex();
    }

    internal static class PathSecurityUtil
    {
        private static StringComparison Cmp => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        public static string NormalizeAbsolute(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("パスが空です。");
            return Path.GetFullPath(path);
        }

        public static bool IsWithin(string baseDirAbs, string targetAbs)
        {
            // 末尾にディレクトリ区切りを付与して比較（プレフィックス衝突防止）
            var b = EnsureTrailingSeparator(baseDirAbs);
            var t = targetAbs;
            return t.StartsWith(b, Cmp);
        }

        public static string EnsureTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            var sep = Path.DirectorySeparatorChar;
            return path.EndsWith(sep) ? path : path + sep;
        }

        public static bool HasReparsePointInAncestry(string absPath)
        {
            // 存在する最深の親まで遡って、ReparsePoint を検出（ジャンクション/シンボリックリンク等）
            var dirPath = Path.GetDirectoryName(absPath);
            while (!string.IsNullOrEmpty(dirPath))
            {
                var di = new DirectoryInfo(dirPath);
                if (di.Exists)
                {
                    if ((di.Attributes & FileAttributes.ReparsePoint) != 0)
                        return true;
                }
                // ルートに到達したら終了
                var parent = Directory.GetParent(dirPath);
                if (parent == null) break;
                dirPath = parent.FullName;
            }
            return false;
        }

        // 追加: 祖先ディレクトリのうち、1つでも“実在”を見つけたかどうかを知らせる out 付き
        public static bool HasReparsePointInAncestry(string absPath, out bool encounteredExistingAncestor)
        {
            encounteredExistingAncestor = false;

            var dirPath = Path.GetDirectoryName(absPath);
            while (!string.IsNullOrEmpty(dirPath))
            {
                var di = new DirectoryInfo(dirPath);
                if (di.Exists)
                {
                    encounteredExistingAncestor = true;
                    if ((di.Attributes & FileAttributes.ReparsePoint) != 0)
                        return true;
                }

                var parent = Directory.GetParent(dirPath);
                if (parent == null) break;
                dirPath = parent.FullName;
            }
            return false;
        }
    }

    // ==========================================================
    // ValidateChapter（実装版）
    // ==========================================================
    // ---- ValidateChapter の差し替え（RootArg 参照削除版） ----
    internal sealed class ValidateChapter : IChapter<IValidateArg>
    {
        private readonly ILoggerFactory _logFactory;
        private readonly ILogger<ValidateChapter> _log;

        public ValidateChapter(ILoggerFactory logFactory)
        {
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _log = logFactory.CreateLogger<ValidateChapter>();
        }

        public void Handle(IValidateArg arg, IContextBuffer<IValidateArg> buffer)
        {
            try
            {
                // 1) BaseDir 決定（--out-dir 優先、無指定は CWD）
                string? outDirRaw = (arg as IParseArgsArg)?.OutDirRaw;
                var baseDirAbs = PathSecurityUtil.NormalizeAbsolute(
                    string.IsNullOrWhiteSpace(outDirRaw) ? Environment.CurrentDirectory : outDirRaw!);

                if (PathSecurityUtil.HasReparsePointInAncestry(PathSecurityUtil.EnsureTrailingSeparator(baseDirAbs), out var baseHasExisting))
                    throw new InvalidOperationException("BaseDir に再解析ポイント（シンボリックリンク/ジャンクション）が含まれています。");

                if (!baseHasExisting)
                    _log.LogWarning("警告: 指定された BaseDir の祖先に既存ディレクトリが見つかりません。未マウント/タイプミスの可能性があります: {BaseDir}", baseDirAbs);


                // 2) 出力パス正規化（BaseDir 配下強制）
                var extRaw = arg.OutputExtension?.Trim().ToLowerInvariant();
                var kind = (extRaw is ".gif" or "gif") ? "gif" : "png";

                var defaultFile = kind == "gif" ? "lissajous.gif" : "lissajous.png";
                var outputRaw = string.IsNullOrWhiteSpace(arg.OutputPathRaw) ? defaultFile : arg.OutputPathRaw.Trim();

                var outputAbs = Path.IsPathFullyQualified(outputRaw)
                    ? PathSecurityUtil.NormalizeAbsolute(outputRaw)
                    : PathSecurityUtil.NormalizeAbsolute(Path.Combine(baseDirAbs, outputRaw));

                if (!PathSecurityUtil.IsWithin(baseDirAbs, outputAbs))
                    throw new InvalidOperationException("出力先が BaseDir 配下ではありません。--out-dir を見直してください。");

                if (PathSecurityUtil.HasReparsePointInAncestry(outputAbs, out var outHasExisting))
                    throw new InvalidOperationException("出力先パスに再解析ポイント（シンボリックリンク/ジャンクション）が含まれています。");

                if (!outHasExisting)
                {
                    // ルート直下などで null の可能性に備え、baseDirAbs へフォールバック
                    var outDir = Path.GetDirectoryName(outputAbs) ?? baseDirAbs;
                    _log.LogWarning("警告: 出力先の祖先に既存ディレクトリが見つかりません（まだ存在しない可能性）。書き込み時に作成します: {OutDir}", outDir);
                }

                // 3) 上書きポリシー
                var exists = File.Exists(outputAbs);
                if (exists && !(arg.AllowOverwrite && arg.IsConfirm))
                    throw new IOException("既存ファイルが存在します。--allow-overwrite と --assume-yes の併用でのみ上書きを許可します。");

                // 4) GIF 時間パラメータの最終確定
                (int frames, double fps, double durSec) = ResolveGifTiming(arg);

                // 5) 安全弁（粗いサイズ見積り）
                try
                {
                    checked
                    {
                        var bytesEstimate =
                            (long)arg.Width * arg.Height * (kind == "png" ? 4 : 1) *
                            (kind == "gif" ? Math.Max(1, frames) : 1);

                        if (bytesEstimate > arg.MaxBytes)
                        {
                            if (arg.IsConfirm)
                                _log.LogWarning("推定サイズ {Bytes:N0} が上限 {Max:N0} を超えています。失敗する可能性があります。", bytesEstimate, arg.MaxBytes);
                            else
                                _log.LogInformation("dry-run: 推定サイズ {Bytes:N0} が上限 {Max:N0} を超えています。--assume-yes 前に見直しを推奨します。", bytesEstimate, arg.MaxBytes);
                        }
                    }
                }
                catch (OverflowException)
                {
                    _log.LogWarning("推定サイズ計算でオーバーフローが発生しました。サイズが大きすぎます。");
                }

                // 6) 検証結果をアダプタに封入して次段へ
                //    ※ RootArg へのキャストは行わない
                var adapted = new ValidatedArgAdapter(
                    src: arg, // IValidateArg は IRenderArg を継承
                    baseDirAbsolute: baseDirAbs,
                    outDirAbsolute: baseDirAbs,
                    outputPathAbsolute: outputAbs,
                    outputKind: kind,
                    isPathSecurityChecked: true,
                    resolvedFrames: frames,
                    resolvedFps: fps,
                    resolvedDurationSeconds: durSec
                );

                _log.LogInformation("Validate: kind={Kind}, output={Out}", kind, outputAbs);

                buffer.PushBack(new ChapterContext<IRenderArg>(new RenderChapter(_logFactory), adapted));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Validate: 入力検証でエラーが発生しました。");
                _log.LogWarning("指定されたパスやオプションが無効です。ベースディレクトリ/拡張子/上限値をご確認ください。");
            }
        }

        // ---- 検証結果アクセサ IF（内部用） ----
        internal interface IValidatedInfoAccessor
        {
            string BaseDirAbsolute { get; }
            string OutDirAbsolute { get; }
            string OutputPathAbsolute { get; }
            string OutputKind { get; }
            bool IsPathSecurityChecked { get; }

            int ResolvedFrames { get; }
            double ResolvedFps { get; }
            double ResolvedDurationSeconds { get; }
        }

        // ---- IRenderArg に検証結果を同梱するアダプタ ----
        internal sealed class ValidatedArgAdapter : IRenderArg, IValidatedInfoAccessor
        {
            private readonly IRenderArg _src;

            public ValidatedArgAdapter(
                IRenderArg src,
                string baseDirAbsolute,
                string outDirAbsolute,
                string outputPathAbsolute,
                string outputKind,
                bool isPathSecurityChecked,
                int resolvedFrames,
                double resolvedFps,
                double resolvedDurationSeconds)
            {
                _src = src ?? throw new ArgumentNullException(nameof(src));
                BaseDirAbsolute = baseDirAbsolute ?? throw new ArgumentNullException(nameof(baseDirAbsolute));
                OutDirAbsolute = outDirAbsolute ?? throw new ArgumentNullException(nameof(outDirAbsolute));
                OutputPathAbsolute = outputPathAbsolute ?? throw new ArgumentNullException(nameof(outputPathAbsolute));
                OutputKind = outputKind ?? throw new ArgumentNullException(nameof(outputKind));
                IsPathSecurityChecked = isPathSecurityChecked;
                ResolvedFrames = resolvedFrames;
                ResolvedFps = resolvedFps;
                ResolvedDurationSeconds = resolvedDurationSeconds;
            }

            // --- IValidatedInfoAccessor ---
            public string BaseDirAbsolute { get; }
            public string OutDirAbsolute { get; }
            public string OutputPathAbsolute { get; }
            public string OutputKind { get; }
            public bool IsPathSecurityChecked { get; }
            public int ResolvedFrames { get; }
            public double ResolvedFps { get; }
            public double ResolvedDurationSeconds { get; }

            // --- IArg（必要箇所は上書き） ---
            public string ProgramName => _src.ProgramName;
            public bool IsDryRun => _src.IsDryRun;
            public bool IsConfirm => _src.IsConfirm;
            public bool AllowOverwrite => _src.AllowOverwrite;
            // 検証済みの絶対パスを返す（元の値は無視）
            string? Abstractions.IArg.OutDirAbsolute => OutDirAbsolute;
            string? Abstractions.IArg.OutputPathAbsolute => OutputPathAbsolute;
            public long MaxBytes => _src.MaxBytes;
            public int Dpi => _src.Dpi;
            public string CultureName => _src.CultureName;
            public bool Verbose => _src.Verbose;
            public bool Quiet => _src.Quiet;
            public int? Seed => _src.Seed;

            // --- IWriteArg ---
            public int Width => _src.Width;
            public int Height => _src.Height;
            public string Background => _src.Background;
            public string Foreground => _src.Foreground;
            public string OutputPathRaw => _src.OutputPathRaw;
            public string OutputExtension => _src.OutputExtension;

            // --- IEncodeArg ---
            public int PngCompressLevel => _src.PngCompressLevel;
            public PngFilterStrategy PngFilter => _src.PngFilter;
            public IReadOnlyList<KeyValuePair<string, string>> Metadata => _src.Metadata;
            public int? GifFrames => _src.GifFrames;
            public double? GifFps => _src.GifFps;
            public double? GifDurationSeconds => _src.GifDurationSeconds;
            public int GifLoopCount => _src.GifLoopCount;
            public int? GifDelayMilliseconds => _src.GifDelayMilliseconds;
            public GifPaletteMethod GifPalette => _src.GifPalette;
            public bool GifDither => _src.GifDither;
            public int GifQuantizeColors => _src.GifQuantizeColors;

            // --- IRenderArg ---
            public int AX => _src.AX;
            public int AY => _src.AY;
            public double PhaseDegrees => _src.PhaseDegrees;
            public double AmpX => _src.AmpX;
            public double AmpY => _src.AmpY;
            public int Samples => _src.Samples;
            public double Thickness => _src.Thickness;
            public bool AntiAlias => _src.AntiAlias;
            public int Margin => _src.Margin;
            public DoubleRange? PhaseSweepDegrees => _src.PhaseSweepDegrees;
            public DoubleRange? AmpXSweep => _src.AmpXSweep;
            public DoubleRange? AmpYSweep => _src.AmpYSweep;
        }

        private static (int frames, double fps, double durationSeconds) ResolveGifTiming(IValidateArg a)
        {
            // 返却用の変数は最初に1セットだけ宣言しておく
            int outFrames;
            double outFps;
            double outDur;

            // delay 指定がある場合は最優先
            if (a.GifDelayMilliseconds is int delayMs && delayMs > 0)
            {
                var fpsFromDelay = 1000.0 / delayMs;

                outFrames = a.GifFrames ?? (a.GifDurationSeconds is double d && d > 0
                    ? Math.Max(1, (int)Math.Round(d * fpsFromDelay))
                    : 120); // 既定

                outFps = (a.GifFps is double f && f > 0) ? f : fpsFromDelay;
                outDur = (a.GifDurationSeconds is double dd && dd > 0) ? dd : outFrames / outFps;

                return (Math.Max(1, outFrames), Math.Max(0.0001, outFps), Math.Max(0.0001, outDur));
            }

            // 通常ルール（コメント生成手順 3.B）
            bool hasF = a.GifFrames is int _f && _f > 0;
            bool hasR = a.GifFps is double _r && _r > 0;
            bool hasD = a.GifDurationSeconds is double _d && _d > 0;

            if (hasF && hasR && !hasD)
            {
                outFrames = (int)a.GifFrames!;
                outFps = (double)a.GifFps!;
                outDur = outFrames / outFps;
            }
            else if (hasF && hasD && !hasR)
            {
                outFrames = (int)a.GifFrames!;
                outDur = (double)a.GifDurationSeconds!;
                outFps = outFrames / outDur;
            }
            else if (hasF && !hasR && !hasD)
            {
                outFrames = (int)a.GifFrames!;
                outFps = 30.0;
                outDur = outFrames / outFps;
            }
            else if (!hasF && hasD && !hasR)
            {
                outDur = (double)a.GifDurationSeconds!;
                outFps = 30.0;
                outFrames = Math.Max(1, (int)Math.Round(outDur * outFps));
            }
            else if (!hasF && hasR && !hasD)
            {
                outFps = (double)a.GifFps!;
                outFrames = 120;
                outDur = outFrames / outFps;
            }
            else if (hasF && hasR && hasD)
            {
                outFrames = (int)a.GifFrames!;
                outFps = (double)a.GifFps!;
                outDur = (double)a.GifDurationSeconds!;
                var expect = Math.Round(outDur * outFps);
                if (Math.Abs(outFrames - expect) > 1)
                    throw new ArgumentException("frames と duration*fps の不整合が大きすぎます。");
            }
            else
            {
                // 何も指定が無ければ既定
                outFrames = 120;
                outFps = 30.0;
                outDur = outFrames / outFps;
            }

            return (Math.Max(1, outFrames), Math.Max(0.0001, outFps), Math.Max(0.0001, outDur));
        }
    }

    // このブロックは「コード生成手順 4.（RenderChapter の実装）」です。
    // 目的: I/O を伴わずにリサージュ曲線の点群（ポリライン）を生成し、次段（Encode）へ橋渡しする。
    // 方針:
    //  - 計算は純粋関数（LissajousMath）で行い、副作用はログ出力のみ。
    //  - 次段へは IEncodeArg を実装したアダプタ（EncodeInputAdapter）を渡す。
    //    * これにより契約（IEncodeArg）は保ちつつ、Render 生成物（RenderProduct）を同梱できる。
    //  - エラーは握り込み（RULE #13）。ユーザー向けは簡潔、ログは構造化。
    internal sealed class RenderChapter : IChapter<IRenderArg>
    {
        private readonly ILoggerFactory _logFactory;
        private readonly ILogger<RenderChapter> _log;

        public RenderChapter(ILoggerFactory logFactory)
        {
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _log = logFactory.CreateLogger<RenderChapter>();
        }

        public void Handle(IRenderArg arg, IContextBuffer<IRenderArg> buffer)
        {
            try
            {
                // --- 1) 前提の軽い検証（I/O なし） -----------------------------
                if (arg.Samples < 2)
                    throw new ArgumentOutOfRangeException(nameof(arg.Samples), "samples は 2 以上で指定してください。");
                if (arg.AmpX <= 0 || arg.AmpY <= 0)
                    throw new ArgumentOutOfRangeException(nameof(arg.AmpX), "AmpX/AmpY は正の値にしてください。");
                if (arg.AX == 0 || arg.AY == 0)
                    throw new ArgumentOutOfRangeException(nameof(arg.AX), "AX/AY は 0 以外の整数にしてください。");
                if (arg.Margin < 0 || arg.Margin * 2 >= Math.Min(arg.Width, arg.Height))
                    throw new ArgumentOutOfRangeException(nameof(arg.Margin), "margin がキャンバスに収まりません。");

                // --- 2) 曲線点群の生成 ----------------------------------------
                var phaseRad = arg.PhaseDegrees * (Math.PI / 180.0);
                // ポリライン生成（CLI 指定を反映）
                var poly = LissajousMath.BuildPolyline(
                    width:   arg.Width,
                    height:  arg.Height,
                    margin:  arg.Margin,
                    ax:      arg.AX,
                    ay:      arg.AY,
                    ampX:    arg.AmpX,
                    ampY:    arg.AmpY,
                    phaseRad: phaseRad,
                    samples: arg.Samples,
                    thickness: Math.Clamp(arg.Thickness, 0.1, 256.0),
                    antiAlias: arg.AntiAlias);

                // ログ（ユーザー向け: CurrentCulture／詳細は構造化）
                _log.LogInformation(
                    "Render: 点群生成 {Count} 点完了。canvas={W}x{H}, margin={M}, freq=({Ax},{Ay}), amp=({Axv:N2},{Ayv:N2}), phase={PhaseDeg:N2}deg, thickness={T}, aa={AA}",
                    poly.Points.Length, arg.Width, arg.Height, arg.Margin,
                    arg.AX, arg.AY, arg.AmpX, arg.AmpY, arg.PhaseDegrees,
                    arg.Thickness, arg.AntiAlias
                );

                // --- 3) 次段へ橋渡し ------------------------------------------
                // Encode には IEncodeArg として渡し、同梱の RenderProduct はアダプタ経由で参照可能。
                var encodeAdapter = new EncodeInputAdapter(arg, poly);
                buffer.PushBack(new ChapterContext<IEncodeArg>(new EncodeChapter(_logFactory), encodeAdapter));
            }
            catch (Exception ex)
            {
                // 伝播禁止（RULE #13）
                _log.LogWarning("描画準備に失敗しました。パラメータ（周波数比/位相/振幅/サンプル数/余白）を確認してください。");
                _log.LogError(ex, "Render: 点群生成エラー。");
            }
        }
    }

    // ---------------------------------------------------------------------
    // 内部: Render の生成物
    // ---------------------------------------------------------------------
    internal readonly struct PixelF
    {
        public readonly float X;
        public readonly float Y;
        public PixelF(float x, float y) { X = x; Y = y; }
    }

    internal sealed class RenderProduct
    {
        public PixelF[] Points { get; }
        public double Thickness { get; }
        public bool AntiAlias { get; }

        public RenderProduct(PixelF[] points, double thickness, bool antiAlias)
        {
            Points = points;
            Thickness = thickness;
            AntiAlias = antiAlias;
        }
    }

    // ---------------------------------------------------------------------
    // 内部: IEncodeArg アダプタ（RenderProduct を同梱）
    // ---------------------------------------------------------------------
    internal interface IRenderProductAccessor
    {
        RenderProduct Render { get; }
    }

    internal sealed class EncodeInputAdapter : IEncodeArg, IRenderProductAccessor
    {
        private readonly IEncodeArg _src;
        public RenderProduct Render { get; }

        public EncodeInputAdapter(IEncodeArg source, RenderProduct render)
        {
            _src = source ?? throw new ArgumentNullException(nameof(source));
            Render = render ?? throw new ArgumentNullException(nameof(render));
        }

        // ---- IArg / IWriteArg / IEncodeArg を _src へ委譲 ----
        // IArg
        public string ProgramName => _src.ProgramName;
        public bool IsDryRun => _src.IsDryRun;
        public bool IsConfirm => _src.IsConfirm;
        public bool AllowOverwrite => _src.AllowOverwrite;
        public string? OutDirAbsolute => _src.OutDirAbsolute;
        public string? OutputPathAbsolute => _src.OutputPathAbsolute;
        public long MaxBytes => _src.MaxBytes;
        public int Dpi => _src.Dpi;
        public string CultureName => _src.CultureName;
        public bool Verbose => _src.Verbose;
        public bool Quiet => _src.Quiet;
        public int? Seed => _src.Seed;

        // IWriteArg
        public int Width => _src.Width;
        public int Height => _src.Height;
        public string Background => _src.Background;
        public string Foreground => _src.Foreground;
        public string OutputPathRaw => _src.OutputPathRaw;
        public string OutputExtension => _src.OutputExtension;

        // IEncodeArg
        public int PngCompressLevel => _src.PngCompressLevel;
        public PngFilterStrategy PngFilter => _src.PngFilter;
        public IReadOnlyList<KeyValuePair<string, string>> Metadata => _src.Metadata;
        public int? GifFrames => _src.GifFrames;
        public double? GifFps => _src.GifFps;
        public double? GifDurationSeconds => _src.GifDurationSeconds;
        public int GifLoopCount => _src.GifLoopCount;
        public int? GifDelayMilliseconds => _src.GifDelayMilliseconds;
        public GifPaletteMethod GifPalette => _src.GifPalette;
        public bool GifDither => _src.GifDither;
        public int GifQuantizeColors => _src.GifQuantizeColors;
    }

    // ---------------------------------------------------------------------
    // 内部: 計算ユーティリティ（純粋関数）
    // ---------------------------------------------------------------------
    internal static class LissajousMath
    {
        public static RenderProduct BuildPolyline(
            int width, int height, int margin,
            int ax, int ay,
            double ampX, double ampY,
            double phaseRad,
            int samples,
            double thickness = 1.0,
            bool antiAlias = true)
        {
            // キャンバス座標系: 左上 (0,0) / Y+ 下向き
            var cx = (width - 1) * 0.5;
            var cy = (height - 1) * 0.5;
            var sx = Math.Max(1.0, (width - margin * 2) * 0.5);
            var sy = Math.Max(1.0, (height - margin * 2) * 0.5);

            var pts = new PixelF[samples];

            // t in [0, 2π] で十分（ax, ay が整数のため）
            var tMax = 2.0 * Math.PI;
            for (int i = 0; i < samples; i++)
            {
                var t = (samples == 1) ? 0.0 : (tMax * i) / (samples - 1);
                var x = ampX * Math.Sin(ax * t + phaseRad);
                var y = ampY * Math.Sin(ay * t);

                // [-1..1] 正規化 → ピクセル
                var px = cx + x * sx;
                var py = cy - y * sy; // 画面は Y 軸が下向き

                // 範囲の安全化（端の ±ε を吸収）
                var fx = (float)Math.Clamp(px, 0.0, width - 1.0);
                var fy = (float)Math.Clamp(py, 0.0, height - 1.0);
                pts[i] = new PixelF(fx, fy);
            }

            return new RenderProduct(pts, thickness, antiAlias);
        }
    }

    // ==========================================================
    // EncodeChapter
    // 役割: PNG/GIF へのエンコード（ImageSharp 等の実エンジン呼び出しはコード生成手順 4 で実装）
    // 次段: WriteChapter（IWriteArg）
    // ==========================================================
    // ============================================================================
    // コード生成手順 4.（EncodeChapter の実装）
    //
    // 方針:
    //  - 依存ライブラリ（ImageSharp 等）にハードリンクしない抽象 IImageEncoderEngine を導入。
    //  - EncodeChapter は PNG/GIF いずれも「エンジン非依存」でリクエストを構築して委譲する。
    //  - GIF のフレーム列は、RenderChapter が同梱した RenderProduct が 1 枚のみの場合、
    //    IRenderArg のスイープ指定（Phase/AmpX/AmpY）から Encode 側で線形補間して生成。
    //  - 実バイト列は IEncodedBinaryAccessor として次段（WriteChapter）に手渡す。
    //  - 実エンコード手段は将来差し替え（ImageSharp 実装など）で提供する。
    // ============================================================================

    // -------------------------------------------------------------
    // 下流への受け渡し: エンコード済バイナリのアクセサ
    // -------------------------------------------------------------
    internal interface IEncodedBinaryAccessor
    {
        ReadOnlyMemory<byte> Binary { get; }
        string MimeType { get; }
        string Extension { get; }
        long UncompressedBytesEstimate { get; }
    }

    // -------------------------------------------------------------
    // IWriteArg へエンコード結果を同梱して引き継ぐアダプタ
    // -------------------------------------------------------------
    internal sealed class EncodeOutputAdapter : IWriteArg, IEncodedBinaryAccessor
    {
        private readonly IWriteArg _src;
        public ReadOnlyMemory<byte> Binary { get; }
        public string MimeType { get; }
        public string Extension { get; }
        public long UncompressedBytesEstimate { get; }

        public EncodeOutputAdapter(
            IWriteArg source,
            ReadOnlyMemory<byte> binary,
            string mimeType,
            string extension,
            long uncompressedBytesEstimate)
        {
            _src = source ?? throw new ArgumentNullException(nameof(source));
            Binary = binary;
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
            Extension = extension ?? throw new ArgumentNullException(nameof(extension));
            UncompressedBytesEstimate = uncompressedBytesEstimate;
        }

        // ---- IArg ----
        public string ProgramName => _src.ProgramName;
        public bool IsDryRun => _src.IsDryRun;
        public bool IsConfirm => _src.IsConfirm;
        public bool AllowOverwrite => _src.AllowOverwrite;
        public string? OutDirAbsolute => _src.OutDirAbsolute;
        public string? OutputPathAbsolute => _src.OutputPathAbsolute;
        public long MaxBytes => _src.MaxBytes;
        public int Dpi => _src.Dpi;
        public string CultureName => _src.CultureName;
        public bool Verbose => _src.Verbose;
        public bool Quiet => _src.Quiet;
        public int? Seed => _src.Seed;

        // ---- IWriteArg ----
        public int Width => _src.Width;
        public int Height => _src.Height;
        public string Background => _src.Background;
        public string Foreground => _src.Foreground;
        public string OutputPathRaw => _src.OutputPathRaw;
        public string OutputExtension => _src.OutputExtension;
    }

    // -------------------------------------------------------------
    // エンコードエンジンの抽象
    //   - ここでは依存を生やさず、将来 ImageSharp 等で実装差し替え
    // -------------------------------------------------------------
    internal interface IImageEncoderEngine
    {
        ReadOnlyMemory<byte> EncodePng(PngEncodeRequest request);
        ReadOnlyMemory<byte> EncodeGif(GifEncodeRequest request);
    }

    internal sealed class PngEncodeRequest
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public int Dpi { get; init; }
        public string Background { get; init; } = "#000000";
        public string Foreground { get; init; } = "#00FFFF";
        public IReadOnlyList<KeyValuePair<string, string>> Metadata { get; init; } = Array.Empty<KeyValuePair<string, string>>();
        public PngFilterStrategy Filter { get; init; }
        public int CompressLevel { get; init; }
        public RenderProduct Polyline { get; init; } = default!;
    }

    internal sealed class GifEncodeRequest
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public int Dpi { get; init; }
        public string Background { get; init; } = "#000000";
        public string Foreground { get; init; } = "#00FFFF";
        public IReadOnlyList<KeyValuePair<string, string>> Metadata { get; init; } = Array.Empty<KeyValuePair<string, string>>();

        public int Frames { get; init; }
        public double Fps { get; init; }
        public double DurationSeconds { get; init; }
        public int LoopCount { get; init; }
        public int? DelayMilliseconds { get; init; }

        public GifPaletteMethod Palette { get; init; }
        public bool Dither { get; init; }
        public int QuantizeColors { get; init; }

        // 各フレームのポリライン（既にスイープ反映済み）
        public IReadOnlyList<RenderProduct> FramePolylines { get; init; } = Array.Empty<RenderProduct>();
    }

    // -------------------------------------------------------------
    // 既定エンジン（未設定時のダミー）
    //   - dry-run なら空配列、実行なら NotSupported を投げる
    // -------------------------------------------------------------
    internal sealed class NotConfiguredEncoderEngine : IImageEncoderEngine
    {
        public ReadOnlyMemory<byte> EncodePng(PngEncodeRequest request)
            => throw new NotSupportedException("実エンコーダが構成されていません。ImageSharp などの実装を差し込んでください。");

        public ReadOnlyMemory<byte> EncodeGif(GifEncodeRequest request)
            => throw new NotSupportedException("実エンコーダが構成されていません。ImageSharp などの実装を差し込んでください。");
    }

    // ==========================================================
    // EncodeChapter
    // 役割: PNG/GIF へのエンコード（エンジン抽象へ委譲）
    // 次段: WriteChapter（IWriteArg）
    // ==========================================================
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
                // 出力種別の決定（Validate 済みなら .OutputExtension は小文字拡張子）
                var ext = NormalizeExt(arg.OutputExtension);
                var mime = (ext == ".gif") ? "image/gif" : "image/png";

                // PNG: Render の生成物があればそれを使う（Render → Encode の基本線）
                if (ext == ".png")
                {
                    var render = ResolveRenderOnce(arg);
                    var req = new PngEncodeRequest
                    {
                        Width = arg.Width,
                        Height = arg.Height,
                        Dpi = arg.Dpi,
                        Background = arg.Background,
                        Foreground = arg.Foreground,
                        Metadata = arg.Metadata,
                        Filter = arg.PngFilter,
                        CompressLevel = arg.PngCompressLevel,
                        Polyline = render
                    };

                    ReadOnlyMemory<byte> bin;
                    if (arg.IsDryRun)
                    {
                        bin = ReadOnlyMemory<byte>.Empty;
                        _log.LogInformation("Encode(PNG): dry-run のため実エンコードを省略しました。");
                    }
                    else
                    {
                        bin = _engine.EncodePng(req);
                        _log.LogInformation("Encode(PNG): {Bytes} bytes を生成しました。", bin.Length);
                    }

                    var est = EstimateUncompressedBytes(arg.Width, arg.Height, 1);
                    var output = new EncodeOutputAdapter(arg, bin, mime, ext, est);
                    buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(_logFactory), output));
                    return;
                }

                // GIF: タイミングを解決し、フレーム列（ポリライン）を準備
                var (frames, fps, durSec) = ResolveGifTimingForEncode(arg);
                var framesList = BuildFramePolylines(arg, frames);

                var gifReq = new GifEncodeRequest
                {
                    Width = arg.Width,
                    Height = arg.Height,
                    Dpi = arg.Dpi,
                    Background = arg.Background,
                    Foreground = arg.Foreground,
                    Metadata = arg.Metadata,
                    Frames = frames,
                    Fps = fps,
                    DurationSeconds = durSec,
                    LoopCount = arg.GifLoopCount,
                    DelayMilliseconds = arg.GifDelayMilliseconds,
                    Palette = arg.GifPalette,
                    Dither = arg.GifDither,
                    QuantizeColors = arg.GifQuantizeColors,
                    FramePolylines = framesList
                };

                ReadOnlyMemory<byte> gifBin;
                if (arg.IsDryRun)
                {
                    gifBin = ReadOnlyMemory<byte>.Empty;
                    _log.LogInformation("Encode(GIF): dry-run のため実エンコードを省略しました。frames={Frames}, fps={Fps}", frames, fps.ToString("N", CultureInfo.CurrentCulture));
                }
                else
                {
                    gifBin = _engine.EncodeGif(gifReq);
                    _log.LogInformation("Encode(GIF): {Bytes} bytes / {Frames} frames を生成しました。", gifBin.Length, frames);
                }

                var gifEst = EstimateUncompressedBytes(arg.Width, arg.Height, frames);
                var gifOut = new EncodeOutputAdapter(arg, gifBin, mime, ext, gifEst);
                buffer.PushBack(new ChapterContext<IWriteArg>(new WriteChapter(_logFactory), gifOut));
            }
            catch (Exception ex)
            {
                // 伝播禁止（RULE #13）
                _log.LogWarning("エンコードに失敗しました。形式（png/gif）や圧縮/パレット設定を見直してください。");
                _log.LogError(ex, "Encode: エンコードエラー。");
            }
        }

        // ---- helpers -------------------------------------------------------

        private static string NormalizeExt(string ext)
        {
            var s = (ext ?? string.Empty).Trim().ToLowerInvariant();
            if (!s.StartsWith(".")) s = "." + s;
            return s switch { ".png" => ".png", ".gif" => ".gif", _ => ".png" };
        }

        private static long EstimateUncompressedBytes(int width, int height, int frames)
        {
            // 非可逆な上限見積り: 32bpp RGBA * pixels * frames
            try
            {
                checked
                {
                    return (long)width * height * 4L * frames;
                }
            }
            catch
            {
                // オーバーフロー時は MaxValue で頭打ち
                return long.MaxValue;
            }
        }

        private RenderProduct ResolveRenderOnce(IEncodeArg arg)
        {
            // Render からの同梱があればそれを使う
            if (arg is IRenderProductAccessor acc)
                return acc.Render;

            // 無ければ IRenderArg が持つ幾何情報から 1 枚生成（フォールバック）
            if (arg is not IRenderArg r)
                throw new ArgumentException("Render 情報が見つかりません。", nameof(arg));

            var phaseRad = r.PhaseDegrees * (Math.PI / 180.0);
            return LissajousMath.BuildPolyline(
                width: r.Width,
                height: r.Height,
                margin: r.Margin,
                ax: r.AX,
                ay: r.AY,
                ampX: r.AmpX,
                ampY: r.AmpY,
                phaseRad: phaseRad,
                samples: r.Samples,
                thickness: r.Thickness,
                antiAlias: r.AntiAlias);
        }

        private static (int frames, double fps, double duration) ResolveGifTimingForEncode(IEncodeArg a)
        {
            int outFrames;
            double outFps;
            double outDur;

            if (a.GifDelayMilliseconds is int delayMs && delayMs > 0)
            {
                var fpsFromDelay = 1000.0 / delayMs;
                outFps = (a.GifFps is double rf && rf > 0) ? rf : fpsFromDelay;
                outFrames = a.GifFrames ?? (a.GifDurationSeconds is double dd && dd > 0
                    ? Math.Max(1, (int)Math.Round(dd * outFps))
                    : 120);
                outDur = (a.GifDurationSeconds is double d2 && d2 > 0) ? d2 : outFrames / outFps;
                return (Math.Max(1, outFrames), Math.Max(0.0001, outFps), Math.Max(0.0001, outDur));
            }

            bool hasF = a.GifFrames is int _f && _f > 0;
            bool hasR = a.GifFps is double _r && _r > 0;
            bool hasD = a.GifDurationSeconds is double _d && _d > 0;

            if (hasF && hasR && !hasD)
            {
                outFrames = (int)a.GifFrames!;
                outFps = (double)a.GifFps!;
                outDur = outFrames / outFps;
            }
            else if (hasF && hasD && !hasR)
            {
                outFrames = (int)a.GifFrames!;
                outDur = (double)a.GifDurationSeconds!;
                outFps = outFrames / outDur;
            }
            else if (hasF && !hasR && !hasD)
            {
                outFrames = (int)a.GifFrames!;
                outFps = 30.0;
                outDur = outFrames / outFps;
            }
            else if (!hasF && hasD && !hasR)
            {
                outDur = (double)a.GifDurationSeconds!;
                outFps = 30.0;
                outFrames = Math.Max(1, (int)Math.Round(outDur * outFps));
            }
            else if (!hasF && hasR && !hasD)
            {
                outFps = (double)a.GifFps!;
                outFrames = 120;
                outDur = outFrames / outFps;
            }
            else if (hasF && hasR && hasD)
            {
                outFrames = (int)a.GifFrames!;
                outFps = (double)a.GifFps!;
                outDur = (double)a.GifDurationSeconds!;
                var expect = Math.Round(outDur * outFps);
                if (Math.Abs(outFrames - expect) > 1)
                    throw new ArgumentException("frames と duration*fps の不整合が大きすぎます。");
            }
            else
            {
                outFrames = 120;
                outFps = 30.0;
                outDur = outFrames / outFps;
            }

            return (Math.Max(1, outFrames), Math.Max(0.0001, outFps), Math.Max(0.0001, outDur));
        }

        private static double Lerp(DoubleRange range, int i, int n)
        {
            if (n <= 1) return range.Start;
            var t = (double)i / (n - 1);
            return range.Start + (range.End - range.Start) * t;
        }

        private static IReadOnlyList<RenderProduct> BuildFramePolylines(IEncodeArg arg, int frames)
        {
            if (arg is not IRenderArg r)
                throw new ArgumentException("アニメーション生成には IRenderArg が必要です。", nameof(arg));

            // スイープ未指定なら、1 枚を複製（可視上は静止）
            if (r.PhaseSweepDegrees is null && r.AmpXSweep is null && r.AmpYSweep is null)
            {
                var baseRender = new List<RenderProduct>(frames);
                var single = LissajousMath.BuildPolyline(
                    width: r.Width, height: r.Height, margin: r.Margin,
                    ax: r.AX, ay: r.AY,
                    ampX: r.AmpX, ampY: r.AmpY,
                    phaseRad: r.PhaseDegrees * (Math.PI / 180.0),
                    samples: r.Samples,
                    thickness: r.Thickness,
                    antiAlias: r.AntiAlias);
                for (int i = 0; i < frames; i++) baseRender.Add(single);
                return baseRender;
            }

            // スイープ指定あり: 各フレームで線形補間
            var list = new List<RenderProduct>(frames);
            for (int i = 0; i < frames; i++)
            {
                var phaseDeg = r.PhaseSweepDegrees is DoubleRange pr
                    ? Lerp(pr, i, frames)
                    : r.PhaseDegrees;

                var ampX = r.AmpXSweep is DoubleRange xr ? Lerp(xr, i, frames) : r.AmpX;
                var ampY = r.AmpYSweep is DoubleRange yr ? Lerp(yr, i, frames) : r.AmpY;

                var product = LissajousMath.BuildPolyline(
                    width: r.Width, height: r.Height, margin: r.Margin,
                    ax: r.AX, ay: r.AY,
                    ampX: ampX, ampY: ampY,
                    phaseRad: phaseDeg * (Math.PI / 180.0),
                    samples: r.Samples,
                    thickness: r.Thickness,
                    antiAlias: r.AntiAlias);

                list.Add(product);
            }
            return list;
        }

        // --- engine ----------------------------------------------------
        internal sealed class ImageSharpEncoderEngine : IImageEncoderEngine
        {
            public ReadOnlyMemory<byte> EncodePng(PngEncodeRequest request)
            {
                var bg = ParseColor(request.Background);
                var fg = ParseColor(request.Foreground);

                using var img = new Image<Rgba32>(request.Width, request.Height, bg);
                SetResolution(img.Metadata, request.Dpi);
                TryApplyPngText(img.Metadata, request.Metadata);

                // 線を描く
                DrawPolyline(img, request.Polyline, fg);

                var encoder = new PngEncoder
                {
                    // enum へのキャストが必要
                    CompressionLevel = (PngCompressionLevel)Clamp(request.CompressLevel, 0, 9),
                    FilterMethod = MapFilter(request.Filter)
                };

                using var ms = new MemoryStream(capacity: Math.Max(1024, request.Width * request.Height / 2));
                img.Save(ms, encoder);
                return new ReadOnlyMemory<byte>(ms.ToArray());
            }

            public ReadOnlyMemory<byte> EncodeGif(GifEncodeRequest request)
            {
                var bg = ParseColor(request.Background);
                var fg = ParseColor(request.Foreground);

                // フレーム遅延（1/100 秒単位）
                int frameDelay = request.DelayMilliseconds is int delayMs && delayMs > 0
                    ? Math.Max(1, (int)Math.Round(delayMs / 10.0))
                    : Math.Max(1, (int)Math.Round(100.0 / Math.Max(0.0001, request.Fps)));

                // 先頭フレーム
                using var first = new Image<Rgba32>(request.Width, request.Height, bg);
                SetResolution(first.Metadata, request.Dpi);
                DrawPolyline(first, request.FramePolylines.Count > 0 ? request.FramePolylines[0] : EmptyPolyline(request), fg);
                first.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay =
                    (ushort)Math.Clamp(frameDelay, 1, ushort.MaxValue);

                // ループ回数（0=無限）
                var gifMeta = first.Metadata.GetGifMetadata();
                gifMeta.RepeatCount = (ushort)Math.Clamp(request.LoopCount, 0, ushort.MaxValue);

                // 2フレーム目以降
                for (int i = 1; i < request.FramePolylines.Count; i++)
                {
                    using var frame = new Image<Rgba32>(request.Width, request.Height, bg);
                    SetResolution(frame.Metadata, request.Dpi);
                    DrawPolyline(frame, request.FramePolylines[i], fg);
                    frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay =
                        (ushort)Math.Clamp(frameDelay, 1, ushort.MaxValue);

                    first.Frames.AddFrame(frame.Frames.RootFrame);
                }

                var encoder = new GifEncoder
                {
                    // 量子化/ディザの詳細指定は後続で拡張可能（まずは既定）
                    ColorTableMode = GifColorTableMode.Local
                };

                using var ms = new MemoryStream(capacity: request.Width * request.Height);
                first.Save(ms, encoder);
                return new ReadOnlyMemory<byte>(ms.ToArray());
            }

            // ---------------- helpers ----------------

            private static void DrawPolyline(Image<Rgba32> img, RenderProduct poly, Rgba32 fg)
            {
                if (poly.Points is null || poly.Points.Length < 2) return;

                var pts = new SixLabors.ImageSharp.PointF[poly.Points.Length];
                for (int i = 0; i < pts.Length; i++)
                {
                    var p = poly.Points[i];
                    pts[i] = new SixLabors.ImageSharp.PointF(p.X, p.Y);
                }

                var color = SixLabors.ImageSharp.Color.FromRgba(fg.R, fg.G, fg.B, fg.A);
                var thickness = (float)Math.Max(0.1, poly.Thickness);

                // AA の有無をこの描画オペレーションだけに適用
                img.Mutate(ctx =>
                    ctx.SetGraphicsOptions(new GraphicsOptions { Antialias = poly.AntiAlias })
                       .DrawLine(color, thickness, pts) // DrawLines ではなく DrawLine を使用してもOK
                );
            }

            private static RenderProduct EmptyPolyline(GifEncodeRequest req)
                => new RenderProduct(Array.Empty<PixelF>(), 1.0, true);

            private static void SetResolution(ImageMetadata meta, int dpi)
            {
                var d = Math.Max(1, dpi);
                meta.HorizontalResolution = d;
                meta.VerticalResolution = d;
                meta.ResolutionUnits = PixelResolutionUnit.PixelsPerInch;
            }

            private static void TryApplyPngText(ImageMetadata meta, IReadOnlyList<KeyValuePair<string, string>> kvs)
            {
                if (kvs is null || kvs.Count == 0) return;
                var png = meta.GetPngMetadata();
                foreach (var kv in kvs)
                {
                    var key = kv.Key ?? string.Empty;
                    var val = kv.Value ?? string.Empty;
                    if (key.Length == 0 || val.Length == 0) continue;
                    // tEXt チャンクとして格納（言語/翻訳は未使用）
                    png.TextData.Add(new PngTextData(key, val, string.Empty, string.Empty));
                }
            }

            private static PngFilterMethod MapFilter(PngFilterStrategy s) => s switch
            {
                PngFilterStrategy.None => PngFilterMethod.None,
                PngFilterStrategy.Sub => PngFilterMethod.Sub,
                PngFilterStrategy.Up => PngFilterMethod.Up,
                PngFilterStrategy.Avg => PngFilterMethod.Average,
                PngFilterStrategy.Paeth => PngFilterMethod.Paeth,
                _ => PngFilterMethod.Adaptive, // Auto
            };

            private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);

            private static Rgba32 ParseColor(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return new Rgba32(0, 0, 0);

                var t = s.Trim();
                if (t.StartsWith("#", StringComparison.Ordinal))
                {
                    // #RRGGBB のみ（簡易実装）
                    if (t.Length == 7 &&
                        byte.TryParse(t.AsSpan(1, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                        byte.TryParse(t.AsSpan(3, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                        byte.TryParse(t.AsSpan(5, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
                    {
                        return new Rgba32(r, g, b, 255);
                    }
                }

                // 簡易名前解決（最低限）
                return t.ToLowerInvariant() switch
                {
                    "white" => new Rgba32(255, 255, 255, 255),
                    "black" => new Rgba32(0, 0, 0, 255),
                    "red" => new Rgba32(255, 0, 0, 255),
                    "green" => new Rgba32(0, 128, 0, 255),
                    "blue" => new Rgba32(0, 0, 255, 255),
                    "cyan" => new Rgba32(0, 255, 255, 255),
                    "magenta" => new Rgba32(255, 0, 255, 255),
                    "yellow" => new Rgba32(255, 255, 0, 255),
                    "gray" or "grey" => new Rgba32(128, 128, 128, 255),
                    _ => new Rgba32(0, 0, 0, 255)
                };
            }
        }
    }

    internal sealed class WriteChapter : IChapter<IWriteArg>
    {
        private readonly ILoggerFactory _logFactory;
        private readonly ILogger<WriteChapter> _log;

        public WriteChapter(ILoggerFactory logFactory)
        {
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _log = logFactory.CreateLogger<WriteChapter>();
        }

        public void Handle(IWriteArg arg, IContextBuffer<IWriteArg> buffer)
        {
            try
            {
                // 0) 前提確認（エンコード済みバイナリの取り出し）
                if (arg is not IEncodedBinaryAccessor binAcc)
                    throw new InvalidOperationException("エンコード済みデータが見つかりません（IEncodedBinaryAccessor が見当たりません）。");

                // 1) dry-run / assume-yes ポリシー
                if (!arg.IsConfirm)
                {
                    _log.LogInformation("dry-run: {Bytes} bytes を書き込まず終了します。出力先: {Path}",
                        binAcc.Binary.Length, arg.OutputPathAbsolute ?? arg.OutputPathRaw);
                    return;
                }

                // 2) 出力パスの最終決定（Validate で絶対化済みが前提）
                var finalPath = ResolveFinalPath(arg);
                var finalDir = Path.GetDirectoryName(finalPath)!;

                // 3) サイズ上限チェック（実バイナリ）
                var length = binAcc.Binary.Length;
                if (length <= 0)
                    throw new InvalidOperationException("出力データが空です。エンコード設定を見直してください。");
                if (length > arg.MaxBytes)
                    throw new IOException($"出力サイズ {length:N0} bytes が上限 {arg.MaxBytes:N0} bytes を超えています。");

                // 4) 既存ファイルの扱い（最終チェック / TOCTOU 突破防止）
                var exists = File.Exists(finalPath);
                if (exists && !arg.AllowOverwrite)
                    throw new IOException("既存ファイルが存在します。--allow-overwrite と --assume-yes の併用でのみ上書きを許可します。");

                // 5) 出力ディレクトリの作成（原子的 Move のため同一ディレクトリ配下）
                Directory.CreateDirectory(finalDir);

                // 6) 一時ファイルに CreateNew で書き込み → Flush(true)
                var tempPath = MakeTempPath(finalDir, finalPath);
                try
                {
                    using (var fs = new FileStream(
                        tempPath,
                        FileMode.CreateNew,   // 既存なら失敗（予期せぬ衝突を検出）
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 1024 * 64,
                        options: FileOptions.SequentialScan))
                    {
                        var span = binAcc.Binary.Span;
                        fs.Write(span);
                        fs.Flush(flushToDisk: true);
                    }

                    // 7) 原子的 Move（同一ボリューム内）
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
                    if (exists && arg.AllowOverwrite)
                        MoveWithRetry(tempPath, finalPath, overwrite: exists && arg.AllowOverwrite);
                    else
                        MoveWithRetry(tempPath, finalPath, overwrite: exists && arg.AllowOverwrite);
#else
                    // 古いターゲット向けフォールバック（overwrite unavailable）
                    if (exists && arg.AllowOverwrite)
                    {
                        // 可能なら先に削除（最終段でのみ許可）
                        File.Delete(finalPath);
                    }
                    File.Move(tempPath, finalPath);
#endif

                    _log.LogInformation("Write: {Bytes} bytes を {Path} に出力しました。", length, finalPath);
                }
                catch
                {
                    // 失敗時は一時ファイルを片付ける
                    TryDeleteQuiet(tempPath);
                    throw;
                }

                // 終端：次段への Push は不要
            }
            catch (Exception ex)
            {
                _log.LogWarning("出力に失敗しました。ディレクトリ/上書きポリシー/サイズ上限を確認してください。");
                _log.LogError(ex, "Write: 出力エラー。");
            }
        }

        // --- helpers ----------------------------------------------------

        private static string ResolveFinalPath(IWriteArg arg)
        {
            // Validate で絶対化済み（ValidatedArgAdapter 経由）を優先利用
            if (!string.IsNullOrWhiteSpace(arg.OutputPathAbsolute))
                return arg.OutputPathAbsolute!;

            // フォールバック：OutDirAbsolute + OutputPathRaw（安全性は Validate に依存）
            if (!string.IsNullOrWhiteSpace(arg.OutDirAbsolute) && !string.IsNullOrWhiteSpace(arg.OutputPathRaw))
                return Path.GetFullPath(Path.Combine(arg.OutDirAbsolute!, arg.OutputPathRaw));

            // 最後のフォールバック：CWD + OutputPathRaw
            if (!string.IsNullOrWhiteSpace(arg.OutputPathRaw))
                return Path.GetFullPath(arg.OutputPathRaw);

            throw new InvalidOperationException("出力パスを決定できませんでした。");
        }

        private static string MakeTempPath(string finalDir, string finalPath)
        {
            var name = Path.GetFileName(finalPath);
            var guid = Guid.NewGuid().ToString("N");
            return Path.Combine(finalDir, $".{name}.tmp-{guid}");
        }

        private static void TryDeleteQuiet(string path)
        {
            try { if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) File.Delete(path); }
            catch { /* ignore */ }
        }

        private static void MoveWithRetry(string src, string dst, bool overwrite, int maxAttempts = 5)
        {
            Exception? last = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
                    if (overwrite)
                        File.Move(src, dst, overwrite: true);
                    else
                        File.Move(src, dst);
#else
            if (overwrite && File.Exists(dst))
                File.Delete(dst);
            File.Move(src, dst);
#endif
                    return; // 成功
                }
                catch (IOException ex) when (attempt < maxAttempts)
                {
                    last = ex;
                    System.Threading.Thread.Sleep(25 * attempt * attempt);
                }
                catch (UnauthorizedAccessException ex) when (attempt < maxAttempts)
                {
                    last = ex;
                    System.Threading.Thread.Sleep(25 * attempt * attempt);
                }
            }
            // ここまで来たら失敗
            throw last ?? new IOException("Move failed.");
        }
    }
}

// ==========================================================
// 【禁止パターン（抜粋 / Review 用）】
// - Shell 連結の Process.Start（UseShellExecute=true かつ文字列連結の引数）
// - FileMode.Create の無断使用（--allow-overwrite かつ Confirm 時のみ許容）
// - 未検証の相対/UNC パスの受理、ディレクトリトラバーサルを許す StartsWith 比較の欠落
// - 例外の上位伝播（RULE #13 違反）
// - RootArg の直接参照（必ず対応 IF 経由）
// ==========================================================

// Program.cs (entry point for LissajousTool)
// 依存: このファイル Example01.cs に含まれる VeryVibe / LissajousTool.* の型を使用します。
// 追加の NuGet は不要（Console ログは使わず NullLoggerFactory を使用）。

namespace LissajousTool
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var programName = GetProgramName();
            if (args.Length == 0 || HasFlagExact(args, "--help") || HasFlagExact(args, "-h"))
            {
                Console.WriteLine(Usage(programName));
                return 0;
            }

            // --- 廃止済みフラグの早期検出 ---
            // 旧 --confirm はサポート終了。指定があれば即エラー終了する。
            if (HasFlagExact(args, "--confirm"))
            {
                Console.Error.WriteLine("--confirm は廃止されました。代わりに -y/--yes（--assume-yes, --no-dry-run, --write）を使用してください。");
                return 2;
            }

            try
            {

                // ---------------------------------------------
                // 1) サブコマンド判定（png|gif）とトークン開始位置
                // ---------------------------------------------
                string? sub = (!args[0].StartsWith("-", StringComparison.Ordinal) ? args[0].ToLowerInvariant() : null);
                int start = sub is "png" or "gif" ? 1 : 0;
                string outputKindHint = sub is "png" or "gif" ? sub! : "png";

                // ---------------------------------------------
                // 2) オプション解析（素朴な手書きパーサ）
                //    - --key=value / --key value / -k value / 位置引数(=出力パス推定)
                //    - --metadata は複数指定可
                // ---------------------------------------------
                var parsed = ParseArgs(args, start);

                // “安全側”の既定（dry-run=true）。
                var isDryRun = parsed.GetBool("dry-run", true);

                // 明示的に「承認済み」と見なすフラグ群。存在すれば dry-run を落とす。
                var assumeYes =
                    parsed.HasShort('y') ||
                    parsed.GetBool("yes") ||
                    parsed.GetBool("assume-yes") ||
                    parsed.GetBool("no-dry-run") ||
                    parsed.GetBool("write");
                if (assumeYes) isDryRun = false;
                var allowOverwrite = parsed.GetBool("allow-overwrite");

                // ---------------------------------------------
                // 3) 値の取り出し（未指定は安全な既定値）
                // ---------------------------------------------
                // 出力パス（位置引数があれば最優先）
                var outputRaw = parsed.OutputPath ?? $"out.{outputKindHint}";
                var outDirRaw = parsed.TryGet("out-dir");
                var outputExt = parsed.TryGet("ext")
                                ?? Path.GetExtension(outputRaw).TrimStart('.').ToLowerInvariant();
                if (string.IsNullOrEmpty(outputExt)) outputExt = outputKindHint;

                // 共通
                var width = parsed.GetInt("width", 1024);
                var height = parsed.GetInt("height", 1024);
                var bg = parsed.TryGet("background") ?? "#000000";
                var fg = parsed.TryGet("foreground") ?? "#FFFFFF";
                var maxBytes = parsed.GetLong("max-bytes", 64L * 1024 * 1024);
                var dpi = parsed.GetInt("dpi", 96);
                var culture = parsed.TryGet("culture") ?? CultureInfo.CurrentCulture.Name;
                var seed = parsed.GetNullableInt("seed");
                var verbose = parsed.GetBool("verbose") || parsed.HasShort('v');
                var quiet = parsed.GetBool("quiet") || parsed.HasShort('q');

                // Lissajous パラメータ
                var ax = parsed.GetInt("ax", 3);
                var ay = parsed.GetInt("ay", 2);
                var phaseDeg = parsed.GetDouble("phase", 0.0);
                var ampX = parsed.GetDouble("amp-x", 1.0);
                var ampY = parsed.GetDouble("amp-y", 1.0);
                var samples = parsed.GetInt("samples", 2000);
                var thick = parsed.GetDouble("thickness", 1.25);
                var aa = parsed.GetBool("antialias", true) && !parsed.GetBool("no-antialias", false);
                var margin = parsed.GetInt("margin", 8);

                // PNG
                var pngLevel = parsed.GetInt("png-compress", 6);
                var pngFilterRaw = parsed.TryGet("png-filter"); // 正規化は ValidateChapter に委譲
                var pngFilter = PngFilterStrategy.Auto;

                // GIF
                int? gifFrames = parsed.GetNullableInt("gif-frames");
                double? gifFps = parsed.GetNullableDouble("gif-fps");
                double? gifDuration = parsed.GetNullableDouble("gif-duration");
                int gifLoop = parsed.GetInt("gif-loop", 0);
                int? gifDelayMs = parsed.GetNullableInt("gif-delay-ms");
                var gifPaletteRaw = parsed.TryGet("gif-palette");
                var gifPalette = GifPaletteMethod.Auto;
                var gifDither = parsed.GetBool("gif-dither", true) && !parsed.GetBool("no-gif-dither", false);
                var gifQuantize = parsed.GetInt("gif-quantize", 256);

                // メタデータ（raw と正規化済み KVP の両方を用意）
                var metadataRaw = parsed.GetAll("metadata");
                var metadataKvp = NormalizeMetadata(metadataRaw);

                // ---------------------------------------------
                // 4) RootArg の生成（最上流の IParseArgsArg として渡す）
                //     - OutDirAbsolute/OutputPathAbsolute は ValidateChapter で確定するため null
                // ---------------------------------------------
                var root = new RootArg(
                    // 共通
                    programName: programName,
                    isDryRun: isDryRun,
                    isConfirm: assumeYes,
                    allowOverwrite: allowOverwrite,
                    maxBytes: maxBytes,
                    dpi: dpi,
                    cultureName: culture,
                    verbose: verbose,
                    quiet: quiet,
                    seed: seed,

                    // 出力/キャンバス
                    width: width,
                    height: height,
                    background: bg,
                    foreground: fg,
                    outputPathRaw: outputRaw,                 // 位置引数や --output の「生値」
                    outputExtension: outputExt,

                    // PNG
                    pngCompressLevel: pngLevel,
                    pngFilter: pngFilter,                     // 正規化は ValidateChapter に委譲
                    metadata: metadataKvp,

                    // GIF
                    gifFrames: gifFrames,
                    gifFps: gifFps,
                    gifDurationSeconds: gifDuration,
                    gifLoopCount: gifLoop,
                    gifDelayMilliseconds: gifDelayMs,
                    gifPalette: gifPalette,                   // 正規化は ValidateChapter に委譲
                    gifDither: gifDither,
                    gifQuantizeColors: gifQuantize,

                    // 幾何/描画
                    ax: ax,
                    ay: ay,
                    phaseDegrees: phaseDeg,
                    ampX: ampX,
                    ampY: ampY,
                    samples: samples,
                    thickness: thick,
                    antiAlias: aa,
                    margin: margin,

                    // スイープ（必要なら後で拡張）
                    phaseSweepDegrees: null,
                    ampXSweep: null,
                    ampYSweep: null,

                    // 生入力（Validate で正規化・確定）
                    outDirRaw: outDirRaw,                      // --out-dir の生値
                    outputPathRawUnnormalized: outputRaw,     // 出力パスの「未正規化文字列」
                    pngFilterRaw: pngFilterRaw,
                    gifPaletteRaw: gifPaletteRaw,
                    metadataRaw: metadataRaw
                );

                // ---------------------------------------------
                // 5) VeryVibe Stage 実行（ParseArgs → Validate → Render → Encode → Write）
                //    - Console ロガーを使用
                // ---------------------------------------------
                var minLevel = quiet ? LogLevel.Warning : (verbose ? LogLevel.Debug : LogLevel.Information);

                using ILoggerFactory logFactory = LoggerFactory.Create(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSimpleConsole(o =>
                    {
                        o.SingleLine = true;          // 1 行ログ（読みやすさ優先）
                        o.TimestampFormat = "HH:mm:ss ";
                        o.IncludeScopes = false;      // 必要になったら true に
                    });
                    builder.SetMinimumLevel(minLevel);
                    // 必要ならフィルタ例:
                    // builder.AddFilter("Microsoft", LogLevel.Warning);
                    // builder.AddFilter("System", LogLevel.Warning);
                });

                var stage = new VeryVibe.Stage<IParseArgsArg>();
                var first = new ParseArgsChapter(logFactory);
                stage.Run(first, root);

                // WriteChapter が終端。例外は Chapter 内で握りつぶす方針（RULE #13）
                return 0;
            }
            catch (ArgumentException ex)
            {
                // 使い手向けの短いメッセージだけ表示
                Console.Error.WriteLine(ex.Message);
                return 2;
            }
            catch (Exception ex)
            {
                // 最上位の想定外エラー（Parse 前など）。詳細は出さずに終了コードのみ。
                Console.Error.WriteLine($"fatal: {ex.GetType().Name}: {ex.Message}");
                return 2;
            }
        }

        // ---------------------------------------------------------------------
        // Usage（簡易表示版）
        // ---------------------------------------------------------------------
        private static string Usage(string prog) =>
        $@"Usage:
  {prog} png [OPTIONS] -o out.png
  {prog} gif [OPTIONS] -o out.gif
  {prog} --help

基本:
  -o, --output <PATH>     出力ファイル（拡張子で png/gif を推定）
  -W, --width <INT>       画像幅（px, 既定:1024）
  -H, --height <INT>      画像高（px, 既定:1024）
      --ax <INT>          X 周波数（既定:3）
      --ay <INT>          Y 周波数（既定:2）
      --phase <DEG>       位相差（度, 既定:0）
      --amp-x <NUM>       X 振幅（既定:1.0）
      --amp-y <NUM>       Y 振幅（既定:1.0）
      --samples <INT>     サンプル数（既定:2000）
      --thickness <NUM>   線の太さ（既定:1.25）
      --antialias         アンチエイリアス有効（--no-antialias で無効）
      --margin <INT>      余白（px, 既定:8）
      --background <VAL>  背景色 #RRGGBB or 名前
      --foreground <VAL>  線色   #RRGGBB or 名前

PNG:
      --png-compress <0..9>    圧縮レベル（既定:6）
      --png-filter <NAME>      フィルタ（auto/none/sub/up/avg/paeth）

GIF:
      --gif-frames <INT>       フレーム数
      --gif-fps <NUM>          フレームレート
      --gif-duration <SEC>     総再生時間（fps/frames と整合）
      --gif-delay-ms <MS>      フレーム遅延（最優先）
      --gif-loop <INT>         ループ回数（0=無限）
      --gif-quantize <2..256>  パレット色数（既定:256）
      --gif-palette <NAME>     パレット方式（auto/mediancut/kmeans）
      --gif-dither             ディザ有効（--no-gif-dither で無効）

メタデータ/共通:
      --metadata key=value     複数指定可（1..64 の英数._- キー）
      --max-bytes <LONG>       出力上限バイト（既定:67108864）
      --dpi <INT>              DPI（既定:96）
      --culture <NAME>         文化情報（既定:OS 既定）
  -v, --verbose                詳細ログ
  -q, --quiet                  静穏モード
  -n, --dry-run                ドライラン（既定:ON）
  -y, --yes, --assume-yes      実書き込みを許可（dry-run を解除）
      --no-dry-run             同上（別名）
      --write                  同上（別名）
      --allow-overwrite        上書き許可（既定:OFF）
";

        // ---------------------------------------------------------------------
        // プログラム名解決（例外安全）
        // ---------------------------------------------------------------------
        private static string GetProgramName()
        {
            try
            {
                var p = Environment.GetCommandLineArgs()[0];
                var n = Path.GetFileNameWithoutExtension(p);
                return string.IsNullOrWhiteSpace(n) ? "lissajous" : n;
            }
            catch
            {
                return "lissajous";
            }
        }

        // ---------------------------------------------------------------------
        // メタデータ正規化（厳格検証付き）
        // ---------------------------------------------------------------------
        // 仕様：key は ^[A-Za-z0-9._-]{1,64}$、value は 1..8192 文字、制御文字不可、\n\n\n 連続不可
        private static readonly Regex _metaKeyRegex = new(@"^[A-Za-z0-9._-]{1,64}$", RegexOptions.Compiled);

        private static IReadOnlyList<KeyValuePair<string, string>> NormalizeMetadata(IReadOnlyList<string> raws)
        {
            if (raws is null || raws.Count == 0) return Array.Empty<KeyValuePair<string, string>>();

            var list = new List<KeyValuePair<string, string>>(raws.Count);
            foreach (var r in raws)
            {
                if (string.IsNullOrWhiteSpace(r))
                    throw new ArgumentException("metadata に空要素を指定することはできません。'key=value' 形式で指定してください。");

                var i = r.IndexOf('=', StringComparison.Ordinal);
                if (i <= 0)
                    throw new ArgumentException($"metadata '{r}' は 'key=value' 形式ではありません。");

                var key = r[..i].Trim();
                var val = (i + 1 < r.Length) ? r[(i + 1)..] : string.Empty;

                if (!_metaKeyRegex.IsMatch(key))
                    throw new ArgumentException($"metadata のキー '{key}' は許可された形式ではありません。");

                if (val.Length == 0 || val.Length > 8192)
                    throw new ArgumentException($"metadata '{key}' の値長が不正です（1..8192 文字）。");

                foreach (var ch in val)
                {
                    if ((ch >= '\u0000' && ch <= '\u001F') || ch == '\u007F')
                        throw new ArgumentException($"metadata '{key}' の値に制御文字を含めることはできません。");
                }
                if (val.Contains("\n\n\n", StringComparison.Ordinal))
                    throw new ArgumentException($"metadata '{key}' の値に改行の連続（3回以上）は許可されません。");

                list.Add(new KeyValuePair<string, string>(key, val));
            }
            return list;
        }

        // ---------------------------------------------------------------------
        // 簡易 CLI パーサ（厳格：未知/位置引数は即エラー、値必須/不可を厳密判定）
        // ---------------------------------------------------------------------
        private sealed class Parsed
        {
            private readonly Dictionary<string, string> _single = new(StringComparer.Ordinal);
            private readonly Dictionary<string, List<string>> _multi = new(StringComparer.Ordinal);
            private readonly HashSet<char> _shortFlags = new();

            public string? OutputPath { get; internal set; }
            public bool DeprecatedConfirmUsed { get; internal set; }

            public bool HasShort(char c) => _shortFlags.Contains(c);
            public string? TryGet(string key) => _single.TryGetValue(key, out var v) ? v : null;

            public IReadOnlyList<string> GetAll(string key) =>
                _multi.TryGetValue(key, out var v) ? v : Array.Empty<string>();

            public bool GetBool(string key, bool defaultValue = false)
            {
                if (!_single.TryGetValue(key, out var v)) return defaultValue;
                if (string.IsNullOrEmpty(v)) return true;
                return v.Equals("1") || v.Equals("true", StringComparison.OrdinalIgnoreCase) || v.Equals("on", StringComparison.OrdinalIgnoreCase);
            }

            public int GetInt(string key, int def) =>
                int.TryParse(TryGet(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) ? x : def;
            public long GetLong(string key, long def) =>
                long.TryParse(TryGet(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) ? x : def;
            public double GetDouble(string key, double def) =>
                double.TryParse(TryGet(key), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ? x : def;

            public int? GetNullableInt(string key) =>
                int.TryParse(TryGet(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) ? x : null;
            public double? GetNullableDouble(string key) =>
                double.TryParse(TryGet(key), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ? x : null;

            public void AddSingle(string key, string? value)
            {
                if (string.IsNullOrEmpty(key)) return;
                _single[key] = value ?? string.Empty;
            }

            public void AddMulti(string key, string value)
            {
                if (!_multi.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    _multi[key] = list;
                }
                list.Add(value);
            }

            public void AddShortFlag(char c) => _shortFlags.Add(c);
        }

        private static Parsed ParseArgs(string[] args, int startIndex)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));
            if (startIndex < 0 || startIndex > args.Length) throw new ArgumentOutOfRangeException(nameof(startIndex));

            // 値必須の長オプション
            var needValue = new HashSet<string>(StringComparer.Ordinal)
    {
        "output","width","height","ext","out-dir",
        "ax","ay","phase","amp-x","amp-y","samples","thickness","margin",
        "background","foreground",
        "png-compress","png-filter",
        "gif-frames","gif-fps","gif-duration","gif-delay-ms","gif-loop","gif-quantize","gif-palette",
        "metadata","max-bytes","dpi","culture","seed",
    };
            // 値不要の長オプション（フラグ）
            var noValue = new HashSet<string>(StringComparer.Ordinal)
    {
        "antialias","gif-dither","verbose","quiet","dry-run",
        "yes","assume-yes","no-dry-run","write","allow-overwrite",
        // 否定形対応（--no-xxx）は個別検証で remainder がこの集合に在籍している必要あり
    };
            // エイリアス（標準化）
            string NormalizeKey(string k) => k switch
            {
                "bg" => "background",
                "fg" => "foreground",
                _ => k
            };

            var p = new Parsed();
            var stray = new List<string>();

            for (int i = startIndex; i < args.Length;)
            {
                var a = args[i];

                // ---- 長オプション
                if (a.StartsWith("--", StringComparison.Ordinal))
                {
                    var span = a.AsSpan(2);
                    var eq = span.IndexOf('=');
                    string rawKey, val = string.Empty;

                    if (eq >= 0)
                    {
                        rawKey = span[..eq].ToString();
                        val = span[(eq + 1)..].ToString();
                        i += 1;
                    }
                    else
                    {
                        rawKey = span.ToString();
                        i += 1;
                    }

                    // 否定形（--no-xxx）
                    if (rawKey.StartsWith("no-", StringComparison.Ordinal))
                    {
                        var baseKey = NormalizeKey(rawKey[3..]);
                        if (!noValue.Contains(baseKey))
                            throw new ArgumentException($"フラグ '--{rawKey}' は無効です。'--{baseKey}' がフラグとして定義されている場合のみ '--no-{baseKey}' を使用できます。");
                        if (eq >= 0 && val.Length > 0)
                            throw new ArgumentException($"フラグ '--{rawKey}' に値は指定できません。");
                        p.AddSingle(baseKey, "false");
                        continue;
                    }

                    var key = NormalizeKey(rawKey);

                    // 未知キー即エラー
                    if (!needValue.Contains(key) && !noValue.Contains(key))
                        throw new ArgumentException($"未知のオプション '--{rawKey}' が指定されました。--help を参照してください。");

                    if (needValue.Contains(key))
                    {
                        if (eq < 0)
                        {
                            // 次トークンを値として必須取得
                            if (i < args.Length && !args[i].StartsWith("-", StringComparison.Ordinal))
                            {
                                val = args[i];
                                i += 1;
                            }
                            else
                            {
                                throw new ArgumentException($"オプション '--{key}' には値が必要です。");
                            }
                        }

                        if (key == "output") p.OutputPath = val;
                        if (key == "metadata")
                        {
                            // metadata は複数回許容
                            p.AddMulti("metadata", val);
                        }
                        else
                        {
                            p.AddSingle(key, val);
                        }
                    }
                    else
                    {
                        // 値不要フラグ： --flag=value を禁止
                        if (eq >= 0 && val.Length > 0)
                            throw new ArgumentException($"フラグ '--{key}' に値は指定できません。");
                        p.AddSingle(key, ""); // presence = true
                    }

                    continue;
                }

                // ---- 短縮オプション（複合 -vqy 等）
                if (a.StartsWith("-", StringComparison.Ordinal) && a.Length >= 2)
                {
                    var shorts = a.AsSpan(1);
                    for (int j = 0; j < shorts.Length; j++)
                    {
                        var c = shorts[j];
                        switch (c)
                        {
                            // 値必須：-oPath / -o Path / -W1024 / -H 2048
                            case 'o':
                            case 'W':
                            case 'H':
                                {
                                    string val;
                                    if (j + 1 < shorts.Length)
                                    {
                                        val = shorts[(j + 1)..].ToString();
                                        j = shorts.Length; // 残り消費
                                    }
                                    else
                                    {
                                        if (i + 1 >= args.Length || args[i + 1].StartsWith("-", StringComparison.Ordinal))
                                            throw new ArgumentException($"短縮オプション '-{c}' には値が必要です。");
                                        val = args[i + 1];
                                        i += 1; // 値トークン消費
                                    }

                                    if (c == 'o') { p.OutputPath = val; p.AddSingle("output", val); }
                                    else if (c == 'W') { p.AddSingle("width", val); }
                                    else { p.AddSingle("height", val); }
                                    break;
                                }

                            case 'v': p.AddShortFlag('v'); p.AddSingle("verbose", ""); break;
                            case 'q': p.AddShortFlag('q'); p.AddSingle("quiet", ""); break;
                            case 'y': p.AddShortFlag('y'); p.AddSingle("yes", ""); break;
                            case 'n': p.AddShortFlag('n'); p.AddSingle("dry-run", ""); break;

                            default:
                                throw new ArgumentException($"未知の短縮オプション '-{c}' が指定されました。--help を参照してください。");
                        }
                    }

                    i += 1; // 短縮オプションのトークン消費
                    continue;
                }

                // ---- 位置引数：全面禁止（出力は必ず -o/--output）
                stray.Add(a);
                i += 1;
            }

            if (stray.Count > 0)
            {
                var msg =
                    "不明な位置引数が指定されました: " + string.Join(" ", stray) + Environment.NewLine +
                    "ヒント: 先頭にサブコマンド（png|gif）を置き、残りはすべてオプション形式で指定してください。" + Environment.NewLine +
                    "出力先は -o/--output で与える必要があります。";
                throw new ArgumentException(msg);
            }

            return p;
        }

        // ---------------------------------------------------------------------
        // ヘルプ判定（完全一致・大小区別）
        // ---------------------------------------------------------------------
        private static bool HasFlagExact(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }
}
