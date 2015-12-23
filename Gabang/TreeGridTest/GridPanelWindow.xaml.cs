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

namespace Gabang.TreeGridTest {
    /// <summary>
    /// Interaction logic for GridPanelWindow.xaml
    /// </summary>
    public partial class GridPanelWindow : Window {
        public GridPanelWindow() {
            InitializeComponent();
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
}
