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
    /// 
    /// </summary>
    public class VariableGridCell : ContentControl {
        static VariableGridCell() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VariableGridCell), new FrameworkPropertyMetadata(typeof(VariableGridCell)));
        }


        private VariableGridRow _row;

        [DefaultValue(-1)]
        public int Row { get; set; }

        [DefaultValue(-1)]
        public int Column { get; set; }

        internal void Prepare(VariableGridRow owningRow, object item) {
            _row = owningRow;
            int columnIndex = _row.ItemContainerGenerator.IndexFromContainer(this);

            this.Content = item;
        }

        internal void Prepare(object item) {
            this.Content = item;
        }

        internal void CleanUp(object item) {
            this.Content = null;
        }

        protected override Size MeasureOverride(Size constraint) {
            //return new Size(70,  _row.EstimatedHeight);
            return base.MeasureOverride(constraint);
        }
    }
}
