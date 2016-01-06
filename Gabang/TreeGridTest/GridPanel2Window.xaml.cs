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
    /// Interaction logic for GridPanel2Window.xaml
    /// </summary>
    public partial class GridPanel2Window : Window {
        public GridPanel2Window() {
            InitializeComponent();
        }

        Stopwatch _watch;
        private void Refresh_Click(object sender, RoutedEventArgs e) {
            RootGrid.InvalidateMeasure();
            RootGrid.InvalidateArrange();

            Trace.WriteLine("Layout: start");
            _watch = Stopwatch.StartNew();
            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
        }

        private void RootGrid_LayoutUpdated(object sender, EventArgs e) {
            Trace.WriteLine(string.Format("Layout: {0}", _watch.ElapsedMilliseconds));
            RootGrid.LayoutUpdated -= RootGrid_LayoutUpdated;
        }

        private void Experiment_Click(object sender, RoutedEventArgs e) {

            Trace.WriteLine("Experiment: start");
            _watch = Stopwatch.StartNew();

            RootGrid.Toggle();
            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
        }

        private void Transform_Click(object sender, RoutedEventArgs e) {

            Trace.WriteLine("Transform: start");
            _watch = Stopwatch.StartNew();

            RootGrid.ChangeText();
            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
        }
    }
}
