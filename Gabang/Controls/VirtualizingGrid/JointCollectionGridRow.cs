using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Gabang.Controls {
    /// <summary>
    /// Row of <see cref="JointCollectionGrid"/>, which is ItemsControl itself
    /// </summary>
    public class JointCollectionGridRow : ItemsControl {
        static JointCollectionGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JointCollectionGridRow), new FrameworkPropertyMetadata(typeof(JointCollectionGridRow)));
        }

        public JointCollectionGridRow() {
        }

        public object Header { get; set; }

        public double EstimatedHeight {
            get {
                return 40;
            }
        }

        internal IScrollInfo ScrollOwner { get; private set; }

        internal JointCollectionGrid OwningJointGrid { get; private set; }

        protected override DependencyObject GetContainerForItemOverride() {
            return new JointCollectionGridCell();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            var cell = (JointCollectionGridCell)element;
            cell.Prepare(this, item);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        internal void Prepare(JointCollectionGrid owner, object item) {
            if (!(item is IList)) {
                throw new NotSupportedException("JointCollectionGridRow supports only IList for item");
            }

            OwningJointGrid = owner;

            var items = (IList)item;
            ItemsSource = items;
        }

        internal void Clear(JointCollectionGrid owner, object item) {
        }

        internal void NotifyScroll(ScrollEventArgs e) {
            ScrollOwner?.SetHorizontalOffset(e.NewValue);
        }

        protected override Size MeasureOverride(Size constraint) {
            var desired = base.MeasureOverride(constraint);

            if (this.ScrollOwner == null) {

                var sv = (VirtualizingStackPanel)ControlHelper.GetChild(this, typeof(VirtualizingStackPanel));
                this.ScrollOwner = sv;

                OwningJointGrid.NotifyScrollInfo(ScrollOwner.ExtentWidth, ScrollOwner.HorizontalOffset, ScrollOwner.ViewportWidth);
            }

            return desired;
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            var arranged = base.ArrangeOverride(arrangeBounds);

            return arranged;
        }
    }
}
