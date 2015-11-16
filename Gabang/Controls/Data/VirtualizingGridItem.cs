using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public abstract class VirtualizingGridItem<T> : IVirtualizable {
        public VirtualizingGridItem(int row, int column) {
            Row = row;
            Column = column;
        }

        public int Row { get; }

        public int Column { get; }

        public bool Equals(IVirtualizable other) {
            var otherItem = other as VirtualizingGridItem<T>;
            if (otherItem != null) {
                return (Row == otherItem.Row && Column == otherItem.Column);
            }
            return false;
        }

        public abstract Task RealizeAsync();

        public abstract Task VirtualizeAsync();
    }
}
