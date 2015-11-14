using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public class VirtualizingGrid<T> : IGrid<T> {
        private IGridProvider<T> _provider;

        private Range _realizedRows;
        private Range _realizedColumns;

        private object _syncObj = new object();
        private GridByDictionary<IVirtualizable<IGrid<T>>> _pages;
        private Queue<IVirtualizable<T>> _works = new Queue<IVirtualizable<T>>();
        private Task _pumpTask;

        public VirtualizingGrid(int rowCount, int columnCount, IGridProvider<T> provider) {
            _provider = provider;
            RowCount = rowCount;
            ColumnCount = columnCount;

            _pages = new GridByDictionary<IVirtualizable<IGrid<T>>>(rowCount, columnCount);
            _realizedRows = new Range();
            _realizedColumns = new Range();
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public int RowPageSize { get; }

        public int ColumnPageSize { get; }

        public void GetRealizedRanges(out Range rowRange, out Range columnRange) {
            throw new NotImplementedException();
        }

        public event EventHandler<RealizedRangeChangedArgs> RealizedRangeChanged;

        public void RequestRealize(GridRange gridRange) {
            QueueOperation(Operation.Realization, gridRange);
        }

        public void RequestVirtualize(GridRange gridRange) {
            QueueOperation(Operation.Virtualization, gridRange);
        }

        public bool GetAt(int rowIndex, int columnIndex, out T value) {
            int rowPageIndex, columnPageIndex;

            GetPageIndex(rowIndex, columnIndex, out rowPageIndex, out columnPageIndex);

            IGrid<T> page;
            if (_pages.GetAt(rowPageIndex, columnPageIndex, out page)) {
                return page.GetAt(rowIndex, columnIndex, out value);
            }

            value = default(T);
            return false;
        }

        public void SetAt(int row, int column, T value) {
            throw new NotSupportedException("VirtualizingGrid is readonly yet");
        }

        enum Operation {
            Virtualization,
            Realization,
        }

        private void QueueOperation(Operation operation, GridRange gridRange) {
            // intersect
        }

        private async Task<IGrid<T>> RealizePageAsync(Tuple<int, int> id) {
            GridRange pageRange = GetPageRange(id.Item1, id.Item2);

            return await _provider.GetRangeAsync(pageRange);
        }

        private void GetPageIndex(int rowIndex, int columnIndex, out int rowPageIndex, out int columnPageIndex) {
            throw new NotImplementedException();
        }

        private GridRange GetPageRange(int rowPageIndex, int columnPageIndex) {
            throw new NotImplementedException();
        }

        #region Virtualizer

        public void Virtualize(IVirtualizable<T> item) {
            lock (_syncObj) {
                if (item.Status == VirtualizingState.Virtualized) {
                    return;
                }

                var preQueue = _works.FirstOrDefault((q) => q.Id == item.Id);
                if (preQueue != null) {
                    switch (preQueue.Status) {
                        case VirtualizingState.Virtualized:
                        case VirtualizingState.PendingVirtualization:
                            // do nothing
                            break;
                        case VirtualizingState.Realized:
                            item.Status = VirtualizingState.PendingVirtualization;
                            break;
                        case VirtualizingState.PendingRealization:
                            item.Status = VirtualizingState.Virtualized;    // cancel realization
                            break;
                    }
                } else {
                    item.Status = VirtualizingState.PendingVirtualization;
                    _works.Enqueue(item);
                }
            }
        }

        public void Realize(IVirtualizable<T> item) {
            lock (_syncObj) {
                if (item.Status == VirtualizingState.Realized) {
                    return;
                }

                var preQueue = _works.FirstOrDefault((q) => q.Id == item.Id);
                if (preQueue != null) {
                    switch (preQueue.Status) {
                        case VirtualizingState.Virtualized:
                            item.Status = VirtualizingState.PendingRealization;
                            break;
                        case VirtualizingState.PendingVirtualization:
                            item.Status = VirtualizingState.Realized;   // cancel virtualization
                            break;
                        case VirtualizingState.Realized:
                        case VirtualizingState.PendingRealization:
                            // do nothing
                            break;
                    }
                } else {
                    item.Status = VirtualizingState.PendingRealization;
                    _works.Enqueue(item);
                }
            }
        }

        private void EnsurePump() {
            lock (_syncObj) {
                if (_pumpTask == null) {
                    _pumpTask = Task.Run(async () => {
                        try {
                            await Pump();
                        } catch (Exception ex) {
                            System.Diagnostics.Debug.Fail(ex.ToString());   // TODO: error handling. retry? at which level?
                            throw;
                        }
                    });
                }
            }
        }

        private async Task Pump() {
            bool fContinue = true;

            while (fContinue) {
                IVirtualizable<T> item = null;
                lock (_syncObj) {
                    if (_works.Count > 0) {
                        item = _works.Dequeue();
                    } else {
                        fContinue = false;
                        _pumpTask = null;
                    }
                }

                if (item != null) {
                    switch (item.Status) {
                        case VirtualizingState.Virtualized:
                        case VirtualizingState.Realized:
                            // do nothing
                            break;
                        case VirtualizingState.PendingVirtualization:
                            ClearValue(item);
                            break;
                        case VirtualizingState.PendingRealization:
                            await SetValueAsync(item);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private async Task SetValueAsync(IVirtualizable<T> item) {
            T value = await _providerAsync(item.Id);

            lock (_syncObj) {
                item.Value = value;
                item.Status = VirtualizingState.Realized;
            }
        }

        private void ClearValue(IVirtualizable<T> item) {
            lock (_syncObj) {
                item.Value = default(T);
                item.Status = VirtualizingState.Virtualized;
            }
        }

        #endregion
    }
}
