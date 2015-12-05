using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class DynamicGridDataSource : ConstantCountList<VirtualItemsSource<GridItem>> {

        PageManager<GridItem> _pageManager;

        public DynamicGridDataSource(int rowCount, int columnCount) : base(rowCount) {
            if (rowCount < 0) {
                throw new ArgumentOutOfRangeException("rowCount");
            }
            if (columnCount < 0) {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;

            _pageManager = new PageManager<GridItem>(
                new ItemsProvider(rowCount, columnCount),
                64,
                TimeSpan.FromMinutes(1.0),
                4);
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        public override VirtualItemsSource<GridItem> this[int index] {
            get {
                return GetItem(index);
            }

            set {
                throw new NotSupportedException();
            }
        }

        public override int IndexOf(VirtualItemsSource<GridItem> item) {
            return item.Index;
        }

        private VirtualItemsSource<GridItem> GetItem(int index) {
            return new VirtualItemsSource<GridItem>(
                index,
                (i) => GetGridItemFromPageManager(_pageManager, index, i),
                ColumnCount);
        }

        private static GridItem GetGridItemFromPageManager(PageManager<GridItem> pm, int key, int index) {
            Page<GridItem> page;
            if (pm.TryGetPage(key, index, out page, true)) {
                return page.GetItem(key, index);
            }

            return new GridItem(key, index, true);
        }
    }
}
