using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            var tree = ObservableDataTreeNode.CreateParentNode(
                new Variable() {
                    VariableName = "1",
                    VariableValue = "v1",
                    TypeName = "t1",
                    Sub = new SubVariable() { X = "sub1" }, },
                CreateVariables);

            var collection = new TreeNodeCollection(tree);
            varGrid.ItemsSource = collection.View;
        }

        Task<List<object>> CreateVariables(object parent)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(10);
                return CreateVariablesInternal(parent);
            });
        }

        List<object> CreateVariablesInternal(object parent)
        {
            List<object> instances = new List<object>();
            for (int i = 0; i < 10; i++)
            {
                string name = ((Variable)parent).VariableName + i.ToString();
                var newOne = new Variable()
                {
                    VariableName = name,
                    VariableValue = "v" + name,
                    TypeName = "t" + name,
                    Sub = new SubVariable() { X = "sub" + name },
                };

                instances.Add(newOne);
            }

            return instances;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
