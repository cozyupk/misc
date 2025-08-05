using Boostable.OCPOrientedTree.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Boostable.OCPOrientedTree.Base
{
    public abstract class TreeNodeProtectedAbstractions
    {
        internal protected interface ITreeNodeHavingValue<TSelfValue>
        {
            TSelfValue? Value { get; set; }
            IReadOnlyTreeNodeHavingValue<TSelfValue> AsReadOnlyWithSelfType();
        }

        internal protected interface IReadOnlyTreeNodeHavingValue<out TSelfValue>
        {
            TSelfValue? Value { get; }
        }
        internal protected interface ITreeChildNode<TParentValue>
        {
            ITreeNodeHavingValue<TParentValue> Parent { get; }
            IReadOnlyTreeChildNode<TParentValue> AsReadOnlyWithParentType();
        }

        internal protected interface ITreeChildNode<TSelfValue, TParentValue>
            : ITreeChildNode<TParentValue>, ITreeNodeHavingValue<TSelfValue>
        {
            IReadOnlyTreeChildNode<TSelfValue, TParentValue> AsReadOnlyWithSelfAndParentType();
        }

        internal protected interface IReadOnlyTreeChildNode<out TParentValue>
        {
            IReadOnlyTreeNodeHavingValue<TParentValue> Parent { get; }
        }

        internal protected interface IReadOnlyTreeChildNode<out TSelfValue, out TParentValue>
            : IReadOnlyTreeChildNode<TParentValue>, IReadOnlyTreeNodeHavingValue<TSelfValue>
        {
        }

        internal protected interface IChildrenManager<TParentSelfValue>
        {
            ITreeIntermediateNode<TChildValue, TParentSelfValue> Spawn<TChildValue>(TChildValue? value);
            ITreeChildNode<TLeafValue, TParentSelfValue> SpawnLeaf<TLeafValue>(TLeafValue? value);
            IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> GetChildren<TChildValue>();
            IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>();
        }

        internal protected interface ITreeParentNode<TParentSelfValue>
        {
            ITreeIntermediateNode<TChildValue, TParentSelfValue> Spawn<TChildValue>(TChildValue? value);
            ITreeChildNode<TLeafValue, TParentSelfValue> SpawnLeaf<TLeafValue>(TLeafValue? value);
            IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> GetChildren<TChildValue>();
            IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>();
            IReadOnlyParentNode<TParentSelfValue> AsReadOnlyWithParentType();
        }

        internal protected interface IReadOnlyParentNode<TParentSelfValue> 
            : IReadOnlyTreeNodeHavingValue<TParentSelfValue>
        {
            IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>();
        }

        internal protected interface ITreeIntermediateNode<TParentSelfValue, TValueOfMyParent>
            : ITreeParentNode<TParentSelfValue>, ITreeChildNode<TParentSelfValue, TValueOfMyParent>
        {
        }
    }

    public abstract class TreeNodeHavingValue<TSelfValue>
        : TreeNodeProtectedAbstractions
        , TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue>
        , TreeNodeProtectedAbstractions.IReadOnlyTreeNodeHavingValue<TSelfValue>
    {

        internal protected virtual TSelfValue? ValueProtected { get; set; }

        TSelfValue? ITreeNodeHavingValue<TSelfValue>.Value
        {
            get => ValueProtected; set => ValueProtected = value;
        }

        TSelfValue? IReadOnlyTreeNodeHavingValue<TSelfValue>.Value => ValueProtected;

        internal protected TreeNodeHavingValue(TSelfValue? value)
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
        internal protected virtual TreeNodeHavingValue<TParentValue> ParentProtected { get; }

        ITreeNodeHavingValue<TParentValue> ITreeChildNode<TParentValue>.Parent => ParentProtected;

        IReadOnlyTreeNodeHavingValue<TParentValue> IReadOnlyTreeChildNode<TParentValue>.Parent => ParentProtected;

        internal protected TreeChildNode(TreeNodeHavingValue<TParentValue> parent, TSelfValue? value) : base(value)
        {
            ParentProtected = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        IReadOnlyTreeChildNode<TParentValue> ITreeChildNode<TParentValue>.AsReadOnlyWithParentType()
            => this;

        IReadOnlyTreeChildNode<TSelfValue, TParentValue> ITreeChildNode<TSelfValue, TParentValue>.AsReadOnlyWithSelfAndParentType()
            => this;
    }

    public class ChildrenManager<TParentSelfValue>
        : TreeNodeProtectedAbstractions
        , TreeNodeProtectedAbstractions.IChildrenManager<TParentSelfValue>
    {
        internal protected TreeNodeHavingValue<TParentSelfValue> ParentSelfProtected { get; }
        internal protected object ChildrenSyncLockProtected { get; } = new();

        internal protected virtual IList<ITreeChildNode<TParentSelfValue>> ChildListProtected { get; } = new List<ITreeChildNode<TParentSelfValue>>();
        internal protected virtual HashSet<ITreeChildNode<TParentSelfValue>> ChildrenSetProtected { get; } = new();

        internal protected ChildrenManager(TreeNodeHavingValue<TParentSelfValue> parentSelf)
        {
            ParentSelfProtected = parentSelf ?? throw new ArgumentNullException(nameof(parentSelf));
        }

        internal protected virtual bool TryAddChildProtected(ITreeChildNode<TParentSelfValue> childNode)
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
        internal protected virtual ITreeIntermediateNode<TChildValue, TParentSelfValue> SpawnProtected<TChildValue>(TChildValue? value)
        {
            var child = new TreeNodeInternal<TChildValue, TParentSelfValue>(ParentSelfProtected, value);
            // Assuming TryAddChild is a method that adds the child to the collection
            if (TryAddChildProtected(child))
            {
                return child;
            }
            // TryAddChildProtected must always succeed because a new instance is used.
            // If this fails, it indicates a bug in the children management logic.
            throw new InvalidOperationException("Failed to add child.");
        }

        internal protected virtual ITreeChildNode<TLeafValue, TParentSelfValue> SpawnLeafProtected<TLeafValue>(TLeafValue? value)
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
        internal protected virtual IEnumerable<ITreeChildNode<TChildValue, TParentSelfValue>> GetChildrenProtected<TChildValue>()
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
        internal protected virtual IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> GetReadOnlyChildren<TChildValue>()
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
        , TreeNodeProtectedAbstractions.IReadOnlyParentNode<TRootSelfValue>
    {
        internal protected IChildrenManager<TRootSelfValue> ChildrenManagerProtected { get; }

        internal protected virtual ChildrenManager<TRootSelfValue> CreateChildrenManager()
        {
            return new ChildrenManager<TRootSelfValue>(this);
        }

        internal protected TreeRootNode(TRootSelfValue? value) : base(value)
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

        IReadOnlyParentNode<TRootSelfValue> ITreeParentNode<TRootSelfValue>.AsReadOnlyWithParentType()
            => this;

        IEnumerable<IReadOnlyTreeChildNode<TChildValue, TRootSelfValue>> IReadOnlyParentNode<TRootSelfValue>.GetReadOnlyChildren<TChildValue>()
            => ChildrenManagerProtected.GetReadOnlyChildren<TChildValue>();
    }

    public class TreeNodeInternal<TParentSelfValue, TValueOfMyParent>
        : TreeChildNode<TParentSelfValue, TValueOfMyParent>
        , TreeNodeProtectedAbstractions.ITreeIntermediateNode<TParentSelfValue, TValueOfMyParent>
        , TreeNodeProtectedAbstractions.IReadOnlyParentNode<TParentSelfValue>
    {
        internal protected IChildrenManager<TParentSelfValue> ChildrenManagerProtected { get; }

        internal protected virtual ChildrenManager<TParentSelfValue> CreateChildrenManager()
        {
            return new ChildrenManager<TParentSelfValue>(this);
        }

        protected internal TreeNodeInternal(TreeNodeHavingValue<TValueOfMyParent> parent, TParentSelfValue? value) : base(parent, value)
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

        IReadOnlyParentNode<TParentSelfValue> ITreeParentNode<TParentSelfValue>.AsReadOnlyWithParentType()
            => this;

        IEnumerable<IReadOnlyTreeChildNode<TChildValue, TParentSelfValue>> IReadOnlyParentNode<TParentSelfValue>.GetReadOnlyChildren<TChildValue>()
            => ChildrenManagerProtected.GetReadOnlyChildren<TChildValue>();
    }
}

namespace Boostable.OCPOrientedTree.Simple
{
    public interface IReadOnlySimpleTreeNode<TSelfValue>
    {
        TSelfValue? Value { get; }
        IEnumerable<ISimpleTreeNode<TChildValue, TSelfValue>> GetRaadOnlyChildren<TChildValue>();
    }

    public interface ISimpleTreeNode<TSelfValue>
    {
        TSelfValue? Value { get; set; }
        bool TrySpawn<TChildValue>(out ISimpleTreeNode<TChildValue, TSelfValue>? spawned, TChildValue? value = default);
        IEnumerable<ISimpleTreeNode<TChildValue, TSelfValue>> GetChildren<TChildValue>();
        IEnumerable<ISimpleTreeNode<TChildValue, TSelfValue>> GetRaadOnlyChildren<TChildValue>();
        IReadOnlySimpleTreeNode<TSelfValue> AsReadOnly();
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

    internal class SimpleTreeNode<TSelfValue> : ISimpleTreeNode<TSelfValue>, IReadOnlySimpleTreeNode<TSelfValue>
    {
        internal TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue> NodeInternal { get; }
        public TSelfValue? Value { 
            get => NodeInternal.Value;
            set => NodeInternal.Value = value;
        }

        internal SimpleTreeNode(TreeNodeProtectedAbstractions.ITreeNodeHavingValue<TSelfValue> nodeInternal)
        {
            NodeInternal = nodeInternal ?? throw new ArgumentNullException(nameof(NodeInternal));
        }
        public bool TrySpawn<TChildValue>(out ISimpleTreeNode<TChildValue, TSelfValue>? spawned, TChildValue? value = default)
        {
            if (NodeInternal is TreeNodeProtectedAbstractions.ITreeParentNode<TSelfValue> parentNode)
            {
                var spawnedInternal = parentNode.Spawn(value);
                spawned = new SimpleTreeNode<TChildValue, TSelfValue>(spawnedInternal);
                return true;
            }
            spawned = default;
            return false;
        }

        public IEnumerable<ISimpleTreeNode<TChildValue, TSelfValue>> GetChildren<TChildValue>()
        {
            lock (CacheSyncLock)
            {
                if (NodeInternal is not TreeNodeProtectedAbstractions.ITreeParentNode<TSelfValue> parentNode)
                    return Enumerable.Empty<ISimpleTreeNode<TChildValue, TSelfValue>>();

                var cached = ChildrenCacheRoot.GetOrAdd(
                    typeof(TChildValue),
                    _ => new ConcurrentDictionary<TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue>, object>()
                );

                return parentNode
                    .GetChildren<TChildValue>()
                    .Select(child =>
                        (ISimpleTreeNode<TChildValue, TSelfValue>)
                        cached.GetOrAdd(child, c =>
                            new SimpleTreeNode<TChildValue, TSelfValue>(
                                (TreeNodeProtectedAbstractions.ITreeChildNode<TChildValue, TSelfValue>)c
                            )
                        )
                    )
                    .ToArray();
            }
        }

        private object CacheSyncLock { get; } = new();

        private ConcurrentDictionary<
            Type,
            ConcurrentDictionary<TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue>, object>
        > ChildrenCacheRoot { get; } = new();

        public IEnumerable<ISimpleTreeNode<TChildValue, TSelfValue>> GetRaadOnlyChildren<TChildValue>()
        {
            lock (ReadOnlyCacheSyncLock)
            {
                if (NodeInternal is not TreeNodeProtectedAbstractions.ITreeParentNode<TSelfValue> parentNode)
                    return Enumerable.Empty<ISimpleTreeNode<TChildValue, TSelfValue>>();

                var cached = ReadOnlyChildrenCacheRoot.GetOrAdd(
                    typeof(TChildValue),
                    _ => new ConcurrentDictionary<TreeNodeProtectedAbstractions.IReadOnlyTreeChildNode<TSelfValue>, object>()
                );

                return parentNode
                    .GetReadOnlyChildren<TChildValue>()
                    .Select(child =>
                        (ISimpleTreeNode<TChildValue, TSelfValue>)
                        cached.GetOrAdd(child, c =>
                            new SimpleTreeNode<TChildValue, TSelfValue>(
                                (TreeNodeProtectedAbstractions.ITreeChildNode<TChildValue, TSelfValue>)c
                            )
                        )
                    )
                    .ToArray();
            }
        }
        private object ReadOnlyCacheSyncLock { get; } = new();

        private ConcurrentDictionary<
            Type,
            ConcurrentDictionary<TreeNodeProtectedAbstractions.IReadOnlyTreeChildNode<TSelfValue>, object>
        > ReadOnlyChildrenCacheRoot { get; } = new();

        public override bool Equals(object? obj)
        {
            return obj is SimpleTreeNode<TSelfValue> other &&
                   ReferenceEquals(NodeInternal, other.NodeInternal);
        }

        public override int GetHashCode()
        {
            return NodeInternal?.GetHashCode() ?? 0;
        }

        public IReadOnlySimpleTreeNode<TSelfValue> AsReadOnly()
            => this;
    }

    internal sealed class SimpleTreeNode<TSelfValue, TParentValue>
        : SimpleTreeNode<TSelfValue>
        , ISimpleTreeNode<TSelfValue, TParentValue>, IReadOnlySimpleTreeNode<TSelfValue, TParentValue>
    {
        public ISimpleTreeNode<TParentValue> Parent { get; }

        internal SimpleTreeNode(TreeNodeProtectedAbstractions.ITreeChildNode<TSelfValue, TParentValue> nodeInternal)
            : base(nodeInternal)
        {
            Parent = new SimpleTreeNode<TParentValue>(nodeInternal.Parent);
        }

        public override bool Equals(object? obj)
        {
            return obj is SimpleTreeNode<TSelfValue, TParentValue> other &&
                   ReferenceEquals(NodeInternal, other.NodeInternal);
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
            return new SimpleTreeNode<TRootSelfValue>(new TreeRootNode<TRootSelfValue>(value));
        }
    }
}
