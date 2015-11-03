using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Gabang.Controls {
    /// <summary>
    /// Row of <see cref="JointCollectionGrid"/>, which is ItemsControl itself
    /// </summary>
    public class JointCollectionGridRow : ListBox {
        static JointCollectionGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JointCollectionGridRow), new FrameworkPropertyMetadata(typeof(JointCollectionGridRow)));
        }

        public JointCollectionGridRow() {
        }

        public object Header { get; set; }

        private ScrollViewer ScrollOwner { get; set; }

        internal JointCollectionGrid OwningJointGrid { get; private set; }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            this.ScrollOwner = ControlHelper.FindVisualParent<ScrollViewer>(this);
            if (ScrollOwner != null) {
                ScrollOwner.ScrollChanged += Owner_ScrollChanged;
            }
        }

        internal void Prepare(JointCollectionGrid owner, object item) {
            if (!(item is IList)) {
                throw new NotSupportedException("JointCollectionGridRow supports only IList for item");
            }

            OwningJointGrid = owner;

            var items = (IList)item;
            ItemsSource = items;
        }

        internal void Clear(JointCollectionGrid owner, object item) {
            if (ScrollOwner != null) {
                ScrollOwner.ScrollChanged -= Owner_ScrollChanged;
            }
            ScrollOwner = null;
        }

        private void Owner_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (ScrollOwner == null) return;

            if (e.HorizontalChange != 0) {
                ScrollOwner.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

    }
}
