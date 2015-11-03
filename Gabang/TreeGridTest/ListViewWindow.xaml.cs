using System;
using System.Collections.Generic;
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

namespace Gabang.TreeGridTest {
    /// <summary>
    /// Interaction logic for ListViewWindow.xaml
    /// </summary>
    public partial class ListViewWindow : Window {
        public ListViewWindow() {
            InitializeComponent();

            RootListView.ItemsSource = new GridDataSource(100, 100);
        }
    }

    public class ListViewItemEx : ListViewItem {
        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }
    }

    public class ListViewEx : ListView {
        protected override DependencyObject GetContainerForItemOverride() {
            return new ListViewItemEx();
        }
    }
}
