using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {

    public class JointGridRow : ItemsControl {
        static JointGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JointGridRow), new FrameworkPropertyMetadata(typeof(JointGridRow)));
        }

        public JointGridRow() {
            DataContextChanged += VirtualizingGridRowPresenter_DataContextChanged;
        }

        public object Header { get; set; }

        private void VirtualizingGridRowPresenter_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue is IEnumerable) {
                this.ItemsSource = (IEnumerable) e.NewValue;
            }
        }
    }
}
