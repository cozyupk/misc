using Boostable.Prototype.OCPOrientedTree.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Boostable.Prototype.OCPOrientedTree.Base
{
    public interface ITreeNode
    {
        // Just a marker interface for nodes.
    }

    public abstract class TreeNodeProtectedAbstractions
    {
        protected internal interface ITreeNodeHavingValue<TSelfValue> : ITreeNode
        {
            public TSelfValue? Value { get; set; }
            IReadOnlyTreeNodeHavingValue<TSelfValue> AsReadOnlyWithSelfType();
        }

        protected internal interface IReadOnlyTreeNodeHavingValue<out TSelfValue> : ITreeNode
        {
            TSelfValue? Value { get; }
        }

        protected internal interface ITreeChildNode<TParentValue> : ITreeNode
        {
            ITreeNodeHavingValue<TParentValue> Parent { get; }
            ITreeParentNode<TTarget>? FindAncestor<TTarget>(int numSkipMatchedAncestor);
            IReadOnlyTreeParentNode<TTarget>? FindReadOnlyAncestor<TTarget>(int numSkipMatchedAncestor);
            IReadOnlyTreeChildNode<TParentValue> AsReadOnlyWithParentType();
        }

        protected internal interface ITreeChildNode<TSelfValue, TParentValue>
            : ITreeChildNode<TParentValue>, ITreeNodeHavingValue<TSelfValue>
        {
            IReadOnlyTreeChildNode<TSelfValue, TParentValue> AsReadOnlyWithSelfAndParentType();
        }

        protected internal interface IReadOnlyTreeChildNode<out TParentValue> : ITreeNode
        {
            IReadOnlyTreeNodeHavingValue<TParentValue> Parent { get; }
            IReadOnlyTreeParentNode<TTarget>? FindReadOnlyAncestor<TTarget>(int numSkipMatchedAncestor);
        }

        protected internal interface IReadOnlyTreeChildNode<out TSelfValue, out TParentValue>
            : IReadOnlyTreeChildNode<TParentValue>, IReadOnlyTreeNodeHavingValue<TSelfValue>
        {
        }

        protected internal interface IChildrenManager<TParentSelfValue>
        {
            ITreeIntermediateNode<TChildValue, TParentSelfValue> Spawn<TChildValue>(TChildValue? value);
            ITreeChildNode<TLeafValue, TParentSelfValue> SpawnLeaf<TLeafValue>(TLeafValue? value);
            IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> GetChildren<TChildValue>();
            IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>();
        }

        protected internal interface ITreeParentNode<TParentSelfValue>
            : ITreeNodeHavingValue<TParentSelfValue>
        {
            ITreeIntermediateNode<TChildValue, TParentSelfValue> Spawn<TChildValue>(TChildValue? value);
            ITreeChildNode<TLeafValue, TParentSelfValue> SpawnLeaf<TLeafValue>(TLeafValue? value);
            IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> GetChildren<TChildValue>();
            IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>();
            IReadOnlyTreeParentNode<TParentSelfValue> AsReadOnlyWithParentType();
        }

        protected internal interface IReadOnlyTreeParentNode<out TParentSelfValue> 
            : IReadOnlyTreeNodeHavingValue<TParentSelfValue>
        {
            IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>();
        }

        protected internal interface ITreeIntermediateNode<TSelfValue, TValueOfMyParent>
            : ITreeParentNode<TSelfValue>, ITreeChildNode<TSelfValue, TValueOfMyParent>
        {
            IReadOnlyTreeIntermediateNode<TSelfValue, TValueOfMyParent> AsReadonlyWithSelfAndParentType();
        }

        protected internal interface IReadOnlyTreeIntermediateNode<out TParentSelfValue, out TValueOfMyParent>
            : IReadOnlyTreeParentNode<TParentSelfValue>, IReadOnlyTreeChildNode<TParentSelfValue, TValueOfMyParent>
        {
        }

        protected internal interface INodeWrapperUniquenizer
        {
            TWrapper GetOrCreate<TProtectedNode, TWrapper>(TProtectedNode node, Func<TProtectedNode, TWrapper> factory)
                where TProtectedNode : class, ITreeNode
                where TWrapper : class;
        }
    }

    public abstract class TreeNodeHavingValue<TSelfValue>
        : TreeNodeProtectedAbstractions
        , TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue>
        , TreeNodeProtectedAbstractions.IReadOnlyTreeNodeHavingValue<TSelfValue>
    {
        protected internal virtual TSelfValue? ValueProtected { get; set; }

        TSelfValue? ITreeNodeHavingValue<TSelfValue>.Value
        {
            get => ValueProtected; set => ValueProtected = value;
        }

        TSelfValue? IReadOnlyTreeNodeHavingValue<TSelfValue>.Value => ValueProtected;

        // Constructor for TreeNodeHavingValue
        protected internal TreeNodeHavingValue(TSelfValue? value)
        {
            ValueProtected = value;
        }

        IReadOnlyTreeNodeHavingValue<TSelfValue> ITreeNodeHavingValue<TSelfValue>.AsReadOnlyWithSelfType()
            => this;
    }

    public class TreeChildNode<TSelfValue, TParentValue>
        : TreeNodeHavingValue<TSelfValue>
        , TreeNodeProtectedAbstractions.ITreeChildNode<TParentValue>
        , TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue, TParentValue>
        , TreeNodeProtectedAbstractions.IReadOnlyTreeChildNode<TParentValue>
        , TreeNodeProtectedAbstractions.IReadOnlyTreeChildNode<TSelfValue, TParentValue>
    {
        protected internal virtual TreeNodeHavingValue<TParentValue> ParentProtected { get; }

        // Constructor for TreeChildNode
        protected internal TreeChildNode(TreeNodeHavingValue<TParentValue> parent, TSelfValue? value) : base(value)
        {
            ParentProtected = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        protected internal virtual ITreeParentNode<TTarget>? FindAncestorProtected<TTarget>(int numSkipMatchedAncestor)
        {
            if (numSkipMatchedAncestor < 0)
                throw new ArgumentOutOfRangeException(nameof(numSkipMatchedAncestor), "Number of ancestors to skip cannot be negative.");

            object? current = ParentProtected;

            while (current != null)
            {
                if (current is ITreeParentNode<TTarget> matched)
                {
                    if (numSkipMatchedAncestor == 0)
                        return matched;

                    numSkipMatchedAncestor--;
                }

                if (current is ITreeChildNode<TTarget> childNode)
                {
                    current = childNode.Parent;
                }
                else
                {
                    break;
                }
            }

            return null;
        }

        ITreeNodeHavingValue<TParentValue> ITreeChildNode<TParentValue>.Parent => ParentProtected;

        IReadOnlyTreeNodeHavingValue<TParentValue> IReadOnlyTreeChildNode<TParentValue>.Parent => ParentProtected;

        IReadOnlyTreeChildNode<TParentValue> ITreeChildNode<TParentValue>.AsReadOnlyWithParentType()
            => this;

        IReadOnlyTreeChildNode<TSelfValue, TParentValue> ITreeChildNode<TSelfValue, TParentValue>.AsReadOnlyWithSelfAndParentType()
            => this;

        ITreeParentNode<TTarget>? ITreeChildNode<TParentValue>.FindAncestor<TTarget>(int numSkipMatchedAncestor)
            => FindAncestorProtected<TTarget>(numSkipMatchedAncestor);

        IReadOnlyTreeParentNode<TTarget>? ITreeChildNode<TParentValue>.FindReadOnlyAncestor<TTarget>(int numSkipMatchedAncestor)
            => FindAncestorProtected<TTarget>(numSkipMatchedAncestor)?.AsReadOnlyWithParentType();

        IReadOnlyTreeParentNode<TTarget>? IReadOnlyTreeChildNode<TParentValue>.FindReadOnlyAncestor<TTarget>(int numSkipMatchedAncestor)
            => FindAncestorProtected<TTarget>(numSkipMatchedAncestor)?.AsReadOnlyWithParentType();
    }

    public class ChildrenManager<TParentSelfValue>
        : TreeNodeProtectedAbstractions
        , TreeNodeProtectedAbstractions.IChildrenManager<TParentSelfValue>
    {
        protected internal virtual TreeNodeHavingValue<TParentSelfValue> ParentSelfProtected { get; }
        protected internal virtual object ChildrenSyncLockProtected { get; } = new();

        protected internal virtual IList<ITreeChildNode<TParentSelfValue>> ChildListProtected { get; } = new List<ITreeChildNode<TParentSelfValue>>();
        protected internal virtual HashSet<ITreeChildNode<TParentSelfValue>> ChildrenSetProtected { get; } = new();

        // Constructor for ChildrenManager
        protected internal ChildrenManager(TreeNodeHavingValue<TParentSelfValue> parentSelf)
        {
            ParentSelfProtected = parentSelf ?? throw new ArgumentNullException(nameof(parentSelf));
        }

        protected internal virtual bool TryAddChildProtected(ITreeChildNode<TParentSelfValue> childNode)
        {
            lock (ChildrenSyncLockProtected)
            {
                if (ChildrenSetProtected.Add(childNode))
                {
                    ChildListProtected.Add(childNode);
                    return true;
                }
                return false;
            }
        }
        protected internal virtual ITreeIntermediateNode<TChildValue, TParentSelfValue> SpawnProtected<TChildValue>(TChildValue? value)
        {
            var child = new Base.ITreeIntermediateNode<TChildValue, TParentSelfValue>(ParentSelfProtected, value);
            // Assuming TryAddChild is a method that adds the child to the collection
            if (TryAddChildProtected(child))
            {
                return child;
            }
            // TryAddChildProtected must always succeed because a new instance is used.
            // If this fails, it indicates a bug in the children management logic.
            throw new InvalidOperationException("Failed to add child.");
        }

        protected internal virtual ITreeChildNode<TLeafValue, TParentSelfValue> SpawnLeafProtected<TLeafValue>(TLeafValue? value)
        {
            var child = new TreeChildNode<TLeafValue, TParentSelfValue>(ParentSelfProtected, value);
            // Assuming TryAddChild is a method that adds the child to the collection
            if (TryAddChildProtected(child))
            {
                return child;
            }
            // Safety check to ensure the child was added successfully
            throw new InvalidOperationException("Failed to add child.");
        }
        protected internal virtual IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> GetChildrenProtected<TChildValue>()
        {
            lock (ChildrenSyncLockProtected)
            {
                return ChildListProtected
                            .Select(x => x as ITreeChildNode<TChildValue, TParentSelfValue>)
                            .Where(x => x != null)!
                            .Select(x => x!)
                            .ToArray();
            }
        }
        protected internal virtual IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>()
        {
            lock (ChildrenSyncLockProtected)
            {
                return ChildListProtected
                            .Select(x => x as IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>)
                            .Where(x => x != null)!
                            .Select(x => x!)
                            .ToArray();
            }
        }

        ITreeIntermediateNode<TChildValue, TParentSelfValue> IChildrenManager<TParentSelfValue>.Spawn<TChildValue>(TChildValue? value) where TChildValue : default
            => SpawnProtected(value);
        ITreeChildNode<TLeafValue, TParentSelfValue> IChildrenManager<TParentSelfValue>.SpawnLeaf<TLeafValue>(TLeafValue? value) where TLeafValue : default
            => SpawnLeafProtected(value);
        IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> IChildrenManager<TParentSelfValue>.GetChildren<TChildValue>()
            => GetChildrenProtected<TChildValue>();
        IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> IChildrenManager<TParentSelfValue>.GetReadOnlyChildren<TChildValue>()
            => GetReadOnlyChildren<TChildValue>();
    }

    public class TreeRootNode<TRootSelfValue>
        : TreeNodeHavingValue<TRootSelfValue>
        , TreeNodeProtectedAbstractions.ITreeParentNode<TRootSelfValue>
        , TreeNodeProtectedAbstractions.IReadOnlyTreeParentNode<TRootSelfValue>
    {
        protected internal virtual IChildrenManager<TRootSelfValue> ChildrenManagerProtected { get; }

        protected internal virtual ChildrenManager<TRootSelfValue> CreateChildrenManager()
        {
            return new ChildrenManager<TRootSelfValue>(this);
        }

        // Constructor for TreeRootNode
        protected internal TreeRootNode(TRootSelfValue? value) : base(value)
        {
            ChildrenManagerProtected = CreateChildrenManager();
        }

        ITreeIntermediateNode<TChildValue, TRootSelfValue> ITreeParentNode<TRootSelfValue>.Spawn<TChildValue>(TChildValue? value) where TChildValue : default
            => ChildrenManagerProtected.Spawn(value);

        ITreeChildNode<TLeafValue, TRootSelfValue> ITreeParentNode<TRootSelfValue>.SpawnLeaf<TLeafValue>(TLeafValue? value) where TLeafValue : default
            => ChildrenManagerProtected.SpawnLeaf<TLeafValue>(value);

        IEnumerable<ITreeChildNode<TChildValue, TRootSelfValue>> ITreeParentNode<TRootSelfValue>.GetChildren<TChildValue>()
            => ChildrenManagerProtected.GetChildren<TChildValue>();

        IEnumerable<IReadOnlyTreeChildNode<TChildValue, TRootSelfValue>> ITreeParentNode<TRootSelfValue>.GetReadOnlyChildren<TChildValue>()
            => ChildrenManagerProtected.GetReadOnlyChildren<TChildValue>();

        IReadOnlyTreeParentNode<TRootSelfValue> ITreeParentNode<TRootSelfValue>.AsReadOnlyWithParentType()
            => this;

        IEnumerable<IReadOnlyTreeChildNode<TChildValue, TRootSelfValue>> IReadOnlyTreeParentNode<TRootSelfValue>.GetReadOnlyChildren<TChildValue>()
            => ChildrenManagerProtected.GetReadOnlyChildren<TChildValue>();
    }

    public class ITreeIntermediateNode<TParentSelfValue, TValueOfMyParent>
        : TreeChildNode<TParentSelfValue, TValueOfMyParent>
        , TreeNodeProtectedAbstractions.ITreeIntermediateNode<TParentSelfValue, TValueOfMyParent>
        , TreeNodeProtectedAbstractions.IReadOnlyTreeParentNode<TParentSelfValue>
        , TreeNodeProtectedAbstractions.IReadOnlyTreeIntermediateNode<TParentSelfValue, TValueOfMyParent>
    {
        protected internal virtual IChildrenManager<TParentSelfValue> ChildrenManagerProtected { get; }

        protected internal virtual ChildrenManager<TParentSelfValue> CreateChildrenManager()
        {
            return new ChildrenManager<TParentSelfValue>(this);
        }

        // Constructor for ITreeIntermediateNode
        protected internal ITreeIntermediateNode(TreeNodeHavingValue<TValueOfMyParent> parent, TParentSelfValue? value) : base(parent, value)
        {
            ChildrenManagerProtected = CreateChildrenManager();
        }

        ITreeNodeHavingValue<TValueOfMyParent> ITreeChildNode<TValueOfMyParent>.Parent
            => base.ParentProtected;

        ITreeIntermediateNode<TChildValue, TParentSelfValue> ITreeParentNode<TParentSelfValue>.Spawn<TChildValue>(TChildValue? value)
            where TChildValue : default
            => ChildrenManagerProtected.Spawn(value);

        ITreeChildNode<TLeafValue, TParentSelfValue> ITreeParentNode<TParentSelfValue>.SpawnLeaf<TLeafValue>(TLeafValue? value)
            where TLeafValue : default
           => ChildrenManagerProtected.SpawnLeaf<TLeafValue>(value);

        IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> ITreeParentNode<TParentSelfValue>.GetChildren<TChildValue>()
            => ChildrenManagerProtected.GetChildren<TChildValue>();

        IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> ITreeParentNode<TParentSelfValue>.GetReadOnlyChildren<TChildValue>()
            => ChildrenManagerProtected.GetReadOnlyChildren<TChildValue>();

        IReadOnlyTreeParentNode<TParentSelfValue> ITreeParentNode<TParentSelfValue>.AsReadOnlyWithParentType()
            => this;

        IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> IReadOnlyTreeParentNode<TParentSelfValue>.GetReadOnlyChildren<TChildValue>()
            => ChildrenManagerProtected.GetReadOnlyChildren<TChildValue>();

        IReadOnlyTreeIntermediateNode<TParentSelfValue, TValueOfMyParent> ITreeIntermediateNode<TParentSelfValue, TValueOfMyParent>.AsReadonlyWithSelfAndParentType()
            => this;
    }

    public class NodeWrapperUniquenizer
        : TreeNodeProtectedAbstractions
        , TreeNodeProtectedAbstractions.INodeWrapperUniquenizer
    {
        protected internal sealed class Table<TProtectedNode, TWrapper>
            where TProtectedNode : class, ITreeNode
            where TWrapper : class
        {
            internal static readonly ConditionalWeakTable<TProtectedNode, TWrapper> Instance = new();
        }

        protected internal virtual TWrapper GetOrCreate<TProtectedNode, TWrapper>(TProtectedNode node, Func<TProtectedNode, TWrapper> factory)
            where TProtectedNode : class, ITreeNode
            where TWrapper : class
        {
            TWrapper LocalFactory(TProtectedNode n) => factory(n);
            return Table<TProtectedNode, TWrapper>.Instance.GetValue(node, LocalFactory);
        }

        TWrapper INodeWrapperUniquenizer.GetOrCreate<TProtectedNode, TWrapper>(TProtectedNode node, Func<TProtectedNode, TWrapper> factory)
            => GetOrCreate<TProtectedNode, TWrapper>(node, factory);
    }
    /*
}

namespace Boostable.Prototype.OCPOrientedTree.Simple
{
    */
    public interface IReadOnlySimpleTreeNode<TSelfValue>
    {
        TSelfValue? Value { get; }
        IEnumerable<IReadOnlySimpleTreeNode<TChildValue, TSelfValue>> GetReadOnlyChildren<TChildValue>();
        IReadOnlySimpleTreeNode<TTarget>? FindReadOnlyAncestor<TTarget>(int numSkipMatchedAncestor);
    }

    public interface ISimpleTreeNode<TSelfValue>
    {
        TSelfValue? Value { get; set; }
        bool TrySpawn<TChildValue>(out ISimpleTreeNode<TChildValue, TSelfValue>? spawned, TChildValue? value = default);
        IEnumerable<ISimpleTreeNode<TChildValue, TSelfValue>> GetChildren<TChildValue>();
        IEnumerable<IReadOnlySimpleTreeNode<TChildValue, TSelfValue>> GetReadOnlyChildren<TChildValue>();
        IReadOnlySimpleTreeNode<TSelfValue> AsReadOnly();
        ISimpleTreeNode<TTarget>? FindAncestor<TTarget>(int numSkipMatchedAncestor);
        IReadOnlySimpleTreeNode<TTarget>? FindReadOnlyAncestor<TTarget>(int numSkipMatchedAncestor);
    }

    public interface IReadOnlySimpleTreeNode<TSelfValue, TParent>
        : IReadOnlySimpleTreeNode<TSelfValue>
    {
        ISimpleTreeNode<TParent> Parent { get; }
    }

    public interface ISimpleTreeNode<TSelfValue, TParent>
        : ISimpleTreeNode<TSelfValue>
    {
        ISimpleTreeNode<TParent> Parent { get; }
        IReadOnlySimpleTreeNode<TSelfValue, TParent> AsReadOnlyWithParentType();
    }

    internal static class SimpleTreeNodeFactory<TSelfValue>
    {
        public static SimpleTreeNode<TSelfValue> CreateInstance(TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue> nodeInternal)
        {
            if (nodeInternal is TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue> childNodeInternal)
            {
                var nodeType = childNodeInternal.GetType();

                // ITreeChildNode<TSelfValue, TParentValue> のインタフェースを探す
                var targetInterface = nodeType
                    .GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(TreeNodeProtectedAbstractions.ITreeChildNode<,>) &&
                        i.GetGenericArguments()[0] == typeof(TSelfValue)
                    );

                SimpleTreeNode<TSelfValue>? resultInstance = null;

                if (targetInterface != null)
                {
                    var parentType = targetInterface.GetGenericArguments()[1]; // TParentValue の実型を取得

                    // typeof(SimpleTreeNode<TSelfValue, TParentValue>) を構築
                    var constructedType = typeof(SimpleTreeNode<,>)
                        .MakeGenericType(typeof(TSelfValue), parentType);

                    // コンストラクタ引数は ITreeChildNode<TSelfValue, TParentValue>
                    resultInstance = Activator.CreateInstance(constructedType, childNodeInternal) as SimpleTreeNode<TSelfValue>;
                }

                if (resultInstance != null)
                {
                    return resultInstance;
                }
            }
            return SimpleTreeNode<TSelfValue>.GetOrCreateInstance(nodeInternal);
        }
    }

    internal class SimpleTreeNode
    {
        protected static TreeNodeProtectedAbstractions.INodeWrapperUniquenizer NodeWrapperUniquenizer { get; } = new NodeWrapperUniquenizer();
    }
    internal class SimpleTreeNode<TSelfValue> : SimpleTreeNode, ISimpleTreeNode<TSelfValue>, IReadOnlySimpleTreeNode<TSelfValue>
    {
        internal TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue> NodeInternal { get; }
        public TSelfValue? Value { 
            get => NodeInternal.Value;
            set => NodeInternal.Value = value;
        }

        internal static SimpleTreeNode<TSelfValue> GetOrCreateInstance(TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue> nodeInternal)
        {
            _ = nodeInternal ?? throw new ArgumentNullException(nameof(nodeInternal));
            return NodeWrapperUniquenizer.GetOrCreate(
                nodeInternal,
                n => new SimpleTreeNode<TSelfValue>(n)
            );
        }

        protected internal SimpleTreeNode(TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue> nodeInternal)
        {
            NodeInternal = nodeInternal ?? throw new ArgumentNullException(nameof(NodeInternal));
        }

        public bool TrySpawn<TChildValue>(out ISimpleTreeNode<TChildValue, TSelfValue>? spawned, TChildValue? value = default)
        {
            if (NodeInternal is TreeNodeProtectedAbstractions.ITreeParentNode<TSelfValue> parentNode)
            {
                var spawnedInternal = parentNode.Spawn(value);
                spawned = SimpleTreeNode<TChildValue, TSelfValue>.GetOrCreateInstance(spawnedInternal);
                return true;
            }
            spawned = default;
            return false;
        }

        public IEnumerable<ISimpleTreeNode<TChildValue, TSelfValue>> GetChildren<TChildValue>()
        {
            if (NodeInternal is not TreeNodeProtectedAbstractions.ITreeParentNode<TSelfValue> parentNode)
                return Enumerable.Empty<ISimpleTreeNode<TChildValue, TSelfValue>>();

            return parentNode
                .GetChildren<TChildValue>()
                .Select(child => (ISimpleTreeNode<TChildValue, TSelfValue>)SimpleTreeNodeFactory<TChildValue>.CreateInstance(child));
        }

        public IEnumerable<IReadOnlySimpleTreeNode<TChildValue, TSelfValue>> GetReadOnlyChildren<TChildValue>()
        {
            if (NodeInternal is not TreeNodeProtectedAbstractions.ITreeParentNode<TSelfValue> parentNode)
                return Enumerable.Empty<IReadOnlySimpleTreeNode<TChildValue, TSelfValue>>();

            return parentNode
                .GetChildren<TChildValue>()
                .Select(child => (IReadOnlySimpleTreeNode<TChildValue, TSelfValue>)SimpleTreeNodeFactory<TChildValue>.CreateInstance(child));
        }

        public override bool Equals(object? obj)
        {
            return obj is SimpleTreeNode<TSelfValue> other &&
                           NodeInternal.Equals(other.NodeInternal);
        }

        public override int GetHashCode()
        {
            return NodeInternal?.GetHashCode() ?? 0;
        }

        public IReadOnlySimpleTreeNode<TSelfValue> AsReadOnly()
            => this;

        public ISimpleTreeNode<TTarget>? FindAncestor<TTarget>(int numSkipMatchedAncestor)
        {
            if (NodeInternal is not TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue> nodeInternalAsAChild)
                return null;

            var retval = nodeInternalAsAChild.FindAncestor<TTarget>(numSkipMatchedAncestor);
            if (retval == null)
                return null;

            return SimpleTreeNode<TTarget>.GetOrCreateInstance(retval);
        }

        public IReadOnlySimpleTreeNode<TTarget>? FindReadOnlyAncestor<TTarget>(int numSkipMatchedAncestor)
        {
            return FindAncestor<TTarget>(numSkipMatchedAncestor)?.AsReadOnly();
        }
    }

    internal sealed class SimpleTreeNode<TSelfValue, TParentValue>
        : SimpleTreeNode<TSelfValue>
        , ISimpleTreeNode<TSelfValue, TParentValue>, IReadOnlySimpleTreeNode<TSelfValue, TParentValue>
    {
        public ISimpleTreeNode<TParentValue> Parent { get; }

        internal static SimpleTreeNode<TSelfValue, TParentValue> GetOrCreateInstance(TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue, TParentValue> nodeInternal)
        {
            _ = nodeInternal ?? throw new ArgumentNullException(nameof(nodeInternal));
            return NodeWrapperUniquenizer.GetOrCreate(
                nodeInternal,
                n => new SimpleTreeNode<TSelfValue, TParentValue>(n)
            );
        }

        internal SimpleTreeNode(TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue, TParentValue> nodeInternal)
            : base(nodeInternal)
        {
            Parent = SimpleTreeNode<TParentValue>.GetOrCreateInstance(nodeInternal.Parent);
        }

        public override bool Equals(object? obj)
        {
            return obj is SimpleTreeNode<TSelfValue> other &&
                           NodeInternal.Equals(other.NodeInternal);
        }

        public override int GetHashCode()
        {
            return NodeInternal?.GetHashCode() ?? 0;
        }

        public IReadOnlySimpleTreeNode<TSelfValue, TParentValue> AsReadOnlyWithParentType()
            => this;
    }

    public static class SimpleTreeFactory
    {
        public static ISimpleTreeNode<TRootSelfValue> CreateRootNode<TRootSelfValue>(TRootSelfValue? value)
        {
            return SimpleTreeNode<TRootSelfValue>.GetOrCreateInstance(new TreeRootNode<TRootSelfValue>(value));
        }
    }
}
