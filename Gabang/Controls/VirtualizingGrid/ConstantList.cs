using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.VirtualizingGrid {
    public class DefaultValuedList<T> : ConstantCountList<T> {
        private List<T> _list;

        public DefaultValuedList(int startIndex, int count) : base(startIndex, count) {
            _list = new List<T>(count);
            for (int i = 0; i < count; i++) {
                _list.Add(default(T));
            }
        }

        public override T this[int index] {
            get {
                return _list[StartIndex + index];
            }

            set {
                _list[StartIndex + index] = value;
            }
        }

        public override int IndexOf(T item) {
            int offset = _list.IndexOf(item);
            if (offset == -1) {
                return -1;
            }
            return StartIndex + offset;
        }
    }
}
