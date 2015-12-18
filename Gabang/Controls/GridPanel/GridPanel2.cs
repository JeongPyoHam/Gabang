using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gabang.Controls {

    public class GridPanel2 : FrameworkElement {
        private double[] _xPositions;
        private double[] _yPositions;
        private double[] _width;
        private double[] _height;

        private DrawingVisual _gridLine;
        private VisualCollection _visualChildren;

        public GridPanel2() {
            _visualChildren = new VisualCollection(this);
            ClipToBounds = true;
        }

        public int RowCount { get { return 50; } }

        public int ColumnCount { get { return 50; } }

        public double MinItemWidth { get { return 20.0; } }

        public double MinItemHeight { get { return 10.0; } }

        public double GridLineThickness { get { return 1.0; } }

        public Brush GridLineBrush { get { return Brushes.Black; } }

        public void AddChileren() {
            Stopwatch watch = Stopwatch.StartNew();

            _visualChildren.Clear();

            _xPositions = new double[ColumnCount];
            _yPositions = new double[RowCount];
            _width = new double[ColumnCount];
            _height = new double[RowCount];

            _gridLine = new DrawingVisual();

            for (int r = 0; r < RowCount; r++) {
                for (int c = 0; c < ColumnCount; c++) {
                    var visual = new GridPanel2Visual();
                    visual.Row = r;
                    visual.Column = c;

                    _visualChildren.Add(visual);
                }
            }

            RefreshChildren();

            Trace.WriteLine(string.Format("Add:{0}", watch.ElapsedMilliseconds));
        }

        private int generation = 0;
        public void RefreshChildren() {
            Stopwatch watch = Stopwatch.StartNew();

            foreach (GridPanel2Visual visual in _visualChildren) {
                visual.Text = string.Format("{0}:{1}:{2}", visual.Row.ToString(), visual.Column.ToString(), generation.ToString());

                DrawingContext dc = visual.RenderOpen();
                try {
                    var formattedText = visual.GetFormattedText();
                    dc.DrawText(formattedText, new Point(0, 0));
                } finally {
                    dc.Close();
                }
                int r = visual.Row;
                int c = visual.Column;

                _width[c] = Math.Max(_width[c], visual.ContentBounds.Width + GridLineThickness);
                _height[r] = Math.Max(_height[r], visual.ContentBounds.Height + GridLineThickness);
            }

            ComputePositions();

            foreach (GridPanel2Visual visual in _visualChildren) {
                var transform = visual.Transform as TranslateTransform;
                if (transform == null) {
                    visual.Transform = new TranslateTransform(_xPositions[visual.Column], _yPositions[visual.Row]);
                } else {
                    transform.X = _xPositions[visual.Column];
                    transform.Y = _yPositions[visual.Row];
                }
            }

            DrawGridLine();

            generation++;

            Trace.WriteLine(string.Format("Refesh:{0}", watch.ElapsedMilliseconds));
        }

        private void DrawGridLine() {
            if (_gridLine == null) return;
            DrawingContext drawingContext = _gridLine.RenderOpen();
            DoubleCollection xCollection = new DoubleCollection();
            DoubleCollection yCollection = new DoubleCollection();
            try {
                // vertical line
                for (int i = 0; i < ColumnCount; i++) {
                    Rect rect = new Rect(new Size(GridLineThickness, RenderSize.Height));
                    rect.X = _xPositions[i] + _width[i] - GridLineThickness;

                    drawingContext.DrawRectangle(GridLineBrush, null, rect);

                    xCollection.Add(_xPositions[i]);
                }

                // horizontal line
                for (int i = 0; i < RowCount; i++) {
                    Rect rect = new Rect(new Size(RenderSize.Width, GridLineThickness));
                    rect.Y = _yPositions[i] + _height[i] - GridLineThickness;

                    drawingContext.DrawRectangle(GridLineBrush, null, rect);
                    yCollection.Add(_yPositions[i]);
                }

                _gridLine.XSnappingGuidelines = xCollection;
                _gridLine.YSnappingGuidelines = yCollection;
            } finally {
                drawingContext.Close();
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            DrawGridLine();
        }

        protected override Size MeasureOverride(Size availableSize) {
            Stopwatch watch = Stopwatch.StartNew();

            var size = base.MeasureOverride(availableSize);

            Trace.WriteLine(string.Format("Measure:{0}", watch.ElapsedMilliseconds));
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            Stopwatch watch = Stopwatch.StartNew();

            var size = base.ArrangeOverride(finalSize);

            Trace.WriteLine(string.Format("Arrange:{0}", watch.ElapsedMilliseconds));
            return size;
        }

        protected override int VisualChildrenCount {
            get {
                if (_visualChildren.Count == 0) return 0;
                return _visualChildren.Count + 1;
            }
        }

        protected override Visual GetVisualChild(int index) {
            if (index < 0 || index >= _visualChildren.Count + 1)
                throw new ArgumentOutOfRangeException("index");
            if (index == 0) return _gridLine;
            return _visualChildren[index - 1];
        }
    }

    public class GridPanel2Visual : DrawingVisual {
        public int Row { get; set; }
        public int Column { get; set; }

        private string _text;
        public string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                _formattedText = null;
            }
        }

        private FormattedText _formattedText;
        public FormattedText GetFormattedText() {
            if (_formattedText == null) {
                _formattedText = new FormattedText(
                    Text,
                    CultureInfo.CurrentUICulture,
                    CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                    new Typeface("Segoe"),
                    12.0,
                    Brushes.Black);
            }
            return _formattedText;
        }
    }
}
