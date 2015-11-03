using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls.VirtualizingGrid {
    public class JointCollectionGridCell : ContentControl {

        private JointCollectionGrid OwningGrid { get; set; }

        internal void Prepare(JointCollectionGrid owningGrid, object item) {
            OwningGrid = owningGrid;
            this.Content = item;
        }

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }
    }
}
