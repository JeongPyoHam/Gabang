using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Gabang.Controls {
    /// <summary>
    /// ItemsControl for two dimentional collection; collection of collection
    /// Need to put some restriction on IList<IList<T>>
    /// 
    /// GridLine
    /// Row Header
    /// Column Header
    /// </summary>
    public class JointGrid : MultiSelector {

        static JointGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JointGrid), new FrameworkPropertyMetadata(typeof(JointGrid)));
        }

        public JointGrid() {
        }

        #region public

        public IList<object> ColumnHeaderProvider { get; set; }

        public IList<object> RowHeaderProvider { get; set; }

        #endregion

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new JointGridRow();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (newValue is IList<IList<object>>) {
                throw new NotSupportedException($"JointGrid supports only joint IList collection of which type is {typeof(IList<IList<object>>)}");
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        #endregion override
    }
}
