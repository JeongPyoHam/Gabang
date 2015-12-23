using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Gabang.Controls {
    public class GridPanel2 : FrameworkElement {
        private TaskScheduler ui;
        private BlockingCollection<Func<Task>> _visualRefreshActions;

        private GridLineVisual _gridLine;
        private VisualCollection _visualChildren;
        private GridRange _dataViewport;
        private Grid<TextVisual> _visualGrid;

        public GridPanel2() {
            _visualChildren = new VisualCollection(this);
            _gridLine = new GridLineVisual();
            ClipToBounds = true;

            ui = TaskScheduler.FromCurrentSynchronizationContext();
            _visualRefreshActions = new BlockingCollection<Func<Task>>();   // TODO: cancellation action in case of close, P2

            Task.Run(() => StartVisualRefresh());
        }

        public IGridProvider<string> DataProvider { get; set; }

        private GridPoints _gridPoints;
        public GridPoints Points {
            get {
                return _gridPoints;
            }
            set {
                if (_gridPoints != null) {
                    _gridPoints.ViewportChanged -= GridPoints_ViewportChanged;
                }
                _gridPoints = value;
                if (_gridPoints != null) {
                    _gridPoints.ViewportChanged += GridPoints_ViewportChanged;
                }
            }
        }

        private void GridPoints_ViewportChanged(object sender, EventArgs e) {
            RefreshVisuals();
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
                RefreshVisuals();
            }
        }

        public double HorizontalExtent { get; set; }
        public double HorizontalViewport { get; set; }

        public double VerticalOffset {
            get { return Points.VerticalOffset; }
            set {
                Points.VerticalOffset = value;
                RefreshVisuals();
            }
        }

        public double VerticalExtent { get; set; }
        public double VerticalViewport { get; set; }

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

        private void RefreshVisuals() {
            if (_visualRefreshActions.Count > 0) return;
            _visualRefreshActions.Add(RefreshVisualsInternalAsync);
        }

        private async void StartVisualRefresh() {
            foreach (var action in _visualRefreshActions.GetConsumingEnumerable()) {
                try {
                    await action();

                } catch (Exception ex) {
                    Trace.WriteLine(ex.Message);    // TODO: handle exception
                }
            }
        }

        private void SetVisuals(GridRange newViewport, IGrid<string> data) {
            var orgGrid = _visualGrid;
            _visualGrid = new Grid<TextVisual>(
                newViewport,
                (r, c) => {
                    if (_dataViewport.Contains(r, c)) {
                        return orgGrid[r, c];
                    }
                    var visual = new TextVisual();
                    visual.Row = r;
                    visual.Column = c;
                    visual.Text = data[r, c];
                    visual.Typeface = Typeface;
                    visual.FontSize = FontSize * (96.0 / 72.0);  // TODO: test in High DPI
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
                if (!visual.Draw()) {
                    continue;
                }

                int c = visual.Column;
                Points.SetWidth(c, Math.Max(Points.GetWidth(c), visual.ContentBounds.Right + GridLineThickness));

                int r = visual.Row;
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

        private async Task RefreshVisualsInternalAsync() {
            using (var elapsed = new Elapsed("RefreshVisuals:")) {
                Debug.Assert(TaskUtilities.IsOnBackgroundThread());

                GridRange newViewport = ComputeDataViewport();

                if (newViewport.Equals(_dataViewport)) {
                    return;
                }

                var data = await DataProvider.GetRangeAsync(newViewport);

                await Task.Factory.StartNew(() => SetVisuals(newViewport, data), CancellationToken.None, TaskCreationOptions.None, ui);
            }
        }

        private void DrawGridLine() {
            if (_gridLine == null) return;

            _gridLine.Draw(_dataViewport, Points);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
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
#if DEBUG
                _watch = Stopwatch.StartNew();
#endif
            }

            public void Dispose() {
#if DEBUG
                Trace.WriteLine(_header + _watch.ElapsedMilliseconds);
#endif
            }
        }
    }
}
