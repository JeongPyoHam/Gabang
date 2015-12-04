using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class PageLoadedEventArgs : EventArgs {
        public PageLoadedEventArgs(int start, int count) {
            Start = start;
            Count = count;
        }

        public int Start { get; }

        public int Count { get; }
    }

    /// <summary>
    /// Manages pages used by <seealso cref="VirtualItemsSource{T}"/>
    /// </summary>
    /// <typeparam name="T">type of item in page</typeparam>
    public class PageManager<T> {
        #region synchronized by _syncObj

        private object _syncObj = new object();
        private Dictionary<int, Page<T>> _pages = new Dictionary<int, Page<T>>();
        private Queue<int> _requests = new Queue<int>();
        private Task _loadTask = null;

        #endregion

        private IItemsProvider<T> _itemsProvider;
        private SynchronizationContext _synchronizationContext;

        public PageManager(IItemsProvider<T> itemsProvider, int pageSize, TimeSpan timeout, int keepPageCount) {
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

        public int ItemsCount {
            get {
                return _itemsProvider.Count;
            }
        }

        public int KeepPageCount { get; }

        public int PageSize { get; }

        public TimeSpan Timeout { get; }

        public bool TryGetPage(int index, out Page<T> foundPage, bool autoRequest) {
            lock (_syncObj) {
                int pageNumber = ComputePageNumber(index);
                bool found = _pages.TryGetValue(pageNumber, out foundPage);
                if (!found && autoRequest) {
                    Request(pageNumber);
                }
                return found;
            }
        }

        private void Request(int index) {
            lock(_syncObj) {
                _requests.Enqueue(index);
                EnsureLoadTask();
            }
        }

        private int ComputePageNumber(int index) {
            return index / PageSize;
        }

        private void GetPageInfo(int pageNumber, out int pageStartIndex, out int pageSize) {
            pageStartIndex = pageNumber * PageSize;

            int end = pageStartIndex + PageSize;
            int count = _itemsProvider.Count;
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
                int pageNumber = -1;
                lock (_syncObj) {
                    if (_requests.Count == 0) {
                        if (cleanHasRun) {
                            _loadTask = null;
                            return;
                        } else {

                        }
                    } else {
                        pageNumber = _requests.Dequeue();
                        Debug.Assert(pageNumber != -1);
                    }
                }

                if (pageNumber != -1) {
                    await LoadAsync(pageNumber);
                } else {
                    CleanOldPages();
                    cleanHasRun = true;
                }
            }
        }

        private int PopRequest() {
            lock (_syncObj) {
                return _requests.Count == 0 ? -1 : _requests.Dequeue();
            }
        }

        private void CleanOldPages() {
            while(_pages.Count > KeepPageCount) {
                DateTime lastTime = DateTime.UtcNow - Timeout;
                IEnumerable<KeyValuePair<int, Page<T>>> toRemove;

                lock (_syncObj) {
                    toRemove = _pages.Where(kv => (kv.Value.LastAccessTime < lastTime)).ToList();
                }

                foreach (var item in toRemove) {
                    lock (_syncObj) {
                        _pages.Remove(item.Key);
                    }
                    _itemsProvider.Release(item.Value.StartIndex, item.Value.Count);
                }
            }
        }

        private async Task LoadAsync(int pageNumber) {
            lock (_syncObj) {
                if (_pages.ContainsKey(pageNumber)) {
                    return;
                }
            }

            int start, count;
            GetPageInfo(pageNumber, out start, out count);

            var task = Task.Run(() => _itemsProvider.Acquire(start, count));

            IList<T> data = await task;

            var page = new Page<T>(pageNumber, data, start);

            lock (_syncObj) {
                _pages.Add(pageNumber, page);
            }

            if (PageLoaded != null) {
                var args = new PageLoadedEventArgs(page.StartIndex, page.Count);
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
}
