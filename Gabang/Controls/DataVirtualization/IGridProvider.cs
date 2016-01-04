using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public interface IGridData<TData> {
        IList<TData> ColumnHeader { get; }

        IList<TData> RowHeader { get; }

        IGrid<TData> Grid { get; }
    }

    /// <summary>
    /// Two dimensional data provider
    /// </summary>
    /// <typeparam name="TData">data type</typeparam>
    public interface IGridProvider<TData> {
        /// <summary>
        /// total number of items in row
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// total number of items in column
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Returns portion of data
        /// </summary>
        Task<IGrid<TData>> GetRangeAsync(GridRange gridRange);

        Task<IGridData<TData>> GetAsync(GridRange range);
    }
}
