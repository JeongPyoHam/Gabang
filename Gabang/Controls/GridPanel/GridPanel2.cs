using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Gabang.Controls {

    public class BatchPropertyChangeEventArgs : EventArgs {
        public enum BatchState {
            Begin,
            End,
        }

        public BatchPropertyChangeEventArgs(BatchState state) {
            State = state;
        }

        public BatchState State { get; }
    }

    public interface IBatchPropertyChagned {
        /// <summary>
        /// Hint a batch of PropertyChanged event begins, and end
        /// </summary>
        event EventHandler<BatchPropertyChangeEventArgs> BatchStateChanged;
    }

    public class GridPanel2 : FrameworkElement {
        private GridLineVisual _gridLine;
        private VisualCollection _visualChildren;

        private GridPoints _points;

        public GridPanel2() {
            _visualChildren = new VisualCollection(this);
            ClipToBounds = true;

            Initialize();
        }

        #region Font

        public static readonly DependencyProperty FontFamilyProperty =
                TextElement.FontFamilyProperty.AddOwner(typeof(GridPanel2));

        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set {
                _typeFace = null;
                SetValue(FontFamilyProperty, value);
            }
        }

        public static readonly DependencyProperty FontSizeProperty =
                TextElement.FontSizeProperty.AddOwner(
                        typeof(GridPanel2));

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        private Typeface _typeFace;
        private Typeface Typeface {
            get {
                if (_typeFace == null) {
                    _typeFace = FontFamily.GetTypefaces().First(tf => tf.Style == FontStyles.Normal && tf.Weight == FontWeights.Normal);
                }
                return _typeFace;
            }
        }

        #endregion

        public int RowCount { get { return 50; } }

        public int ColumnCount { get { return 60; } }

        public double MinItemWidth { get { return _points.MinItemWidth; } }

        public double MinItemHeight { get { return _points.MinItemHeight; } }

        public double GridLineThickness { get { return _gridLine.GridLineThickness; } }

        public Brush GridLineBrush { get { return _gridLine.GridLineBrush; } }

        public double HorizontalOffset {
            get { return _points.HorizontalOffset; }
            set {
                _points.HorizontalOffset = value;
                InvalidateScroll();
            }
        }

        public double HorizontalExtent { get; set; }
        public double HorizontalViewport { get; set; }

        public double VerticalOffset {
            get { return _points.VerticalOffset; }
            set {
                _points.VerticalOffset = value;
                InvalidateScroll();
            }
        }

        public double VerticalExtent { get; set; }
        public double VerticalViewport { get; set; }

        private void InvalidateScroll() {
            RefreshVisuals();
        }

        GridRange _dataViewport;
        Grid<TextVisual> _visualGrid;

        public void AddChileren() {
            Stopwatch watch = Stopwatch.StartNew();

            Initialize();

            RefreshChildren();

            Trace.WriteLine(string.Format("Add:{0}", watch.ElapsedMilliseconds));
        }

        private GridRange ComputeDataViewport() {
            int columnStart = _points.xIndex(HorizontalOffset);
            int rowStart = _points.yIndex(VerticalOffset);

            double width = 0.0;
            int columnCount = 0;
            for (int c = columnStart; c < ColumnCount; c++) {
                width += _points.GetWidth(c);
                columnCount++;
                if (width >= RenderSize.Width) {    // TODO: DoubleUtil
                    break;
                }
            }

            double height = 0.0;
            int rowEnd = rowStart;
            int rowCount = 0;
            for (int r = rowStart; r < RowCount; r++) {
                height += _points.GetHeight(r);
                rowCount++;
                if (height >= RenderSize.Height) {    // TODO: DoubleUtil
                    break;
                }
            }

            return new GridRange(
                new Range(rowStart, rowCount),
                new Range(columnStart, columnCount));
        }

        private void Initialize() {
            if (_visualChildren != null) {
                _visualChildren.Clear();
            }
            _visualChildren = new VisualCollection(this);

            _points = new GridPoints(RowCount, ColumnCount);
            
            _gridLine = new GridLineVisual();
        }

        private int generation = 0;
        public void RefreshChildren() {
            Stopwatch watch = Stopwatch.StartNew();

            generation++;
            if (generation > 10) {
                generation = 0;
            }

            foreach (TextVisual visual in _visualChildren) {
                visual.Text = Text(visual.Row, visual.Column);
            }

            RefreshVisuals();

            Trace.WriteLine(string.Format("Refesh:{0}", watch.ElapsedMilliseconds));
        }

        private string Text(int r, int c) {
            return string.Format("{0}:{1}:{2}", r.ToString(), c.ToString(), generation.ToString());
        }

        private void RefreshVisuals() {
            using (var elapsed = new Elapsed("RefreshVisuals:")) {
                GridRange orgViewport = _dataViewport;
                _dataViewport = ComputeDataViewport();

                var orgGrid = _visualGrid;
                _visualGrid = new Grid<TextVisual>(
                    _dataViewport,
                    (r, c) => {
                        if (orgViewport.Contains(r, c)) {
                            return orgGrid[r, c];
                        }
                        var visual = new TextVisual();
                        visual.Row = r;
                        visual.Column = c;
                        visual.Text = Text(r, c);
                        visual.Typeface = Typeface;
                        visual.FontSize = FontSize * (96.0 /72.0);  // TODO: test in High DPI
                        return visual;
                    });


                // add children
                _visualChildren.Clear();
                foreach (int c in _dataViewport.Columns.GetEnumerable()) {
                    foreach (int r in _dataViewport.Rows.GetEnumerable()) {
                        _visualChildren.Add(_visualGrid[r, c]);
                    }
                }

                foreach (TextVisual visual in _visualChildren) {
                    visual.Draw();

                    int r = visual.Row;
                    int c = visual.Column;

                    _points.SetWidth(c, Math.Max(_points.GetWidth(c), visual.ContentBounds.Right+ GridLineThickness));
                    _points.SetHeight(r, Math.Max(_points.GetHeight(r), visual.ContentBounds.Bottom + GridLineThickness));
                }

                foreach (TextVisual visual in _visualChildren) {
                    var transform = visual.Transform as TranslateTransform;
                    if (transform == null) {
                        visual.Transform = new TranslateTransform(_points.xPosition(visual.Column), _points.yPosition(visual.Row));
                    } else {
                        transform.X = _points.xPosition(visual.Column);
                        transform.Y = _points.yPosition(visual.Row);
                    }
                }

                DrawGridLine();

                HorizontalExtent = _points.HorizontalExtent;
                HorizontalViewport = _points.GetWidth(_dataViewport.Columns);
                VerticalExtent = _points.VerticalExtent;
                VerticalViewport = _points.GetHeight(_dataViewport.Rows);
            }
        }

        public void SetColumnWidth(int columnIndex, double width) {
            // TOOD: if change is trivial, return

            _points.SetWidth(columnIndex, width);
            if (_dataViewport.Columns.Contains(columnIndex)) {
                RefreshVisuals();
            }
        }

        private void DrawGridLine() {
            if (_gridLine == null) return;

            _gridLine.Draw(_dataViewport, _points);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            RefreshVisuals();

            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override Size MeasureOverride(Size availableSize) {
            using (var elapsed = new Elapsed("Measure:")) {
                return base.MeasureOverride(availableSize);
            }
        }

        protected override Size ArrangeOverride(Size finalSize) {
            using (var elapsed = new Elapsed("Arrange:")) {
                return base.ArrangeOverride(finalSize);
            }
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

        class Elapsed : IDisposable {
            Stopwatch _watch;
            string _header;
            public Elapsed(string header) {
                _header = header;
                _watch = Stopwatch.StartNew();
            }

            public void Dispose() {
                Trace.WriteLine(_header + _watch.ElapsedMilliseconds);
            }
        }
    }
}
