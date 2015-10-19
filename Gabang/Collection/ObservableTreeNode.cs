﻿using System;
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
    public class ObservableTreeNode : INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region factory

        /// <summary>
        /// create new instance of <see cref="ObservableTreeNode"/>, usually root node
        /// </summary>
        /// <param name="nodeValue">Value for the node</param>
        /// <param name="selfInCollection">flag to set collection notification index include self, Useful for root node</param>
        public ObservableTreeNode(
            object nodeValue,
            bool selfInCollection = true)
        {
            Parent = null;
            Content = nodeValue;
            SelfInCollection = selfInCollection;
            ResetTotalNodeCount();
        }

        #endregion

        #region public/protected

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

        private Visibility _visibility = Visibility.Visible;
        /// <summary>
        /// Visibility of this node
        /// </summary>
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

        /// <summary>
        /// parent node, null if root
        /// </summary>
        protected ObservableTreeNode Parent { get; private set; }

        /// <summary>
        /// Depth from the root. Root has 1
        /// </summary>
        public int Depth
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }
                return Parent.Depth + 1;
            }
        }

        List<ObservableTreeNode> _children;
        /// <summary>
        /// Direct children of this node
        /// </summary>
        public IReadOnlyList<ObservableTreeNode> Children
        {
            get{ return ChildrenInternal; }
        }

        protected List<ObservableTreeNode> ChildrenInternal
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<ObservableTreeNode>();
                }
                return _children;
            }
        }

        /// <summary>
        /// the number of node including all children and itself
        /// </summary>
        public int TotalNodeCount { get; private set; }

        private object _content;
        /// <summary>
        ///  value  contained in this node
        /// </summary>
        public object Content
        {
            get { return _content; }
            set { SetProperty<object>(ref _content, value); }
        }

        /// <summary>
        /// Insert a direct child node at given position
        /// </summary>
        /// <param name="index">child position, relative to direct Children</param>
        /// <param name="item">tree node</param>
        public void InsertChildAt(int index, ObservableTreeNode item)
        {
            item.Parent = this;
            SetChildVisibility(item);
            SetHasChildren();

            int addedStartingIndex = AddUpChildCount(index);

            ChildrenInternal.Insert(index, item);
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
        public void AddChild(ObservableTreeNode item)
        {
            InsertChildAt(ChildrenInternal.Count, item);
        }

        /// <summary>
        /// remove a direct child node, and all children, of course
        /// </summary>
        /// <param name="index">direct child index</param>
        public void RemoveChild(int index)
        {
            if (!HasChildren)
            {
                throw new ArgumentException("No child node to remove");
            }

            int removedStartingIndex = AddUpChildCount(index);

            ObservableTreeNode toBeRemoved = Children[index];
            toBeRemoved.CollectionChanged -= Item_CollectionChanged;
            ChildrenInternal.RemoveAt(index);

            SetHasChildren();

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
            foreach (var child in Children)
            {
                child.CollectionChanged -= Item_CollectionChanged;
            }
            ChildrenInternal.Clear();
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

        private List<ObservableTreeNode> Linearize(ObservableTreeNode tree)
        {
            var linear = new List<ObservableTreeNode>();

            Traverse(tree, (n) => linear.Add(n));

            return linear;
        }

        private void Traverse(ObservableTreeNode tree, Action<ObservableTreeNode> action)
        {
            action(tree);
            if (tree.HasChildren && tree.Children != null)
            {
                foreach (var child in tree.Children)
                {
                    Traverse(child, action);
                }
            }
        }

        private void Item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int nodeIndex = -1;
            var node = sender as ObservableTreeNode;
            if (node != null)
            {
                nodeIndex = ChildrenInternal.IndexOf(node);
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
                count += Children[i].TotalNodeCount;
            }
            return count;
        }

        private void SetChildVisibility(ObservableTreeNode child)
        {
            Visibility childVisibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;

            Traverse(child, (c) => c.Visibility = childVisibility);
        }

        private void SetHasChildren()
        {
            if (_children != null && _children.Count > 0)
            {
                HasChildren = true;
            }
            else
            {
                HasChildren = false;
            }
        }

        #endregion
    }
}
