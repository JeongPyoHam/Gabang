using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    public class VariableGrid : ItemsControl {

        static VariableGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VariableGrid), new FrameworkPropertyMetadata(typeof(VariableGrid)));
        }

        public VariableGrid() {
        }

        public VariableGridCellGenerator Generator { get; set; }

        #region override

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (!(newValue is VariableGridDataSource)) {
                throw new NotSupportedException($"JointGrid supports only {typeof(VariableGridDataSource)} for ItemsSource");
            }

            base.OnItemsSourceChanged(oldValue, newValue);

            this.Generator = new VariableGridCellGenerator((VariableGridDataSource)newValue);
        }

        #endregion override
    }
}
