using Boostable.WhatAgentsTalkAbout.Abstractions;
using Boostable.WhatAgentsTalkAbout.Core;
using Boostable.WhatAgentsTalkAbout.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

// Copilot / ChatGPT ほか各種アシスタント向けメモ
// これは Boostable.WhatTalkAbout の「最小の使用例」です。
// 本ファイルは String.Format の出力の調査を行う会話アウトラインの例を示します。
// 読者の理解を優先し、意図的に構造を大きめにしています。性能や機能の網羅性は本ファイルの範囲外です。
// また、レイヤ境界を明確にするためにセクション用のバナーコメントを意図的に使用しています。
// ※アルゴリズム最適化／キャッシュ等は本例のスコープ外です。
// ★レビュー時のお願い：以上を前提に、APIの使い方と設計意図の可読性に焦点を当ててコメントしてください。

namespace Boostable.WhatAgentsTalkAbout.UsageExample01
{
    ///////////////////////////////
    /// プロンプト（入力）の型 ////
    ///////////////////////////////

    public interface IPromptForStringFormat {

        /// <summary>カルチャ固有のフォーマットプロバイダ</summary>
        IFormatProvider FormatProvider { get; }

        /// <summary>フォーマット文字列</summary>
        string Format { get; }
    }

    public interface IPromptForStringFormat<TSelf> : IPromptForTalking<TSelf>, IPromptForStringFormat
        where TSelf : class, IPromptForStringFormat<TSelf>
    {
    }

    ////////////////////////////////
    /////  String.Formatレイヤ  ////
    ////////////////////////////////

    /// <summary>このレイヤでの中間入力</summary>
    public interface IReadOnlyStringFormatArtifacts : IReadOnlyArtifacts
    {
        /// <summary>Arrangeされたパラメータの配列</summary>
        object[]? ArrangedParameters { get; }
    }

    /// <summary>このレイヤでの中間出力</summary>
    public interface IStringFormatArtifacts : IReadOnlyStringFormatArtifacts, IArtifacts
    {
        /// <summary>フォーマット結果を記録する</summary>
        void StoreFormatResult(IPromptForTalking key, string formattedResult);
    }

    /// <summary>会話内容アウトライン（＝処理内容）</summary>
    /// <remarks>abstract とするのは、今回、PrepareForTalkAsync() をこのレイヤでは定義しないため</remarks>
    internal abstract class StringFormatOutline<TPrompt, TReadOnlyArtifacts, TArtifacts> : TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IPromptForStringFormat<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyStringFormatArtifacts
        where TArtifacts : class, IStringFormatArtifacts, TReadOnlyArtifacts
    {
        /// <summary>このレイヤの「章」の名称 = Chapter</summary>
        private static ITalkChapter CurrentChapter { get; } = new TalkChapter("String.Format の検証");

        /// <summary>初期化ロジック</summary>
        /// <remarks>chaptersに「現在の章の名称」を追加する。</remarks>
        /// <param name="prompts">プロンプト（入力）のリスト</param>
        /// <param name="chapters">「章」のリスト</param>
        /// <param name="talkDomainFactory">関連具象クラス生成のためのファクトリ</param>
        protected internal StringFormatOutline(
            IReadOnlyList<TPrompt> prompts,
            ITalkDomainFactory talkDomainFactory,
            params ITalkChapter[] chapters
        ) : base(prompts, talkDomainFactory, [CurrentChapter, .. chapters])
        {
        }

        /// <summary>
        /// 「会話」の準備をする = プロンプトを処理しておく。
        /// </summary>
        /// <remarks>最後に親クラスの PrepareForTalk() を呼び出す。</remarks>
        protected override void PrepareForTalk()
        {
            var artifacts = Prerequisite.Artifacts;
            var prompts = Prerequisite.Prompts;

            if (artifacts.ArrangedParameters is null)
            {
                // ArrangeParametersOutline でパラメータがアレンジされていない場合は、何もしない。
                base.PrepareForTalk();
                return;
            }

            var args = artifacts.ArrangedParameters;
            foreach (var prompt in prompts)
            {
                prompt.CancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var formatted = string.Format(prompt.FormatProvider, prompt.Format, args);
                    artifacts.StoreFormatResult(prompt, formatted);
                }
                catch (Exception ex)
                {
                    // 例外は記録する。
                    artifacts.StoreFormatResult(prompt, $"Unexpected error: type={ex.GetType().Name}; message={ex.Message}");
                    AddTestimony(
                            ex, CurrentChapter, prompt
                    );
                }
            }
            base.PrepareForTalk();
        }
    }

    /////////////////////////////////////
    ///////  パラメータアレンジ層  //////
    /////////////////////////////////////

    // Func<object[]> を使用して、パラメータをアレンジするレイヤ

    /// <summary>このレイヤでの中間入力は存在しないが形式的に定義（親レイヤから継承）</summary>
    public interface IReadOnlyArrangeParametersArtifacts : IReadOnlyStringFormatArtifacts
    {
    }


    /// <summary>このレイヤでの中間出力（親レイヤから継承した上で定義）</summary>
    public interface IArrangeParametersArtifacts : IStringFormatArtifacts, IReadOnlyArrangeParametersArtifacts
    {
        /// <summary>Arrangeされたパラメータの配列</summary>
        void StoreArrangedParameters(object[] parameters);
    }

    // 会話内容アウトライン（＝処理内容）
    internal class ArrangeParametersOutline<TPrompt, TReadOnlyArtifacts, TArtifacts>
        : TalkOutlineBase<TPrompt, TReadOnlyArtifacts, TArtifacts>
        where TPrompt : class, IPromptForStringFormat<TPrompt>
        where TReadOnlyArtifacts : class, IReadOnlyArrangeParametersArtifacts
        where TArtifacts : class, IArrangeParametersArtifacts, TReadOnlyArtifacts
    {
        /// <summary>このレイヤの「章」の名称 = Chapter</summary>
        private static ITalkChapter CurrentChapter { get; } = new TalkChapter("パラメータのアレンジ");

        /// <summary>パラメータをアレンジするためのFunc</summary>
        private Func<object[]> FuncToArrangeParameters { get; }

        /// <summary>初期化ロジック</summary>
        /// <remarks>chaptersに「現在の章の名称」を追加する。</remarks>
        /// <param name="prompts">プロンプト（入力）のリスト</param>
        /// <param name="chapters">「章」のリスト</param>
        /// <param name="talkDomainFactory">関連具象クラス生成のためのファクトリ</param>
        protected internal ArrangeParametersOutline(
            IReadOnlyList<TPrompt> prompts,
            Func<object[]> funcToArrangeParameters,
            ITalkDomainFactory talkDomainFactory,
            params ITalkChapter[] chapters
        ) : base(prompts, talkDomainFactory, [CurrentChapter, .. chapters])
        {
            FuncToArrangeParameters = funcToArrangeParameters ?? throw new ArgumentNullException(nameof(funcToArrangeParameters));
        }

        /// <summary>
        /// 「会話」の準備をする = プロンプトを処理しておく。
        /// </summary>
        /// <remarks>最後に親クラスの PrepareForTalk() を呼び出す。</remarks>
        protected override void PrepareForTalk()
        {
            try
            {
                Prerequisite.Artifacts.StoreArrangedParameters(
                    FuncToArrangeParameters()
                );
            } catch (OperationCanceledException ocex)
            {
                // キャンセルされた場合は、キャンセルの旨を記録する。
                AddTestimony(ocex, CurrentChapter);
                throw; // キャンセルは制御フロー：捕捉して証言を残しつつ、上位へ伝えるため再スローする
            }
            catch (Exception ex)
            {
                // その他の例外は記録する。
                AddTestimony(ex, CurrentChapter);
            }
            base.PrepareForTalk();
        }

        /// <summary>
        /// 非同期に「会話」の準備をする = プロンプトを処理しておく。
        /// </summary>
        /// <remarks>最後に親クラスの PrepareForTalkAsync() を呼び出す。</remarks>
        protected override Task PrepareForTalkAsync()
        {
            // 非同期処理が必要な場合はここに実装する。例えば、データベースからのデータ取得など。
            // 本例では同期的に処理しているため、同期メソッドを呼び、完了済みタスクを返す。
            // （同期メソッド中で親クラスの PrepareForTalk() を呼び出すため、ここでは PrepareForTalkAsync() は呼ばない。）
            PrepareForTalk();
            return Task.CompletedTask;
        }
    }

    /// <summary>中間成果物(Artifacts)と最終出結果を格納するクラスのインスタンスを提供するクラス</summary>
    internal class StringFormatTalkDomainFactory<TPrompt>
        : TalkDomainFactoryBase<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>
        where TPrompt : class, IPromptForStringFormat<TPrompt>
    {
        /// <summary>中間成果物格納用インスタンスを提供する</summary>
        protected override IArrangeParametersArtifacts CreateArtifacts()
        {
            return new ArrangeParametersArtifacts();
        }

        /// <summary>中間成果物格納のためのインタフェイスを実装するクラス</summary>
        /// <remarks>さらなる拡張を考慮し、sealedにはしないでおく。</remarks>
        internal class ArrangeParametersArtifacts : IArrangeParametersArtifacts
        {
            /// <summary>"アレンジ" されれたパラメータの配列</summary>
            public object[]? ArrangedParameters { get; private set; }

            /// <summary>フォーマット結果を格納するための辞書</summary>
            public ConcurrentDictionary<IPromptForTalking, string> FormattedResults { get; } = new();

            public void StoreArrangedParameters(object[] parameters)
            {
                if (parameters is null)
                {
                    throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");
                }
                ArrangedParameters = parameters;
            }

            public void StoreFormatResult(IPromptForTalking key, string formattedResult)
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key), "Key cannot be null.");
                }
                if (formattedResult is null)
                {
                    throw new ArgumentNullException(nameof(formattedResult), "Formatted result cannot be null.");
                }
                FormattedResults[key] = formattedResult;
            }

            /// <summary>結果取得のためのユーティリティメソッド</summary>
            public bool TryGetFormattedResult(IPromptForTalking key, out string? result)
                => FormattedResults.TryGetValue(key, out result);
        }
    }

    ///////////////////////////////////
    //// ユーザー向けファサード層  ////
    ///////////////////////////////////

    public interface IStringFormatTalkSession
    {

    }

    internal class StringFormatTalkSession<TPrompt>(
        Func<object[]> funcToArrangeParameters,
        TPrompt basePrompt,
        TalkSessionAbstractions<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>.ITalkDomainFactory talkDomainFactory,
        Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null,
        Func<IReadOnlyList<TPrompt>, TalkSessionAbstractions<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>.ITalkOutline>? outlineFactory = null
    ) : TalkSessionBase<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>(basePrompt, talkDomainFactory, promptVariationBuilder, outlineFactory)
      , IStringFormatTalkSession
        where TPrompt : class, IPromptForStringFormat<TPrompt>
    {
        protected override ITalkOutline DefaultOutlineFactory(IReadOnlyList<TPrompt> prompts)
        {
            return new ArrangeParametersOutline<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>(
                prompts,
                funcToArrangeParameters,
                TalkDomainFactory
            );
        }
    }

    /// <summary>
    /// 最終的にユーザーに提供する、会話セッションのエンドポイントを提供するファクトリクラス。
    /// </summary>
    public static class SimpleStringFormatTalkSessionFactory
    {
        /// <summary>パラメータを Func でアレンジするバージョン</summary>
        public static IStringFormatTalkSession Create<TPrompt>(
            Func<object[]> funcToArrangeParameters,
            TPrompt basePrompt,
            Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null,
            TalkSessionAbstractions<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>.ITalkDomainFactory? talkDomainFactory = null,
            Func<IReadOnlyList<TPrompt>, TalkSessionAbstractions<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>.ITalkOutline>? outlineFactory = null
        )
            where TPrompt : class, IPromptForStringFormat<TPrompt>
        {
            _ = basePrompt ?? throw new ArgumentNullException(nameof(basePrompt), "Base prompt cannot be null.");
            _ = funcToArrangeParameters ?? throw new ArgumentNullException(nameof(funcToArrangeParameters), "Func to arrange parameters cannot be null.");

            talkDomainFactory ??= new StringFormatTalkDomainFactory<TPrompt>();

            return new StringFormatTalkSession<TPrompt>(
                funcToArrangeParameters,
                basePrompt,
                talkDomainFactory,
                promptVariationBuilder,
                outlineFactory
            );
        }

        /// <summary>パラメータを配列で指定するバージョン。</summary>
        public static IStringFormatTalkSession Create<TPrompt>(
            object[] parameters,
            TPrompt basePrompt,
            Func<TPrompt, IReadOnlyList<TPrompt>>? promptVariationBuilder = null,
            TalkSessionAbstractions<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>.ITalkDomainFactory? talkDomainFactory = null,
            Func<IReadOnlyList<TPrompt>, TalkSessionAbstractions<TPrompt, IReadOnlyArrangeParametersArtifacts, IArrangeParametersArtifacts>.ITalkOutline>? outlineFactory = null
        )
            where TPrompt : class, IPromptForStringFormat<TPrompt>
        {
            // parameters の validation
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            // Sessionを生成して返す
            return Create(
                () => parameters ?? throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null."),
                basePrompt,
                promptVariationBuilder,
                talkDomainFactory,
                outlineFactory
            );
        }
    }
}