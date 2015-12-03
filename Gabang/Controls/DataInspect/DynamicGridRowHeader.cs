using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gabang.Controls {
    internal class DynamicGridRowHeader : DynamicGridCell {

        internal override void Prepare(DynamicGridStripe columnStipe) {
            base.Prepare(columnStipe);
        }

        internal override void CleanUp() {
            base.CleanUp();
        }

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }
    }
}
