using System;
using System.Collections.Generic;

namespace Gabang.Controls.Data {
    /// <summary>
    /// <see cref="IGrid{T}"/> implmented by <see cref="Dictionary{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="T">the type of value in the grid</typeparam>
    public class GridByDictionary<T> : IGrid<T> {
        private Dictionary<int, Dictionary<int, T>> _columns;

        public GridByDictionary(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _columns = new Dictionary<int, Dictionary<int, T>>();
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public virtual T GetAt(int rowIndex, int columnIndex) {
            CheckIndex(rowIndex, columnIndex);

            Dictionary<int, T> column;
            if (_columns.TryGetValue(columnIndex, out column)) {
                T value;
                if (column.TryGetValue(rowIndex, out value)) {
                    return value;
                }
            }

            return default(T);
        }

        public virtual void SetAt(int rowIndex, int columnIndex, T value) {
            CheckIndex(rowIndex, columnIndex);

            Dictionary<int, T> column;
            if (_columns.TryGetValue(columnIndex, out column)) {
                if (value.Equals(default(T))) {
                    column.Remove(rowIndex);
                    if (column.Count == 0) {
                        _columns.Remove(columnIndex);
                    }
                } else {
                    column[rowIndex] = value;
                }
            } else {
                if (!value.Equals(default(T))) {
                    column = new Dictionary<int, T>();
                    column.Add(rowIndex, value);

                    _columns.Add(columnIndex, column);
                }
            }
        }

        private void CheckIndex(int rowIndex, int columnIndex) {
            if (rowIndex < 0 || rowIndex >= RowCount) {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (columnIndex < 0 || columnIndex >= ColumnCount) {
                throw new ArgumentOutOfRangeException("columnIndex");
            }
        }
    }
}
