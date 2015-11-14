using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public abstract class VirtualizingGridItem<T> : IVirtualizable<T> {
        public int Row { get; set; }

        public int Column { get; set; }

        public bool Equals(IVirtualizable<T> other) {
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
