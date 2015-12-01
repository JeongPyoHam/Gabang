using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    /// <summary>
    /// Grid item container that maps to a cell in grid
    /// </summary>
    public class DynamicGridCell : ContentControl {
        static DynamicGridCell() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridCell), new FrameworkPropertyMetadata(typeof(DynamicGridCell)));
        }

        /// <summary>
        /// Vertical item index
        /// </summary>
        [DefaultValue(-1)]
        public int Row { get; set; }

        /// <summary>
        /// Horizontal item index
        /// </summary>
        [DefaultValue(-1)]
        public int Column { get; set; }

        internal DynamicGridStripe RowStripe { get; set; }

        internal DynamicGridStripe ColumnStripe { get; set; }

        internal void Prepare(DynamicGridStripe columnStipe) {
            if (ColumnStripe != null) {
                ColumnStripe.LayoutSize.MaxChanged -= LayoutSize_MaxChanged;
            }

            ColumnStripe = columnStipe;
            ColumnStripe.LayoutSize.MaxChanged += LayoutSize_MaxChanged;
        }

        private void LayoutSize_MaxChanged(object sender, EventArgs e) {
            InvalidateMeasure();
        }



        /// <summary>
        /// Clean up data when virtualized
        /// </summary>
        internal void CleanUp() {
            this.Content = null;

            if (ColumnStripe != null) {
                ColumnStripe.LayoutSize.MaxChanged -= LayoutSize_MaxChanged;
            }
        }

        protected override Size MeasureOverride(Size constraint) {

            if (RowStripe != null) {
                constraint.Height = Math.Min(constraint.Height, RowStripe.GetSizeConstraint());
            }

            if (ColumnStripe != null) {
                constraint.Width = Math.Min(constraint.Width, ColumnStripe.GetSizeConstraint());
            }

            Size desired = base.MeasureOverride(constraint);

            if (RowStripe != null) {
                RowStripe.LayoutSize.Max = desired.Height;
            }

            if (ColumnStripe != null) {
                ColumnStripe.LayoutSize.Max = desired.Width;
                desired.Width = ColumnStripe.LayoutSize.Max;
            }

            return desired;
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            if (ColumnStripe != null) {
                arrangeBounds.Width = ColumnStripe.LayoutSize.Max;
            }

            return base.ArrangeOverride(arrangeBounds);
        }
    }
}
