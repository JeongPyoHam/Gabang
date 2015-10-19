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
    [TemplatePart(Name = "PART_TreeGrid")]
    public class TreeGrid1 : Selector
    {
        public TreeGrid1()
        {
            Columns = new ObservableCollection<DataGridColumn>();

            var headerPresenter = ControlHelper.GetChild(this, typeof(DataGridColumnHeadersPresenter)) as DataGridColumnHeadersPresenter;
            if (headerPresenter != null)
            {
                headerPresenter.ItemsSource = this.Columns;
            }
        }

        public ObservableCollection<DataGridColumn> Columns { get; }
    }

    public class TreeGridItem : ListBox // ItemsControl
    {
        public TreeGridItem()
        {
            Children = new ObservableCollection<TreeGridItem>();
        }

        public ObservableCollection<TreeGridItem> Children { get; }

        private void AddColumn(DataGridColumn column)
        {
            FrameworkElement cellContent = column.GetCellContent(this.DataContext);
            this.Items.Add(cellContent);
        }
    }
}
