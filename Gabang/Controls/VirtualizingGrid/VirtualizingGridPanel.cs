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

            elements = new UIElement[RowCount, ColumnCount];
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        private UIElement[,] elements;  // TODO: do not use 2D element

        public UIElement GenerateAt(int rowIndex, int columnIndex) {
            if (rowIndex < 0 || rowIndex >= RowCount) {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (columnIndex < 0 || columnIndex >= ColumnCount) {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            var element = new VariableGridCell();
            element.Prepare(_dataSource[rowIndex][columnIndex]);

            elements[rowIndex, columnIndex] = element;

            return element;
        }

        public UIElement GetAt(int rowIndex, int columnIndes) {
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
            throw new NotImplementedException();
        }

        public void SetVerticalOffset(double offset) {
            throw new NotImplementedException();
        }

        private double GetLineDelta(Orientation orientation) {
            throw new NotImplementedException();
        }

        private double GetPageDelta(Orientation orientation) {
            throw new NotImplementedException();
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

        int firstRowIndex = 0;  // TODO: bogus value
        int realizedRowCount = 0;

        int firstColumnIndex = 0;
        int realizedColumnCount = 0;

        Dictionary<int, MaxDouble> RowHeight = new Dictionary<int, MaxDouble>();
        Dictionary<int, MaxDouble> ColumnWidth = new Dictionary<int, MaxDouble>();

        protected override Size MeasureOverride(Size availableSize) {
            EnsurePrerequisite();

            // TODO: get the first row in viewport
            // TODO: get the first column in viewport

            Size desiredSize = new Size();
            do {
                MeasureRow(firstRowIndex + realizedRowCount, ref desiredSize);
                realizedRowCount += 1;

                MeasureColumn(firstColumnIndex + realizedColumnCount, ref desiredSize);
                realizedColumnCount += 1;

            } while (desiredSize.Width < availableSize.Width || desiredSize.Height < availableSize.Height);

            // TODO: mark measure pass

            return desiredSize;
        }

        private void MeasureRow(int index, ref Size desiredSize) {
            MaxDouble height = new MaxDouble();

            double width = 0.0;
            int i = firstColumnIndex;
            do {
                UIElement child = Generator.GenerateAt(index, i);
                AddInternalChild(child);

                // TODO: bug. width follows the first element always
                Size constraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
                child.Measure(constraint);

                // adjust to desired size
                if (!ColumnWidth.ContainsKey(i)) {
                    ColumnWidth[i] = new MaxDouble();
                }

                ColumnWidth[i].Max = child.DesiredSize.Width;
                height.Max = child.DesiredSize.Height;

                width += ColumnWidth[i].Max.Value;
                i++;
            } while (i<realizedColumnCount) ;

            RowHeight[index] = height;

            desiredSize.Height += height.Max.Value;
            desiredSize.Width = width;
        }


        private void MeasureColumn(int index, ref Size desiredSize) {
            MaxDouble width = new MaxDouble();

            double height = 0.0;
            for (int i = firstRowIndex; i < realizedRowCount; i++) {
                UIElement child = Generator.GenerateAt(i, index);
                AddInternalChild(child);

                Size constraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
                child.Measure(constraint);

                // adjust to desired size
                RowHeight[i].Max = child.DesiredSize.Width;
                width.Max = child.DesiredSize.Height;

                height += RowHeight[i].Max.Value;
            }

            ColumnWidth[index] = width;

            desiredSize.Height = height;
            desiredSize.Width += width.Max.Value;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            double left = 0.0, top = 0.0;
            for (int r = firstRowIndex; r < realizedRowCount; r++) {
                left = 0.0;
                double height = RowHeight[r].Max.Value;
                for (int c = firstColumnIndex; c < realizedColumnCount; c++) {
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
