﻿using System;
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

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new JointCollectionGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            JointCollectionGridRow row = (JointCollectionGridRow)element;
            row.Prepare(this, item);
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