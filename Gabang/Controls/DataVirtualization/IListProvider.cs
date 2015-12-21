using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gabang.Controls {
    /// <summary>
    /// One dimensional data provider
    /// </summary>
    /// <typeparam name="TData">data type</typeparam>
    public interface IListProvider<TData> {
        /// <summary>
        /// total number of items
        /// </summary>
        int Count { get; }

        /// <summary>
        /// returns portion of data
        /// </summary>
        Task<IList<TData>> GetRangeAsync(Range range);
    }
}
