﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public struct GridItem : IndexedItem {
        public GridItem(int row, int column, bool isDefault = false) {
            Row = row;
            Column = column;
            Default = isDefault;
        }

        public bool Default { get; }

        public int Row { get; }

        public int Column { get; }

        int IndexedItem.Index
        {
            get
            {
                return Column;
            }
        }

        public override string ToString() {
            if (Default) {
                return "-:-";
            }
            return string.Format("{0}:{1}", Row, Column);
        }
    }

    public class ItemsProvider : IGridProvider<GridItem> {
        public ItemsProvider(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<List<GridItem>> GetHeaderAsync(Range range, bool isRow) {
            throw new NotImplementedException();
        }

        public Task<IGrid<GridItem>> GetRangeAsync(GridRange gridRange) {
            return Task.Run(() => {
                List<GridItem> data = new List<GridItem>(gridRange.Rows.Count * gridRange.Columns.Count);
                for (int c = 0; c < gridRange.Columns.Count; c++) {
                    for (int r = 0; r < gridRange.Rows.Count; r++) {
                        data.Add(
                            new GridItem(
                                r + gridRange.Rows.Start,
                                c + gridRange.Columns.Start));
                    }
                }
                var grid = new Grid<GridItem>(gridRange.Rows.Count, gridRange.Columns.Count, data);
                return (IGrid<GridItem>)grid;
            });
        }
    }
}
