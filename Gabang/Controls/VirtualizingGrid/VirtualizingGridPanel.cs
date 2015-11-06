using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Gabang.Controls {

    /// <summary>
    /// a panel(vertical) of row panel(horizontal)
    /// send horizontal scroll to each row panel
    /// 
    /// item generation (virtualization) should be synchronized, too
    /// column item layout should be synchronized, too
    /// 
    /// </summary>
    public class VariableGridPanel : VirtualizingStackPanel, IScrollInfo {

        protected override Size MeasureOverride(Size constraint) {
            var desired = base.MeasureOverride(constraint);

            //SetScrollInfo();

            return desired;
        }

        public IScrollInfo ChildScrollInfo { get; private set; }

        private void SetScrollInfo() {
            if (Children.Count > 0) {
                var childPanel = Children[0] as VirtualizingStackPanel;
                if (childPanel != null) {
                    ChildScrollInfo = childPanel;
                }
            }
        }
    }

    public class MaxDouble {
        public MaxDouble() { }
        public MaxDouble(double initialValue) {
            _max = initialValue;
        }

        [DefaultValue(false)]
        public bool Frozen { get; set; }


        double? _max;
        public double? Max {
            get {
                return _max;
            }
            set {
                if (!Frozen) {
                    if (_max.HasValue && value.HasValue) {
                        _max = Math.Max(_max.Value, value.Value);
                    } else {
                        _max = value;
                    }
                }
            }
        }
    }

    public class VariableGridCellGenerator {
        private VariableGridDataSource _dataSource;

        public VariableGridCellGenerator(VariableGridDataSource dataSource) {
            _dataSource = dataSource;
            RowCount = _dataSource.RowCount;
            ColumnCount = _dataSource.ColumnCount;

            elements = new VariableGridCell[RowCount, ColumnCount];
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        private VariableGridCell[,] elements;  // TODO: do not use 2D element

        public VariableGridCell GenerateAt(int rowIndex, int columnIndex) {
            if (rowIndex < 0 || rowIndex >= RowCount) {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (columnIndex < 0 || columnIndex >= ColumnCount) {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            var element = GetAt(rowIndex, columnIndex);
            if (element != null) {
                return element;
            }

            element = new VariableGridCell();
            element.Prepare(_dataSource[rowIndex][columnIndex]);

            elements[rowIndex, columnIndex] = element;

            element.Row = rowIndex;
            element.Column = columnIndex;

            return element;
        }

        public VariableGridCell GetAt(int rowIndex, int columnIndes) {
            return elements[rowIndex, columnIndes];
        }

        public void RemoveAt(int rowIndex, int columnIndex) {
            elements[rowIndex, columnIndex] = null; // TODO: give a chance each element to clean up such as unregistering event handler
        }

        public void RemoveColumn(int column) {
            for (int r = 0; r < RowCount; r++) {
                RemoveAt(r, column);
            }
        }

        public void RemoveRow(int row) {
            for (int c = 0; c < ColumnCount; c++) {
                RemoveAt(row, c);
            }
        }

        public void RemoteAll() {
            for (int r = 0; r < RowCount; r++) {
                RemoveRow(r);
            }
        }
    }

    /// <summary>
    /// assumes, IsVirtualizing = true, VirtualizationMode = Standard, ScrollUnit = Pixel, CacheLength = 1, CacheLengthUnit = Item
    /// </summary>
    public class VariableGridPanel2 : VirtualizingPanel, IScrollInfo {

        #region IScrollInfo

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

        public void SetHorizontalOffset(double offset) {
            if (offset >= 0 && offset < ExtentWidth) {
                HorizontalOffset = offset;
                InvalidateMeasure();    // TODO: if IsVirtualizing==false, InvalidaArrange() should be enough
            }
        }

        public void SetVerticalOffset(double offset) {
            if (offset >= 0 && offset < ExtentWidth) {
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

        private VariableGridCellGenerator Generator { get; set; }

        #region Overrides

        int _realizedRowIndex = 0;  // TODO: bogus value
        int _realizedRowCount = 0;

        int _realizedColumnIndex = 0;
        int _realizedColumnCount = 0;

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

            Size desiredSize = MeasureChild(viewportRow.Start, viewportColumn.Start);
            viewportRow.Count++;
            rowIndex++;
            viewportColumn.Count++;
            columnIndex++;

            bool isRow = true;
            while (!IsBiggerThan(desiredSize, availableSize)) {
                if (isRow) {
                    MeasureRow(rowIndex, ref desiredSize, ref viewportColumn);
                    viewportRow.Count++;
                    rowIndex++;
                } else {
                    MeasureColumn(columnIndex, ref desiredSize, ref viewportRow);
                    viewportColumn.Count++;
                    columnIndex++;
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

        private Range FindFirstChildrenRangeOutsideViewport() {
            Range firstBlock = new Range();
            for (int i = InternalChildren.Count - 1; i >= 0; i--) {
                VariableGridCell child = (VariableGridCell) InternalChildren[i];

                if (!_lastMeasureViewportRow.Contains(child.Row)
                    || !_lastMeasureViewportColumn.Contains(child.Column)) {
                    if (firstBlock.Count == 0) {
                        firstBlock.Start = i;
                        firstBlock.Count++;
                    }
                } else {
                    if (firstBlock.Count != 0) {
                        break;
                    }
                }
            }
            return firstBlock;
        }

        private void Clean() {
#if DEBUG
            DateTime startTime = DateTime.Now;
#endif
            while (true) {
                Range cleanBlock = FindFirstChildrenRangeOutsideViewport();

                if (cleanBlock.Count == 0) {
                    break;
                }

                RemoveInternalChildRange(cleanBlock.Start, cleanBlock.Count);
            }

            var rowsOutsideViewport = RowHeight.Keys.Where(key => !_lastMeasureViewportRow.Contains(key)).ToList();
            foreach (var row in rowsOutsideViewport) {
                RowHeight.Remove(row);
                Generator.RemoveRow(row);
            }

            var columnsOutsideViewport = ColumnWidth.Keys.Where(key => !_lastMeasureViewportColumn.Contains(key)).ToList();
            foreach (var column in columnsOutsideViewport) {
                ColumnWidth.Remove(column);
                Generator.RemoveColumn(column);
            }

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
            Debug.WriteLine("VirtualizingGridPanel:Clean: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
        }

        private Size GetIntersectSize(int rowIndex, int columnIndex, out int rowIntersectLimit, out int columnIntersectLimit) {
            Size interSect = new Size();

            double height = 0.0;
            int r = Math.Max(rowIndex, _realizedRowIndex);
            while (r < (_realizedRowIndex + _realizedRowCount)) {
                height += RowHeight[r].Max.Value;
                r++;
            }

            double width = 0.0;
            int c = Math.Max(columnIndex, _realizedColumnIndex);
            while (c < (_realizedColumnIndex + _realizedColumnCount)) {
                width += ColumnWidth[c].Max.Value;
                c++;
            }

            interSect.Height = height;
            interSect.Width = width;

            rowIntersectLimit = r;
            columnIntersectLimit = c;

            return interSect;
        }

        private bool IsBiggerThan(Size left, Size right) {
            return (left.Width >= right.Width) && (left.Height >= right.Height);
        }

        private void UpdateScrollInfo() {
            if (Generator != null) {
                ExtentWidth = Generator.ColumnCount;
                ExtentHeight = Generator.RowCount;
                ViewportWidth = _lastMeasureViewportColumn.Count;
                ViewportHeight = _lastMeasureViewportRow.Count;
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        private Size MeasureChild(int row, int column) {
            if (_lastMeasureViewportRow.Contains(row) && _lastMeasureViewportColumn.Contains(column)) {
                Debug.Assert(RowHeight.ContainsKey(row) && ColumnWidth.ContainsKey(column));
                Debug.Assert(Generator.GetAt(row, column) != null);

                // TODO: measure again? maybe, not
                return new Size(ColumnWidth[column].Max.Value, RowHeight[row].Max.Value);
            } else {
                var child = Generator.GenerateAt(row, column);
                AddInternalChild(child);

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

                child.Measure(new Size(heightConstraint, widthConstraint));

                if (updateWidth) {
                    ColumnWidth[column].Max = child.DesiredSize.Width;
                }
                if (updateHeight) {
                    RowHeight[row].Max = child.DesiredSize.Height;
                }

                return child.DesiredSize;
            }
        }

        private void MeasureRow(int row, ref Size desiredSize, ref Range viewportColumn) {
            double width = 0.0;
            int column = viewportColumn.Start;
            do {
                Size childDesiredSize = MeasureChild(row, column);

                width += childDesiredSize.Width;

                column++;
            } while (viewportColumn.Contains(column)) ;

            desiredSize.Height += RowHeight[row].Max.Value;
            desiredSize.Width = width;
        }


        private void MeasureColumn(int column, ref Size desiredSize, ref Range viewportRow) {
            double height = 0.0;
            int row = viewportRow.Start;
            do {
                Size childDesiredSize = MeasureChild(row, column);

                height += childDesiredSize.Height;

                row++;
            } while (viewportRow.Contains(row));

            desiredSize.Height = height;
            desiredSize.Width = ColumnWidth[column].Max.Value;
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
            Clean();    // TODO: do this in background job

            return finalSize;
        }

        #endregion

        internal ItemsControl OwningItemsControl { get; private set; }

        internal VirtualizationCacheLength CacheLength { get; private set; }

        internal VirtualizationCacheLengthUnit CacheLengthUnit { get; private set; }

        void EnsurePrerequisite() {
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
