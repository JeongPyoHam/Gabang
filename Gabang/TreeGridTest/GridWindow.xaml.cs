using System;
using System.Collections;
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

        public static int RowCount = 1000;
        public static int ColumnCount = 1000;

        private DynamicGridDataSource _dataSource;

        private Page2DManager<GridItem> _pageManager;
        private PageManager<string> _rowPageManager;
        private PageManager<string> _columnPageManager;

        public GridWindow() {
            InitializeComponent();

            _rowPageManager = new PageManager<string>(
                new HeaderProvider(RowCount, true),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            _columnPageManager = new PageManager<string>(
                new HeaderProvider(ColumnCount, true),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            _pageManager = new Page2DManager<GridItem>(
                new ItemsProvider(RowCount, ColumnCount),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            this.VGrid.ItemsSource = _dataSource = new DynamicGridDataSource(_pageManager);

            this.VGrid.RowHeaderSource = new DelegateList<PageItem<string>>(0, (i) => _rowPageManager.GetItem(i), _rowPageManager.Count);
            this.VGrid.ColumnHeaderSource = new DelegateList<PageItem<string>>(0, (i) => _columnPageManager.GetItem(i), _columnPageManager.Count);
        }

        private void ResetCollection_Click(object sender, RoutedEventArgs e) {
            //_dataSource.RaiseReplace();
        }
    }
}
