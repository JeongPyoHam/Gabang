using System;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    /// <summary>
    /// <see cref="IGrid{T}"/> of which item is <see cref="IVirtualizable{T}"/>
    /// Realized when item is accessed, and Virtualized when item is set to other value (e.g. null)
    /// </summary>
    /// <typeparam name="T">the type of value in grid</typeparam>
    public class VirtualizingGrid<T> where T : IVirtualizable {
        private GridByDictionary<T> _grid;
        private Func<int, int, T> _itemFactory;

        public VirtualizingGrid(
            int rowCount,
            int columnCount,
            Func<int, int, T> factory) {
            _grid = new GridByDictionary<T>(rowCount, columnCount);

            _itemFactory = factory;
        }

        public async Task<T> GetAtAsync(int rowIndex, int columnIndex) {
            var item = _grid.GetAt(rowIndex, columnIndex);

            if (item == null) {
                item = _itemFactory(rowIndex, columnIndex);
                await item.RealizeAsync();
                _grid.SetAt(rowIndex, columnIndex, item);
            }

            return item;
        }

        public void ClearExcept(GridRange range) {
            _grid.ClearExcept(range);
        }

        public void ClearAt(int rowIndex, int columnIndex) {
            var item = _grid.GetAt(rowIndex, columnIndex);

            if (item != null) {
                _grid.SetAt(rowIndex, columnIndex, default(T));
            }
        }
    }
}
