using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="node">Model for this node</param>
        public ObservableTreeNode(INode node, bool startUpdatingChildren = false)
        {
            ResetCount();

            Parent = null;
            Content = node.Content;
            Model = node;

            if (startUpdatingChildren)
            {
                StartUpdatingChildren().DoNotWait();
            }
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
                foreach (var child in Children)
                {
                    SetNodeVisibility(child, value);
                }

                SetProperty<bool>(ref _isExpanded, value);

                if (_isExpanded)
                {
                    // TODO: more intelligently when huge number of children
                    foreach (var child in ChildrenInternal)
                    {
                        child.StartUpdatingChildren().DoNotWait();
                    }
                }
            }
        }

        private Visibility _visibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of this node
        /// </summary>
        public Visibility Visibility
        {
            get { return _visibility; }
            set { SetProperty<Visibility>(ref _visibility, value); }
        }

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
        public int Count { get; private set; }

        private object _content;
        /// <summary>
        ///  value  contained in this node
        /// </summary>
        public object Content
        {
            get { return _content; }
            set { SetProperty<object>(ref _content, value); }
        }

        private object _errorContent;
        /// <summary>
        /// Content when any error happens internally
        /// </summary>
        public object ErrorContent
        {
            get { return _errorContent; }
            set { SetProperty(ref _errorContent, value); }
        }

        /// <summary>
        /// Insert a direct child node at given position
        /// </summary>
        /// <param name="index">child position, relative to direct Children</param>
        /// <param name="item">tree node</param>
        public virtual void InsertChildAt(int index, ObservableTreeNode item)
        {
            item.Parent = this;
            SetNodeVisibility(item, this.IsExpanded);

            int addedStartingIndex = AddUpChildCount(index);

            ChildrenInternal.Insert(index, item);
            SetHasChildren();

            item.CollectionChanged += Item_CollectionChanged;

            Count += item.Count;

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
        public virtual void AddChild(ObservableTreeNode item)
        {
            InsertChildAt(ChildrenInternal.Count, item);
        }

        /// <summary>
        /// remove a direct child node, and all children, of course
        /// </summary>
        /// <param name="index">direct child index</param>
        public virtual void RemoveChild(int index)
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

            Count -= toBeRemoved.Count;
            Debug.Assert(Count >= 1);

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
            int removedStartingIndex = AddUpChildCount(0);
            List<ObservableTreeNode> removedItems = new List<ObservableTreeNode>();
            foreach (var child in Children)
            {
                child.CollectionChanged -= Item_CollectionChanged;
                removedItems.Add(child);
            }

            ChildrenInternal.Clear();
            SetHasChildren();

            ResetCount();

            if (CollectionChanged != null)
            {
                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        removedItems,
                        removedStartingIndex));
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

        private INode _model;
        private INode Model
        {
            get { return _model; }
            set
            {
                _model = value;
                Content = _model == null ? null : _model.Content;
            }
        }

        private void ResetCount()
        {
            Count = 1;
        }

        private List<ObservableTreeNode> Linearize(ObservableTreeNode tree)
        {
            var linear = new List<ObservableTreeNode>();

            Traverse(tree, (n) => linear.Add(n));

            return linear;
        }

        protected void Traverse(ObservableTreeNode tree, Action<ObservableTreeNode> action)
        {
            action(tree);
            if (tree.HasChildren && tree.ChildrenInternal != null)
            {
                foreach (var child in tree.ChildrenInternal)
                {
                    Traverse(child, action);
                }
            }
        }

        protected void Traverse(
            ObservableTreeNode tree,
            Action<ObservableTreeNode> action,
            Func<ObservableTreeNode, bool> parentPredicate)
        {
            action(tree);
            if (tree.HasChildren && parentPredicate(tree) && tree.Children != null)
            {
                foreach (var child in tree.Children)
                {
                    Traverse(child, action, parentPredicate);
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
                    Count += e.NewItems.Count;
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
                    Count -= e.OldItems.Count;
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
                    Count -= deleted.Count;
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
            int count = 1;
            for (int i = 0; i < nodeIndex; i++)
            {
                count += Children[i].Count;
            }
            return count;
        }

        private void SetNodeVisibility(ObservableTreeNode node, bool expanded)
        {
            Visibility childVisibility = expanded ? Visibility.Visible : Visibility.Collapsed;

            Traverse(
                node,
                (c) => c.Visibility = childVisibility,
                (p) => p.IsExpanded);
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

        private async Task StartUpdatingChildren()
        {
            if (Model == null)
            {
                return;
            }

            try
            {
                var nodes = await Model.GetChildrenAsync(CancellationToken.None);

                if (nodes != null)
                {
                    UpdateChildren(nodes);
                }
            }
            catch (Exception e)
            {
                SetStatus("Errot at enumerating members", e);   // TODO: move to resource, or bypass the exception message
            }
        }

        private void SetStatus(string message, Exception e)
        {
            ErrorContent = new Exception(message, e);
        }

        private void UpdateChildren(IList<INode> update)
        {
            int srcIndex = 0;
            int updateIndex = 0;

            while (srcIndex < ChildrenInternal.Count)
            {
                int sameUpdateIndex = -1;
                for (int u = updateIndex; u < update.Count; u++)
                {
                    if (ChildrenInternal[srcIndex].Model.IsSame(update[u]))
                    {
                        sameUpdateIndex = u;
                        break;
                    }
                }

                if (sameUpdateIndex != -1)
                {
                    int insertIndex = srcIndex;
                    for (int i = updateIndex; i < sameUpdateIndex; i++)
                    {
                        InsertChildAt(insertIndex++, new ObservableTreeNode(update[i]));
                        srcIndex++;
                    }

                    ChildrenInternal[srcIndex].Model = update[sameUpdateIndex];
                    srcIndex++;

                    updateIndex = sameUpdateIndex + 1;
                }
                else
                {
                    RemoveChild(srcIndex);
                }
            }

            if (updateIndex < update.Count)
            {
                Debug.Assert(srcIndex == ChildrenInternal.Count);

                int insertIndex = srcIndex;
                for (int i = updateIndex; i < update.Count; i++)
                {
                    InsertChildAt(insertIndex++, new ObservableTreeNode(update[i]));
                }
            }
        }

        #endregion
    }
}
