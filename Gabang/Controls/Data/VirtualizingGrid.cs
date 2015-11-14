using System;

namespace Gabang.Controls.Data {
    /// <summary>
    /// <see cref="IGrid{T}"/> of which item is <see cref="IVirtualizable{T}"/>
    /// Realized when item is accessed, and Virtualized when item is set to other value (e.g. null)
    /// </summary>
    /// <typeparam name="T">the type of value in grid</typeparam>
    public class VirtualizingGrid<T> : GridByDictionary<IVirtualizable<T>> {
        private Virtualizer<T> _virtualizer;
        private Func<int, int, VirtualizingGridItem<T>> _itemFactory;

        public VirtualizingGrid(
            int rowCount,
            int columnCount,
            Func<int, int, VirtualizingGridItem<T>> factory) : base (rowCount, columnCount) {
            _virtualizer = new Virtualizer<T>();
            _itemFactory = factory;
        }

        public override IVirtualizable<T> GetAt(int rowIndex, int columnIndex) {
            var item = base.GetAt(rowIndex, columnIndex);

            if (item == null) {
                item = _itemFactory(rowIndex, columnIndex);
                base.SetAt(rowIndex, columnIndex, item);
                _virtualizer.Realize(item);
            }

            return item;
        }

        public void SetAt(int rowIndex, int columnIndex, T value) {
            var item = base.GetAt(rowIndex, columnIndex);

            if (item != null) {
                if (!object.Equals(value, item)) {  // same object doesn't virtualize
                    _virtualizer.Virtualize(item);
                    base.SetAt(rowIndex, columnIndex, item);
                }
            } else {
                base.SetAt(rowIndex, columnIndex, item);
            }
        }
    }
}
