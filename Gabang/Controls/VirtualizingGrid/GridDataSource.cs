using System;
using System.Collections;
using System.Collections.Generic;

namespace Gabang.Controls {
    public class GridDataSource : IList<DataSource>, IList {
        public GridDataSource(int nrow, int ncol) {
            RowCount = nrow;
            ColumnCount = ncol;
        }

        public int RowCount { get; }
        public int ColumnCount { get; }

        #region IList support

        public int Count { get { return RowCount; } }

        public bool IsReadOnly { get { return true; } }

        public bool IsFixedSize { get { return true; } }

        public object SyncRoot { get { return null; } }

        public bool IsSynchronized { get { return false; } }

        object IList.this[int index] {
            get {
                return this[index];
            }

            set {
                this[index] = (DataSource) value;
            }
        }

        public DataSource this[int index] {
            get {
                return new DataSource(ColumnCount, index * ColumnCount);
            }

            set {
                throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support assigning item's value");
            }
        }

        public int IndexOf(DataSource item) {
            if (item.Count == ColumnCount
                && item.Start >=0
                && item.Start <= (RowCount * (ColumnCount - 1))) {
                int remainder;
                int index = Math.DivRem(item.Start, ColumnCount, out remainder);
                if (remainder == 0) return index;
            }
            return -1;
        }

        public void Insert(int index, DataSource item) {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support inserting item");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support removing item");
        }

        public void Add(DataSource item) {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support adding item");
        }

        public void Clear() {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support clearing item");
        }

        public bool Contains(DataSource item) {
            return IndexOf(item) != -1;
        }

        public void CopyTo(DataSource[] array, int arrayIndex) {
            throw new NotImplementedException();  
        }

        public bool Remove(DataSource item) {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support removing item");
        }

        public IEnumerator<DataSource> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Add(object value) {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support adding item");
        }

        public bool Contains(object value) {
            if (value is DataSource) {
                return Contains((DataSource)value);
            }
            return false;
        }

        public int IndexOf(object value) {
            if (value is DataSource) {
                return IndexOf((DataSource)value);
            }
            return -1;
        }

        public void Insert(int index, object value) {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support inserting item");
        }

        public void Remove(object value) {
            throw new NotSupportedException($"{typeof(GridDataSource)} doesn't support removing item");
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
