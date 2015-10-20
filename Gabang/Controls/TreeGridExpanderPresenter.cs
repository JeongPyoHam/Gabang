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
    }
}
