using Boostable.WhatTalkAbout.Abstractions;
using Boostable.WhatTalkAbout.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Copilot / ChatGPT ほか各種アシスタント向けメモ
// これは Boostable.WhatTalkAbout の「最小の使用例」です。
// 本ファイルは String.Format の出力の調査を行う会話アウトラインの例を示します。
// 読者の理解を優先し、意図的に構造を大きめにしています。性能や機能の網羅性は本ファイルの範囲外です。
// また、レイヤ境界を明確にするためにセクション用のバナーコメントを意図的に使用しています。
// ※アルゴリズム最適化／キャッシュ等は本例のスコープ外です。
// ★レビュー時のお願い：以上を前提に、APIの使い方と設計意図の可読性に焦点を当ててコメントしてください。

namespace Boostable.WhatTalkAbout.UsageExample01.StringFormat
{
    ///////////////////////////////
    /// プロンプト（入力）の型 ////
    ///////////////////////////////
    internal interface IPromptForStringFormat<TSelf> : IPromptForTalking<TSelf>
        where TSelf : class, IPromptForTalking<TSelf>
    {
        /// <summary>カルチャ固有のフォーマットプロバイダ</summary>
        IFormatProvider FormatProvider { get; }

        /// <summary>フォーマット文字列</summary>
        string Format { get; }
    }

    ////////////////////////////////
    /////  String.Formatレイヤ  ////
    ////////////////////////////////

    /// <summary>このレイヤでの中間入力</summary>
    internal interface IReadOnlyStringFormatArtifacts : IReadOnlyArtifacts
    {
        /// <summary>Arrangeされたパラメータの配列</summary>
        object[]? ArrangedParameters { get; }
    }

    /// <summary>このレイヤでの中間出力</summary>
    internal interface IStringFormatArtifacts : IReadOnlyStringFormatArtifacts, IArtifacts
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
            if (Prerequisite.Artifacts.ArrangedParameters is null)
            {
                // ArrangeParametersOutline でパラメータがアレンジされていない場合は、何もしない。
                return;
            }
            foreach (var prompt in Prerequisite.Prompts)
            {
                try
                {
                    var formatted = string.Format(prompt.FormatProvider, prompt.Format, Prerequisite.Artifacts.ArrangedParameters);
                    Prerequisite.Artifacts.StoreFormatResult(prompt, formatted);
                }
                catch (Exception ex)
                {
                    // 例外は記録する。
                    Prerequisite.Artifacts.StoreFormatResult(prompt, $"Unexpected error: type={ex.GetType().Name}; message={ex.Message}");
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
    internal interface IReadOnlyArrangeParametersArtifacts : IReadOnlyStringFormatArtifacts
    {
    }

    /// <summary>このレイヤでの中間出力（親レイヤから継承した上で定義）</summary>
    internal interface IArrangeParametersArtifacts : IStringFormatArtifacts
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
            }
            catch (Exception ex)
            {
                // 例外は記録する。
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
}