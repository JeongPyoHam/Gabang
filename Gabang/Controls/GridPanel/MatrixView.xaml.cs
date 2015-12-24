using System;
using System.Collections.Generic;
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

namespace Gabang.Controls {
    public partial class MatrixView : UserControl {

        private GridPoints _gridPoints;

        public MatrixView() {
            InitializeComponent();
        }

        private void Initialize() {
            _gridPoints = new GridPoints(RowCount, ColumnCount);

            RowHeader.RowCount = RowCount;
            RowHeader.ColumnCount = 1;
            RowHeader.Points = _gridPoints;
            RowHeader.DataProvider = DataProvider;

            ColumnHeader.RowCount = 1;
            ColumnHeader.ColumnCount = ColumnCount;
            ColumnHeader.Points = _gridPoints;
            ColumnHeader.DataProvider = DataProvider;

            Data.RowCount = RowCount;
            Data.ColumnCount = ColumnCount;
            Data.Points = _gridPoints;
            Data.DataProvider = DataProvider;
        }

        private IGridProvider<string> _dataProvider;
        public IGridProvider<string> DataProvider {
            get {
                return _dataProvider;
            }
            set {
                _dataProvider = value;
                Initialize();
            }
        }

        public int RowCount {
            get {
                return _dataProvider.RowCount;
            }
        }

        public int ColumnCount {
            get {
                return _dataProvider.ColumnCount;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            _gridPoints.OnViewportChanged(ViewportChangeType.SizeChange);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            Point pt = e.GetPosition(this);

            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            if (result.VisualHit is TextVisual) {
                var textVisual = (TextVisual)result.VisualHit;
                textVisual.ToggleHighlight();

                e.Handled = true;
            }

            if (!e.Handled) {
                base.OnMouseLeftButtonDown(e);
            }
        }

        private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            _gridPoints.VerticalOffset = e.NewValue;
        }

        private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            _gridPoints.HorizontalOffset = e.NewValue;
        }
    }
}
