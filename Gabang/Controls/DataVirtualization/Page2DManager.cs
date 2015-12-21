﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gabang.Controls {

    public class Page2DManager<T> {
        #region synchronized by _syncObj

        private readonly object _syncObj = new object();
        private Dictionary<int, Dictionary<int, Page2D<T>>> _banks = new Dictionary<int, Dictionary<int, Page2D<T>>>();
        private Queue<Page2D<T>> _requests = new Queue<Page2D<T>>();
        private Task _loadTask = null;
        private Dictionary<int, DelegateList<PageItem<T>>> _rows = new Dictionary<int, DelegateList<PageItem<T>>>();

        #endregion

        private IGridProvider<T> _itemsProvider;

        public Page2DManager(IGridProvider<T> itemsProvider, int pageSize, TimeSpan timeout, int keepPageCount) {
            if (itemsProvider == null) {
                throw new ArgumentNullException("itemsProvider");
            }
            if (pageSize <= 0) {
                throw new ArgumentOutOfRangeException("pageSize");
            }
            if (keepPageCount <= 0) {
                throw new ArgumentOutOfRangeException("keepPageCount");
            }

            _itemsProvider = itemsProvider;

            PageSize = pageSize;
            Timeout = timeout;
            KeepPageCount = keepPageCount;
        }

        public int RowCount { get { return _itemsProvider.RowCount; } }

        public int ColumnCount { get { return _itemsProvider.ColumnCount; } }

        public int KeepPageCount { get; }

        public int PageSize { get; }

        public TimeSpan Timeout { get; }

        public bool WaitForItemRealization { get { return true; } }

        public PageItem<T> GetItem(int row, int column) {
            bool shouldWait = false;
            Page2D<T> foundPage = null;
            Task waitTask;

            lock (_syncObj) {
                int rowPageNumber, columnPageNumber;
                ComputePageNumber(row, column, out rowPageNumber, out columnPageNumber);

                Dictionary<int, Page2D<T>> bank;

                if (_banks.TryGetValue(rowPageNumber, out bank)) {
                    if (!bank.TryGetValue(columnPageNumber, out foundPage)) {
                        foundPage = CreateEmptyPage(new PageNumber(rowPageNumber, columnPageNumber));
                        shouldWait = WaitForItemRealization;
                        bank.Add(columnPageNumber, foundPage);
                    }
                } else {
                    bank = new Dictionary<int, Page2D<T>>();
                    foundPage = CreateEmptyPage(new PageNumber(rowPageNumber, columnPageNumber));
                    shouldWait = WaitForItemRealization;
                    bank.Add(columnPageNumber, foundPage);
                    _banks.Add(rowPageNumber, bank);
                }

                if (shouldWait) {
                    int firstRow = foundPage.Range.Rows.Start;
                    int firstColumn = foundPage.Range.Columns.Start;
                    var pageItem = foundPage.GetItem(firstRow, firstColumn);
                    waitTask = WaitPropertyChangeAsync(pageItem);
                } else {
                    waitTask = Task.FromResult<object>(null);
                }
            }

            waitTask.Wait(TimeSpan.FromMilliseconds(1000));

            return foundPage.GetItem(row, column);
        }

        private static async Task WaitPropertyChangeAsync(PageItem<T> pageItem) {
            var tcs = new TaskCompletionSource<object>();
            PropertyChangedEventHandler handler = (sender, args) => tcs.TrySetResult(null);
            try {
                pageItem.PropertyChanged += handler;
                await TaskUtilities.SwitchToBackgroundThread();
                await tcs.Task;
            } finally {
                pageItem.PropertyChanged -= handler;
            }
        }

        public DelegateList<DelegateList<PageItem<T>>> GetItemsSource() {
            var itemsSource = new DelegateList<DelegateList<PageItem<T>>>(
                0,
                (row) => GetRow(row),
                RowCount);

            return itemsSource;
        }

        public DelegateList<PageItem<T>> GetRow(int row) {
            DelegateList<PageItem<T>> list;
            lock (_syncObj) {
                if (_rows.TryGetValue(row, out list)) {
                    return list;
                }
                list = new DelegateList<PageItem<T>>(
                    row,
                    (column) => GetItem(row, column),
                    ColumnCount);
                _rows.Add(row, list);
                return list;
            }
        }

        private Page2D<T> CreateEmptyPage(PageNumber pageNumber) {
            int rowStart, rowCount;
            GetPageInfo(pageNumber.Row, RowCount, out rowStart, out rowCount);

            int columnStart, columnCount;
            GetPageInfo(pageNumber.Column, ColumnCount, out columnStart, out columnCount);

            var range = new GridRange(
                    new Range(rowStart, rowCount),          // row
                    new Range(columnStart, columnCount));   // column

            var page = new Page2D<T>(pageNumber, range);

            lock (_syncObj) {
                _requests.Enqueue(page);
                EnsureLoadTask();
            }

            return page;
        }

        private void ComputePageNumber(int row, int column, out int rowPageNumber, out int columnPageNumber) {
            rowPageNumber = row / PageSize;
            columnPageNumber = column / PageSize;
        }

        private GridRange GetPageRange(PageNumber pageNumber) {
            int start, count;

            GetPageInfo(pageNumber.Row, RowCount, out start, out count);
            Range row = new Range(start, count);

            GetPageInfo(pageNumber.Column, ColumnCount, out start, out count);
            Range column = new Range(start, count);

            return new GridRange(row, column);
        }

        private void GetPageInfo(int pageNumber, int itemCount, out int pageStartIndex, out int pageSize) {
            pageStartIndex = pageNumber * PageSize;

            int end = pageStartIndex + PageSize;
            int count = itemCount;
            if (end > count) {    // last page
                pageSize = count - pageStartIndex;
            } else {
                pageSize = PageSize;
            }
        }

        private void EnsureLoadTask() {
            if (_loadTask == null) {
                lock (_syncObj) {
                    if (_loadTask == null) {
                        _loadTask = Task.Run(async () => await LoadAndCleanPagesAsync());
                    }
                }
            }
        }

        private async Task LoadAndCleanPagesAsync() {
            bool cleanHasRun = false;
            while (true) {
                Page2D<T> page = null;
                lock (_syncObj) {
                    if (_requests.Count == 0) {
                        if (cleanHasRun) {
                            _loadTask = null;
                            break;
                        } else {

                        }
                    } else {
                        page = _requests.Dequeue();
                        Debug.Assert(page != null);
                    }
                }

                if (page != null) {
                    IGrid<T> data = await _itemsProvider.GetRangeAsync(page.Range);

                    page.PopulateData(data);
                } else {
                    CleanOldPages();
                    cleanHasRun = true;
                }
            }
        }

        private void CleanOldPages() {
            foreach (var bank in _banks.Values) {
                while (bank.Count > KeepPageCount) {
                    DateTime lastTime = DateTime.UtcNow - Timeout;
                    IEnumerable<KeyValuePair<int, Page2D<T>>> toRemove;

                    lock (_syncObj) {
                        toRemove = bank.Where(kv => (kv.Value.LastAccessTime < lastTime) && kv.Key != 0).ToList();
                        if (toRemove.Count() <= 0) {
                            break;
                        }
                    }

                    foreach (var item in toRemove) {
                        Trace.WriteLine(string.Format("Removing:Page:{0}", item.Value.PageNumber));
                        lock (_syncObj) {
                            bank.Remove(item.Key);
                        }
                        // TODO: release hint
                    }
                }
            }
        }
    }
}
