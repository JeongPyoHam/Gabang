//#define PRINT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gabang.Controls {
    public class VariableGridCellGenerator {
        private VariableGridDataSource _dataSource;

        public VariableGridCellGenerator(VariableGridDataSource dataSource) {
            _dataSource = dataSource;
            RowCount = _dataSource.RowCount;
            ColumnCount = _dataSource.ColumnCount;

            elements = new VariableGridCell[RowCount, ColumnCount];
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        private VariableGridCell[,] elements;  // TODO: do not use 2D element

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

            var element = elements[rowIndex, columnIndex];
            if (element != null) {
                newlyCreated = false;
                return element;
            }

            element = new VariableGridCell();
            element.Prepare(_dataSource[rowIndex][columnIndex]);
            element.Row = rowIndex;
            element.Column = columnIndex;

            elements[rowIndex, columnIndex] = element;

            newlyCreated = true;
            return element;
        }

        public bool RemoveAt(int rowIndex, int columnIndex) {
#if DEBUG && PRINT
            Debug.WriteLine("VariableGridCellGenerator:RemoveAt: {0} {1}", rowIndex, columnIndex);
#endif
            var element = elements[rowIndex, columnIndex];

            if (element != null) {
                element.CleanUp(_dataSource[rowIndex][columnIndex]);
                elements[rowIndex, columnIndex] = null; // TODO: give a chance each element to clean up such as unregistering event handler
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

        private void RemoveStacksExcept(SortedList<int, VariableGridStack> stackDictionary,  Range except) {
            var toBeDeleted = stackDictionary.Keys.Where(key => !except.Contains(key)).ToList();
            foreach (var row in toBeDeleted) {
                stackDictionary.Remove(row);
            }
        }

        public void RemoveRow(int rowIndex) {
            _rows.Remove(rowIndex);
        }

        public void RemoveColumn(int columnIndex) {
            _columns.Remove(columnIndex);
        }

        public VariableGridStack GetColumn(int index) {
            return GetStack(_columns, index);
        }

        public VariableGridStack GetRow(int index) {
            return GetStack(_rows, index);
        }

        private VariableGridStack GetStack(SortedList<int, VariableGridStack> stacks, int index) {
            VariableGridStack stack;
            if (stacks.TryGetValue(index, out stack)) {
                return stack;
            }

            stack = new VariableGridColumn(index);
            stacks.Add(index, stack);

            return stack;
        }

        public void FreezeStacks() {
            // freeze remaining row and columns
            foreach (var row in _rows.Values) {
                if (row.LayoutSize.Max.HasValue) {
                    row.LayoutSize.Frozen = true;
                }
            }

            foreach (var column in _columns.Values) {
                if (column.LayoutSize.Max.HasValue) {
                    column.LayoutSize.Frozen = true;
                }
            }
        }

        public void ComputeStackPosition(Range viewportRow, Range viewportColumn) {
            ComputeStackPosition(_rows, viewportRow, 0.0);
            ComputeStackPosition(_columns, viewportColumn, 0.0);
        }

        private void ComputeStackPosition(SortedList<int, VariableGridStack> stacks, Range viewport, double startingOffset) {
            double offset = startingOffset;
            foreach (var key in stacks.Keys) {
                var stack = stacks[key];

                stack.LayoutPosition = offset;
                offset += stack.LayoutSize.Max.Value;
            }
        }

        private SortedList<int, VariableGridStack> _rows = new SortedList<int, VariableGridStack>();
        private SortedList<int, VariableGridStack> _columns = new SortedList<int, VariableGridStack>();
    }

    public class GricStackCollection {
        private LinkedList<VariableGridStack> _stacks = new LinkedList<VariableGridStack>();

        public GricStackCollection() { }

        public int Start { get; private set; }

        public int Count {
            get {
                return _stacks.Count;
            }
        }

        public void Insert(int index) {
        }

        public IEnumerable<VariableGridStack> Enumerable {
            get {
                return _stacks;
            }
        }
    }
}
