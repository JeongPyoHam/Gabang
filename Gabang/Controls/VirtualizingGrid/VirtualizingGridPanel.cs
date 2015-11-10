#define PANELTRACE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Gabang.Controls {
    /// <summary>
    /// assumes, IsVirtualizing = true, VirtualizationMode = Standard, ScrollUnit = Pixel, CacheLength = 1, CacheLengthUnit = Item
    /// </summary>
    public class VariableGridPanel : VirtualizingPanel, IScrollInfo {
        #region IScrollInfo

        [Flags]
        enum ScrollHint {
            None = 0x00,
            Horizontal = 0x01,
            Vertial = 0x02,
        }

        [DefaultValue(false)]
        public bool CanHorizontallyScroll { get; set; }

        [DefaultValue(false)]
        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight { get; private set; }

        public double ExtentWidth { get; private set; }

        public double HorizontalOffset { get; private set; }

        public ScrollViewer ScrollOwner { get; set; }

        public double VerticalOffset { get; private set; }

        public double ViewportHeight { get; private set; }

        public double ViewportWidth { get; private set; }

        public void LineUp() {
            SetVerticalOffset(VerticalOffset - GetLineDelta(Orientation.Vertical));
        }

        public void LineDown() {
            SetVerticalOffset(VerticalOffset + GetLineDelta(Orientation.Vertical));
        }

        public void LineLeft() {
            SetHorizontalOffset(HorizontalOffset - GetLineDelta(Orientation.Horizontal));
        }

        public void LineRight() {
            SetHorizontalOffset(HorizontalOffset + GetLineDelta(Orientation.Horizontal));
        }

        public void MouseWheelUp() {
            SetVerticalOffset(VerticalOffset - GetMouseWheelDelta(Orientation.Vertical));
        }

        public void MouseWheelDown() {
            SetVerticalOffset(VerticalOffset + GetMouseWheelDelta(Orientation.Vertical));
        }

        public void MouseWheelLeft() {
            SetHorizontalOffset(HorizontalOffset - GetMouseWheelDelta(Orientation.Horizontal));
        }

        public void MouseWheelRight() {
            SetHorizontalOffset(HorizontalOffset + GetMouseWheelDelta(Orientation.Horizontal));
        }

        public void PageUp() {
            SetVerticalOffset(VerticalOffset - GetPageDelta(Orientation.Vertical));
        }

        public void PageDown() {
            SetVerticalOffset(VerticalOffset + GetPageDelta(Orientation.Vertical));
        }

        public void PageLeft() {
            SetHorizontalOffset(HorizontalOffset - GetPageDelta(Orientation.Horizontal));
        }

        public void PageRight() {
            SetHorizontalOffset(HorizontalOffset + GetPageDelta(Orientation.Horizontal));
        }

        private ScrollHint _scrollHint = ScrollHint.None;

        public void SetHorizontalOffset(double offset) {
            if (offset > ExtentWidth - ViewportWidth) {
                offset = ExtentWidth - ViewportWidth;
            }
            if (offset < 0) {
                offset = 0;
            }

            if (HorizontalOffset != offset) {
                _scrollHint |= ScrollHint.Horizontal;
                HorizontalOffset = offset;
                InvalidateMeasure();    // TODO: if IsVirtualizing==false, InvalidaArrange() should be enough
            }
        }

        public void SetVerticalOffset(double offset) {
            if (offset > ExtentHeight - ViewportHeight) {
                offset = ExtentHeight - ViewportHeight;
            }
            if (offset < 0) {
                offset = 0;
            }

            if (VerticalOffset != offset) {
                _scrollHint |= ScrollHint.Vertial;
                VerticalOffset = offset;
                InvalidateMeasure();
            }
        }

        private double GetLineDelta(Orientation orientation) {
            return 1.0; // TODO: change to pixed based!
        }

        private double GetPageDelta(Orientation orientation) {
            return 10.0;
        }

        private double GetMouseWheelDelta(Orientation orientation) {
            return 1.0; // TODO: change to pixed based!
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides

        Point _lastMeasureOffset = new Point();
        Size _lastMeasureDesiredSize = new Size();
        Range _lastMeasureViewportRow = new Range();
        Range _lastMeasureViewportColumn = new Range();

        protected override Size MeasureOverride(Size availableSize) {
            EnsurePrerequisite();

            // TODO: check infinity
            EnsureMeasureOperation(availableSize);

            return availableSize;
        }

        private DispatcherOperation _measureOperation;

        private void EnsureMeasureOperation(Size availableSize) {
            if (_measureOperation == null) {
                _measureOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnMeasure), availableSize);
            }
        }

        private void ResetMeasureOperation() {
            _measureOperation = null;
        }

        private object OnMeasure(object arg) {
#if PANELTRACE
            DateTime startTime = DateTime.Now;
            _childMeasure = new TimeSpan();
#endif
            Size availableSize = (Size)arg;

            // start from scroll offset
            Range viewportRow = new Range();
            viewportRow.Start = (int)VerticalOffset;
            Range viewportColumn = new Range();
            viewportColumn.Start = (int)HorizontalOffset;

            int horizontalGrowth = 1;
            int verticalGrowth = 1;

            // create first cell
            var rowStack = Generator.GetRow(viewportRow.Start);
            var columnStack = Generator.GetColumn(viewportColumn.Start);
            Size desiredSize = MeasureChild(rowStack, columnStack);
            viewportRow.Count += verticalGrowth;
            viewportColumn.Count += horizontalGrowth;

            // grows area from first cell to fill available area
            bool isRow = _scrollHint.HasFlag(ScrollHint.Vertial);
            while ((horizontalGrowth != 0 || verticalGrowth != 0)
                && (viewportRow.Count != Generator.RowCount || viewportColumn.Count != Generator.ColumnCount)) {
                if (isRow) {
                    verticalGrowth = GrowVertically(availableSize.Height, ref desiredSize, ref viewportRow, ref viewportColumn);
#if PANELTRACE
                    Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: vertical growth {0}", verticalGrowth);
#endif
                } else {
                    horizontalGrowth = GrowHorizontally(availableSize.Width, ref desiredSize, ref viewportRow, ref viewportColumn);
#if PANELTRACE
                    Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: horizontal growth {0}", horizontalGrowth);
#endif
                }

                isRow ^= true;  // toggle
            }

            _lastMeasureViewportRow = viewportRow;
            _lastMeasureViewportColumn = viewportColumn;
            _lastMeasureDesiredSize = desiredSize;

            if (desiredSize.Height > availableSize.Height) {
                if (_lastMeasureViewportRow.Start == 0) {
                    _lastMeasureOffset.Y = 0;
                } else if (_lastMeasureViewportRow.Start + _lastMeasureViewportRow.Count >= Generator.RowCount) {   // BUG: last time scroll a little bit more than one item
                    _lastMeasureOffset.Y = availableSize.Height - desiredSize.Height;
                }
            }
            if (desiredSize.Width > availableSize.Width) {
                if (_lastMeasureViewportColumn.Start == 0) {
                    _lastMeasureOffset.X = 0;
                } else if (_lastMeasureViewportColumn.Start + _lastMeasureViewportColumn.Count >= Generator.ColumnCount) {
                    _lastMeasureOffset.X = availableSize.Width - desiredSize.Width;
                }
            }

            UpdateScrollInfo(desiredSize, availableSize);

#if PANELTRACE
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: row Count {0} column Count {1}", _lastMeasureViewportRow.Count, _lastMeasureViewportColumn.Count);
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: ChildMeasure {0} msec", _childMeasure.TotalMilliseconds);
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif

            ResetMeasureOperation();

            return desiredSize;
        }

        private void UpdateScrollInfo(Size desiredSize, Size availableSize) {
            if (Generator != null) {
                ExtentWidth = Generator.ColumnCount;
                ExtentHeight = Generator.RowCount;

                VerticalOffset = _lastMeasureViewportRow.Start;
                HorizontalOffset = _lastMeasureViewportColumn.Start;

                ViewportWidth = _lastMeasureViewportColumn.Count;
                ViewportHeight = _lastMeasureViewportRow.Count;

                ScrollOwner?.InvalidateScrollInfo();
            }

            _scrollHint = ScrollHint.None;
        }

#if PANELTRACE
        private TimeSpan _childMeasure;
#endif

        private Size MeasureChild(VariableGridStack rowStack, VariableGridStack columnStack) {
#if PANELTRACE
            Trace(TraceLevel.Verbose, "VirtualizingGridPanel:MeasureChild: row {0} column {1}", rowStack.Index, columnStack.Index);
#endif
            double widthConstraint = columnStack.GetSizeConstraint();
            double heightConstraint = rowStack.GetSizeConstraint();

            bool newlyCreated;
            var child = Generator.GenerateAt(rowStack.Index, columnStack.Index, out newlyCreated);
            if (newlyCreated) {
                AddInternalChild(child);
            } else {
                Debug.Assert(InternalChildren.Contains(child));
            }

            if (newlyCreated || !rowStack.LayoutSize.Frozen || !columnStack.LayoutSize.Frozen) {
#if PANELTRACE
                DateTime startChildMeasure = DateTime.Now;
#endif
                child.Measure(new Size(widthConstraint, heightConstraint));

#if PANELTRACE
                _childMeasure += (DateTime.Now - startChildMeasure);
#endif

                columnStack.LayoutSize.Max = child.DesiredSize.Width;
                rowStack.LayoutSize.Max = child.DesiredSize.Height;
            }

            return new Size(columnStack.LayoutSize.Max.Value, rowStack.LayoutSize.Max.Value);
        }

        private int GrowVertically(double extent, ref Size desiredSize, ref Range viewportRow, ref Range viewportColumn) {
            int growth = 0;
            int growStartAt = viewportRow.Start + viewportRow.Count;

            // grow forward
            while ((desiredSize.Height < extent) && ((growStartAt + growth) < Generator.RowCount)) {
                var rowStack = Generator.GetRow(growStartAt + growth);
                MeasureRow(rowStack, ref desiredSize, ref viewportColumn);
                growth += 1;
            }

            // grow backward
            int growthBackward = 0;
            int growBackwardStartAt = viewportRow.Start - 1;
            while ((desiredSize.Height < extent) && (growBackwardStartAt - growthBackward >= 0)) {
                var rowStack = Generator.GetRow(growBackwardStartAt - growthBackward);
                MeasureRow(rowStack, ref desiredSize, ref viewportColumn);
                growthBackward += 1;
            }

            viewportRow.Start -= growthBackward;
            viewportRow.Count += growth + growthBackward;

            return growth + growthBackward;
        }


        private int GrowHorizontally(double extent, ref Size desiredSize, ref Range viewportRow, ref Range viewportColumn) {
            int growth = 0;
            int growStartAt = viewportColumn.Start + viewportColumn.Count;

            // grow forward
            while ((desiredSize.Width < extent) && ((growStartAt + growth) < Generator.ColumnCount)) {
                var columnStack = Generator.GetColumn(growStartAt + growth);
                MeasureColumn(columnStack, ref desiredSize, ref viewportRow);
                growth += 1;
            }

            // grow backward
            int growthBackward = 0;
            int growBackwardStartAt = viewportColumn.Start - 1;
            while ((desiredSize.Width < extent) && ((growBackwardStartAt - growthBackward >= 0))) {
                var columnStack = Generator.GetColumn(growBackwardStartAt - growthBackward);
                MeasureColumn(columnStack, ref desiredSize, ref viewportRow);
                growthBackward += 1;
            }

            viewportColumn.Start -= growthBackward;
            viewportColumn.Count += growth + growthBackward;

            return growth;
        }

        private void MeasureRow(VariableGridStack rowStack, ref Size desiredSize, ref Range viewportColumn) {
            double width = 0.0;
            int column = viewportColumn.Start;
            while (viewportColumn.Contains(column) && column < Generator.ColumnCount) {
                var columnStack = Generator.GetColumn(column);
                Size childDesiredSize = MeasureChild(rowStack, columnStack);

                width += childDesiredSize.Width;

                column++;
            }

            desiredSize.Height += rowStack.LayoutSize.Max.Value;
            desiredSize.Width = width;
        }

        private void MeasureColumn(VariableGridStack columnStack, ref Size desiredSize, ref Range viewportRow) {
            double height = 0.0;
            int row = viewportRow.Start;
            while (viewportRow.Contains(row) && row < Generator.RowCount) {
                var rowStack = Generator.GetRow(row);
                Size childDesiredSize = MeasureChild(rowStack, columnStack);

                height += childDesiredSize.Height;

                row++;
            }

            desiredSize.Height = height;
            desiredSize.Width += columnStack.LayoutSize.Max.Value;
        }

        protected override Size ArrangeOverride(Size finalSize) {
#if PANELTRACE
            DateTime startTime = DateTime.Now;
#endif
            double computedRowOffset, computedColumnOffset;
            Generator.ComputeStackPosition(_lastMeasureViewportRow, _lastMeasureViewportColumn, out computedRowOffset, out computedColumnOffset);

            foreach (VariableGridCell child in InternalChildren) {
                var columnStack = Generator.GetColumn(child.Column);
                var rowStack = Generator.GetRow(child.Row);

                Rect rect = new Rect(
                    columnStack.LayoutPosition.Value - computedColumnOffset + _lastMeasureOffset.X,
                    rowStack.LayoutPosition.Value - computedRowOffset + _lastMeasureOffset.Y,
                    columnStack.LayoutSize.Max.Value,
                    rowStack.LayoutSize.Max.Value);
#if PANELTRACE
                Trace(TraceLevel.Verbose, "VirtualizingGridPanel:Arrange: row {0} column {1} rect {2}", child.Row, child.Column, rect);
#endif
                child.Arrange(rect);
            }
#if PANELTRACE
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Arrange(except PostArrange): {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
            PostArrange();

            return finalSize;
        }

#endregion

        #region Clean Up

        private void PostArrange() {
#if PANELTRACE
            DateTime startTime = DateTime.Now;
#endif
            EnsureCleanupOperation();

            Generator.FreezeLayoutSize();
#if PANELTRACE
            Trace(TraceLevel.Info, "VirtualizingGridPanel:PostArrange: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
        }

        private DispatcherOperation _cleanupOperation;

        private void EnsureCleanupOperation() {
            if (_cleanupOperation == null) {
                _cleanupOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnCleanUp), null);
            }
        }

        private object OnCleanUp(object args) {
            try {
#if PANELTRACE
                DateTime startTime = DateTime.Now;
#endif
                // Remove from visual children
                int begin = -1;
                while (true) {
                    Range cleanBlock = FindFirstChildrenRangeOutsideViewport(ref begin);

                    if (cleanBlock.Count == 0) {
                        break;
                    }

                    for (int i = 0; i < cleanBlock.Count; i++) {
                        var child = (VariableGridCell) InternalChildren[cleanBlock.Start + i];
                        bool removed = Generator.RemoveAt(child.Row, child.Column);
#if PANELTRACE
                        Trace(TraceLevel.Verbose, "VirtualizingGridPanel:OnCleanUp: remove: row {0} column {1}", child.Row, child.Column);
#endif
                        Debug.Assert(removed);
                    }

                    RemoveInternalChildRange(cleanBlock.Start, cleanBlock.Count);
                }
#if PANELTRACE
                Trace(TraceLevel.Info, "VirtualizingGridPanel:OnCleanUp 1: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
                // clean up from container generator
                Generator.RemoveRowsExcept(_lastMeasureViewportRow);
                Generator.RemoveColumnsExcept(_lastMeasureViewportColumn);
#if PANELTRACE
                Trace(TraceLevel.Info, "VirtualizingGridPanel:OnCleanUp 2: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
            } finally {
                _cleanupOperation = null;
            }

            return null;
        }

        private Range FindFirstChildrenRangeOutsideViewport(ref int start) {
            int begin = start == -1 ? InternalChildren.Count - 1 : start;

            int? nextStart = null;
            int lastIndex = 0; int count = 0;
            for (int i = begin; i >= 0; i--) {
                VariableGridCell child = (VariableGridCell)InternalChildren[i];

                if (!_lastMeasureViewportRow.Contains(child.Row)
                    || !_lastMeasureViewportColumn.Contains(child.Column)) {
                    if (count == 0) {
                        lastIndex = i;
                    }
                    count++;
                } else {
                    if (count != 0) {
                        nextStart = i;
                        break;
                    }
                }
            }

            if (!nextStart.HasValue) {
                nextStart = 0;
            }

            Debug.Assert((start == -1) || (InternalChildren.Count >= start + count));
            return new Range() { Start = lastIndex - count + 1, Count = count };
        }

        #endregion

        private VariableGridCellGenerator Generator { get; set; }

        internal ItemsControl OwningItemsControl { get; private set; }

        internal VirtualizationCacheLength CacheLength { get; private set; }

        internal VirtualizationCacheLengthUnit CacheLengthUnit { get; private set; }

        private void EnsurePrerequisite() {
            ItemsControl owningItemsControl = ItemsControl.GetItemsOwner(this);
            if (owningItemsControl == null) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports only ItemsPanel. Can't use stand alone");
            }
            OwningItemsControl = owningItemsControl;

            if (!(owningItemsControl is VariableGrid)) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports only {typeof(VariableGrid)}'s ItemsPanel");
            }
            this.Generator = ((VariableGrid)owningItemsControl).Generator;

            if (ScrollOwner == null) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} must be used for top level scrolling panel");
            }

            if (!GetIsVirtualizing(owningItemsControl)) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports onlly IsVirtualizing=\"true\"");
            }

            if (GetVirtualizationMode(owningItemsControl) != VirtualizationMode.Standard) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports onlly VirtualizationMode=\"Standard\"");
            }

            if (GetScrollUnit(owningItemsControl) != ScrollUnit.Pixel) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports onlly ScrollUnit=\"Pixel\"");
            }

            CacheLength = GetCacheLength(owningItemsControl);

            CacheLengthUnit = GetCacheLengthUnit(owningItemsControl);

            var children = this.Children;   // this ensures that ItemContainerGenerator is not null
        }

        #region TRACE
#if PANELTRACE
        private List<string> _traces = new List<string>();
        private DispatcherOperation _traceOperation;
        private TraceLevel _traceLevel = TraceLevel.Info;

        object OnFlush(object args) {
            foreach (var message in _traces) {
                Debug.WriteLine(message);
            }
            _traces.Clear();

            _traceOperation = null;
            return null;
        }

        void EnsureFlushOperation() {
            if (_traceOperation == null) {
                _traceOperation = Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new DispatcherOperationCallback(OnFlush), null);
            }
        }

        [Conditional("PANELTRACE")]
        void Trace(TraceLevel level, string message) {
            if (level <= _traceLevel) {
                _traces.Add(message);

                EnsureFlushOperation();
            }
        }

        [Conditional("PANELTRACE")]
        void Trace(TraceLevel level, string format, params object[] args) {
            if (level <= _traceLevel) {
                _traces.Add(string.Format(format, args));

                EnsureFlushOperation();
            }
        }
#endif

        #endregion
    }
}
