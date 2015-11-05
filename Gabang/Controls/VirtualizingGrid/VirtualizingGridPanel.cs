using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public void RemoteAll() {

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
            throw new NotImplementedException();
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

        Range _viewportRow = new Range();
        Range _viewportColumn = new Range();

        Dictionary<int, MaxDouble> RowHeight = new Dictionary<int, MaxDouble>();
        Dictionary<int, MaxDouble> ColumnWidth = new Dictionary<int, MaxDouble>();

        protected override Size MeasureOverride(Size availableSize) {
            EnsurePrerequisite();

            // TODO: get the first row in viewport
            // TODO: get the first column in viewport
            int rowIndex = (int)VerticalOffset;
            int columnIndex = (int)HorizontalOffset;

            _viewportRow.Start = rowIndex;
            _viewportColumn.Start = columnIndex;

            // TODO: assume intersect left top corner
            Size desiredSize = GetIntersectSize(rowIndex, columnIndex, out rowIndex, out columnIndex);

            if (!IsBiggerThan(desiredSize, availableSize)) {
                do {
                    if (desiredSize.Height < availableSize.Height) {
                        MeasureRow(rowIndex, ref desiredSize);
                        rowIndex++;
                        if (columnIndex == 0) { // TODO: hack!!
                            columnIndex++;
                        }
                    }

                    if (desiredSize.Width < availableSize.Width) {
                        MeasureColumn(columnIndex, ref desiredSize);
                        columnIndex++;
                    }
                } while (!IsBiggerThan(desiredSize, availableSize));
            }

            _viewportRow.Count = rowIndex - _viewportRow.Start;
            _viewportColumn.Count = columnIndex - _viewportColumn.Start;

            // TODO: mark measure pass
            UpdateScrollInfo();

            return desiredSize;
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
                ViewportWidth = 1;
                ViewportHeight = 1;
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        private void MeasureRow(int index, ref Size desiredSize) {
            MaxDouble height = new MaxDouble();

            double width = 0.0;
            int column = _viewportColumn.Start;
            do {
                UIElement child = Generator.GenerateAt(index, column);
                AddInternalChild(child);

                // TODO: bug. width follows the first element always
                Size constraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
                child.Measure(constraint);

                if (!ColumnWidth.ContainsKey(column)) {
                    ColumnWidth[column] = new MaxDouble();
                }

                ColumnWidth[column].Max = child.DesiredSize.Width;
                height.Max = child.DesiredSize.Height;

                width += ColumnWidth[column].Max.Value;
                column++;
            } while (column < _realizedColumnCount) ;

            RowHeight[index] = height;

            desiredSize.Height += height.Max.Value;
            desiredSize.Width = width;

            _realizedRowCount++;
            if (_realizedColumnCount == 0) {
                _realizedColumnCount++;
            }
        }


        private void MeasureColumn(int index, ref Size desiredSize) {
            MaxDouble width = new MaxDouble();

            double height = 0.0;
            int row = _viewportRow.Start;
            do {
                UIElement child = Generator.GenerateAt(row, index);
                AddInternalChild(child);

                Size constraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
                child.Measure(constraint);

                if (!RowHeight.ContainsKey(row)) {
                    RowHeight[row] = new MaxDouble();
                }

                RowHeight[row].Max = child.DesiredSize.Width;
                width.Max = child.DesiredSize.Height;

                height += RowHeight[row].Max.Value;
                row++;
            } while (row < _realizedRowCount);

            ColumnWidth[index] = width;

            desiredSize.Height = height;
            desiredSize.Width += width.Max.Value;

            _realizedColumnCount++;
            if (_realizedRowCount == 0) {
                _realizedRowCount++;
            }
        }

        protected override Size ArrangeOverride(Size finalSize) {
            double left = 0.0, top = 0.0;
            for (int r = _viewportRow.Start; r < (_viewportRow.Start + _viewportRow.Count); r++) {
                left = 0.0;
                double height = RowHeight[r].Max.Value;
                for (int c = _viewportColumn.Start; c < (_viewportColumn.Start + _viewportColumn.Count); c++) {
                    var element = Generator.GetAt(r, c);
                    double width = ColumnWidth[c].Max.Value;
                    element.Arrange(new Rect(left, top, width, height));
                    left += width;
                }
                top += height;
            }

            return new Size(left, top);
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
