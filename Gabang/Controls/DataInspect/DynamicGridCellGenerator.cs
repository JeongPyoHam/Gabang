//#define PRINT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gabang.Controls {
    internal class DynamicGridCellGenerator {
        private DynamicGridDataSource _dataSource;

        public DynamicGridCellGenerator(DynamicGridDataSource dataSource) {
            _dataSource = dataSource;
            RowCount = _dataSource.RowCount;
            ColumnCount = _dataSource.ColumnCount;
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        public VariableGridCell GenerateAt(int rowIndex, int columnIndex, out bool newlyCreated) {
            if (rowIndex < 0 || rowIndex >= RowCount) {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (columnIndex < 0 || columnIndex >= ColumnCount) {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

#if DEBUG && PRINT
            Debug.WriteLine("VariableGridCellGenerator:GenerateAt: {0} {1}", rowIndex, columnIndex);
#endif

            var element = _rows[rowIndex].GetItemAt(columnIndex);
            if (element != null) {
                newlyCreated = false;
                return element;
            }

            element = new VariableGridCell();
            element.Prepare(_dataSource[rowIndex][columnIndex]);
            element.Row = rowIndex;
            element.Column = columnIndex;

            _rows[rowIndex].SetItemAt(columnIndex, element);
            _columns[columnIndex].SetItemAt(rowIndex, element);

            newlyCreated = true;
            return element;
        }

        public bool RemoveAt(int rowIndex, int columnIndex) {
#if DEBUG && PRINT
            Debug.WriteLine("VariableGridCellGenerator:RemoveAt: {0} {1}", rowIndex, columnIndex);
#endif
            var element = _rows[rowIndex].GetItemAt(columnIndex);
            Debug.Assert(object.Equals(element, _columns[columnIndex].GetItemAt(rowIndex)));

            if (element != null) {
                element.CleanUp(_dataSource[rowIndex][columnIndex]);

                _rows[rowIndex].ClearAt(columnIndex);
                _columns[columnIndex].ClearAt(rowIndex);

                return true;
            }

            return false;
        }

        public void RemoveRowsExcept(Range except) {
            RemoveStacksExcept(_rows, except);
        }

        public void RemoveColumnsExcept(Range except) {
            RemoveStacksExcept(_columns, except);
        }

        private void RemoveStacksExcept(SortedList<int, DynamicGridStripe> stackDictionary,  Range except) {
            var toBeDeleted = stackDictionary.Keys.Where(key => !except.Contains(key)).ToList();
            foreach (var row in toBeDeleted) {
                stackDictionary.Remove(row);
            }
        }

        public DynamicGridStripe GetColumn(int index) {
            return GetStack(Orientation.Vertical, _columns, index);
        }

        public DynamicGridStripe GetRow(int index) {
            return GetStack(Orientation.Horizontal, _rows, index);
        }

        private DynamicGridStripe GetStack(Orientation orientation, SortedList<int, DynamicGridStripe> stacks, int index) {
            DynamicGridStripe stack;
            if (stacks.TryGetValue(index, out stack)) {
                return stack;
            }

            stack = new DynamicGridStripe(orientation, index);
            stacks.Add(index, stack);

            return stack;
        }

        public void FreezeLayoutSize() {
            // freeze remaining row and columns
            foreach (var row in _rows.Values) {
                row.LayoutSize.Frozen = true;
            }

            foreach (var column in _columns.Values) {
                column.LayoutSize.Frozen = true;
            }
        }

        public void ComputeStackPosition(Range viewportRow, Range viewportColumn, out double computedRowOffset, out double computedColumnOffset) {
            if (viewportRow.Count > 0) {
                ComputeStackPosition(_rows, viewportRow.Start, out computedRowOffset);
            } else {
                computedRowOffset = 0;
            }

            if (viewportColumn.Count > 0) {
                ComputeStackPosition(_columns, viewportColumn.Start, out computedColumnOffset);
            } else {
                computedColumnOffset = 0;
            }
        }

        private void ComputeStackPosition(SortedList<int, DynamicGridStripe> stacks, int startingIndex, out double computedOffset) {
            double offset = 0;
            computedOffset = 0;
            foreach (var key in stacks.Keys) {
                var stack = stacks[key];

                if (key == startingIndex) {
                    computedOffset = offset;
                }

                stack.LayoutPosition = offset;
                offset += stack.LayoutSize.Max;
            }
        }

        // TODO: improve to better collection
        private SortedList<int, DynamicGridStripe> _rows = new SortedList<int, DynamicGridStripe>();
        private SortedList<int, DynamicGridStripe> _columns = new SortedList<int, DynamicGridStripe>();
    }
}
