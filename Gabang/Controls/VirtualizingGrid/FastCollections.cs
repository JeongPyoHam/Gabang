using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls.VirtualizingGrid {
    public class VariableGridStackCollection : RangeList<VariableGridStack>, INotifyCollectionChanged {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public override void Add(int index, VariableGridStack item) {
            base.Add(index, item);

            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        public override void Remove(int index) {
            var item = base.Get(index);
            base.Remove(index);

            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        public override void Clear() {
            base.Clear();

            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }

    public class RangeList<T> {
        private int _pStart;
        private List<T> _list;

        public RangeList() {
            _pStart = 0;
            ResetBuffer(64);
        }

        public T this[int index] {
            get {
                return Get(index);
            }
        }

        private Range _range;
        public Range Range { get { return _range; } }

        public int Capacity { get; private set; }

        public virtual void Clear() {
            _pStart = 0;
            _range.Start = 0;
            _range.Count = 0;
            _list = null;

            ResetBuffer(64);
        }

        public virtual void Add(int index, T item) {
            if (_range.Count == Capacity) {
                ResetBuffer(Capacity * 2);
            }

            if (_range.Count == 0) {
                _list[0] = item;
                _pStart = 0;
                _range.Start = index;
                _range.Count = 1;
            } else {
                // allows add only at start or end
                if (index == _range.Start - 1) {
                    int pIndex = GetPhysicalIndex(index - _range.Start);
                    _list[pIndex] = item;
                    _pStart = pIndex;
                    _range.Start = _range.Start - 1;
                    _range.Count += 1;
                } else if (index == (_range.Start + _range.Count)) {
                    int pIndex = GetPhysicalIndex(index - _range.Start);
                    _list[pIndex] = item;
                    _range.Count += 1;
                } else {
                    throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        public virtual T Get(int index) {
            if (!_range.Contains(index)) {
                throw new ArgumentOutOfRangeException("index");
            }
            return _list[GetPhysicalIndex(index - _range.Start)];
        }

        public virtual bool TryGet(int index, out T value) {
            if (!_range.Contains(index)) {
                value = default(T);
                return false;
            }
            value = _list[GetPhysicalIndex(index - _range.Start)];
            return true;
        }

        public virtual void Remove(int index) {
            if (!_range.Contains(index)) {
                throw new ArgumentOutOfRangeException("index");
            }

            // allows remove only first or last
            if (index == _range.Start) {
                int pStart = GetPhysicalIndex(0);
                _list[pStart] = default(T);
                _pStart = (_pStart + 1) % Capacity;
                _range.Start = _range.Start + 1;
                _range.Count -= 1;
            } else if (index == _range.Start + _range.Count - 1) {
                int pIndex = GetPhysicalIndex(_range.Count - 1);
                _list[pIndex] = default(T);
                _range.Count -= 1;
            } else {
                throw new ArgumentOutOfRangeException("index");
            }

            if (_range.Count == 0) {
                _pStart = 0;
                _range.Start = 0;
            }
        }

        private int GetPhysicalIndex(int index) {
            return (_pStart + index + Capacity) % Capacity;
        }

        private void ResetBuffer(int size) {
            List<T> larger = new List<T>();
            if (_list != null) {
                for (int i = 0; i < _list.Count; i++) {
                    larger.Add(_list[(_pStart + i) % Capacity]);
                }
            }
            for (int i = larger.Count; i < size; i++) {
                larger.Add(default(T));
            }

            _list = larger;
            _pStart = 0;
            Capacity = size;
            Debug.Assert(_list.Count == size);
        }
    }
}
