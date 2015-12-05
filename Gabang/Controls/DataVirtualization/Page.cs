using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gabang.Controls {
    public class Page<T> {
        private IGrid<T> _items;

        public Page(PageNumber pageNumber, IGrid<T> items, int rowStartIndex, int columnStartIndex) {
            _items = items;

            PageNumber = pageNumber;

            Range = new GridRange(
                new Range(rowStartIndex, items.RowCount),
                new Range(columnStartIndex, items.ColumnCount));

            Node = new LinkedListNode<Page<T>>(this);

            LastAccessTime = DateTime.MinValue;
        }

        public PageNumber PageNumber { get; }

        public GridRange Range { get; }

        public LinkedListNode<Page<T>> Node;

        public DateTime LastAccessTime { get; set; }

        public T GetItem(int row, int column) {
            Debug.Assert(Range.Contains(row, column));

            return _items[row - Range.Rows.Start, column - Range.Columns.Start];
        }
    }
}
