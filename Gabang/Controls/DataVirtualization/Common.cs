using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    /// <summary>
    /// Abstract two dimentional data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGrid<T> {
        /// <summary>
        /// number of rows
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// number of columns
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Return value at given position
        /// </summary>
        /// <param name="rowIndex">row index, zero based</param>
        /// <param name="columnIndex">column index, zero based</param>
        /// <returns>item value</returns>
        /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
        /// <exception cref="InvalidOperationException">when failed at setting or getting the value</exception>
        /// <exception cref="NotSupportedException">setter, when the grid is read only</exception>
        T this[int rowIndex, int columnIndex]
        {
            get; set;
        }
    }

    public class Grid<T> : IGrid<T> {

        private IList<T> _list;

        public Grid(int rowCount, int columnCount, Func<int, int, T> createNew) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _list = new List<T>(RowCount * ColumnCount);

            for (int c = 0; c < columnCount; c++) {
                for (int r = 0; r < rowCount; r++) {
                    _list.Add(createNew(r, c));
                }
            }
        }

        public Grid(int rowCount, int columnCount, IList<T> list) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            if (list.Count < (RowCount * ColumnCount)) {
                throw new ArgumentException("list doesn't contain enough data");
            }

            _list = list;
        }

        public T this[int rowIndex, int columnIndex]
        {
            get
            {
                return _list[(columnIndex * RowCount) + rowIndex];
            }

            set
            {
                _list[(columnIndex * RowCount) + rowIndex] = value;
            }
        }

        public int ColumnCount { get; }

        public int RowCount { get; }
    }
}
