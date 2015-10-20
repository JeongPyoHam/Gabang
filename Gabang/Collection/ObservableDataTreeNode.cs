using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GabangCollection
{
    [Flags]
    public enum DataTreeNodeState
    {
        Invalid         = 0x0,
        ValidData       = 0x1,
        ValidChildren   = 0x2,
        Error           = 0x8,
    }

    public interface INode
    {
        object Content { get; set; }

        bool IsSame(INode node);

        Task<IList<INode>> GetChildrenAsync(CancellationToken cancellationToken);
    }

    public class ObservableDataTreeNode : ObservableTreeNode
    {
        private Func<object, Task<List<object>>> queryChildren;

        private ObservableDataTreeNode(
            object nodeValue,
            DataTreeNodeState initialState)
            : base(null)
        {
            _state = initialState;

            base.PropertyChanged += Base_PropertyChanged;
        }

        private DataTreeNodeState _state;
        public DataTreeNodeState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        private Visibility _statusVisibility = Visibility.Collapsed;
        public Visibility StatusVisibility
        {
            get { return _statusVisibility; }
            set { SetProperty(ref _statusVisibility, value); }
        }

        public void Invalidate()
        {
            State = DataTreeNodeState.Invalid;

            foreach (var child in Children)
            {
                Traverse(
                    child,
                    (c) => ((ObservableDataTreeNode)c).State = DataTreeNodeState.Invalid);
            }
        }

        private void Base_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExpanded")
            {
                StartQueryChildren(this);
            }
        }

        public static ObservableDataTreeNode CreateParentNode(object nodeValue, Func<object, Task<List<object>>> query)
        {
            var newNode = new ObservableDataTreeNode(nodeValue, DataTreeNodeState.ValidData);
            newNode.HasChildren = true;
            newNode.queryChildren = query;

            return newNode;
        }

        private async void StartQueryChildren(ObservableDataTreeNode node)  // TODO: NoWait();
        {
            if (node.State.HasFlag(DataTreeNodeState.ValidChildren))
            {
                return;
            }

            if (queryChildren == null)
            {
                node.State = DataTreeNodeState.Error;
                return;
            }

            try
            {
                StatusVisibility = Visibility.Visible;

                Task<List<object>> queryChildrenTask = queryChildren(node.Content);
                var children = await queryChildrenTask;

                RemoveAllChildren();
                foreach (var child in children)
                {
                    node.AddChild(ObservableDataTreeNode.CreateParentNode(child, queryChildren));
                }
                node.State |= DataTreeNodeState.ValidChildren;

                StatusVisibility = Visibility.Collapsed;
            }
            catch
            {
                node.State = DataTreeNodeState.Error;
            }
        }
    }
}
