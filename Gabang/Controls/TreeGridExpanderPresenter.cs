using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls
{
    public class TreeGridExpanderPresenter : ContentControl
    {
        static TreeGridExpanderPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGridExpanderPresenter), new FrameworkPropertyMetadata(typeof(TreeGridExpanderPresenter)));
        }

        public TreeGridExpanderPresenter()
        {
        }

        public static DependencyProperty DepthProperty = DependencyProperty.Register("Depth", typeof(int), typeof(TreeGridExpanderPresenter));
        public int Depth
        {
            get { return (int)GetValue(DepthProperty); }
            set { SetValue(DepthProperty, value); }
        }
    }
}
