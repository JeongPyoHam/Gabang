using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Gabang.Collection;

namespace GabangCollection
{
    public class TreeNodeCollection : CollectionViewSource
    {
        private readonly TreeToListCollection _treeToList;

        public TreeNodeCollection(ObservableTreeNode rootNode)
        {
            _treeToList = new TreeToListCollection(rootNode);
            this.Source = _treeToList;
            this.Filter += CollectionView_Filter;

            _treeToList.View = this;
        }

        internal void Refresh()
        {
            this.Filter -= CollectionView_Filter;
            this.Filter += CollectionView_Filter;
        }

        private static void CollectionView_Filter(object sender, FilterEventArgs e)
        {
            var node = e.Item as ObservableTreeNode;
            if (node != null)
            {
                e.Accepted = (node.Visibility == Visibility.Visible);
            }
            else
            {
                e.Accepted = false;
            }
        }
    }

    class TreeToListCollection : ObservableCollection<ObservableTreeNode>
    {
        public TreeToListCollection(ObservableTreeNode rootNode)
        {
            rootNode.PropertyChanged += Node_PropertyChanged;
            rootNode.CollectionChanged += Root_CollectionChanged;

            this.AddNode(rootNode);
        }

        public TreeNodeCollection View { get; set; }

        void AddNode(ObservableTreeNode node)
        {
            node.PropertyChanged += Node_PropertyChanged;
            Add(node);
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    AddNode(child);
                }
            }
        }

        private void Root_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int insertIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems)
                    {
                        var node = (ObservableTreeNode)item;
                        node.PropertyChanged += Node_PropertyChanged;

                        this.Insert(insertIndex, node);
                        insertIndex++;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        this[e.OldStartingIndex].PropertyChanged -= Node_PropertyChanged;
                        this.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException();
            }
        }

        private bool Filter(object item)
        {
            var node = item as ObservableTreeNode;
            if (node != null)
            {
                return node.Visibility == Visibility.Visible;
            }
            return false;
        }

        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExpanded")
            {
                View?.Refresh();
            }
        }
    }
}
