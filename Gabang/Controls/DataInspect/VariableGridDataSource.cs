using System;
using System.Collections;
using System.Collections.Generic;

namespace Gabang.Controls {
    public class DynamicGridDataSource : ConstantCountList<IntegerList> {  // TODO: change from IntegerList to generic
        public DynamicGridDataSource(int rowCount, int columnCount) : base(rowCount) {
            if (rowCount < 0) {
                throw new ArgumentOutOfRangeException("rowCount");
            }
            if (columnCount < 0) {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int RowCount { get; }
        public int ColumnCount { get; }

        #region ConstantCountList abstract implementation

        public override IntegerList this[int index] {
            get {
                return new IntegerList(ColumnCount * index, ColumnCount);
            }

            set {
                throw new NotSupportedException($"{typeof(DynamicGridDataSource)} is read only");
            }
        }

        public override int IndexOf(IntegerList item) {
            if (item.Start >= 0 && item.Count == ColumnCount) {
                int remainder;
                int rowIndex = Math.DivRem(item.Start, ColumnCount, out remainder);
                if (remainder == 0 && rowIndex < RowCount) {
                    return rowIndex;
                }
            }
            return -1;
        }

        #endregion
    }
}
