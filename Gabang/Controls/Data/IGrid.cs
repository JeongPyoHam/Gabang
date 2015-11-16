using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    /// <summary>
    /// two dimentional data
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
        /// <exception cref="InvalidOperationException">when failed at returning the value</exception>
        T GetAt(int rowIndex, int columnIndex);

        /// <summary>
        /// Change value at given position
        /// </summary>
        /// <param name="rowIndex">row index, zero based</param>
        /// <param name="columnIndex">column index, zero based</param>
        /// <param name="value">value to be set</param>
        /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
        /// <exception cref="NotSupportedException">when the grid is read only</exception>
        void SetAt(int rowIndex, int columnIndex, T value);
    }
}
