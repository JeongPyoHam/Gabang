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
            _value = initialValue;
        }

        [DefaultValue(false)]
        public bool Frozen { get; set; }


        double? _value;
        public double? Value {
            get {
                return _value;
            }
            set {
                if (!Frozen) {
                    if (_value.HasValue && value.HasValue) {
                        _value = Math.Max(_value.Value, value.Value);
                    } else {
                        _value = value;
                    }
                }
            }
        }
    }

    public interface IVariableGridCellGenerator {
        int RowCount { get; }
        int ColumnCount { get; }

        UIElement GenerateAt(int rowIndex, int columnIndex);

        void RemoteAll();
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

        private IVariableGridCellGenerator Generator { get; set; }

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

            Size desiredSize = Size.Empty;
            do {
                MeasureRow(firstRowIndex + realizedRowCount, ref desiredSize);
                realizedRowCount += 1;

                MeasureColumn(firstColumnIndex + realizedColumnCount, ref desiredSize);
                realizedColumnCount += 1;

            } while (desiredSize.Width < availableSize.Width || desiredSize.Height < availableSize.Height);

            // mark measure pass

            throw new NotImplementedException();
        }

        private void MeasureRow(int index, ref Size desiredSize) {
            int firstColumnIndex = 0, realizedColumnCount = 0;

            MaxDouble height = new MaxDouble();

            for (int i = firstColumnIndex; i < realizedColumnCount; i++) {
                UIElement child = Generator.GenerateAt(index, i);

                Size infinity = new Size(double.PositiveInfinity, (ColumnWidth[i]??new MaxDouble(double.PositiveInfinity)).Value??double.PositiveInfinity);
                child.Measure(infinity);

                // adjust to desired size
                ColumnWidth[i].Value = child.DesiredSize.Width;
                height.Value = child.DesiredSize.Height;
            }

            RowHeight[index] = height;
        }


        private void MeasureColumn(int index, ref Size desiredSize) {
            throw new NotImplementedException();
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
