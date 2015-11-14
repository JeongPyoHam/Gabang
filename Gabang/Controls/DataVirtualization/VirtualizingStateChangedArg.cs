using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.DataVirtualization {
    public class VirtualizingStateChangedArg<T> : EventArgs {

        public VirtualizingStateChangedArg(Tuple<int, int> id, VirtualizingState state, T value) {
            Id = id;
            State = state;
            Value = value;
        }


        public Tuple<int, int> Id { get; }

        public VirtualizingState State { get; }

        public T Value { get; }
    }
}
