using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class Page<T> {
        private IList<T> _items;

        public Page(int pageNumber, IList<T> items, int startIndex) {
            PageNumber = pageNumber;

            _items = items;
            StartIndex = startIndex;
            Count = items.Count;

            Node = new LinkedListNode<Page<T>>(this);

            LastAccessTime = DateTime.MinValue;
        }

        public int PageNumber { get; }

        public int StartIndex { get; }

        public int Count { get; }

        public bool Contains(int index) {
            return this.StartIndex <= index
                && index < (this.StartIndex + this.Count);
        }

        public LinkedListNode<Page<T>> Node;

        public DateTime LastAccessTime { get; set; }

        public T GetItem(int index) {
            Debug.Assert(Contains(index));

            return _items[index - StartIndex];
        }
    }
}
