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
    public class JointCollectionGrid : MultiSelector {

        static JointCollectionGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JointCollectionGrid), new FrameworkPropertyMetadata(typeof(JointCollectionGrid)));
        }

        public JointCollectionGrid() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var scrollBar = (ScrollBar)ControlHelper.GetChild(this, "PART_HorizontalScrollBar");
            if (scrollBar != null) {
                scrollBar.ViewportSize = 10;
            }
        }

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new JointCollectionGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            JointCollectionGridRow row = (JointCollectionGridRow)element;
            row.Prepare(this, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            JointCollectionGridRow row = (JointCollectionGridRow)element;
            row.Clear(this, item);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (!(newValue is GridDataSource)) {
                throw new NotSupportedException($"JointGrid supports only {typeof(GridDataSource)} for ItemsSource");
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }


        #endregion override
    }
}
