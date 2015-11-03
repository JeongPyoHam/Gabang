using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls {
    /// <summary>
    /// Row of <see cref="JointCollectionGrid"/>, which is ItemsControl itself
    /// </summary>
    public class JointCollectionGridRow : ItemsControl {
        static JointCollectionGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JointCollectionGridRow), new FrameworkPropertyMetadata(typeof(JointCollectionGridRow)));
        }

        public JointCollectionGridRow() {
        }

        public object Header { get; set; }

        internal JointCollectionGrid OwningJointGrid { get; private set; }

        internal void Prepare(JointCollectionGrid owner, object item) {
            if (!(item is IList)) {
                throw new NotSupportedException("");
            }

            OwningJointGrid = owner;

            var items = (IList)item;
            ItemsSource = items;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new TextBlock();// base.GetContainerForItemOverride();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            //base.PrepareContainerForItemOverride(element, item);
            ((TextBlock)element).Text = item.ToString();
        }
    }
}
