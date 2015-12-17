using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.TestApp {

    public class GridTextBox : TextBlock {
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class GridPanel : Panel {

        private double[] _xPositions;
        private double[] _yPositions;
        private double[] _width;
        private double[] _height;
        private Package.DataInspect.Grid<GridTextBox> _grid;

        public GridPanel() {
            MinWidth = 20.0;
            MinHeight = 20.0;
        }


        public int RowCount { get { return 50; } }

        public int ColumnCount { get { return 50; } }

        public void AddChileren() {
            var watch = Stopwatch.StartNew();

            InternalChildren.Clear();

            _grid = new Package.DataInspect.Grid<GridTextBox>(
                RowCount,
                RowCount,
                (r, c) => new GridTextBox() { Row = r, Column = c, Text = string.Format("{0}:{1}", r, c) });

            for (int r = 0; r < RowCount; r++) {
                for (int c = 0; c < ColumnCount; c++) {
                    InternalChildren.Add(_grid[r, c]);
                }
            }

            _xPositions = new double[RowCount];
            for (int i = 1; i < RowCount; i++) {
                _xPositions[i] = _xPositions[i - 1] + MinWidth;
            }

            _yPositions = new double[ColumnCount];
            for (int i = 1; i < RowCount; i++) {
                _yPositions[i] = _yPositions[i - 1] + MinHeight;
            }

            _width = new double[ColumnCount];
            _height = new double[RowCount];

            Trace.WriteLine(string.Format("{0}:Add", watch.ElapsedMilliseconds));
        }

        static int touchCount = 0;
        public void TouchChildren() {
            var watch = Stopwatch.StartNew();

            foreach (GridTextBox child in InternalChildren) {
                //child.Text = touchCount % 2 == 0 ? "aaa" : "bbb";
                child.Text = string.Format("{0}:{1}:{2}", child.Row, child.Column, touchCount);
            }
            touchCount++;

            Trace.WriteLine(string.Format("{0}:Touch", watch.ElapsedMilliseconds));
        }

        public double HorizontalOffset { get; set; }

        public double VerticalOffset { get; set; }

        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost) {
            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);

            if (newIsItemsHost) {
                throw new NotSupportedException($"{typeof(GridPanel)} doesn't support ItemsHost. It must stand alone");
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            if (InternalChildren.Count == 0) {
                return Size.Empty;
            }

            var watch = Stopwatch.StartNew();

            int rowIndex, columnIndex;

            FindFirstIndex(HorizontalOffset, VerticalOffset, out rowIndex, out columnIndex);

            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (GridTextBox child in InternalChildren) {
                child.Measure(infiniteSize);

                _height[child.Row] = Math.Max(Math.Max(child.DesiredSize.Height, MinHeight), _height[child.Row]);
                _width[child.Column] = Math.Max(Math.Max(child.DesiredSize.Width, MinWidth), _width[child.Column]);
            }

            ComputePositions();

            var desired = new Size(_xPositions[ColumnCount - 1] + MinWidth, _yPositions[RowCount - 1] + MinHeight);

            Trace.WriteLine(string.Format("{0}:Measure", watch.ElapsedMilliseconds));

            return desired;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            var watch = Stopwatch.StartNew();

            foreach (GridTextBox child in InternalChildren) {
                child.Arrange(new Rect(_xPositions[child.Column], _yPositions[child.Row], _width[child.Column], _height[child.Row]));
            }

            Trace.WriteLine(string.Format("{0}:Arrange", watch.ElapsedMilliseconds));

            return finalSize;
        }

        private void FindFirstIndex(double x, double y, out int row, out int column) {
            row = -1;
            for (int i = RowCount - 1; i >= 0; i--) {
                if (_xPositions[i] <= x) {
                    row = i;
                }
            }

            column = -1;
            for (int i = ColumnCount - 1; i >= 0; i--) {
                if (_yPositions[i] <= y) {
                    column = i;
                }
            }
        }

        private void ComputePositions() {
            double height = 0.0;
            for (int i = 0; i < RowCount - 1; i++) {
                height += _height[i];
                _yPositions[i + 1] = height;
            }

            double width = 0.0;
            for (int i = 0; i < ColumnCount - 1; i++) {
                width += _width[i];
                _xPositions[i + 1] = width;
            }
        }

        private void ComputePositions(GridTextBox element) {
            int nextIndex = element.Row + 1;
            if (nextIndex != RowCount) {
                double height = _yPositions[nextIndex] - _yPositions[element.Row];
                double diff = element.DesiredSize.Height - height;
                if (diff > 0) {
                    for (int i = nextIndex; i < RowCount; i++) {
                        _yPositions[i] += diff;
                    }
                }
            }

            nextIndex = element.Column + 1;
            if (nextIndex != ColumnCount) {
                double width = _xPositions[nextIndex] - _xPositions[element.Column];
                double diff = element.DesiredSize.Width - width;
                if (diff > 0) {
                    for (int i = nextIndex; i < ColumnCount; i++) {
                        _xPositions[i] += diff;
                    }
                }
            }
        }
    }
}
