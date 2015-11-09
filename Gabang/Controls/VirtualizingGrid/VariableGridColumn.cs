using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    internal class VariableGridStripe {

        public VariableGridStripe(Orientation orientation) {
            this.Orientation = orientation;
        }

        public bool IsColumn {
            get {
                return this.Orientation == Orientation.Vertical;
            }
        }

        public bool IsRow {
            get {
                return this.Orientation == Orientation.Horizontal;
            }
        }

        public Orientation Orientation { get; }

        public MaxDouble ComputedWidth { get; set; }

        public object HeaderContent { get; set; }

        public DataTemplate HeaderTemplate { get; set; }
    }

    internal class VariableGridColumn : VariableGridStripe {

        public VariableGridColumn() : base(Orientation.Vertical) { }
    }

    internal class VariableGridRow : VariableGridStripe {
        public VariableGridRow() : base(Orientation.Horizontal) { }
    }
}
