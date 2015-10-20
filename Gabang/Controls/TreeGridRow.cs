using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Gabang.Controls
{
    public class TreeGridRow : DataGridRow
    {
        static TreeGridRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGridRow), new FrameworkPropertyMetadata(typeof(TreeGridRow)));
        }

        public TreeGridRow()
        {
        }
    }
}
