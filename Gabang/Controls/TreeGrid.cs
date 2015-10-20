using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gabang.Controls
{
    public class TreeGrid : DataGrid
    {
        public TreeGrid()
        {
            this.CanUserSortColumns = false;
        }

        public virtual BindingBase DepthBinding { get; set; }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeGridRow();
        }
    }

}
