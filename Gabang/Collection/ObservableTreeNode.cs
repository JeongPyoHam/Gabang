using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GabangCollection
{
    /// <summary>
    /// Tree node that supposts <see cref="INotifyCollectionChanged"/>
    /// This node notifies collection change in linear order including children.
    /// </summary>
    /// <typeparam name="T">Value for tree node</typeparam>
    public class ObservableTreeNode<T> : INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region factory

        /// <summary>
        /// create new instance of <see cref="ObservableTreeNode{T}"/>, usually root node
        /// </summary>
        /// <param name="nodeValue">Value for the node</param>
        /// <param name="canHaveChildren">flag used to mark leaf node or not</param>
        /// <param name="selfInCollection">flag to set collection notification index include self, Useful for root node</param>
        public ObservableTreeNode(
            T nodeValue,
            bool canHaveChildren = true,
            bool selfInCollection = true)
        {
            Parent = null;
            Content = nodeValue;
            HasChildren = canHaveChildren;
            SelfInCollection = selfInCollection;
            ResetTotalNodeCount();

            if (HasChildren)
            {
                _children = new List<ObservableTreeNode<T>>();
            }
        }

        #endregion

        #region public

        private bool _hasChildren;
        /// <summary>
        /// true for non-leaf node, false for leaf node
        /// </summary>
        public bool HasChildren
        {
            get { return _hasChildren; }
            set { SetProperty<bool>(ref _hasChildren, value); }
        }

        private bool _isExpanded = false;
        /// <summary>
        /// Indicate this node expand to show children
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                SetProperty<bool>(ref _isExpanded, value);

                foreach (var child in Children)
                {
                    SetChildVisibility(child);
                }
            }
        }

        private void SetChildVisibility(ObservableTreeNode<T> child)
        {
            Visibility childVisibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;

            Traverse(child, (c) => c.Visibility = childVisibility);
        }

        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get { return _visibility; }
            set { SetProperty<Visibility>(ref _visibility, value); }
        }

        /// <summary>
        /// Flag that indicates if collection changed notificaiton index includes self
        /// If true, child index starts at 1. If flase, child index starts at 0
        /// </summary>
        public bool SelfInCollection { get; }

        public ObservableTreeNode<T> Parent { get; private set; }

        public int Level
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }
                return Parent.Level + 1;
            }
        }

        List<ObservableTreeNode<T>> _children;
        /// <summary>
        /// Direct children of this node
        /// </summary>
        public IReadOnlyList<ObservableTreeNode<T>> Children
        {
            get
            {
                return _children;
            }
        }

        /// <summary>
        /// the number of node including all children and itself
        /// </summary>
        public int TotalNodeCount { get; private set; }

        private T _content;
        /// <summary>
        ///  value  contained in this node
        /// </summary>
        public T Content
        {
            get { return _content; }
            set { SetProperty<T>(ref _content, value); }
        }


        /// <summary>
        /// Insert a direct child node at given position
        /// </summary>
        /// <param name="index">child position, relative to direct Children</param>
        /// <param name="item">tree node</param>
        public void InsertChildAt(int index, ObservableTreeNode<T> item)
        {
            if (!HasChildren)
            {
                throw new InvalidOperationException("This tree node can not have children");
            }

            item.Parent = this;
            SetChildVisibility(item);


            int addedStartingIndex = AddUpChildCount(index);

            _children.Insert(index, item);
            item.CollectionChanged += Item_CollectionChanged;

            TotalNodeCount += item.TotalNodeCount;

            if (CollectionChanged != null)
            {
                IList addedItems = Linearize(item);

                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        addedItems,
                        addedStartingIndex));
            }
        }

        /// <summary>
        /// add a direct child node
        /// </summary>
        /// <param name="item">a node to be added</param>
        public void AddChild(ObservableTreeNode<T> item)
        {
            if (!HasChildren)
            {
                throw new InvalidOperationException("This tree node can not have children");
            }

            InsertChildAt(_children.Count, item);
        }

        /// <summary>
        /// remove a direct child node, and all children, of course
        /// </summary>
        /// <param name="index">direct child index</param>
        public void RemoveChild(int index)
        {
            int removedStartingIndex = AddUpChildCount(index);

            ObservableTreeNode<T> toBeRemoved = _children[index];
            toBeRemoved.CollectionChanged -= Item_CollectionChanged;
            _children.RemoveAt(index);

            TotalNodeCount -= toBeRemoved.TotalNodeCount;
            Debug.Assert(TotalNodeCount >= (SelfInCollection ? 1 : 0));

            if (CollectionChanged != null)
            {
                IList removedItems = Linearize(toBeRemoved);

                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        removedItems,
                        removedStartingIndex));
            }
        }

        /// <summary>
        /// clear direct children
        /// </summary>
        public void RemoveAllChildren()
        {
            foreach (var child in _children)
            {
                child.CollectionChanged -= Item_CollectionChanged;
            }
            _children.Clear();
            ResetTotalNodeCount();

            if (CollectionChanged != null)
            {
                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #endregion

        #region INotifyCollectionChanged

        /// <summary>
        /// notification for adding and removing child node
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual bool SetProperty<U>(ref U storage, U value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region private

        private void ResetTotalNodeCount()
        {
            TotalNodeCount = SelfInCollection ? 1 : 0;
        }

        private List<ObservableTreeNode<T>> Linearize(ObservableTreeNode<T> tree)
        {
            var linear = new List<ObservableTreeNode<T>>();

            Traverse(tree, (n) => linear.Add(n));

            return linear;
        }

        private void Traverse(ObservableTreeNode<T> tree, Action<ObservableTreeNode<T>> action)
        {
            action(tree);
            if (tree.HasChildren && tree._children != null)
            {
                foreach (var child in tree._children)
                {
                    Traverse(child, action);
                }
            }
        }

        private void Item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int nodeIndex = -1;
            var node = sender as ObservableTreeNode<T>;
            if (node != null)
            {
                nodeIndex = _children.IndexOf(node);
            }
            if (node == null || nodeIndex == -1)
            {
                throw new ArgumentException("CollectionChanged is rasied with wrong sender");
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    TotalNodeCount += e.NewItems.Count;
                    if (CollectionChanged != null)
                    {
                        int nodeStartIndex = AddUpChildCount(nodeIndex);

                        CollectionChanged(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Add,
                                e.NewItems, e.NewStartingIndex + nodeStartIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    TotalNodeCount -= e.OldItems.Count;
                    if (CollectionChanged != null)
                    {
                        int nodeStartIndex = AddUpChildCount(nodeIndex);

                        CollectionChanged(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                e.OldItems, e.OldStartingIndex + nodeStartIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var deleted = Linearize(node);
                    TotalNodeCount -= deleted.Count;
                    if (CollectionChanged != null)
                    {
                        CollectionChanged(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                deleted,
                                nodeIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException("ObservableTreeNode doesn't support Replace or Move item");
            }
        }

        /// <summary>
        /// Returns all descendant nodes upto given direct child's index (open ended)
        /// </summary>
        /// <param name="nodeIndex">open ended upper boundary to which cound is added up</param>
        /// <returns>number of total children nodes</returns>
        private int AddUpChildCount(int nodeIndex)
        {
            int count = SelfInCollection ? 1 : 0;
            for (int i = 0; i < nodeIndex; i++)
            {
                count += _children[i].TotalNodeCount;
            }
            return count;
        }

        #endregion
    }
}
