using System;
using System.Collections;
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
    
    internal enum ScrollType {
        Invalid,
        LineUp,
        LineDown,
        LineLeft,
        LineRight,
        PageUp,
        PageDown,
        PageLeft,
        PageRight,
        SetHorizontalOffset,
        SetVerticalOffset,
        MouseWheel,
        SizeChange,
    }

    internal struct Command {
        private static Lazy<Command> _empty = new Lazy<Command>(() => new Command(ScrollType.Invalid, 0));
        public static Command Empty { get { return _empty.Value; } }

        internal Command(ScrollType code, double param) {
            Debug.Assert(code != ScrollType.SizeChange);

            Code = code;
            Param = param;
            Size = Size.Empty;
        }

        internal Command(ScrollType code, Size size) {
            Code = code;
            Param = double.NaN;
            Size = size;
        }

        public ScrollType Code { get; set; }

        public double Param { get; set; }

        public Size Size { get; set; }
    }

    public class VisualGrid : FrameworkElement {
        private TaskScheduler ui;
        private BlockingCollection<Command> _scrollCommands;

        private GridLineVisual _gridLine;
        private VisualCollection _visualChildren;
        private GridRange _dataViewport;
        private Grid<TextVisual> _visualGrid;

        public VisualGrid() {
            _visualChildren = new VisualCollection(this);
            _gridLine = new GridLineVisual();
            ClipToBounds = true;

            ui = TaskScheduler.FromCurrentSynchronizationContext();

            _scrollCommands = new BlockingCollection<Command>();
            Task.Run(() => ScrollCommandsHandler());
        }

        public GridType GridType { get; set; }

        public IGridProvider<string> DataProvider { get; set; }

        public GridPoints Points { get; set; }

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

        private GridRange ComputeDataViewport(Rect visualViewport) {
            int columnStart = Points.xIndex(visualViewport.X);
            int rowStart = Points.yIndex(visualViewport.Y);

            double width = 0.0;
            int columnCount = 0;
            for (int c = columnStart; c < ColumnCount; c++) {
                width += Points.GetWidth(c);
                columnCount++;
                if (width >= visualViewport.Width) {    // TODO: DoubleUtil
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
                if (height >= visualViewport.Height) {    // TODO: DoubleUtil
                    break;
                }
            }
            Debug.Assert((GridType == GridType.ColumnHeader && rowCount == 1) || (GridType != GridType.ColumnHeader));

            return new GridRange(
                new Range(rowStart, rowCount),
                new Range(columnStart, columnCount));
        }

        private async void ScrollCommandsHandler() {
            List<Command> batch = new List<Command>();

            foreach (var command in _scrollCommands.GetConsumingEnumerable()) {
                try {
                    batch.Add(command);
                    if (_scrollCommands.Count > 0) {
                        // another command has been queued already. continue to next
                        continue;
                    } else {
                        for (int i = 0; i < batch.Count; i++) {
                            if (i < (batch.Count - 1)
                                && ((batch[i].Code == ScrollType.SizeChange && batch[i + 1].Code == ScrollType.SizeChange)
                                    || (batch[i].Code == ScrollType.SetHorizontalOffset && batch[i + 1].Code == ScrollType.SetHorizontalOffset)
                                    || (batch[i].Code == ScrollType.SetVerticalOffset && batch[i + 1].Code == ScrollType.SetVerticalOffset))) {
                                continue;
                            } else {
                                await ExecuteCommand(batch[i]);
                            }
                        }
                        batch.Clear();
                    }
                } catch (Exception ex) {
                    Trace.WriteLine(ex.Message);    // TODO: handle exception
                    batch.Clear();
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

        private async Task RefreshVisualsInternalAsync(Rect visualViewport) {
            using (var elapsed = new Elapsed("RefreshVisuals:")) {
                Debug.Assert(TaskUtilities.IsOnBackgroundThread());

                GridRange newViewport = ComputeDataViewport(visualViewport);

                // TODO: offset may change although data viewports are same
                if (newViewport.Equals(_dataViewport)) {
                    return;
                }

                var data = await DataProvider.GetRangeAsync(newViewport);

                await Task.Factory.StartNew(
                    () => SetVisuals(newViewport, data),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    ui);
            }
        }

        private void DrawGridLine() {
            if (_gridLine == null) return;

            _gridLine.Draw(_dataViewport, Points);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            EnqueueCommand(ScrollType.SizeChange, sizeInfo.NewSize);
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

        #region Command Queue

        private CommandQueue _queue = new CommandQueue();

        

        

        // implements ring buffer of commands
        private struct CommandQueue {
            private const int _capacity = 32;

            //returns false if capacity is used up and entry ignored
            internal void Enqueue(Command command) {
                if (_lastWritePosition == _lastReadPosition) //buffer is empty
                {
                    _array = new Command[_capacity];
                    _lastWritePosition = _lastReadPosition = 0;
                }

                if (!OptimizeCommand(command)) //regular insertion, if optimization didn't happen
                {
                    _lastWritePosition = (_lastWritePosition + 1) % _capacity;

                    if (_lastWritePosition == _lastReadPosition) //buffer is full
                    {
                        // throw away the oldest entry and continue to accumulate fresh input
                        _lastReadPosition = (_lastReadPosition + 1) % _capacity;
                    }

                    _array[_lastWritePosition] = command;
                }
            }

            // this tries to "merge" the incoming command with the accumulated queue
            // for example, if we get SetHorizontalOffset incoming, all "horizontal"
            // commands in the queue get removed and replaced with incoming one,
            // since horizontal position is going to end up at the specified offset anyways.
            private bool OptimizeCommand(Command command) {
                if (_lastWritePosition != _lastReadPosition) //buffer has something
                {

                    if ((command.Code == ScrollType.SetHorizontalOffset
                           && _array[_lastWritePosition].Code == ScrollType.SetHorizontalOffset)
                       || (command.Code == ScrollType.SetVerticalOffset
                           && _array[_lastWritePosition].Code == ScrollType.SetVerticalOffset)) {
                        //if the last command was "set offset" or "make visible", simply replace it and
                        //don't insert new command
                        _array[_lastWritePosition].Param = command.Param;
                        return true;
                    }
                }
                return false;
            }

            // returns Invalid command if there is no more commands
            internal Command Fetch() {
                if (_lastWritePosition == _lastReadPosition) //buffer is empty
                {
                    return new Command(ScrollType.Invalid, 0);
                }
                _lastReadPosition = (_lastReadPosition + 1) % _capacity;

                //array exists always if writePos != readPos
                Command command = _array[_lastReadPosition];
                _array[_lastReadPosition].Param = 0; //to release the allocated object

                if (_lastWritePosition == _lastReadPosition) //it was the last command
                {
                    _array = null; // make GC work. Hopefully the whole queue is processed in Gen0
                }
                return command;
            }

            internal bool IsEmpty() {
                return (_lastWritePosition == _lastReadPosition);
            }

            private int _lastWritePosition;
            private int _lastReadPosition;
            private Command[] _array;
        }

        //returns true if there was a command sent to ISI
        private async Task ExecuteCommand(Command cmd) {
            switch (cmd.Code) {
                case ScrollType.LineUp:
                    await LineUpAsync();
                    break;
                case ScrollType.LineDown:
                    await LineDownAsync();
                    break;
                case ScrollType.LineLeft: LineLeft(); break;
                case ScrollType.LineRight: LineRight(); break;

                case ScrollType.PageUp:
                    await PageUpAsync();
                    break;
                case ScrollType.PageDown:
                    await PageDownAsync();
                    break;
                case ScrollType.PageLeft: PageLeft(); break;
                case ScrollType.PageRight: PageRight(); break;

                case ScrollType.SetHorizontalOffset:
                    await SetHorizontalOffsetAsync(cmd.Param);
                    break;
                case ScrollType.SetVerticalOffset:
                    await SetVerticalOffsetAsync(cmd.Param);
                    break;
                case ScrollType.MouseWheel: SetMouseWheel(cmd.Param); break;

                case ScrollType.SizeChange: {
                        await RefreshVisualsInternalAsync(new Rect(HorizontalOffset, VerticalOffset, RenderSize.Width, RenderSize.Height));
                        break;
                    }
                case ScrollType.Invalid:
                    break;
            }
        }

        internal void EnqueueCommand(ScrollType code, double param) {
            _scrollCommands.Add(new Command(code, param));
        }

        internal void EnqueueCommand(ScrollType code, Size size) {
            _scrollCommands.Add(new Command(code, size));
        }

        private async Task SetVerticalOffsetAsync(double offset) {
            Points.VerticalOffset = offset;

            await RefreshVisualsInternalAsync(new Rect(HorizontalOffset, VerticalOffset, RenderSize.Width, RenderSize.Height));
        }

        private Task LineUpAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset - 10.0);    // TODO: do not hard-code the number here.
        }

        private Task LineDownAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset + 10.0);    // TODO: do not hard-code the number here.
        }

        private Task PageUpAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset - 100.0);    // TODO: do not hard-code the number here.
        }

        private Task PageDownAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset + 100.0);    // TODO: do not hard-code the number here.
        }

        private async Task SetHorizontalOffsetAsync(double offset) {
            Points.HorizontalOffset = offset;

            await RefreshVisualsInternalAsync(new Rect(HorizontalOffset, VerticalOffset, RenderSize.Width, RenderSize.Height));
        }

        private void LineRight() {
            throw new NotImplementedException();
        }

        private void LineLeft() {
            throw new NotImplementedException();
        }

        private void PageRight() {
            throw new NotImplementedException();
        }

        private void PageLeft() {
            throw new NotImplementedException();
        }

        private void SetMouseWheel(double delta) {
            throw new NotImplementedException();
        }
        #endregion

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
