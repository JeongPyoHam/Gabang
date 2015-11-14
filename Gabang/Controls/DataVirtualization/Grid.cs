using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.DataVirtualization {
    public class Grid<T> : IGrid<T> {
        private Dictionary<int, List<T>> _columns;

        public Grid(int rowCount, int columnCount) {
            if (rowCount <= 0 || columnCount <= 0) {
                throw new ArgumentOutOfRangeException("rowCount");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;

            _columns = new Dictionary<int, List<T>>(columnCount);
        }

        public Grid(int rowCount, int columnCount, ReadOnlyCollection<ReadOnlyCollection<T>> values)
            : this(rowCount, columnCount) {
            if (values.Count != columnCount) {
                throw new ArgumentException("values Count must be same as columnCount");
            }
            for (int i = 0; i < values.Count && i < columnCount; i++) {
                if (values[i].Count != rowCount) {
                    throw new ArgumentException("values item length must be same as rowCount");
                }
                _columns.Add(i, new List<T>(values[i]));
            }
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public bool GetAt(int rowIndex, int columnIndex, out T value) {
            List<T> column;
            if (_columns.TryGetValue(columnIndex, out column)) {
                if (column != null) {
                    if (column.Count > rowIndex && rowIndex >= 0) {
                        value = column[rowIndex];
                        return true;
                    }
                }
            }
            value = default(T);
            return false;
        }

        public void SetAt(int rowIndex, int columnIndex, T value) {
            if (rowIndex < 0 || columnIndex < 0 || rowIndex >= RowCount || columnIndex >= ColumnCount) {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            List<T> column;
            if (_columns.TryGetValue(columnIndex, out column)) {
                if (column != null) {
                    column[rowIndex] = value;
                }
            } else {
                column = new List<T>(RowCount);
                for (int i = 0; i < RowCount; i++) {
                    column.Add(default(T));
                }
                column[rowIndex] = value;
                _columns[columnIndex] = column;
            }
        }
    }
}
