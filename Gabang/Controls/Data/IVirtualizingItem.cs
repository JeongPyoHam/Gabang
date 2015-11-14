using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public interface IVirtualizingItem<T> {
        Tuple<int, int> Id { get; }

        VirtualizingState Status { get; set; }

        T Value { get; set; }
    }
}
