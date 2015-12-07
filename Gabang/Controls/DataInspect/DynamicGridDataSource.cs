using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class DynamicGridDataSource : DelegateList<DelegateList<PageItem<GridItem>>> {

        Page2DManager<GridItem> _pageManager;

        public DynamicGridDataSource(Page2DManager<GridItem> pageManager)
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

        private static DelegateList<PageItem<GridItem>> GetItem(Page2DManager<GridItem> pm, int index, int itemCount) {
            return new DelegateList<PageItem<GridItem>>(
                index,
                (i) => GetGridItemFromPageManager(pm, index, i),
                itemCount);
        }

        private static PageItem<GridItem> GetGridItemFromPageManager(Page2DManager<GridItem> pm, int key, int index) {
            return pm.GetItem(key, index);
        }
    }
}
