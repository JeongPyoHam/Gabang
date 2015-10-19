using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Gabang.Controls;

namespace Gabang.TreeGridTest
{
    /// <summary>
    /// Interaction logic for TreeGridWindow.xaml
    /// </summary>
    public partial class TreeGridWindow : Window
    {
        public TreeGridWindow()
        {
            InitializeComponent();

            Populate();

            RootGrid.ItemsSource = Items;
            RootGrid.DataContext = new Variable() { VariableName = "x", VariableValue = "y", TypeName = "t1", Sub = new SubVariable() { X = "sub1" }, };
        }

        ObservableCollection<TreeGrid> Items = new ObservableCollection<TreeGrid>();

        void Populate()
        {
            var item1 = new TreeGrid(RootGrid.Columns);
            item1.Content = new Variable() { VariableName = "1", VariableValue = "v1", TypeName = "t1", Sub = new SubVariable() { X = "sub1" }, };

            Items.Add(item1);
            //Items.Add(
            //    new Variable() { VariableName = "1", VariableValue = "v1", TypeName = "t1", Sub = new SubVariable() { X = "sub1" }, });
            //Items.Add(new Variable() { VariableName = "11", VariableValue = "v11", TypeName = "t11", Sub = new SubVariable() { X = "sub11" }, });
            //Items.Add(new Variable() { VariableName = "12", VariableValue = "v12", TypeName = "t12", Sub = new SubVariable() { X = "sub12" }, });
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (object.Equals(sender, this.TestButton))
            {
                //RootGrid.ItemsVisibility = Visibility.Collapsed;
                
                //var found = GetChild(RootGrid, "ItemsPresenter");
                //var itemsPresenter = found as ItemsPresenter;
                //itemsPresenter.Visibility = Visibility.Collapsed;
            }
            if (object.Equals(sender, this.TestButton1))
            {
                //RootGrid.ItemsVisibility = Visibility.Visible;
                
                //var found = GetChild(RootGrid, "ItemsPresenter");
                //var itemsPresenter = found as ItemsPresenter;
                //itemsPresenter.Visibility = Visibility.Visible;
            }
        }

        DependencyObject GetChild(DependencyObject reference, Type type)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(reference, i);
                if (child.GetType() == type)
                {
                    return child;
                }

                var found = GetChild(child, type);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        DependencyObject GetChild(DependencyObject reference, string name)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(reference, i);
                var element = child as FrameworkElement;
                if(element != null)
                {
                    if (element.Name == name) return child;
                }

                var found = GetChild(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
    }
}
