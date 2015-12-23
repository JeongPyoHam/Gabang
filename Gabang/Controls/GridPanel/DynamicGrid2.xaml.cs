﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gabang.Controls {
    public partial class DynamicGrid2 : UserControl {

        private GridPoints _gridPoints;

        public DynamicGrid2() {
            InitializeComponent();

            _gridPoints = new GridPoints(RowCount, ColumnCount);

            RowHeader.RowCount = RowCount;
            RowHeader.ColumnCount = 1;
            RowHeader.Points = _gridPoints;
            RowHeader.DataProvider = new DataProvider(RowCount, 1);

            ColumnHeader.RowCount = 1;
            ColumnHeader.ColumnCount = ColumnCount;
            ColumnHeader.Points = _gridPoints;
            ColumnHeader.DataProvider = new DataProvider(1, ColumnCount);

            Data.RowCount = RowCount;
            Data.ColumnCount = ColumnCount;
            Data.Points = _gridPoints;
            Data.DataProvider = new DataProvider(RowCount, ColumnCount);
        }

        public int RowCount { get { return 50; } }

        public int ColumnCount { get { return 60; } }
    }

    class DataProvider : IGridProvider<string> {
        public DataProvider(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            return Task.Run(async () => {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                return (IGrid<string>)new Grid<string>(gridRange, (r, c) => string.Format("{0}:{1}", r, c));
            });
        }
    }
}
