using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public interface IGrid<T> {
        int RowCount { get; }
        int ColumnCount { get; }
        T GetAt(int rowIndex, int columnIndex);
        void SetAt(int rowIndex, int columnIndex, T value);
    }
}
