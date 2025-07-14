cozuupk/misc/task-continuation-probe
===============================================

_🌐 **このドキュメントの英語版はこちらです**。_

**提案としての結論:**

WPFアプリなどの同期コンテキストが存在する .NET 環境で `ConfigureAwait(false)` を積極的に使う場合、次のいずれかの設計とすることを推奨します。

* すべての await に `ConfigureAwait(false)` を付与する。明示的に付与しない場合は `ConfigureAwait(true)` として、その意図を明確にする。
* または、非同期メソッドの冒頭で 次のような `TaskHelper.EscapeFromSynchronizationContext()` のようなヘルパーを呼び出す。

```csharp
static class TaskHelper
{
    // asyncメソッドの先頭で呼ぶことで、以降のawaitが同期コンテキストに復帰しないようにする
    public static ConfiguredTaskAwaitable EscapeFromSynchronizationContext(this)
        => Task.Yield().ConfigureAwait(false);
}
```

これにより意図しない同期コンテキスト下でのコード実行を防ぎ、`await` の continuation に起因するデッドロックやパフォーマンスの問題を回避できると考えます。

___________

このリポジトリでは、**.NETにおける`async/await`とタスク継続の挙動**、特に**スレッドの切り替わり**や\*\*`Task.ContinueWith()`使用時の落とし穴**を**可視化**するための一連のデモコードを提供しています。`await`の裏側で本当に何が起きているのか見てみたいですか？ 継続処理（Continuation）が思ったように待機してくれないのはなぜだろうと疑問に思ったことはありませんか？――本リポジトリのコードは、それらを古典的な**printfデバッグ\*\*によって解き明かす手助けをしてくれます。

**関連記事（Qiita）:** 本リポジトリで紹介するコードと概念の詳細な解説は、次のQiita記事に掲載しています。興味があればぜひ参照してください。

*   Qiita記事「最強の .NET async/await 見える化計画（なお printfデバッグのもよう）」（第1部）
    
*   Qiita記事（第2部）– `ConfigureAwait(false)` や `SynchronizationContext` に踏み込んだ続編 （リンク: https://qiita.com/cozyupk/items/5774e4942158fc824034）
    

このリポジトリでできること
-------------

*   **async/await のスレッド切り替えデモ:** `await Task.Delay()` の前後でスレッドがどう移り変わるかを観察するサンプルがあります。一見スレッドが変わっていないように見える場合でも、実際には切り替わっているケースがある理由について探ります。また、それを確実に観察する方法を示します。
    
*   **`Task.ContinueWith()`＋async ラムダの落とし穴:** `async`メソッドの後続処理に `.ContinueWith()` を使うと、しばしば**内部の非同期処理を待ってくれない**問題が発生します。その原因を説明し、`ContinueWith(...).Unwrap()` を使う場合と使わない場合で何が変わるかを実例で示します。
    
*   **スレッドプール「かき混ぜ」ユーティリティ:** .NETの**スレッドプールに負荷をかける**ことでスレッド切り替えの可能性を高めるテクニックです。スレッドプールはスレッドを再利用するため、普通に実行しただけではスレッド切り替えが起きていても見分けにくい場合があります。本リポジトリでは、意図的にスレッドプールを忙しくするユーティリティを用意し、スレッド遷移をはっきり観測できるようにしています。
    
*   **SynchronizationContext と ConfigureAwait の理解:** `async/await`がデフォルトで同期コンテキストを捕捉する仕組みや、`ConfigureAwait(false)`でコンテキスト捕捉をオフにする重要性について解説します。また、呼び出しチェーン全体でコンテキスト捕捉を避けるための`Task.EscapeFromSynchronizationContext()`のようなメソッドの提案についても触れます。
    

以下、各トピックごとにコード例・出力例とその分析を詳述していきます。

___________

async/await とスレッドの切り替え
----------------------

まず、`await`前後のスレッド挙動を確認するシンプルな例から始めましょう。次のような非同期コードを考えてみます。

```csharp
Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId:D2}] Before await");
await Task.Delay(1000);  // 非同期の待機（1秒）
Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId:D2}] After await");
```

このコードは、`await`の前後で現在のスレッドIDを出力します。特に`await`後の継続処理（続きのコード）が**別のスレッド**で動くかどうかを知るためです。通常のコンソールアプリケーション（特別なSynchronizationContextが無い環境）では、`await`後の継続処理は既定ではスレッドプール上で行われます。しかし、実行タイミングによっては**継続が同じスレッドID上で実行される**こともあり、一見スレッドが切り替わっていないように見える場合があります。これはスレッドプールのスレッド再利用による現象です。

**出力例 (サンプル1):** 上記コードを実行すると、例えば次のような結果になることがあります。

```plaintext
[Thread 03] Before await  
[Thread 03] After await  
```

一見すると、`Before await`も`After await`も同じスレッド（この例ではスレッド03）で実行されており、「awaitしてもスレッドは変わらなかったのかな？」と思ってしまうかもしれません。しかし実際には、この間に**スレッドは待機に入り、待機完了後に継続処理がスレッドプールで再開**されています。ただ、このケースではスレッドプールが**同じスレッドID**のスレッドを継続処理に再利用したため、結果としてIDが同じになっただけなのです。

### スレッド切り替えを明確に観測する

本当にスレッドが切り替わりうることを確認するため、スレッドプールが同じスレッドを再利用できない状況を作ってみましょう。つまり、`Task.Delay`で待機している間にスレッドプールに負荷をかけ、継続処理を実行する際に元のスレッドが忙しくて使えないようにします。そうすれば、継続処理は別のスレッドで行わざるを得なくなります。

本リポジトリのコードでは、この目的のために**スレッドプールを意図的に「かき混ぜる」ユーティリティ**を用意しています（多数のダミーのタスクを投入してスレッドプールのスレッドを消費します）。これを利用して負荷をかけた状態で先ほどのコードを実行すると、以下のような結果が得られます。

**出力例 (サンプル2: スレッドプール負荷あり):**

```plaintext
[Thread 07] Before await  
[Thread 12] After await  
```

ご覧のとおり、`Before await`はスレッド07、`After await`はスレッド12と、**異なるスレッド**で実行されています。継続処理が別スレッドに移動したことが明確に確認できました。先ほどのサンプル1ではたまたまスレッドIDが同じだっただけで、実際には待機の前後でスレッドは一度解放され、待機完了時に再度取得し直していたのです。この再取得の際に元と同じIDのスレッドが割り当てられることもあり、それが紛らわしかったわけです。

この事実が示すように、**スレッドIDだけを頼りに「同じスレッドで連続して実行されたか」を判断するのは危険**です。スレッドプールの仕組みにより、IDが同じでも実際には一旦コンテキストスイッチ（中断と再開）が起きている可能性があります。デバッグ時に誤解しないよう注意が必要です。

スレッドIDが当てにならない理由
----------------

上述の通り、.NETのスレッドプールではスレッドの再利用が行われます。そのため、`await`を挟んだ前後で**実行中のタスクが切り替わっても**、同じ`ManagedThreadId`が現れる場合があります。一般的な流れを整理すると:

*   非同期メソッド内で`await`に到達すると（例えば `Task.Delay` のような未完了のタスクをawaitした場合）、現在のスレッドはその時点でいったん解放され、他の作業（他のタスクの処理など）に回されます。
    
*   待機していたタスクが完了すると、残りの継続部分を実行するためのスレッドがスレッドプールから提供されます。**このとき、以前のスレッドがちょうど空いていれば、そのスレッド上で継続処理を再開することもあります**。
    

このようにして継続が行われるため、ログに出力したスレッドIDがたまたま同じだと、「ずっと同じスレッドで実行が直線的に行われた」と誤解してしまう恐れがあります。本当は裏で一度停止して別の再開ポイントを経ているのに、です。

**当リポジトリの対策:** 例では「スレッドプール撹乱（かく乱）」ユーティリティを導入し、意図的に元のスレッドを忙しくさせることで、継続処理が別スレッドに回らざるを得ない状況を作りました。その結果、スレッドIDの変化によって確実にコンテキストスイッチが起こったことを観測できました。コード中でこのユーティリティをON/OFFできますので、負荷あり・なしの両方で実行し、スレッドID出力の違いを見比べてみると理解が深まるでしょう。

落とし穴: `Task.ContinueWith` を asyncメソッドに使う
----------------------------------------

続いて、従来の**Task継続**スタイルと`async/await`を混在させた場合の落とし穴を見ていきます。`Task.ContinueWith()`は、C#に`async/await`が導入される前からあるタスクの継続処理を指定するためのメソッドです。タスク完了時に実行したい処理をデリゲートで渡せる便利な仕組みですが、そこに`async`なラムダ（非同期処理）を渡すと思わぬ罠に陥ります。

次のコードを見てみましょう。

```csharp
// 非同期タスクを開始（前段のタスク）
var task = Task.Run(async () => {
    Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId:D2}] In Task - start");
    await Task.Delay(500);
    Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId:D2}] In Task - end");
});

// asyncラムダで ContinueWith（後続の継続タスク）
task.ContinueWith(async t => {
    Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId:D2}] In ContinueWith - before await()");
    await Task.Delay(500);
    Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId:D2}] In ContinueWith - after await()");
});

// 前段タスクが完了するまで待つ（継続は開始される）
task.Wait();
```

ここで何が起こるか予想してみます。継続の中でも`await`を使っており、その前後でメッセージを出力しています。先ほどの例と同様に、継続内でもスレッドIDを見ています。

**問題点:** `.ContinueWith`に`async`ラムダを渡すと、継続処理自体が\*\*`Task`を返す\*\*ことになります（C#では`async`なラムダは戻り値が常にTask/Task<T>となります）。つまり、継続処理の結果は「別のTaskを内包するTask」（入れ子のタスク）になるのです。

何も対策しない場合、`task.ContinueWith(async t => { ... })`の戻り値は **Task<Task>** 型（Taskに包まれたTask）になります。外側のTaskは、継続デリゲートの実行自体を表し、内側のTaskが継続デリゲート内での非同期処理の完了を表します。**重要なのは、外側のTaskは、継続内の非同期処理が最初に`await`で中断した時点で完了してしまう**（あるいは継続内にawaitがなければ即時完了する）ということです。

上記コードでは、`task.Wait()`を呼んで元のタスク（前段の`task`）が終わるまで待機しています。`task`が完了すれば`ContinueWith`で指定した継続処理は開始されます。しかし、`task.Wait()`は**継続処理の内側までは待ちません**。継続処理は非同期に動いており、しかもそれをunwrap（平坦化）していないため、`task.Wait()`にとっては「継続処理を開始した」ことまでしか面倒を見てくれないのです。

**出力例（Unwrapしない場合のContinueWith）:**

```plaintext
[Thread 03] In Task - start  
[Thread 03] In Task - end  
[Thread 03] In ContinueWith - before await()  
-- （この時点でメインスレッドは全て完了したとみなして処理続行） --  
[Thread 04] In ContinueWith - after await()  
```

この例では、継続内の「before await」までは出力されました（ここではスレッド03上で実行）が、「after await」はしばらく経ってからスレッド04上で出力されています。`--`で示した部分でメイン側（ここではtask.Wait()を呼んでいたスレッド）は待機を終えてしまい、プログラム的には「一通り処理完了」と判断しています。しかし実際には、継続内の非同期処理（500msのDelay後の処理）が**バックグラウンドで続行中**だったため、後になってから「after await」が出力されたのです。

このように、**asyncな継続をContinueWithで書くと、こちらが思っている以上に早く「完了」扱いになってしまい、内部の処理が最後まで終わらないまま先に進んでしまう**ことがあります。実運用では、リソース解放や後続処理のタイミングがずれてしまうなどのバグを招きかねません。

### 対策: 継続タスクの Unwrap（平坦化）

asyncな継続を最後まで正しく待つには、\*\*入れ子になったタスクを平坦化（Unwrap）\*\*する必要があります。対処法はいくつか考えられますが:

1.  **`Unwrap()`メソッドを使う:** Task Parallel Libraryには、`Task<Task>`を中身のTaskに変換してひとつのTaskにする `Task.Unwrap()` というメソッドがあります。`ContinueWith(...).Unwrap()` と呼び出せば、内側のタスクが完了したタイミングで完了扱いとなるTaskを得ることができます。
    
2.  **ContinueWithを使わずawaitを使う:** よりモダンなアプローチは、最初からasync/awaitでフローを記述し、ContinueWithを使わないことです。単純に前のTaskをawaitすれば自動的に後続のコードが継続として実行されますし、コンパイラーが適切にタスクを繋いでくれるのでネストも発生しません。
    

今回のケースに即して言えば、`ContinueWith`を使う必要はあまりなく、例えば以下のように書き換えるのがシンプルです。

```csharp
await task;  // 前のTaskが終わるのを待つ
// 続いて非同期処理を実行
Console.WriteLine("before await");
await Task.Delay(500);
Console.WriteLine("after await");
```

こうすれば、問題となっていた「中途半端に完了扱いになる」現象は起きません。しかし、`ContinueWith`でどうしても書かなければならない場合や、特殊なタスク連携をする場合は、次のようにUnwrapを活用できます。

```csharp
// Unwrapを使って継続タスクを正しく待機
task.ContinueWith(async t => { 
    /* ... 継続処理（非同期） ... */ 
}).Unwrap().Wait();  // Unwrapにより内側のasync処理完了まで待機する
```

上記では、`ContinueWith`の戻り値（Task<Task>）に対して`Unwrap()`を呼ぶことで、内側のTaskが完了するまで完了しないTaskに変換しています。そのため、最後のWait()では「after await」が出力されるまで待つことになります。実際、`Wait()`ではなくこれをさらにawaitする形にすれば、自然に最後まで待機する継続処理となります。

もう一度、ポイントをコードで比較しておきます。

```csharp
task.ContinueWith(...);           // ❌ 内部の非同期処理が最後まで待機されない可能性があります
task.ContinueWith(...).Unwrap();  // ✅ 非同期処理を最後まで待機する正しい方法
```

1行目（Unwrapなし）はasync継続を開始しますが、内部処理が終わるのを待たずに完了扱いになります。2行目（Unwrapあり）は内部の非同期処理を含めて完了を待つため、安全に継続処理全体を待機できます。

SynchronizationContext と `ConfigureAwait(false)` の理解
----------------------------------------------------

ここまでの例は、コンソールアプリケーションなど**SynchronizationContextが存在しない環境**を前提にしていました。この場合、`await`後の継続処理はデフォルトでスレッドプール上で行われます（前節で見たように、同じスレッドIDになることもありますが、それは再利用のためです）。しかし、WindowsフォームやWPFといった**UIスレッドのあるアプリケーション**や従来のASP.NETのように**SynchronizationContextが存在する環境**では、`await`の挙動が少し異なります。

### SynchronizationContextとは？

`SynchronizationContext`（同期コンテキスト）とは、「特定のスレッド（またはスレッド群）でコードを実行するためのスケジューリング用オブジェクト」です。UIアプリではUIスレッド用のSynchronizationContextがあり、これがあるときに`await`すると、**続きの処理をUIスレッドに投げ戻そうとします**。ASP.NET（.NET Framework）ではリクエスト処理用スレッドに戻す動きをします。逆に、コンソールアプリやASP.NET Coreでは既定でSynchronizationContextは存在せず、`await`後の継続は単にスレッドプール上で行われます。

設計上、**`await`は実行中のSynchronizationContext（あれば）を捕捉し、タスク完了後の継続をそのコンテキスト経由で実行します**。UIシナリオでは便利です。例えば`await`後に直接UI操作をしても安全で、特別な措置なしにUIスレッドで継続処理が走ります。しかし、この挙動が思わぬ問題を引き起こすこともあります。

*   **パフォーマンス上のオーバーヘッド:** 必要ない場合でも元のコンテキストに戻すために余計なスレッド切り替えやメッセージポストが発生し、効率が下がる可能性があります。ライブラリなど、UIと無関係な内部処理では不要なコストになることも。
    
*   **デッドロック:** よく知られた問題として、「同期的にResultやWaitで待ってしまうケース」があります。UIスレッド上で`task.Result`や`task.Wait()`をしてしまうと、そのタスクの継続はUIコンテキストに戻りたがるのにUIスレッドは今ブロック中…という状況に陥り、永遠に待ち続けてしまいます。これが\*\*「asyncの上でsync待ち」\*\*によるデッドロックパターンです。
    

### `ConfigureAwait(false)` の活用

こうした問題を避けるために用意されているのが、`ConfigureAwait(false)`です。これは`await`するタスクに対して「**元のコンテキストに戻らなくていい**」という指示を与えます。`ConfigureAwait(false)`を付けてawaitすると、その後の継続は**現在のSynchronizationContextを捕捉せず**、結果として**ThreadPool上で実行**されます。

ライブラリコードや、UIに関係ないビジネスロジックなどでは、ほとんどの場合元のコンテキスト（例えばUIスレッド）に戻る必要はありません。したがって、**そういったコンテキスト非依存の非同期コードでは全てのawaitに`ConfigureAwait(false)`を付ける**のが一般的な推奨事項になっています。これにより、余計なマーシャリングを防ぎ、前述のデッドロックパターンも防止できます。

**例:** 仮にGUIアプリケーション内で次のようなコードがあったとします。

```csharp
// UIスレッド上のコード
await Task.Run(SomeBackgroundWork);               // （既定ではコンテキストを捕捉する）
MessageBox.Show("重い処理が完了しました！");        // -> UIスレッドに戻っているのでUI操作OK

// UIスレッド上だが、継続でUI操作は不要な場合:
await Task.Run(SomeBackgroundWork).ConfigureAwait(false);
// -> コンテキストを捕捉しないので、継続はバックグラウンドスレッドで実行
Debug.WriteLine("バックグラウンド処理完了（別スレッドからログ出力）");
```

上のブロックでは、1つ目の`await`では`ConfigureAwait(false)`を付けていないため、`MessageBox.Show`の部分は自動的にUIスレッドで実行されます（UI同期コンテキストが復帰をハンドルする）。一方、2つ目では`ConfigureAwait(false)`を付けているため、`Debug.WriteLine`はUIスレッドではなくスレッドプール上のスレッドから実行されます。もしこの後UIを触ろうとすれば失敗します（UIスレッドではないので）。つまり、**「この継続ではUIスレッドに戻る必要がない」場合に`ConfigureAwait(false)`を使う**ことで、無駄な切り替えを省き、安全性も確保できるということです。

重要なポイントをまとめると: **「元のコンテキストに戻る必要がない箇所では`ConfigureAwait(false)`を使う」** というのが非同期プログラミングのベストプラクティスの一つです。特にライブラリコードでは呼び出し元の環境に依存しないように徹底すべきです。逆に、UI更新が必要なコード（UI側のコード）では通常`ConfigureAwait(false)`は使いません。ケースバイケースで使い分けが必要です。

### 提案: `Task.EscapeFromSynchronizationContext()`

問題は、非同期処理が多くなってくると、**あらゆる`await`に毎回`.ConfigureAwait(false)`を書くのが煩雑**だということです。つけ忘れも起こりえますし、コードがノイズで読みにくくなる側面もあります。そこで、「一度だけ宣言的に同期コンテキストの捕捉をやめさせることができないか？」という発想が生まれます。これを実現するアイデアの一つが、`Task.EscapeFromSynchronizationContext()`のようなヘルパーメソッドです。

これは正式な.NET APIではありませんが、簡単に自前で実装できます。例えば以下のようなメソッドを用意します。

```csharp
public static class TaskExtensions
{
    // asyncメソッドの先頭で呼ぶことで、以降のawaitが同期コンテキストに復帰しないようにする
    public static ConfiguredTaskAwaitable EscapeFromSynchronizationContext()
        => Task.Yield().ConfigureAwait(false);
}
```

このメソッドでは、`Task.Yield()`を利用しています。`Task.Yield()`は「一旦現在の場所で実行をyieldし、他の仕事をさせてから継続させる」ためのawait可能なオブジェクトを返します。これに`.ConfigureAwait(false)`を付けて返すことで、「このyield後の継続はコンテキストを捕捉しない（＝スレッドプール上で続行する）」という効果を持たせています。**つまり、このメソッドをawaitすれば、そこで同期コンテキストから「脱出」できる**わけです。

使い方としては、非同期メソッドのごく冒頭で次のように呼び出します。

```csharp
public async Task LoadDataAsync()
{
    await TaskExtensions.EscapeFromSynchronizationContext();
    // ここから下では、このメソッド内のawaitは元のコンテキストに戻らない

    var data = await SomeIOBoundOperationAsync();  // （コンテキスト捕捉なしで継続）
    // ... 重い処理 ...
    await AnotherOperationAsync();                // （これもスレッドプールで継続）
    // （もしUIスレッドに戻る必要が出たら、明示的にDispatcher.Invokeなどで切り替える）
    this.Dispatcher.Invoke(() => UpdateUI(data));
}
```

上記のように、最初に一度`EscapeFromSynchronizationContext()`をawaitしておけば、そのメソッド内ではいちいち`ConfigureAwait(false)`と書かなくても**同期コンテキストに復帰しない**状態で非同期処理を続けられます。これは読みやすさや書き忘れ防止の観点でメリットがあります。また、コードレビューの際にも「このメソッドは最初にコンテキストから抜けているからOKだな」と一目で分かる利点もあります。

> **注意:** ASP.NET Coreでは初めからSynchronizationContextがないため、基本的に`ConfigureAwait(false)`は不要です（await後は常にスレッドプールで継続します）。しかし、古いASP.NETやデスクトップUIアプリではデフォルトでコンテキスト復帰がありますので、このようなテクニックが役立ちます。また、上記のようにコンテキストを抜けた後でUIスレッドに戻りたい場合は、DispatcherやSynchronizationContext.Current（UIコンテキスト）にポストするなど明示的な対応が必要になります。

___________

以上、`async/await`によるスレッドの挙動、`ContinueWith`の落とし穴とその対策、そしてSynchronizationContextと`ConfigureAwait(false)`について解説しました。本リポジトリのコードをぜひ実行してみてください。スレッドプール負荷の有無を切り替えたり、`ConfigureAwait(false)`を付けたり外したりして挙動の違いを観察すると、非同期処理の裏側への理解が深まるはずです。非同期デバッグのお供に、本コードがお役に立てば幸いです。
