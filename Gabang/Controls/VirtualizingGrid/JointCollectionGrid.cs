using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class JointCollectionGrid : MultiSelector {

        static JointCollectionGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JointCollectionGrid), new FrameworkPropertyMetadata(typeof(JointCollectionGrid)));
        }

        public JointCollectionGrid() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        public JointCollectionGridColumnCollection Columns { get; private set; }


        private void Bar_Scroll(object sender, ScrollEventArgs e) {
            foreach (var row in _visibleRows) {
                row.NotifyScroll(e);
            }
        }

        List<JointCollectionGridRow> _visibleRows = new List<JointCollectionGridRow>();
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

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new JointCollectionGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            JointCollectionGridRow row = (JointCollectionGridRow)element;
            row.Prepare(this, item);

            _visibleRows.Add(row);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            JointCollectionGridRow row = (JointCollectionGridRow)element;
            row.Clear(this, item);

            _visibleRows.Remove(row);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (!(newValue is GridDataSource)) {
                throw new NotSupportedException($"JointGrid supports only {typeof(GridDataSource)} for ItemsSource");
            }

            base.OnItemsSourceChanged(oldValue, newValue);

            Columns = new JointCollectionGridColumnCollection(((GridDataSource)newValue).ColumnCount);
        }

        protected override Size MeasureOverride(Size constraint) {
            var desired = base.MeasureOverride(constraint);

            return desired;
        }

        #endregion override
    }
}
