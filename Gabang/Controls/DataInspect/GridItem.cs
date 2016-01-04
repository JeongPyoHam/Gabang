using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public struct GridItem : IIndexedItem {
        public GridItem(int row, int column, bool isDefault = false) {
            Row = row;
            Column = column;
            Default = isDefault;
        }

        public bool Default { get; }

        public int Row { get; }

        public int Column { get; }

        int IIndexedItem.Index
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

    public class HeaderProvider : IListProvider<string> {
        private bool _isRow;

        private const string RowFormat = "[{0},]";
        private const string ColumnFormat = "[,{0}]";

        public HeaderProvider(int count, bool isRow) {
            Count = count;
            _isRow = isRow;
        }

        public int Count { get; }

        public async Task<IList<string>> GetRangeAsync(Range range) {
            await Task.Delay(1);

            List<string> list = new List<string>();

            string format = _isRow ? RowFormat : ColumnFormat;
            for (int i = 0; i < range.Count; i++) {
                list.Add(string.Format(format, range.Start + i));
            }

            return list;
        }
    }

    public class ItemsProvider : IGridProvider<GridItem> {
        public ItemsProvider(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGridData<GridItem>> GetAsync(GridRange range) {
            throw new NotImplementedException();
        }

        public Task<IGrid<GridItem>> GetRangeAsync(GridRange gridRange) {
            return Task.Run(async () => {
                await Task.Delay(1000);

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
