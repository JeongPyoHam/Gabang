using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
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
    class VariableModel : INode
    {
        Variable _variable;
        public VariableModel(string name)
        {
            if (name == "1110")
            {
                throw new Exception("Hello this is an exception");
            }

            _variable = new Variable()
            {
                VariableName = name,
                VariableValue = "v" + name,
                TypeName = "t" + name,
                Sub = new SubVariable() { X = "sub" + name },
            };
        }
        public object Content
        {
            get
            {
                return _variable;
            }

            set
            {
                _variable = (Variable)value;
            }
        }

        public Task<IList<INode>> GetChildrenAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return CreateVariablesInternal(this._variable);
            });
        }

        public bool IsSame(INode node)
        {
            return _variable.VariableName == ((Variable)node.Content).VariableName;
        }

        IList<INode> CreateVariablesInternal(Variable parent)
        {
            List<INode> instances = new List<INode>();
            for (int i = 0; i < 2; i++)
            {
                string name = parent.VariableName + i.ToString();

                instances.Add(new VariableModel(name));
            }

            return instances;
        }
    }

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
            var tree = new ObservableTreeNode(new VariableModel("1"), true);
            tree.Visibility = Visibility.Visible;

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
