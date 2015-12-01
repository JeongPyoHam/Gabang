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
    /// Row of <see cref="VariableGrid"/>, which is ItemsControl itself
    /// </summary>
    public class DynamicGridRow : ItemsControl {
        static DynamicGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridRow), new FrameworkPropertyMetadata(typeof(DynamicGridRow)));
        }

        public DynamicGridRow() {
        }

        public object Header { get; set; }

        internal IScrollInfo ScrollOwner { get; private set; }

        internal DynamicGrid OwningJointGrid { get; private set; }

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridCell();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;

            int column = this.Items.IndexOf(item);
            if (column == -1) {
                throw new InvalidOperationException("Item is not found in collection");
            }
            cell.Prepare(OwningJointGrid.GetColumn(column));
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            cell.CleanUp();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        internal void Prepare(DynamicGrid owner, object item) {
            if (!(item is IList)) {
                throw new NotSupportedException("JointCollectionGridRow supports only IList for item");
            }

            OwningJointGrid = owner;

            var items = (IList)item;
            ItemsSource = items;
        }

        internal void Clear(DynamicGrid owner, object item) {
        }

        internal void NotifyScroll(ScrollEventArgs e) {
            ScrollOwner?.SetHorizontalOffset(e.NewValue);
        }

        protected override Size MeasureOverride(Size constraint) {
            var desired = base.MeasureOverride(constraint);

            if (this.ScrollOwner == null) {

                var sv = (VirtualizingStackPanel)ControlHelper.GetChild(this, typeof(VirtualizingStackPanel));
                this.ScrollOwner = sv;

                //OwningJointGrid.NotifyScrollInfo(ScrollOwner.ExtentWidth, ScrollOwner.HorizontalOffset, ScrollOwner.ViewportWidth);
            }

            return desired;
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            var arranged = base.ArrangeOverride(arrangeBounds);

            return arranged;
        }
    }
}
