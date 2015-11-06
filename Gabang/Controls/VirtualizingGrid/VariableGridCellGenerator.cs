using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //public void RemoveColumn(int column) {
        //    for (int r = 0; r < RowCount; r++) {
        //        RemoveAt(r, column);
        //    }
        //}

        //public void RemoveRow(int row) {
        //    for (int c = 0; c < ColumnCount; c++) {
        //        RemoveAt(row, c);
        //    }
        //}

        //public void RemoteAll() {
        //    for (int r = 0; r < RowCount; r++) {
        //        RemoveRow(r);
        //    }
        //}
    }
}
