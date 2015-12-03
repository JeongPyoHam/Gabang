﻿using System;
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
            row.Header = _dataSource.IndexOf(item);
            row.HeaderStripe = RowHeaderColumn;
            row.Prepare(this, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            DynamicGridRow row = (DynamicGridRow)element;

            row.Header = null;
            row.HeaderStripe = null;

            _realizedRows.Remove(row.RealizedItemLink);
            row.CleanUp(this, item);
        }

        private DynamicGridDataSource _dataSource;
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            _dataSource = newValue as DynamicGridDataSource;
            if (_dataSource == null) {
                throw new NotSupportedException($"JointGrid supports only {typeof(DynamicGridDataSource)} for ItemsSource");
            }

            ExtentWidth = _dataSource.ColumnCount;
            ViewportWidth = 10;
            ScrollableWidth = ExtentWidth - ViewportWidth;

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override Size MeasureOverride(Size constraint) {
            EnsureHorizontalScrollbar();

            return base.MeasureOverride(constraint);
        }

        #endregion override

        #region Column

        internal DynamicGridStripe RowHeaderColumn { get; } = new DynamicGridStripe(Orientation.Vertical, 0);

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

        Size _panelSize;
        LayoutInfo _layoutInfo;
        internal void OnReportPanelSize(Size size) {
            _panelSize = size;

#if PIXELBASED
            double horizontalOffset = HorizontalOffset;
            int itemIndex = (int)Math.Floor(horizontalOffset / EstimatedWidth);

            _layoutInfo = new LayoutInfo() {
                FirstItemIndex = itemIndex,
                FirstItemOffset = horizontalOffset - (itemIndex * EstimatedWidth),
                ItemCountInViewport = (int)Math.Ceiling(size.Width / EstimatedWidth),
            };
#else
            int horizontalOffset = (int)HorizontalOffset;

            double viewportWidth = Math.Ceiling(size.Width / EstimatedWidth);

            ViewportWidth = viewportWidth;
            ScrollableWidth = ExtentWidth - viewportWidth;

            _layoutInfo = new LayoutInfo() {
                FirstItemIndex = horizontalOffset,
                FirstItemOffset = 0.0,
                ItemCountInViewport = (int)viewportWidth,
            };

            var toRemove = _columns.Where(c => c.Key < _layoutInfo.FirstItemIndex || c.Key >= (_layoutInfo.FirstItemIndex + _layoutInfo.ItemCountInViewport)).ToList();
            foreach (var item in toRemove) {
                _columns.Remove(item.Key);
            }
#endif
        }

        internal LayoutInfo GetLayoutInfo(Size size) {
            return _layoutInfo;
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
                OnReportPanelSize(_panelSize);  // refresh viewport

                HorizontalOffset = e.NewValue;

                foreach (var row in _realizedRows) {
                    row.ScrollChanged();
                }
            }
        }

#endregion
    }
}