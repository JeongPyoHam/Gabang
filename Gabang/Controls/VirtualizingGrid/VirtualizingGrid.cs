using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    /// <summary>
    /// ItemsControl for two dimentional collection; collection of collection
    /// Need to put some restriction on IList<IList<T>>
    /// 
    /// GridLine
    /// Row Header
    /// Column Header
    /// </summary>
    public class VirtualizingGrid : ItemsControl {

        static VirtualizingGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualizingGrid), new FrameworkPropertyMetadata(typeof(VirtualizingGrid)));
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override DependencyObject GetContainerForItemOverride() {
            var grid = new VirtualizingGrid();
            grid.SetValue(VirtualizingStackPanel.OrientationProperty, Orientation.Horizontal);
            grid.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            grid.DataContextChanged += Grid_DataContextChanged;
            return grid;
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue is DataSource) {
                ((VirtualizingGrid)sender).ItemsSource = (DataSource) e.NewValue;
            }
        }
    }

    public class VirtualizingGridRowPresenter : ContentControl {
    }

    /// <summary>
    /// a panel(vertical) of row panel(horizontal)
    /// send horizontal scroll to each row panel
    /// 
    /// item generation (virtualization) should be synchronized, too
    /// column item layout should be synchronized, too
    /// 
    /// </summary>
    public class VirtualizingGridPanel : VirtualizingStackPanel {
    }

    public class SynchronizedStackPanel : VirtualizingStackPanel {
    }
}
