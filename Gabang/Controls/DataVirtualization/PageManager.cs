using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class PageManager<T> {
        #region synchronized by _syncObj

        private object _syncObj = new object();
        private Dictionary<int, Dictionary<int, Page<T>>> _banks = new Dictionary<int, Dictionary<int, Page<T>>>();
        private Queue<PageNumber> _requests = new Queue<PageNumber>();
        private Task _loadTask = null;

        #endregion

        private IGridProvider<T> _itemsProvider;
        private SynchronizationContext _synchronizationContext;

        public PageManager(IGridProvider<T> itemsProvider, int pageSize, TimeSpan timeout, int keepPageCount) {
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

            _synchronizationContext = SynchronizationContext.Current;
        }

        public event EventHandler<PageLoadedEventArgs> PageLoaded;

        public int RowCount { get { return _itemsProvider.RowCount; } }

        public int ColumnCount { get { return _itemsProvider.ColumnCount; } }

        public int KeepPageCount { get; }

        public int PageSize { get; }

        public TimeSpan Timeout { get; }

        public bool TryGetPage(int row, int column, out Page<T> foundPage, bool autoRequest) {
            lock (_syncObj) {
                int rowPageNumber, columnPageNumber;
                ComputePageNumber(row, column, out rowPageNumber, out columnPageNumber);

                Dictionary<int, Page<T>> bank;
                if (_banks.TryGetValue(rowPageNumber, out bank)) {
                    if (bank.TryGetValue(columnPageNumber, out foundPage)) {
                        return true;
                    }
                }

                if (autoRequest) {
                    Request(rowPageNumber, columnPageNumber);
                }

                foundPage = null;
                return false;
            }
        }

        private void ComputePageNumber(int row, int column, out int rowPageNumber, out int columnPageNumber) {
            rowPageNumber = row / PageSize;
            columnPageNumber = column / PageSize;
        }

        private void Request(int row, int column) {
            lock (_syncObj) {
                _requests.Enqueue(new PageNumber(row, column));
                EnsureLoadTask();
            }
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
                PageNumber? pageNumber = null;
                lock (_syncObj) {
                    if (_requests.Count == 0) {
                        if (cleanHasRun) {
                            _loadTask = null;
                            return;
                        } else {

                        }
                    } else {
                        pageNumber = _requests.Dequeue();
                        Debug.Assert(pageNumber != null);
                    }
                }

                if (pageNumber != null) {
                    await LoadAsync(pageNumber.Value);
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
                    IEnumerable<KeyValuePair<int, Page<T>>> toRemove;

                    lock (_syncObj) {
                        toRemove = bank.Where(kv => (kv.Value.LastAccessTime < lastTime)).ToList();
                    }

                    foreach (var item in toRemove) {
                        lock (_syncObj) {
                            bank.Remove(item.Key);
                        }
                        // TODO: release hint
                    }
                }
            }
        }

        private async Task LoadAsync(PageNumber pageNumber) {
            Dictionary<int, Page<T>> bank;
            if (_banks.TryGetValue(pageNumber.Row, out bank)) {
                if (bank.ContainsKey(pageNumber.Column)) {
                    return;
                }
            }

            GridRange range = GetPageRange(pageNumber);

            var task = Task.Run(() => _itemsProvider.GetRangeAsync(range));

            IGrid<T> data = await task;

            Page<T> page = new Page<T>(
                pageNumber,
                data,
                range.Rows.Start,
                range.Columns.Start);

            lock (_syncObj) {
                if (_banks.ContainsKey(pageNumber.Row)) {
                    _banks[pageNumber.Row][pageNumber.Column] = page;
                } else {
                    bank = new Dictionary<int, Page<T>>();
                    bank[pageNumber.Column] = page;
                    _banks[pageNumber.Row] = bank;
                }
            }

            if (PageLoaded != null) {
                var args = new PageLoadedEventArgs(range);
                _synchronizationContext.Post(
                    PageLoadedSynchronizationContextCallback,
                    args);
            }
        }

        private void PageLoadedSynchronizationContextCallback(object state) {
            if (PageLoaded != null) {
                PageLoaded(
                    this,
                    (PageLoadedEventArgs)state);
            }
        }
    }

    public class PageLoadedEventArgs : EventArgs {
        public PageLoadedEventArgs(GridRange range) {
            Range = range;
        }

        public GridRange Range { get; }
    }

    /// <summary>
    /// <see cref="IGrid{T}"/> provider
    /// </summary>
    /// <typeparam name="T">the type of grid item</typeparam>
    public interface IGridProvider<T> {
        int RowCount { get; }

        int ColumnCount { get; }

        Task<IGrid<T>> GetRangeAsync(GridRange gridRange);

        Task<List<T>> GetHeaderAsync(Range range, bool isRow);
    }
}
