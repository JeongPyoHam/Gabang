using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Gabang.Controls {

    internal class SharedScrollChangedEventArgs : EventArgs {
        public SharedScrollChangedEventArgs(Orientation orientation, double extent, double viewport, double offset) {
            Orientation = orientation;
            Extent = extent;
            Viewport = viewport;
            Offset = offset;
        }

        public Orientation Orientation { get; }

        public double Extent { get; }

        public double Viewport { get; }

        public double Offset { get; }
    }

    internal interface SharedScrollInfo {
        event EventHandler<SharedScrollChangedEventArgs> SharedScrollChanged;
    }


    internal class DynamicGridStackPanel : VirtualizingPanel, IScrollInfo {
        private const double ItemMinWidth = 20;
        private SharedScrollInfo _sharedScrollInfo;

        #region Layout

        private bool sharedscrollinit = false;
        private void EnsurePrerequisite() {
            var children = Children;

            if (!sharedscrollinit) {
                _sharedScrollInfo = ItemsControl.GetItemsOwner(this) as SharedScrollInfo;
                if (_sharedScrollInfo != null) {
                    _sharedScrollInfo.SharedScrollChanged += SharedScrollChanged;
                }

                sharedscrollinit = true;
            }
        }

        private void SharedScrollChanged(object sender, SharedScrollChangedEventArgs e) {
            if (e.Orientation == Orientation.Horizontal) {
                HorizontalOffset = e.Offset;
                ExtentWidth = e.Extent;
                ViewportWidth = e.Viewport;

                InvalidateMeasure();
            } else {
                throw new NotImplementedException();
            }
        }

        protected override Size MeasureOverride(Size availableSize) {
            EnsurePrerequisite();

            int startIndex = (int) Math.Floor(HorizontalOffset / ItemMinWidth);
            int viewportCount = (int) Math.Ceiling(availableSize.Width / ItemMinWidth);

            IItemContainerGenerator generator = this.ItemContainerGenerator;
            GeneratorPosition position = generator.GeneratorPositionFromIndex(startIndex);

            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            int childIndex = (position.Offset == 0) ? position.Index : position.Index + 1;

            double height = this.ActualHeight;
            double width = 0;
            int finalCount = 0;
            using (generator.StartAt(position, GeneratorDirection.Forward, true)) {
                for (int i = 0; i < viewportCount; i++, childIndex++) {
                    bool newlyRealized;
                    DynamicGridCell child = generator.GenerateNext(out newlyRealized) as DynamicGridCell;
                    Debug.Assert(child != null);

                    if (newlyRealized) {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= InternalChildren.Count) {
                            AddInternalChild(child);
                        } else {
                            InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    } else {
                        Debug.Assert(child == InternalChildren[childIndex]);
                    }

                    double availableWidth = child.ColumnStripe.GetSizeConstraint();
                    child.Measure(new Size(availableWidth, double.PositiveInfinity));
                    if (child.DesiredSize.Height > height) {
                        height = child.DesiredSize.Height;
                    }

                    child.ColumnStripe.LayoutSize.Max = child.DesiredSize.Width;

                    width += child.DesiredSize.Width;
                    finalCount++;

                    if (width > availableSize.Width) {
                        break;
                    }
                }
            }

            Debug.WriteLine("Available:{0} Desired:{1} start:{2} count:{3}", availableSize.Width, width, startIndex, finalCount);

            Size desired = new Size(width, height);

            UpdateScrollInfo(desired);

            Debug.Assert(finalCount >= 1);
            CleanUpItems(startIndex, startIndex + finalCount - 1);

            return desired;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            UpdateScrollInfo(finalSize);

            Debug.WriteLine("Arrange:Items.Count:{0}", InternalChildren.Count);

            double x = 0.0;
            for (int i = 0; i < InternalChildren.Count; i++) {
                var child = InternalChildren[i] as DynamicGridCell;
                Debug.Assert(child != null);

                child.Arrange(new Rect(x, 0, child.ColumnStripe.LayoutSize.Max, finalSize.Height));
                x += child.ColumnStripe.LayoutSize.Max;
            }

            return finalSize;
        }

        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated) {
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            Debug.WriteLine("Cleanup: min:{0} max:{1} start:{2} end:{3}", minDesiredGenerated, maxDesiredGenerated, InternalChildren[0].ToString(), InternalChildren[InternalChildren.Count -1].ToString());

            for (int i = children.Count - 1; i >= 0; i--) {
                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated) {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        #endregion

        #region IScrollInfo support

        private void UpdateScrollInfo(Size size) {
            if (_sharedScrollInfo == null) {
                ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
                int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

                ViewportWidth = size.Width;
                ViewportHeight = size.Height;
                ExtentWidth = itemCount * ItemMinWidth;   // item width
                ExtentHeight = size.Height;
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        public bool CanVerticallyScroll { get; set; }

        public bool CanHorizontallyScroll { get; set; }

        public double ExtentWidth { get; private set; }

        public double ExtentHeight { get; private set; }

        public double ViewportWidth { get; private set; }

        public double ViewportHeight { get; private set; }

        public double HorizontalOffset { get; private set; }

        public double VerticalOffset { get; private set; }

        public ScrollViewer ScrollOwner { get; set; }

        public void LineUp() {
            throw new NotImplementedException();
        }

        public void LineDown() {
            throw new NotImplementedException();
        }

        public void LineLeft() {
            throw new NotImplementedException();
        }

        public void LineRight() {
            throw new NotImplementedException();
        }

        public void PageUp() {
            throw new NotImplementedException();
        }

        public void PageDown() {
            throw new NotImplementedException();
        }

        public void PageLeft() {
            throw new NotImplementedException();
        }

        public void PageRight() {
            throw new NotImplementedException();
        }

        public void MouseWheelUp() {
            throw new NotImplementedException();
        }

        public void MouseWheelDown() {
            throw new NotImplementedException();
        }

        public void MouseWheelLeft() {
            throw new NotImplementedException();
        }

        public void MouseWheelRight() {
            throw new NotImplementedException();
        }

        public void SetHorizontalOffset(double offset) {
            offset = CoerceOffset(offset, ViewportWidth, ExtentWidth);

            if (HorizontalOffset != offset) {   // TODO: use close instead of equality as it is double
                HorizontalOffset = offset;
                InvalidateMeasure();
            }
        }

        public void SetVerticalOffset(double offset) {
            offset = CoerceOffset(offset, ViewportHeight, ExtentHeight);

            if (VerticalOffset != offset) {
                VerticalOffset = offset;
                InvalidateMeasure();
            }
        }

        private double CoerceOffset(double offset, double viewport, double extent) {
            offset = Math.Floor(offset);    // TODO: Pixel scroll

            if (offset > extent - viewport) {
                offset = extent - viewport;
            }
            if (offset < 0.0) {
                offset = 0.0;
            }

            return offset;
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
