using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Gabang.Controls.Data {
    /// <summary>
    /// <see cref="IGrid{T}"/> implmented by <see cref="List{T}"/>
    /// </summary>
    /// <typeparam name="T">the type of value in the grid</typeparam>
    public class GridByList<T> : IGrid<T> {
        private List<List<T>> _columns;

        public GridByList(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _columns = new List<List<T>>(columnCount);
        }

        public GridByList(int rowCount, int columnCount, ReadOnlyCollection<ReadOnlyCollection<T>> values) : this(rowCount, columnCount) {
            if (values.Count != columnCount) {
                throw new ArgumentException("values Count must be same as columnCount");
            }
            for (int i = 0; i < values.Count && i < columnCount; i++) {
                if (values[i].Count != rowCount) {
                    throw new ArgumentException("values item length must be same as rowCount");
                }
                _columns.Add(new List<T>(values[i]));
            }
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public T GetAt(int rowIndex, int columnIndex) {
            return _columns[columnIndex][rowIndex];
        }

        public void SetAt(int rowIndex, int columnIndex, T value) {
            _columns[columnIndex][rowIndex] = value;
        }
    }
}
