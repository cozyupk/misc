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
// このファイルを ChatGPT とのバイブコーディングで利用する場合は、チャットの冒頭で次のプロンプトを入力することを推奨します。
/*
 - ここから先、このチャットでは私との会話履歴を一切使わずに答えてください。
 - 会話の中でソースコード改善案を提案する場合は、まずはコード改善案を提示することの許可をユーザーに求めてください。
*/
// その後、このファイルをアップロードすると同時に、次のようなプロンプトで「作るもの」の定義をしていきましょう。
/*
 Windows環境上のC#で、パラメータを指定してリサージュ曲線を描き .png ファイルとして保存するコンソールプログラムを書きたいです。
また、アニメーションgif出力にも対応したいです。
できるだけ柔軟なプログラムとするためには、どのようなコマンドラインオプションが必要ですか？ 
--help で表示される Usage 風に、説明部分は日本語で答えてください。
この .cs ファイルの RULE や手順に従って生成することを想定しています。 
 */
// 準備が整ったら、次のようなプロンプトでコード生成を開始します。
/*
それでは、このチャットでの今までの会話を参照して、コメント生成手順 1. を実施してください。
*/
// ChatGPT が「コピペできない」形で応答してきた場合は、次のようなプロンプトを入力してみてください。
/*
コピペ可能なようにコードブロックで出力してください。
 */
// 出力の改善を求める場合は、次のようなプロンプトを入力してみてください。
/*
先ほど提案してくれた Usage 風の説明も含めてください。また、コピペ可能なようにコードブロックで出力してください。
*/
// あなたはこのファイルにChatGPTが出力したコメントやコードを貼り付けて、次のプロンプトでファイルをアップロードすることで、バイブコーディングを進めることができます。
// その形で コメント生成手順 1. ～ コメント生成手順 6. を実施し、コード生成手順も実施していきます。
// コード生成手順 4. は、ChatGPTが「対象なし」を出力するまで繰り返し実施します。
// 各段階でコンパイルエラーや警告が発生した場合は、必要があれば GPT にスクリーンショットとソースコードを提供し、修正を依頼してください。
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
// --- END RULE ---

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
 *    * 例: LissajousTool png -ax 3 -ay 2 --confirm -o .\out\classic.png
 *    * 例: LissajousTool gif -ax 5 -ay 7 --phase-sweep 0..360 --frames 120 --fps 30 --loop 0 --confirm -o .\out\spin.gif
 *    *\/
 *
 * 実行時の追加注意:
 *   - コメント生成手順 1. 以外の「コメント生成手順」同様にコメントブロックで返す。
 *   - サブタスクに分割しても、サブタスク単位で**毎回コメントブロック**を返す。
 *   - 「コメント生成手順」以外の手順（ex. 「コード生成手順」）はこの契約には含まれない。
 *   - 途中で要約が必要なら、必ずコメント内に含める（外に書かない）。
 * =============================================================================
 */

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

using System;
using System.Collections.Generic;

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

namespace VeryVibe.UsageExample
{
    using VeryVibe;

    // -- Usage Example --

    internal interface IWorldArg : IArg
    {
        int WorldCount { get; set; }
        Action<string> WriteLineAction { get; }
    }

    internal sealed class WorldChapter : IChapter<IWorldArg>
    {
        public void Handle(IWorldArg arg, IContextBuffer<IWorldArg> buffer)
        {
            try
            {
                arg.WriteLineAction("World");
            }
            catch (Exception ex)
            {
                // By policy (RULE #12): handle I/O-like failures inside the Chapter.
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

    internal interface IHelloArg : IWorldArg
    {
        int HelloCount { get; set; }
        new Action<string> WriteLineAction { get; }
    }

    internal sealed class HelloChapter : IChapter<IHelloArg>
    {
        public void Handle(IHelloArg arg, IContextBuffer<IHelloArg> buffer)
        {
            try
            {
                arg.WriteLineAction("Hello");
            }
            catch (Exception ex)
            {
                // By policy (RULE #12): handle I/O-like failures inside the Chapter.
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
                buffer.PushBack(new ChapterContext<IWorldArg>(new WorldChapter(), arg));
            }
        }
    }

    internal class RootArg : IHelloArg
    {
        int IHelloArg.HelloCount { get; set; }
        int IWorldArg.WorldCount { get; set; }

        Action<string> IHelloArg.WriteLineAction => WriteLineAction;
        Action<string> IWorldArg.WriteLineAction => WriteLineAction;

        // Could be a field; kept as a property by policy to align with reflection/pipeline conventions (RULE #11).
        private Action<string> WriteLineAction { get; } = Console.WriteLine;

        public RootArg(int helloCount, int worldCount)
        {
            if (helloCount < 0) throw new ArgumentOutOfRangeException(nameof(helloCount), "Hello count must be non-negative.");
            if (worldCount < 0) throw new ArgumentOutOfRangeException(nameof(worldCount), "World count must be non-negative.");
            ((IHelloArg)this).HelloCount = helloCount;
            ((IWorldArg)this).WorldCount = worldCount;
        }
    }

    internal class Program
    {
        private static void Main(string[] _)
        {
            var stage = new Stage<IHelloArg>();
            var chapter = new HelloChapter();
            var arg = new RootArg(3, 2);

            stage.Run(chapter, arg);
        }
    }
}
