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
    public enum GridType {
        Data,
        ColumnHeader,
        RowHeader,
    }

    public class VisualGrid : FrameworkElement {
        private TaskScheduler ui;
        private BlockingCollection<Func<Task>> _visualRefreshActions;

        private GridLineVisual _gridLine;
        private VisualCollection _visualChildren;
        private GridRange _dataViewport;
        private Grid<TextVisual> _visualGrid;

        public VisualGrid() {
            _visualChildren = new VisualCollection(this);
            _gridLine = new GridLineVisual();
            ClipToBounds = true;

            ui = TaskScheduler.FromCurrentSynchronizationContext();
            _visualRefreshActions = new BlockingCollection<Func<Task>>();   // TODO: cancellation action in case of close, P2

            Task.Run(() => StartVisualRefresh());
        }

        public GridType GridType { get; set; }

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

        #region Font

        public static readonly DependencyProperty FontFamilyProperty =
                TextElement.FontFamilyProperty.AddOwner(typeof(VisualGrid));

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
                        typeof(VisualGrid));

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

        public double GridLineThickness { get { return _gridLine.GridLineThickness; } }

        private double HorizontalOffset {
            get {
                if (GridType == GridType.RowHeader) {
                    return 0.0;
                }
                return Points.HorizontalOffset;
            }
        }

        private double VerticalOffset {
            get {
                if (GridType == GridType.ColumnHeader) {
                    return 0.0;
                }
                return Points.VerticalOffset;
            }
        }

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
            Debug.Assert((GridType == GridType.RowHeader && columnCount == 1) || (GridType != GridType.RowHeader));

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
            Debug.Assert((GridType == GridType.ColumnHeader && rowCount == 1) || (GridType != GridType.ColumnHeader));

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

            _visualChildren.Clear();
            foreach (int c in newViewport.Columns.GetEnumerable()) {
                foreach (int r in newViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    double width = Points.GetWidth(c) - GridLineThickness;
                    double height = Points.GetHeight(r) - GridLineThickness;
                    if (visual.Draw(new Size(width, height))) {
                        Points.SetWidth(c, Math.Max(width, visual.Size.Width + GridLineThickness));
                        Points.SetHeight(r, Math.Max(height, visual.Size.Height + GridLineThickness));
                    }

                    _visualChildren.Add(_visualGrid[r, c]);
                }
            }

            foreach (int c in newViewport.Columns.GetEnumerable()) {
                foreach (int r in newViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    var transform = visual.Transform as TranslateTransform;
                    if (transform == null) {
                        visual.Transform = new TranslateTransform(xPosition(visual), yPosition(visual));
                    } else {
                        transform.X = xPosition(visual);
                        transform.Y = yPosition(visual);
                    }
                }
            }
            _dataViewport = newViewport;

            DrawGridLine();
        }

        private double xPosition(TextVisual visual) {
            if (GridType == GridType.RowHeader) {
                return 0.0;
            }
            return Points.xPosition(visual.Column);
        }

        private double yPosition(TextVisual visual) {
            if (GridType == GridType.ColumnHeader) {
                return 0.0;
            }
            return Points.yPosition(visual.Row);
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
                Size measured = base.MeasureOverride(availableSize);
                return measured;
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

        private void GridPoints_ViewportChanged(object sender, ViewportChangedEventArgs e) {
            if (GridType == GridType.ColumnHeader && RowCount > 0) {
                Height = _gridPoints.GetHeight(0);
            } else if (GridType == GridType.RowHeader && ColumnCount > 0) {
                Width = _gridPoints.GetWidth(0);
            }

            RefreshVisuals();
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
