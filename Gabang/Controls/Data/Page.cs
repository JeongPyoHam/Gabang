using System;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {
    public class Page<T> : IGrid<T>, IVirtualizable {
        private Func<Task<IGrid<T>>> _realizingFunc;
        private IGrid<T> _grid;

        public Page(int rowPageIndex, int columnPageIndex, Func<Task<IGrid<T>>> realizer) {
            _realizingFunc = realizer;
        }

        private object syncObj = new object();
        private bool _realized = false;
        public bool Realized {
            get {
                lock(syncObj) {
                    return _realized;
                }
            }
        }

        #region IGrid

        public int RowCount {
            get {
                lock (syncObj) {
                    return _realized ? _grid.RowCount : 0;
                }
            }
        }

        public int ColumnCount {
            get {
                lock (syncObj) {
                    return _realized ? _grid.ColumnCount : 0;
                }
            }
        }

        public T GetAt(int rowIndex, int columnIndex) {
            lock (syncObj) {
                if (_realized) {
                    return _grid.GetAt(rowIndex, columnIndex);
                }
                return default(T);
            }
        }

        public void SetAt(int rowIndex, int columnIndex, T value) {
            lock (syncObj) {
                if (_realized) {
                    _grid.SetAt(rowIndex, columnIndex, value);
                } else {
                    throw new InvalidOperationException("Page is not realized and can't set value");
                }
            }
        }

        #endregion

        #region IVirtualizable

        public Task VirtualizeAsync() {
            lock (syncObj) {
                _realized = false;
            }
            return Task.FromResult<bool>(true);
        }

        public async Task RealizeAsync() {
            _grid = await _realizingFunc();
            lock(syncObj) {
                _realized = true;
            }
        }

        #endregion
    }
}
