using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gabang.Controls
{
    public class TreeGrid : DataGrid
    {
        public TreeGrid()
        {
            Children = new ObservableCollection<TreeGrid>();
        }

        public TreeGrid(IList<DataGridColumn> columns)
            : this()
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }
            this.AutoGenerateColumns = false;
            this.HeadersVisibility &= (~DataGridHeadersVisibility.Column);  // turn off column header
            foreach (var column in columns)
            {
                Columns.Add(column);
            }
        }

        public static readonly DependencyProperty ItemsVisibilityProperty = DependencyProperty.Register("ItemsVisibility", typeof(Visibility), typeof(TreeGrid), new PropertyMetadata(Visibility.Collapsed));
        public Visibility ItemsVisibility
        {
            get { return (Visibility)GetValue(ItemsVisibilityProperty); }
            set { SetValue(ItemsVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(TreeGrid));
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public ObservableCollection<TreeGrid> Children { get; }
    }

}
