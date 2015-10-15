using System;
using System.Collections;
using System.Collections.Generic;

namespace Gabang.Collection
{
    public class TreeEnumerator<T> : IEnumerator<ITreeNode<T>>
    {
        private ITreeNode<T> _root;
        private Stack<IEnumerator<ITreeNode<T>>> _stack;
        private IEnumerator<ITreeNode<T>> _currentEnumerator;

        /// <summary>
        /// Create new instance of TreeEnumerator
        /// </summary>
        /// <param name="root">root node from which start traverse</param>
        public TreeEnumerator(ITreeNode<T> root)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            _root = root;
            _stack = new Stack<IEnumerator<ITreeNode<T>>>();
            _currentEnumerator = new List<ITreeNode<T>>() { _root }.GetEnumerator();
        }

        private ITreeNode<T> _current = null;
        public ITreeNode<T> Current
        {
            get
            {
                return _current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            if (_currentEnumerator != null)
            {
                _currentEnumerator.Dispose();
            }
        }

        public bool MoveNext()
        {
            if (_currentEnumerator != null)
            {
                if (_currentEnumerator.MoveNext())
                {
                    _current = _currentEnumerator.Current;
                    if (_current.Children != null)
                    {
                        _stack.Push(_currentEnumerator);
                        _currentEnumerator = _current.Children.GetEnumerator();
                    }
                    return true;
                }
            }

            while (_stack.Count > 0)
            {
                _currentEnumerator = _stack.Pop();
                if (_currentEnumerator.MoveNext())
                {
                    _current = _currentEnumerator.Current;
                    if (_current.Children != null)
                    {
                        _stack.Push(_currentEnumerator);
                        _currentEnumerator = _current.Children.GetEnumerator();
                    }
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
