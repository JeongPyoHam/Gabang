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
using System.Windows.Input;

namespace Gabang.Controls {
    public class DynamicGrid : MultiSelector {

        private LinkedList<DynamicGridRow> _realizedRows = new LinkedList<DynamicGridRow>();

        static DynamicGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGrid), new FrameworkPropertyMetadata(typeof(DynamicGrid)));
        }

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            DynamicGridRow row = (DynamicGridRow)element;
            _realizedRows.AddFirst(row.RealizedItemLink);
            row.Prepare(this, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            DynamicGridRow row = (DynamicGridRow)element;
            _realizedRows.Remove(row.RealizedItemLink);
            row.Clear(this, item);
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
            EnsureHorizontalScrollbar();

            return base.MeasureOverride(constraint);
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
            stack.LayoutSize.MaxChanged += LayoutSize_MaxChanged; // TODO: clean up columns
            _columns.Add(index, stack);

            return stack;
        }

        private void LayoutSize_MaxChanged(object sender, EventArgs e) {
            double extent = ComputeLayoutPosition();
            ExtentWidth = extent;
            ScrollableWidth = extent - 20.0;
            ViewportWidth = 20.0;
        }

        private const double EstimatedWidth = 20.0; // TODO: configurable

        internal double ComputeLayoutPosition() {
            int index = 0;
            double acc = 0.0;
            foreach (var keyvalue in _columns) {
                DynamicGridStripe column = keyvalue.Value;
                if (column.Index != index) {
                    acc += EstimatedWidth * (column.Index - index);
                }

                column.LayoutPosition = acc;
                acc += column.LayoutSize.Max;
                index = column.Index + 1;
            }

            acc += (_dataSource.ColumnCount - index) * EstimatedWidth;

            return acc;
        }

        public static readonly DependencyProperty ScrollableWidthProperty =
                DependencyProperty.Register(
                        "ScrollableWidth",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double ScrollableWidth {
            get {
                return (double) GetValue(ScrollableWidthProperty);
            }
            set {
                SetValue(ScrollableWidthProperty, value);
            }
        }

        public static readonly DependencyProperty ExtentWidthProperty =
                DependencyProperty.Register(
                        "ExtentWidth",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double ExtentWidth {
            get {
                return (double)GetValue(ExtentWidthProperty);
            }
            set {
                SetValue(ExtentWidthProperty, value);
            }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty =
                DependencyProperty.Register(
                        "HorizontalOffset",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double HorizontalOffset {
            get {
                return (double)GetValue(HorizontalOffsetProperty);
            }
            set {
                SetValue(HorizontalOffsetProperty, value);
            }
        }

        public static readonly DependencyProperty ViewportWidthProperty =
                DependencyProperty.Register(
                        "ViewportWidth",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double ViewportWidth {
            get {
                return (double)GetValue(ViewportWidthProperty);
            }
            set {
                SetValue(ViewportWidthProperty, value);
            }
        }

        private bool _scrollbar = false;
        private void EnsureHorizontalScrollbar() {
            if (!_scrollbar) {

                var scrollbar = TreeHelper.FindChild<ScrollBar>(this, (bar) => bar.Name == "HorizontalScrollBar");
                if (scrollbar != null) {
                    scrollbar.Scroll += Scrollbar_Scroll; ;
                }

                _scrollbar = true;
            }
        }

        private void Scrollbar_Scroll(object sender, ScrollEventArgs e) {
            if (e.ScrollEventType == ScrollEventType.EndScroll) {
                HorizontalOffset = e.NewValue;

                foreach (var row in _realizedRows) {
                    row.ScrollChanged();
                }
            }
        }

        #endregion
    }
}
