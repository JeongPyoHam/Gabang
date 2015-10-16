using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabangCollection
{
    /// <summary>
    /// List loops small sized physical list over logical index
    /// example for Underlying list size: 3 for only { 0, 1, 2 }
    /// 
    /// Logical :  0 1 2 3 4 5 6 7 8 ...
    /// Physical:  0 1 2 0 1 2 0 1 2 ...
    /// 
    /// After Rotate 1,
    /// Logical :  0 1 2 3 4 5 6 7 8 ...
    /// Physical:  1 2 0 1 2 0 1 2 0 ...
    /// 
    /// Every index through public method is logical
    /// </summary>
    public class LoopList<T> : IList<T>, IEnumerator<T>
    {
        #region private

        private int _startPhysicalIndex = 0;
        private List<T> _list = new List<T>();
        private int GetPhysicalIndex(int index)
        {
            return (_startPhysicalIndex + index) % _list.Count;
        }

        #endregion

        /// <summary>
        /// Move logical window by given offset
        /// </summary>
        /// <param name="offset">the amount of movement</param>
        public void Rotate(int offset)
        {
            _startPhysicalIndex = (_startPhysicalIndex + offset) % _list.Count;
            _startPhysicalIndex = (_startPhysicalIndex + _list.Count) % _list.Count;  // for negative case
        }

        #region IList<T> implementation

        public T this[int index]
        {
            get
            {
                if ((index >= _list.Count) || (index < 0))
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return _list[GetPhysicalIndex(index)];
            }

            set
            {
                _list[GetPhysicalIndex(index)] = value;
            }
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            if (_list.Count == 0 || _startPhysicalIndex == 0)    // add at the end
            {
                _list.Add(item);
            }
            else
            {
                var lastIndex = GetPhysicalIndex(_list.Count - 1) + 1;
                _list.Insert(lastIndex, item);
                _startPhysicalIndex = GetPhysicalIndex(1);  // shift one
            }
        }

        public void Clear()
        {
            _startPhysicalIndex = 0;
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        // TODO: limit access to collection modifier such as Add(), when enumerator is acquired
        public IEnumerator<T> GetEnumerator()
        {
            _currentLogicalIndex = -1;

            return this;
        }

        public int IndexOf(T item)
        {
            var index = _list.IndexOf(item);
            if (index == -1)
            {
                return index;
            }

            return GetPhysicalIndex(index + _list.Count);
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }



        #endregion IList<T> implementation

        #region IEnumerator<T> implementation

        private int _currentLogicalIndex = -1;

        public T Current
        {
            get
            {
                return this[_currentLogicalIndex];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public void Dispose()
        {
            _currentLogicalIndex = 0;
        }

        public bool MoveNext()
        {
            var nextIndex = _currentLogicalIndex + 1;
            if (nextIndex >= _list.Count)
            {
                return false;
            }

            _currentLogicalIndex = nextIndex;
            return true;
        }

        public void Reset()
        {
            _currentLogicalIndex = _startPhysicalIndex;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerator<T> implementation
    }
}
