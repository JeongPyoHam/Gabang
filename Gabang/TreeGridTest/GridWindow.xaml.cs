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
    /// Interaction logic for GridWindow.xaml
    /// </summary>
    public partial class GridWindow : Window {
        public GridWindow() {
            InitializeComponent();

            this.VGrid.ItemsSource = new GridDataSource(10, 10);
        }
    }
}
