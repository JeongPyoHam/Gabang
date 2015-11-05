using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    public class VariableGridCell : ContentControl {

        private VariableGridRow _row;

        public int Row { get; set; }

        public int Column { get; set; }

        internal void Prepare(VariableGridRow owningRow, object item) {
            _row = owningRow;
            int columnIndex = _row.ItemContainerGenerator.IndexFromContainer(this);

            this.Content = item;
        }

        internal void Prepare(object item) {
            this.Content = item;
        }

        protected override Size MeasureOverride(Size constraint) {
            //return new Size(70,  _row.EstimatedHeight);
            return base.MeasureOverride(constraint);
        }
    }
}
