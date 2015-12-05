using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class DynamicGridDataSource : DelegateList<DelegateList<GridItem>> {

        PageManager<GridItem> _pageManager;

        public DynamicGridDataSource(PageManager<GridItem> pageManager)
            : base(0,
                  (i) => GetItem(pageManager, i, pageManager.ColumnCount),
                  pageManager.RowCount) {
            _pageManager = pageManager;
        }

        public int RowCount {
            get { return _pageManager.RowCount; }
        }

        public int ColumnCount {
            get { return _pageManager.ColumnCount; }
        }

        private static DelegateList<GridItem> GetItem(PageManager<GridItem> pm, int index, int itemCount) {
            return new DelegateList<GridItem>(
                index,
                (i) => GetGridItemFromPageManager(pm, index, i),
                itemCount);
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
