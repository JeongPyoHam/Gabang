using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    public class JointCollectionGridCell : ContentControl {

        private JointCollectionGrid _grid;
        private JointCollectionGridRow _row;
        private JointCollectionGridColumn _column;



        internal void Prepare(JointCollectionGridRow owningRow, object item) {
            _row = owningRow;
            int columnIndex = _row.ItemContainerGenerator.IndexFromContainer(this);

            _column = _row.OwningJointGrid.Columns[columnIndex];

            this.Content = item;
        }

        protected override Size MeasureOverride(Size constraint) {
            return new Size(_column.EstimatedWidth, _row.EstimatedHeight);
        }
    }
}
