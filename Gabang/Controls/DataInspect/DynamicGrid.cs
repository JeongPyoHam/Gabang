using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Gabang.Controls {
    public class DynamicGrid : MultiSelector {

        static DynamicGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGrid), new FrameworkPropertyMetadata(typeof(DynamicGrid)));
        }

        public DynamicGrid() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        private void Bar_Scroll(object sender, ScrollEventArgs e) {
            foreach (var row in _visibleRows) {
                row.NotifyScroll(e);
            }

            Debug.WriteLine($"Horizontal ScrollBar: Row count({_visibleRows.Count})");
        }

        List<DynamicGridRow> _visibleRows = new List<DynamicGridRow>();
        bool _isBarSet = false;
        internal void NotifyScrollInfo(double max, double offset, double viewportSize) {
            if (_isBarSet) return;

            
            var bar = (ScrollBar)ControlHelper.GetChild(this, "HorizontalScrollBar");
            if (bar != null) {
                bar.Maximum = max;
                bar.Value = offset;
                bar.ViewportSize = viewportSize;

                bar.Scroll += Bar_Scroll;

                _isBarSet = true;
            }
        }

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            int rowIndex = Items.IndexOf(item);

            DynamicGridRow row = (DynamicGridRow)element;
            row.Prepare(this, item);

            _visibleRows.Add(row);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            DynamicGridRow row = (DynamicGridRow)element;
            row.Clear(this, item);

            _visibleRows.Remove(row);
        }

        private DynamicGridDataSource _dataSource;
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            _dataSource = newValue as DynamicGridDataSource;
            if (_dataSource == null) {
                throw new NotSupportedException($"JointGrid supports only {typeof(DynamicGridDataSource)} for ItemsSource");
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override Size MeasureOverride(Size constraint) {
            NotifyScrollInfo(_dataSource.ColumnCount, 0, 10);

            var desired = base.MeasureOverride(constraint);

            return desired;
        }

        #endregion override

        #region Column

        private SortedList<int, DynamicGridStripe> _columns = new SortedList<int, DynamicGridStripe>();
        internal DynamicGridStripe GetColumn(int index) {
            DynamicGridStripe stack;
            if (_columns.TryGetValue(index, out stack)) {
                return stack;
            }

            stack = new DynamicGridStripe(Orientation.Vertical, index);
            _columns.Add(index, stack);

            return stack;
        }

        #endregion
    }

}
