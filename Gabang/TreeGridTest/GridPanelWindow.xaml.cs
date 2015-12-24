using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for GridPanelWindow.xaml
    /// </summary>
    public partial class GridPanelWindow : Window {
        public GridPanelWindow() {
            InitializeComponent();

            RootGrid.DataProvider = new DataProvider(50, 60);
        }

        Stopwatch _stopWatch;

        private void Refresh_Click(object sender, RoutedEventArgs e) {
            _stopWatch = Stopwatch.StartNew();
//            RootGrid.RefreshChildren();

            Trace.WriteLine(string.Format("GridPanel:RefreshClick:{0}", _stopWatch.ElapsedMilliseconds));
            _stopWatch.Reset();
            _stopWatch.Start();

            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
        }

        private void RootGrid_LayoutUpdated(object sender, EventArgs e) {
            Trace.WriteLine(string.Format("GridPanel:LayoutUpdated:{0}", _stopWatch.ElapsedMilliseconds));
            RootGrid.LayoutUpdated -= RootGrid_LayoutUpdated;
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            _stopWatch = Stopwatch.StartNew();

//            RootGrid.AddChileren();

            Trace.WriteLine(string.Format("GridPanel:AddClick:{0}", _stopWatch.ElapsedMilliseconds));
            _stopWatch.Reset();
            _stopWatch.Start();

            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
        }

        private void HorizontalOffsetBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(HorizontalOffsetBox.Text)) return;

            double horizontalOffset;
            if (double.TryParse(HorizontalOffsetBox.Text, out horizontalOffset)) {
//                RootGrid.HorizontalOffset = horizontalOffset;
            }
        }
    }

    class DataProvider : IGridProvider<string> {
        public DataProvider(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            return Task.Run(async () => {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                return (IGrid<string>)new Grid<string>(gridRange, (r, c) => string.Format("{0}:{1}", r, c));
            });
        }
    }
}
