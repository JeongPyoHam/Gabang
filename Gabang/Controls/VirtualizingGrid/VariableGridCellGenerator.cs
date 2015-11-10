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

        private void RemoveStacksExcept(Dictionary<int, VariableGridStack> stackDictionary,  Range except) {
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

        public VariableGridStack PrepareStack(Orientation orientation, int index) {
            if (orientation == Orientation.Vertical) {
                if (!_columns.ContainsKey(index)) {
                    _columns[index] = new VariableGridColumn(index);
                }
                return _columns[index];
            } else if (orientation == Orientation.Horizontal) {
                if (!_rows.ContainsKey(index)) {
                    _rows[index] = new VariableGridRow(index);
                }
                return _rows[index];
            }

            throw new ArgumentException("PrepareLine can't understand orientation argument");
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

        private Dictionary<int, VariableGridStack> _rows = new Dictionary<int, VariableGridStack>();
        private Dictionary<int, VariableGridStack> _columns = new Dictionary<int, VariableGridStack>();
    }
}
