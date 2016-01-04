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
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            if (e.Delta > 0 || e.Delta < 0) {
                Data.EnqueueCommand(ScrollType.MouseWheel, e.Delta);
                e.Handled = true;
            }

            if (!e.Handled) {
                base.OnMouseWheel(e);
            }
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
            switch (e.ScrollEventType) {
                case ScrollEventType.EndScroll:
                    Data.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.First:
                    Data.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.LargeDecrement:
                    Data.EnqueueCommand(ScrollType.PageUp, e.NewValue);
                    break;
                case ScrollEventType.LargeIncrement:
                    Data.EnqueueCommand(ScrollType.PageDown, e.NewValue);
                    break;
                case ScrollEventType.Last:
                    Data.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.SmallDecrement:
                    Data.EnqueueCommand(ScrollType.LineUp, e.NewValue);
                    break;
                case ScrollEventType.SmallIncrement:
                    Data.EnqueueCommand(ScrollType.LineDown, e.NewValue);
                    break;
                case ScrollEventType.ThumbPosition:
                    Data.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.ThumbTrack:
                    Data.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                default:
                    break;
            }
        }

        private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            switch (e.ScrollEventType) {
                case ScrollEventType.EndScroll:
                    Data.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.First:
                    Data.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.LargeDecrement:
                    Data.EnqueueCommand(ScrollType.PageLeft, e.NewValue);
                    break;
                case ScrollEventType.LargeIncrement:
                    Data.EnqueueCommand(ScrollType.PageRight, e.NewValue);
                    break;
                case ScrollEventType.Last:
                    Data.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.SmallDecrement:
                    Data.EnqueueCommand(ScrollType.LineLeft, e.NewValue);
                    break;
                case ScrollEventType.SmallIncrement:
                    Data.EnqueueCommand(ScrollType.LineRight, e.NewValue);
                    break;
                case ScrollEventType.ThumbPosition:
                    Data.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.ThumbTrack:
                    Data.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                default:
                    break;
            }
        }
    }
}
