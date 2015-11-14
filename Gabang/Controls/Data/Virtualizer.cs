using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    /// <summary>
    /// Handles <see cref="IVirtualizable{T}"/> in background
    /// </summary>
    /// <typeparam name="T">the type of value in <see cref="IVirtualizable{T}"/></typeparam>
    public class Virtualizer<T> {
        enum Operation {
            Virtualization,
            Realization,
            Cancelled,
        }

        class WorkItem {
            public Operation Operation;
            public IVirtualizable<T> Virtualizable;
        }

        private object _syncObj = new object();
        private Queue<WorkItem> _works = new Queue<WorkItem>();
        private Task _pumpTask;

        public void Virtualize(IVirtualizable<T> item) {
            lock (_syncObj) {
                bool enqueue = true;

                // look into queue for duplicate item
                var workItemInQueue = _works.FirstOrDefault((q) => q.Virtualizable.Equals(item));
                if (workItemInQueue != null) {
                    if (workItemInQueue.Operation == Operation.Virtualization) {
                        enqueue = false;    // duplicate item. do not enqueue
                    } else {
                        if (workItemInQueue.Operation == Operation.Realization) {
                            // realize queue, cancel it
                            workItemInQueue.Operation = Operation.Cancelled;
                        }
                    }
                }

                if (enqueue) {
                    _works.Enqueue(new WorkItem() { Operation = Operation.Virtualization, Virtualizable = item });
                }

                EnsurePump();
            }
        }

        public void Realize(IVirtualizable<T> item) {
            lock (_syncObj) {
                bool enqueue = true;

                // look into queue for duplicate item
                var workItemInQueue = _works.FirstOrDefault((q) => q.Virtualizable.Equals(item));
                if (workItemInQueue != null) {
                    if (workItemInQueue.Operation == Operation.Realization) {
                        enqueue = false;    // duplicate item. do not enqueue
                    } else {
                        if (workItemInQueue.Operation == Operation.Virtualization) {
                            // realize queue, cancel it
                            workItemInQueue.Operation = Operation.Cancelled;
                        }
                    }
                }

                if (enqueue) {
                    _works.Enqueue(new WorkItem() { Operation = Operation.Realization, Virtualizable = item });
                }

                EnsurePump();
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
                WorkItem item = null;
                lock (_syncObj) {
                    if (_works.Count > 0) {
                        item = _works.Dequeue();
                    } else {
                        fContinue = false;
                        _pumpTask = null;
                    }
                }

                if (item != null) {
                    if (item.Operation == Operation.Realization) {
                        await item.Virtualizable.RealizeAsync();
                    } else if (item.Operation == Operation.Virtualization) {
                        await item.Virtualizable.VirtualizeAsync();
                    } else if (item.Operation == Operation.Cancelled) {
                        // no-op
                    }
                }
            }
        }
    }
}
