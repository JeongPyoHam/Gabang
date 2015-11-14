using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    internal class Virtualizer<T> {
        private object _syncObj;
        private Queue<IVirtualizingItem<T>> _works = new Queue<IVirtualizingItem<T>>();
        private Task _pumpTask;

        private Func<Tuple<int, int>, Task<T>> _providerAsync;

        public Virtualizer(Func<Tuple<int, int>, Task<T>> provider, object syncObj) {
            if (provider == null || syncObj == null) {
                throw new ArgumentNullException("provider");
            }

            _providerAsync = provider;
            _syncObj = syncObj;
        }

        public event EventHandler<VirtualizingStateChangedArg<T>> VirtualizaingStateChanged;

        public void Virtualize(IVirtualizingItem<T> item) {
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

        public void Realize(IVirtualizingItem<T> item) {
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
                IVirtualizingItem<T> item = null;
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

        private async Task SetValueAsync(IVirtualizingItem<T> item) {
            T value = await _providerAsync(item.Id);

            lock (_syncObj) {
                item.Value = value;
                item.Status = VirtualizingState.Realized;
            }
        }

        private void ClearValue(IVirtualizingItem<T> item) {
            lock (_syncObj) {
                item.Value = default(T);
                item.Status = VirtualizingState.Virtualized;
            }
        }
    }
}
