using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gabang.Controls {
    /// <summary>
    /// virtualizing collection that can be used as ItemsSource of ItemsControl
    /// </summary>
    /// <typeparam name="T">type of value of each item</typeparam>
    public class VirtualItemsSource<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged {
        #region field and ctor

        private readonly string ReadOnlyExceptionMessage = $"{typeof(VirtualItemsSource<T>)} is read only";
        private PageManager<T> _pageManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItemsSource&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">items provider</param>
        /// <param name="pageSize">number of items in one page</param>
        /// <param name="pageTimeout">Time out. when page is older than this time, it is virtualized.</param>
        public VirtualItemsSource(PageManager<T> pageManager) {
            if (pageManager == null) {
                throw new ArgumentNullException("pageManager");
            }
            _pageManager = pageManager;
        }

        #endregion

        #region public

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public int PageSize { get; }

        public TimeSpan PageTimeout { get; }

        private bool _isLoading;
        /// <summary>
        /// Indicates that pages are being loaded
        /// </summary>
        public bool IsLoading {
            get {
                return _isLoading;
            }
            set {
                SetProperty<bool>(ref _isLoading, value);
            }
        }

        #endregion

        #region Page

        private T GetItem(int index) {
            Page<T> page;
            if (_pageManager.TryGetPage(index, out page, true)) {
                return page.GetItem(index);
            }
            return default(T);
        }

        #endregion

        #region Collection support (IList<T>, IList)

        public int Count {
            get {
                return _pageManager.ItemsCount;
            }
        }

        public T this[int index] {
            get { return GetItem(index); }
            set { throw new NotSupportedException(ReadOnlyExceptionMessage); }
        }


        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        public int IndexOf(T item) {
            var indexed = item as IndexedItem;
            if (indexed == null)
                return -1;
            return indexed.Index;
        }

        public bool Contains(T item) {
            return IndexOf(item) != -1;
        }

        public void Add(T item) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void Clear() {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void Insert(int index, T item) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public bool Remove(T item) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }
        object IList.this[int index] {
            get { return this[index]; }
            set { throw new NotSupportedException(ReadOnlyExceptionMessage); }
        }

        int IList.Add(object value) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        bool IList.Contains(object value) {
            return Contains((T)value);
        }

        int IList.IndexOf(object value) {
            return IndexOf((T)value);
        }

        void IList.Remove(object value) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        void IList.Insert(int index, object value) {
            Insert(index, (T)value);
        }

        public object SyncRoot {
            get { return null; }
        }

        public bool IsSynchronized {
            get { return false; }
        }

        public bool IsReadOnly { get { return true; } }

        public bool IsFixedSize { get { return false; } }

        #endregion

        #region private/utilities
        private bool SetProperty<U>(ref U storage, U value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<U>.Default.Equals(storage, value)) {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(
                    this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
