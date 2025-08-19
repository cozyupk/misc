using System;

namespace Boostable.NestedExample.ClassLibrary
{
    /// <summary>なんとなくな書き方</summary>
    internal abstract class OrdinaryClass
    {
        /// <summary>コンストラクタ</summary>
        protected OrdinaryClass()
        {
            // 初期化のためのメソッドを呼び出す
            Initialize();
        }

        /// <summary>派生(具象)クラスに初期化の機会を与える</summary>
        protected abstract void Initialize();

        /// <summary>初期化が期待されるプロパティ</summary>
        protected string SomePropertyA { get; set; } = string.Empty;

        /// <summary>初期化が期待されるプロパティ</summary>
        protected int SomePropertyB { get; set; }

        // でもプロパティは普通に派生(具象)クラスのコンストラクタで初期化してもいいし、
        // 派生クラス側でいつでもプロパティの値を書き換えられちゃうよね。
    }

    /// <summary>初期化スコープという概念を導入する</summary>
    internal abstract class InitializeScopeBase<TOwner>(TOwner owner) : IDisposable
    {
        /// <summary>外側のクラスのインスタンス</summary>
        protected TOwner Owner { get; } = owner;

        /// <summary>初期化のタイミングを制御するためのフラグ</summary>
        protected bool IsFrozen { get; private set; } = false;

        /// <summary>初期化用ユーティリティメソッド</summary>
        protected void SetOwnerProperty<T>(T value, Action<T> actionToStore)
        {
            if (IsFrozen)
                throw new InvalidOperationException("もう初期化タイミングは終わってるよ！");
            actionToStore(value);
        }

        /// <summary>初期化の検証を行う</summary>
        public abstract void Validate();

        /// <summary>Disposeで初期化は完了したものとする</summary>
        /// <remarks>リソースは管理していないため、厳密なDisposeパターンは現時点で不要</remarks>
        public virtual void Dispose()
        {
            IsFrozen = true;
        }
    }

    /// <summary>初期化スコープを導入したクラスの例</summary>
    internal abstract class ImprovedClass
    {
        /// <summary>コンストラクタ</summary>
        protected ImprovedClass()
        {
            // 「初期化スコープ」を作成する
            using var scope = new InitializeScope(this);
            // 初期化
            InitializeImprovedInstance(scope);
            // 検証
            scope.Validate();
        }

        /// <summary>派生(具象)クラスに初期化の機会を与える</summary>
        protected abstract void InitializeImprovedInstance(InitializeScope scope);

        /// <summary>初期化時に利用される「スコープ」を提供するクラス</summary>
        /// <remarks>コンストラクタ</remarks>
        protected class InitializeScope(ImprovedClass owner) : InitializeScopeBase<ImprovedClass>(owner)
        {
            /// <summary>初期化用プロパティ</summary>
            public virtual string SomePropertyA
            {
                get => Owner.SomePropertyA;
                set => SetOwnerProperty(value, v => Owner.SomePropertyA = v);
            }

            /// <summary>初期化用プロパティ</summary>
            public virtual int SomePropertyB
            {
                get => Owner.SomePropertyB;
                set => SetOwnerProperty(value, v => Owner.SomePropertyB = v);
            }

            /// <summary>初期化の検証</summary>
            public override void Validate()
            {
                if (string.IsNullOrWhiteSpace(SomePropertyA))
                    throw new InvalidOperationException("SomePropertyA がちゃんと初期化されてないよ！");
                if (Owner.SomePropertyB < 0)
                    throw new InvalidOperationException("SomePropertyB に負数は入れないでね！");
            }
        }

        /// <summary>初期化が期待されるプロパティ</summary>
        protected string SomePropertyA { get; private set; } = string.Empty;

        /// <summary>初期化が期待されるプロパティ</summary>
        protected int SomePropertyB { get; private set; }

        // これで Initialize() の役割がはっきりしたぜ！
    }

    /// <summary>使い方の例</summary>
    internal class ExampleUsage : ImprovedClass
    {
        /// <summary>具象クラスの初期化</summary>
        protected override void InitializeImprovedInstance(InitializeScope scope)
        {
            // 初期化スコープを使ってプロパティを設定
            scope.SomePropertyA = "初期化された値";
            scope.SomePropertyB = 42; // 例えば、42は妥当な値とする
        }
    }

    /// <summary>さらなる拡張の例</summary>
    internal abstract class DerivedImprovedClass : ImprovedClass
    {
        /// <summary>コンストラクタ</summary>
        protected DerivedImprovedClass()
        {
            // 「初期化スコープ」を作成する
            using var scope = new DerivedInitializeScope(this);
            // 初期化
            InitializeDerivedImprovedInstance(scope);
            // 検証
            scope.Validate();
        }

        /// <summary>初期化が必要なプロパティの追加</summary>
        protected object SomeNewProperty { get; private set; } = IndicatingUninitialized;

        /// <summary>未初期化を示すダミーのobject</summary>
        protected static object IndicatingUninitialized { get; } = new object();

        /// <summary>スコープも拡張する</summary>
        protected class DerivedInitializeScope(DerivedImprovedClass owner) : InitializeScope(owner)
        {
            /// <summary>Ownerの型をDerivedImprovedClassに限定する</summary>
            protected new DerivedImprovedClass Owner { get; } = owner;

            /// <summary>追加の初期化用プロパティ</summary>
            public object SomeNewProperty
            {
                get => Owner.SomeNewProperty;
                set => SetOwnerProperty(value, v => Owner.SomeNewProperty = v);
            }
            /// <summary>初期化の検証も追加</summary>
            public override void Validate()
            {
                base.Validate(); // 基底クラスの検証を呼び出す
                if (SomeNewProperty == IndicatingUninitialized)
                    throw new InvalidOperationException("SomeNewProperty が初期化されていないよ！");
            }
        }

        /// <summary>親クラスの初期化メソッドの無効化</summary>
        /// <param name="scope"></param>
        protected sealed override void InitializeImprovedInstance(InitializeScope scope)
        {
            // ImprovedClass の初期化はできない形とする
        }

        /// <summary>派生(具象)クラスに初期化の機会を与える</summary>
        abstract protected void InitializeDerivedImprovedInstance(DerivedInitializeScope scope);
    }

    /// <summary>さらなる拡張の使い方の例</summary>
    internal class ExampleDerivedUsage : DerivedImprovedClass
    {
        /// <summary>具象クラスの初期化</summary>
        protected override void InitializeDerivedImprovedInstance(DerivedInitializeScope scope)
        {
            scope.SomePropertyA = "初期化された値";
            scope.SomePropertyB = 42; // 例えば、42は妥当な値とする
            scope.SomeNewProperty = new object(); // 新しいプロパティも初期化する
        }
    }
}
