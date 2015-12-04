using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Gabang.Controls {
    public class RowItemsProvider : IItemsProvider<GridItem> {
        public RowItemsProvider(int rowIndex, int count) {
            RowIndex = rowIndex;
            Count = count;
        }

        public int RowIndex { get; }

        public int Count { get; }

        public IList<GridItem> Acquire(int startIndex, int count) {
            var list = new List<GridItem>();
            for (int i = 0; i < count; i++) {
                list.Add(new GridItem(RowIndex, startIndex + i));
            }
            return list;
        }

        public void Release(int startIndex, int count) {
            // do nothing
        }
    }

    public class DynamicGridDataSource : ConstantCountList<VirtualItemsSource<GridItem>> {

        public DynamicGridDataSource(int rowCount, int columnCount) : base(rowCount) {
            if (rowCount < 0) {
                throw new ArgumentOutOfRangeException("rowCount");
            }
            if (columnCount < 0) {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        public override VirtualItemsSource<GridItem> this[int index] {
            get {
                return GetItem(index);
            }

            set {
                throw new NotSupportedException();
            }
        }

        public override int IndexOf(VirtualItemsSource<GridItem> item) {
            return item.Index;
        }

        private VirtualItemsSource<GridItem> GetItem(int index) {
            var pm = new PageManager<GridItem>(
                new RowItemsProvider(index, ColumnCount),
                64,
                TimeSpan.FromMinutes(1.0),
                4);
            return new VirtualItemsSource<GridItem>(
                index,
                (i) => new GridItem(index, i, true),
                pm);
        }
    }

    public class DynamicGridDataSource3 : ConstantCountList<IntegerList>, INotifyCollectionChanged {  // TODO: change from IntegerList to generic
        public DynamicGridDataSource3(int rowCount, int columnCount) : base(rowCount) {
            if (rowCount < 0) {
                throw new ArgumentOutOfRangeException("rowCount");
            }
            if (columnCount < 0) {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int RowCount { get; }
        public int ColumnCount { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void RaiseReset() {
            if (CollectionChanged != null) {
                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void RaiseReplace() {
            if (CollectionChanged != null) {

                List<IntegerList> oldItems = new List<IntegerList>();
                oldItems.Add(_zeroth);

                List<IntegerList> newItems = new List<IntegerList>();
                _zeroth = new IntegerList(10, ColumnCount);
                newItems.Add(_zeroth);

                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        newItems,
                        oldItems,
                        0));
            }
        }

        #region ConstantCountList abstract implementation

        private IntegerList _zeroth = null;

        public override IntegerList this[int index] {
            get {
                if (index == 0) {
                    if (_zeroth == null) {
                        _zeroth = new IntegerList(0, ColumnCount);
                    }
                    return _zeroth;
                }
                return new IntegerList(ColumnCount * index, ColumnCount);
            }

            set {
                throw new NotSupportedException($"{typeof(DynamicGridDataSource)} is read only");
            }
        }

        public override int IndexOf(IntegerList item) {
            if (object.Equals(item, _zeroth)) {
                return 0;
            }

            if (item.Start >= 0 && item.Count == ColumnCount) {
                int remainder;
                int rowIndex = Math.DivRem(item.Start, ColumnCount, out remainder);
                if (remainder == 0 && rowIndex < RowCount) {
                    return rowIndex;
                }
            }
            return -1;
        }

        #endregion
    }
}
