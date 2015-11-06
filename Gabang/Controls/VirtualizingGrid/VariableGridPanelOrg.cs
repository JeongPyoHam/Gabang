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
    public class VariableGridPanelOrg : VirtualizingStackPanel, IScrollInfo {

        protected override Size MeasureOverride(Size constraint) {
            var desired = base.MeasureOverride(constraint);

            //SetScrollInfo();

            return desired;
        }

        public IScrollInfo ChildScrollInfo { get; private set; }

        private void SetScrollInfo() {
            if (Children.Count > 0) {
                var childPanel = Children[0] as VirtualizingStackPanel;
                if (childPanel != null) {
                    ChildScrollInfo = childPanel;
                }
            }
        }
    }
}
