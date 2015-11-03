using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gabang.Controls {

    /// <summary>
    /// A simulated collection that provides integer range
    /// </summary>
    public class IntegerList : IList<int>, IList {
        public IntegerList(int start, int count) {
            Start = start;
            Count = count;
        }

        public int Start { get; }

        #region IList support

        public int this[int index] {
            get {
                Debug.WriteLine($"IntegerList[{Start}][{index}] getter");
                return index + Start;
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
                this[index] = (int)value;
            }
        }

        public void Add(int item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support adding new item");
        }

        public void Clear() {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support clearing items");
        }

        public bool Contains(int item) {
            return (item >= Start && item < (Start + Count));
        }

        public void CopyTo(int[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public IEnumerator<int> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        public int IndexOf(int item) {
            if (!Contains(item)) return -1;

            return item - Start;
        }

        public void Insert(int index, int item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support inserting item");
        }

        public bool Remove(int item) {
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
            if (value is int) {
                return IndexOf((int)value);
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
