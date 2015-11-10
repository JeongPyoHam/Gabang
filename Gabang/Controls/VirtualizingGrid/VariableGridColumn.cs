using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    public class VariableGridStack {

        public VariableGridStack(Orientation stackingDirection, int index) {
            this.Orientation = stackingDirection;
            this.Index = index;
            this.LayoutSize = new MaxDouble();
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

        public int Index { get; }

        /// <summary>
        /// position in layout (perpendicular to stacking direction)
        /// </summary>
        public double? LayoutPosition { get; set; }

        /// <summary>
        /// length in layout (perpendicular to stacking direction)
        /// </summary>
        public MaxDouble LayoutSize { get; }

        public double GetSizeConstraint() {
            if (LayoutSize.Frozen) {
                return LayoutSize.Max.Value;
            }

            return double.PositiveInfinity;
        }


        public object HeaderContent { get; set; }

        public DataTemplate HeaderTemplate { get; set; }
    }

    public class VariableGridColumn : VariableGridStack {

        public VariableGridColumn(int index) : base(Orientation.Vertical, index) { }
    }

    public class VariableGridRow : VariableGridStack {
        public VariableGridRow(int index) : base(Orientation.Horizontal, index) { }
    }
}
