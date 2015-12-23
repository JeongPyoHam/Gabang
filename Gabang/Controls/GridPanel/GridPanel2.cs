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
    public class GridPanel2 : FrameworkElement {
        private GridLineVisual _gridLine;
        private VisualCollection _visualChildren;

        public GridPanel2() {
            _visualChildren = new VisualCollection(this);
            _gridLine = new GridLineVisual();
            ClipToBounds = true;
        }

        public IGridProvider<string> DataProvider { get; set; }

        public GridPoints Points { get; set; }

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

        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public double MinItemWidth { get { return Points.MinItemWidth; } }

        public double MinItemHeight { get { return Points.MinItemHeight; } }

        public double GridLineThickness { get { return _gridLine.GridLineThickness; } }

        public Brush GridLineBrush { get { return _gridLine.GridLineBrush; } }

        public double HorizontalOffset {
            get { return Points.HorizontalOffset; }
            set {
                Points.HorizontalOffset = value;
                InvalidateScroll();
            }
        }

        public double HorizontalExtent { get; set; }
        public double HorizontalViewport { get; set; }

        public double VerticalOffset {
            get { return Points.VerticalOffset; }
            set {
                Points.VerticalOffset = value;
                InvalidateScroll();
            }
        }

        public double VerticalExtent { get; set; }
        public double VerticalViewport { get; set; }

        private async void InvalidateScroll() {
            await RefreshVisuals();
        }

        GridRange _dataViewport;
        Grid<TextVisual> _visualGrid;

        private GridRange ComputeDataViewport() {
            int columnStart = Points.xIndex(HorizontalOffset);
            int rowStart = Points.yIndex(VerticalOffset);

            double width = 0.0;
            int columnCount = 0;
            for (int c = columnStart; c < ColumnCount; c++) {
                width += Points.GetWidth(c);
                columnCount++;
                if (width >= RenderSize.Width) {    // TODO: DoubleUtil
                    break;
                }
            }

            double height = 0.0;
            int rowEnd = rowStart;
            int rowCount = 0;
            for (int r = rowStart; r < RowCount; r++) {
                height += Points.GetHeight(r);
                rowCount++;
                if (height >= RenderSize.Height) {    // TODO: DoubleUtil
                    break;
                }
            }

            return new GridRange(
                new Range(rowStart, rowCount),
                new Range(columnStart, columnCount));
        }

        private async Task RefreshVisuals() {
            using (var elapsed = new Elapsed("RefreshVisuals:")) {
                GridRange orgViewport = _dataViewport;
                GridRange newViewport = ComputeDataViewport();

                var data = await DataProvider.GetRangeAsync(newViewport);

                var orgGrid = _visualGrid;
                _visualGrid = new Grid<TextVisual>(
                    newViewport,
                    (r, c) => {
                        if (orgViewport.Contains(r, c)) {
                            return orgGrid[r, c];
                        }
                        var visual = new TextVisual();
                        visual.Row = r;
                        visual.Column = c;
                        visual.Text = data[r, c];
                        visual.Typeface = Typeface;
                        visual.FontSize = FontSize * (96.0 /72.0);  // TODO: test in High DPI
                        return visual;
                    });


                // add children
                _visualChildren.Clear();
                foreach (int c in newViewport.Columns.GetEnumerable()) {
                    foreach (int r in newViewport.Rows.GetEnumerable()) {
                        _visualChildren.Add(_visualGrid[r, c]);
                    }
                }

                foreach (TextVisual visual in _visualChildren) {
                    visual.Draw();

                    int r = visual.Row;
                    int c = visual.Column;

                    Points.SetWidth(c, Math.Max(Points.GetWidth(c), visual.ContentBounds.Right+ GridLineThickness));
                    Points.SetHeight(r, Math.Max(Points.GetHeight(r), visual.ContentBounds.Bottom + GridLineThickness));
                }

                foreach (TextVisual visual in _visualChildren) {
                    var transform = visual.Transform as TranslateTransform;
                    if (transform == null) {
                        visual.Transform = new TranslateTransform(Points.xPosition(visual.Column), Points.yPosition(visual.Row));
                    } else {
                        transform.X = Points.xPosition(visual.Column);
                        transform.Y = Points.yPosition(visual.Row);
                    }
                }
                _dataViewport = newViewport;

                DrawGridLine();

                HorizontalExtent = Points.HorizontalExtent;
                HorizontalViewport = Points.GetWidth(newViewport.Columns);
                VerticalExtent = Points.VerticalExtent;
                VerticalViewport = Points.GetHeight(newViewport.Rows);
            }
        }

        private void DrawGridLine() {
            if (_gridLine == null) return;

            _gridLine.Draw(_dataViewport, Points);
        }

        protected override async void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            try {
                await RefreshVisuals();
            } catch (Exception ex) {
                Debug.Fail(ex.Message);
            }

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
