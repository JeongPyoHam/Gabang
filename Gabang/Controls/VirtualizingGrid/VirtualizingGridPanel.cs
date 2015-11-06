//#define PRINT
//#define ASSERT

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
    public class VariableGridPanel2 : VirtualizingPanel, IScrollInfo {
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

        Range _lastMeasureViewportRow = new Range();
        Range _lastMeasureViewportColumn = new Range();

        Dictionary<int, MaxDouble> RowHeight = new Dictionary<int, MaxDouble>();
        Dictionary<int, MaxDouble> ColumnWidth = new Dictionary<int, MaxDouble>();

        protected override Size MeasureOverride(Size availableSize) {
#if DEBUG
            DateTime startTime = DateTime.Now;
#endif
            EnsurePrerequisite();

            int rowIndex = (int)VerticalOffset;
            int columnIndex = (int)HorizontalOffset;

            Range viewportRow = new Range(); // TODO: is resetting value better than replacing the insntace?
            viewportRow.Start = rowIndex;
            Range viewportColumn = new Range();
            viewportColumn.Start = columnIndex;

            int horizontalGrowth = 1;
            int verticalGrowth = 1;

            Size desiredSize = MeasureChild(viewportRow.Start, viewportColumn.Start);
            viewportRow.Count += verticalGrowth;
            rowIndex += verticalGrowth;
            viewportColumn.Count += horizontalGrowth;
            columnIndex += horizontalGrowth;

            bool isRow = _scrollHint.HasFlag(ScrollHint.Vertial);
            while ((horizontalGrowth != 0 || verticalGrowth != 0)
                && (rowIndex != Generator.RowCount || columnIndex != Generator.ColumnCount)) {
                if (isRow) {
                    verticalGrowth = GrowVertically(rowIndex, availableSize.Height, ref desiredSize, ref viewportColumn);
                    viewportRow.Count += verticalGrowth;
                    rowIndex += verticalGrowth;
                } else {
                    horizontalGrowth = GrowHorizontally(columnIndex, availableSize.Width, ref desiredSize, ref viewportRow);
                    viewportColumn.Count += horizontalGrowth;
                    columnIndex += horizontalGrowth;
                }

                isRow ^= true;  // toggle
            }

            // TODO: mark measure pass
            UpdateScrollInfo();

            _lastMeasureViewportRow = viewportRow;
            _lastMeasureViewportColumn = viewportColumn;
#if DEBUG
            Debug.WriteLine("VirtualizingGridPanel:Measure: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
            return desiredSize;
        }

        private void UpdateScrollInfo() {
            if (Generator != null) {
                ExtentWidth = Generator.ColumnCount;
                ExtentHeight = Generator.RowCount;
                ViewportWidth = _lastMeasureViewportColumn.Count;
                ViewportHeight = _lastMeasureViewportRow.Count;
                ScrollOwner?.InvalidateScrollInfo();
            }

            _scrollHint = ScrollHint.None;
        }

        private Size MeasureChild(int row, int column) {
            if (_lastMeasureViewportRow.Contains(row) && _lastMeasureViewportColumn.Contains(column)) {
                Debug.Assert(RowHeight.ContainsKey(row) && ColumnWidth.ContainsKey(column));

                // TODO: measure again? maybe, not
                return new Size(ColumnWidth[column].Max.Value, RowHeight[row].Max.Value);
            } else {
                bool newlyCreated;
                var child = Generator.GenerateAt(row, column, out newlyCreated);
                if (newlyCreated) {
                    AddInternalChild(child);
                } else {
                    Debug.Assert(InternalChildren.Contains(child));
                }

                bool updateWidth = true; double widthConstraint = double.PositiveInfinity;
                if (!ColumnWidth.ContainsKey(column)) {
                    ColumnWidth[column] = new MaxDouble();
                } else if (ColumnWidth[column].Frozen) {
                    updateWidth = false;
                    widthConstraint = ColumnWidth[column].Max.Value;
                }

                bool updateHeight = true; double heightConstraint = double.PositiveInfinity;
                if (!RowHeight.ContainsKey(row)) {
                    RowHeight[row] = new MaxDouble();
                } else if (RowHeight[row].Frozen) {
                    updateHeight = false;
                    heightConstraint = RowHeight[row].Max.Value;
                }

                child.Measure(new Size(widthConstraint, heightConstraint));

                if (updateWidth) {
                    ColumnWidth[column].Max = child.DesiredSize.Width;
                }
                if (updateHeight) {
                    RowHeight[row].Max = child.DesiredSize.Height;
                }

                return child.DesiredSize;
            }
        }

        private int GrowVertically(int row, double extent, ref Size desiredSize, ref Range viewportColumn) {
            int growth = 0;
            while ((desiredSize.Height < extent) && ((row + growth) < Generator.RowCount)) {
                MeasureRow(row + growth, ref desiredSize, ref viewportColumn);
                growth += 1;
            }
            return growth;
        }

        private void MeasureRow(int row, ref Size desiredSize, ref Range viewportColumn) {
            double width = 0.0;
            int column = viewportColumn.Start;
            while (viewportColumn.Contains(column) && column < Generator.ColumnCount) {
                Size childDesiredSize = MeasureChild(row, column);

                width += childDesiredSize.Width;

                column++;
            }

            desiredSize.Height += RowHeight[row].Max.Value;
            desiredSize.Width = width;
        }

        private int GrowHorizontally(int column, double extent, ref Size desiredSize, ref Range viewportRow) {
            int growth = 0;
            while ((desiredSize.Width < extent) && ((column + growth) < Generator.ColumnCount)) {
                MeasureColumn(column + growth, ref desiredSize, ref viewportRow);
                growth += 1;
            }
            return growth;
        }
        private void MeasureColumn(int column, ref Size desiredSize, ref Range viewportRow) {
            double height = 0.0;
            int row = viewportRow.Start;
            while (viewportRow.Contains(row) && row < Generator.RowCount) {
                Size childDesiredSize = MeasureChild(row, column);

                height += childDesiredSize.Height;

                row++;
            }

            desiredSize.Height = height;
            desiredSize.Width += ColumnWidth[column].Max.Value;
        }

        protected override Size ArrangeOverride(Size finalSize) {
#if DEBUG
            DateTime startTime = DateTime.Now;
#endif
            foreach (VariableGridCell child in InternalChildren) {
                if (_lastMeasureViewportRow.Contains(child.Row)
                    && _lastMeasureViewportColumn.Contains(child.Column)) {
                    // arrange
                    child.Visibility = Visibility.Visible;

                    // top
                    double top = 0.0;
                    for (int r = _lastMeasureViewportRow.Start; r < child.Row; r++) {
                        top  += RowHeight[r].Max.Value;
                    }
                    // left
                    double left = 0.0;
                    for (int c = _lastMeasureViewportColumn.Start; c < child.Column; c++) {
                        left += ColumnWidth[c].Max.Value;
                    }

                    child.Arrange(new Rect(left, top, ColumnWidth[child.Column].Max.Value, RowHeight[child.Row].Max.Value));
                } else {
                    child.Visibility = Visibility.Collapsed;
                }
            }

#if DEBUG
            Debug.WriteLine("VirtualizingGridPanel:Arrange(except Clean): {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
            PostArrange();    // TODO: do this in background job

            return finalSize;
        }

#endregion

        #region Clean Up

        private void PostArrange() {
#if DEBUG
            DateTime startTime = DateTime.Now;
#endif
            EnsureCleanupOperation();

            // freeze remaining row and columns
            foreach (var rowHeight in RowHeight) {
                if (rowHeight.Value.Max.HasValue) {
                    rowHeight.Value.Frozen = true;
                }
            }

            foreach (var columnWidth in ColumnWidth) {
                if (columnWidth.Value.Max.HasValue) {
                    columnWidth.Value.Frozen = true;
                }
            }
#if DEBUG
            Debug.WriteLine("VirtualizingGridPanel:PostArrange: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
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
#if DEBUG
                DateTime startTime = DateTime.Now;
#endif
                int begin = -1;
                while (true) {
                    Range cleanBlock = FindFirstChildrenRangeOutsideViewport(ref begin);

                    if (cleanBlock.Count == 0) {
                        break;
                    }

                    for (int i = 0; i < cleanBlock.Count; i++) {
                        var child = (VariableGridCell) InternalChildren[cleanBlock.Start + i];
                        bool removed = Generator.RemoveAt(child.Row, child.Column);

                        Debug.Assert(removed);
                    }
                    RemoveInternalChildRange(cleanBlock.Start, cleanBlock.Count);
                }

                // TODO: move to clean up task, or create Row Column cache
                var rowsOutsideViewport = RowHeight.Keys.Where(key => !_lastMeasureViewportRow.Contains(key)).ToList();
                foreach (var row in rowsOutsideViewport) {
                    RowHeight.Remove(row);
                }

                // TODO: move to clean up task, or create Row Column cache
                var columnsOutsideViewport = ColumnWidth.Keys.Where(key => !_lastMeasureViewportColumn.Contains(key)).ToList();
                foreach (var column in columnsOutsideViewport) {
                    ColumnWidth.Remove(column);
                }
#if DEBUG
                Debug.WriteLine("VirtualizingGridPanel:OnCleanUp: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
            } finally {
                _cleanupOperation = null;
            }
#if DEBUG && ASSERT
            foreach (VariableGridCell child in InternalChildren) {
                if (!_lastMeasureViewportRow.Contains(child.Row)
                    || !_lastMeasureViewportColumn.Contains(child.Column)) {
                    Debug.Fail("Undeleted item is found after CleanUp");
                }
            }
#endif
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
                throw new NotSupportedException($"{typeof(VariableGridPanel2)} supports only ItemsPanel. Can't use stand alone");
            }
            OwningItemsControl = owningItemsControl;

            if (!(owningItemsControl is VariableGrid)) {
                throw new NotSupportedException($"{typeof(VariableGridPanel2)} supports only {typeof(VariableGrid)}'s ItemsPanel");
            }
            this.Generator = ((VariableGrid)owningItemsControl).Generator;

            if (ScrollOwner == null) {
                throw new NotSupportedException($"{typeof(VariableGridPanel2)} must be used for top level scrolling panel");
            }

            if (!GetIsVirtualizing(owningItemsControl)) {
                throw new NotSupportedException($"{typeof(VariableGridPanel2)} supports onlly IsVirtualizing=\"true\"");
            }

            if (GetVirtualizationMode(owningItemsControl) != VirtualizationMode.Standard) {
                throw new NotSupportedException($"{typeof(VariableGridPanel2)} supports onlly VirtualizationMode=\"Standard\"");
            }

            if (GetScrollUnit(owningItemsControl) != ScrollUnit.Pixel) {
                throw new NotSupportedException($"{typeof(VariableGridPanel2)} supports onlly ScrollUnit=\"Pixel\"");
            }

            CacheLength = GetCacheLength(owningItemsControl);

            CacheLengthUnit = GetCacheLengthUnit(owningItemsControl);

            var children = this.Children;   // this ensures that ItemContainerGenerator is not null
        }
    }
}
