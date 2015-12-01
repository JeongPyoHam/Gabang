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
    /// <summary>
    /// ItemsControl for two dimentional collection; collection of collection
    /// Need to put some restriction on IList<IList<T>>
    /// 
    /// GridLine
    /// Row Header
    /// Column Header
    /// </summary>
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

            _isBarSet = true;
            var bar = (ScrollBar)ControlHelper.GetChild(this, "HorizontalScrollBar");

            bar.Maximum = max;
            bar.Value = offset;
            bar.ViewportSize = viewportSize;

            bar.Scroll += Bar_Scroll;
        }

        public VariableGridCellGenerator Generator { get; set; }

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

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

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (!(newValue is VariableGridDataSource)) {
                throw new NotSupportedException($"JointGrid supports only {typeof(VariableGridDataSource)} for ItemsSource");
            }

            base.OnItemsSourceChanged(oldValue, newValue);

            this.Generator = new VariableGridCellGenerator((VariableGridDataSource)newValue);
        }

        protected override Size MeasureOverride(Size constraint) {
            var desired = base.MeasureOverride(constraint);

            return desired;
        }

        #endregion override
    }

}
