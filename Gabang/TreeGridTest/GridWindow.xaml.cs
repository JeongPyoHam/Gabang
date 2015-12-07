﻿using System;
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

        private HeaderProvider _rowHeaderSource;
        private HeaderProvider _columnHeaderSource;

        private Page2DManager<GridItem> _pageManager;

        public GridWindow() {
            InitializeComponent();

            _rowHeaderSource = new HeaderProvider(RowCount, true);
            _columnHeaderSource = new HeaderProvider(ColumnCount, false);

            _pageManager = new Page2DManager<GridItem>(
                new ItemsProvider(RowCount, ColumnCount),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            this.VGrid.ItemsSource = _dataSource = new DynamicGridDataSource(_pageManager);
        }

        private void ResetCollection_Click(object sender, RoutedEventArgs e) {
            //_dataSource.RaiseReplace();
        }
    }
}
