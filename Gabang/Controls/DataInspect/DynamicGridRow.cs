using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal class DynamicGridRow : ItemsControl, SharedScrollInfo {
        private LinkedList<DynamicGridCell> _realizedCells = new LinkedList<DynamicGridCell>();

        static DynamicGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridRow), new FrameworkPropertyMetadata(typeof(DynamicGridRow)));
        }

        public DynamicGridRow() {
            RealizedItemLink = new LinkedListNode<DynamicGridRow>(this);
        }

        internal LinkedListNode<DynamicGridRow> RealizedItemLink { get; }

        public static readonly DependencyProperty HeaderProperty =
                DependencyProperty.Register(
                        "Header",
                        typeof(object),
                        typeof(DynamicGridRow),
                        new FrameworkPropertyMetadata(null));

        public object Header {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        internal DynamicGridStripe HeaderStripe { get; set; }


        internal DynamicGrid OwningJointGrid { get; private set; }

        #region SharedScrollInfo support

        public LayoutInfo GetLayoutInfo(Size size) {
            Debug.Assert(OwningJointGrid != null);
            return OwningJointGrid.GetLayoutInfo(size);
        }

        public event EventHandler SharedScrollChanged;

        #endregion

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridCell();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            _realizedCells.AddFirst(cell.RealizedItemLink);
            int column = this.Items.IndexOf(item);
            if (column == -1) {
                throw new InvalidOperationException("Item is not found in collection");
            }
            cell.Prepare(OwningJointGrid.GetColumn(column));
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            _realizedCells.Remove(cell.RealizedItemLink);
            cell.CleanUp();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            // row header
            var rowHeader = (DynamicGridRowHeader)ControlHelper.GetChild(this, typeof(DynamicGridRowHeader));
            if (rowHeader != null) {
                rowHeader.Prepare(OwningJointGrid.RowHeaderColumn);
            }
        }

        internal void Prepare(DynamicGrid owner, object item) {
            if (!(item is IList)) {
                throw new NotSupportedException("JointCollectionGridRow supports only IList for item");
            }

            OwningJointGrid = owner;

            var items = (IList)item;
            ItemsSource = items;

            
        }

        internal void CleanUp(DynamicGrid owner, object item) {
            var rowHeader = (DynamicGridRowHeader)ControlHelper.GetChild(this, typeof(DynamicGridRowHeader));
            if (rowHeader != null) {
                rowHeader.CleanUp();

                foreach (var cell in _realizedCells) {
                    cell.CleanUp();
                }
                _realizedCells.Clear();
            }
        }

        internal void ScrollChanged() {
            if (SharedScrollChanged != null) {
                SharedScrollChanged(this, EventArgs.Empty);
            }
        }
    }
}
