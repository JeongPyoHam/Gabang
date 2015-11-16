using System;
using System.Threading.Tasks;

namespace Gabang.Controls.Data {

    public class PagedGrid<T> {
        private VirtualizingGrid<Page<T>> _pages;
        private IGridProvider<T> _provider;

        public PagedGrid(int rowCount, int columnCount, IGridProvider<T> provider) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _provider = provider;

            _pages = new VirtualizingGrid<Page<T>>(RowPageCount, ColumnPageCount, CreatePage);
        }

        public int ColumnPageSize { get; set; }

        public int RowPageSize { get; set; }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public int ColumnPageCount { get; }

        public int RowPageCount { get; }


        private GridRange _currentViewport;

        public void ClearExcept(GridRange range) {
            int rowStartPage, columnStartPage;
            int rowEndPage, columnEndPage;

            GetPageIndex(range.Rows.Start, range.Columns.Start, out rowStartPage, out columnStartPage);
            GetPageIndex(
                range.Rows.Start + range.Rows.Count - 1,
                range.Columns.Start + range.Columns.Count - 1,
                out rowEndPage,
                out columnEndPage);

            GridRange newRange = new GridRange();
            newRange.Rows = new Range() { Start = rowStartPage, Count = rowEndPage - rowStartPage - 1 };
            newRange.Columns = new Range() { Start = columnStartPage, Count = columnEndPage - columnStartPage - 1 };

            _pages.ClearExcept(newRange);

            _currentViewport = newRange;
        }

        public async Task<T> GetAtAsync(int rowIndex, int columnIndex) {
            int rowPageIndex, columnPageIndex;

            GetPageIndex(rowIndex, columnIndex, out rowPageIndex, out columnPageIndex);

            var virtualizingItem = await _pages.GetAtAsync(rowPageIndex, columnPageIndex);

            return virtualizingItem.GetAt(rowIndex, columnIndex);
        }

        public void SetAt(int rowIndex, int columnIndex, T value) {
            throw new NotImplementedException();
        }

        private Page<T> CreatePage(int rowPageIndex, int columnPageIndex) {
            GridRange gridRange = GetPageRnage(rowPageIndex, columnPageIndex);

            return new Page<T>(
                rowPageIndex,
                columnPageIndex,
                () => {
                    return _provider.GetRangeAsync(gridRange);
                });
        }

        private GridRange GetPageRnage(int rowPageIndex, int columnPageIndex) {
            throw new NotImplementedException();
        }

        private void GetPageIndex(int rowIndex, int columnIndex, out int rowPageIndex, out int columnPageIndex) {
            throw new NotImplementedException();
        }
    }
}
