using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Gabang.Controls {
    /// <summary>
    /// a panel(vertical) of row panel(horizontal)
    /// send horizontal scroll to each row panel
    /// 
    /// item generation (virtualization) should be synchronized, too
    /// column item layout should be synchronized, too
    /// 
    /// </summary>
    public class VirtualizingGridPanel : VirtualizingStackPanel {
        protected override Size MeasureOverride(Size availableSize) {
            EnsureItemContainerGenerator();

            IItemContainerGenerator itemGenerator = this.ItemContainerGenerator;
            if (itemGenerator != null) {
                GeneratorPosition position = itemGenerator.GeneratorPositionFromIndex(0);

                using (itemGenerator.StartAt(position, GeneratorDirection.Forward)) {
                    var item = itemGenerator.GenerateNext();
                }
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            return base.ArrangeOverride(finalSize);
        }

        private void EnsureItemContainerGenerator() {
            var children = this.Children;
        }
    }
}
