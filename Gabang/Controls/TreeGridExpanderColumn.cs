using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gabang.Controls
{
    public class TreeGridExpanderColumn : DataGridTemplateColumn
    {
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var presenter = new TreeGridExpanderPresenter();

            BindingOperations.SetBinding(presenter, ContentControl.ContentProperty, new Binding());

            presenter.ContentTemplate = this.CellTemplate;
            presenter.ContentTemplateSelector = this.CellTemplateSelector;

            return presenter;
        }
    }
}
