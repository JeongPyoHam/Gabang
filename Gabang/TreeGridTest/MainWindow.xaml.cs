using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gabang.Controls;
using GabangCollection;

namespace TreeGridTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Populate();
        }

        ObservableCollection<ObservableTreeNode> _linearized = new ObservableCollection<ObservableTreeNode>();

        void Populate()
        {
            var tree = new ObservableTreeNode(
                new Variable() { VariableName = "1", VariableValue = "v1", TypeName = "t1", Sub = new SubVariable() { X = "sub1" }, });
            tree.CollectionChanged += Target_CollectionChanged;

            _linearized.Add(tree);

            tree.AddChild(new ObservableTreeNode(new Variable() { VariableName = "11", VariableValue = "v11", TypeName = "t11" , Sub = new SubVariable() { X = "sub11" }, }, false));
            tree.AddChild(new ObservableTreeNode(new Variable() { VariableName = "12", VariableValue = "v12", TypeName = "t12" , Sub = new SubVariable() { X = "sub12" }, }, false));
            
            this.varGrid.ItemsSource = _linearized;
        }

        private void Target_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int insertIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems)
                    {
                        _linearized.Insert(insertIndex, (ObservableTreeNode)item);
                        insertIndex++;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        _linearized.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
