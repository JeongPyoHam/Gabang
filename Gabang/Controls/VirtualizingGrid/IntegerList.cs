using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gabang.Controls {
    /// <summary>
    /// A simulated collection that provides integer range
    /// </summary>
    public class IntegerList : IList<GridItem>, IList {
        public IntegerList(int start, int count) {
            Start = start;
            Count = count;
        }

        public int Start { get; }

        #region IList support

        public GridItem this[int index] {
            get {
                //Debug.WriteLine($"IntegerList[{Start}][{index}] getter");
                return new GridItem(Start, index);
            }

            set {
                throw new NotSupportedException($"{typeof(IntegerList)} doesn't support assigning item's value");
            }
        }

        public int Count { get; }

        public bool IsReadOnly { get { return true; } }

        public bool IsFixedSize { get { return true; } }

        public object SyncRoot { get { return null; } }

        public bool IsSynchronized { get { return false; } }

        object IList.this[int index] {
            get {
                return this[index];
            }

            set {
                this[index] = (GridItem)value;
            }
        }

        public void Add(GridItem item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support adding new item");
        }

        public void Clear() {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support clearing items");
        }

        public bool Contains(GridItem item) {
            return (item.Row == Start && item.Column >= 0 && item.Column < Count);
        }

        public void CopyTo(GridItem[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public IEnumerator<GridItem> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        public int IndexOf(GridItem item) {
            if (!Contains(item)) return -1;

            return item.Column;
        }

        public void Insert(int index, GridItem item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support inserting item");
        }

        public bool Remove(GridItem item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support removing item");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support removing item");
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Add(object value) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support adding item");
        }

        public bool Contains(object value) {
            if (value is int) {
                return Contains((int)value);
            }
            return false;
        }

        public int IndexOf(object value) {
            if (value is GridItem) {
                return IndexOf((GridItem)value);
            }
            return -1;
        }

        public void Insert(int index, object value) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support inserting item");
        }

        public void Remove(object value) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support removing item");
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        #endregion IList support
    }
}
